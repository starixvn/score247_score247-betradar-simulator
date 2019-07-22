namespace LiveScore.Features.Odds
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using LiveScore.Database.Entities;
    using LiveScore.Soccer;
    using LiveScore.Soccer.Configurations;
    using LiveScore.Soccer.Database;
    using LiveScore.Soccer.Matches;
    using LiveScore.Soccer.Matches.Models;
    using LiveScore.Soccer.Odds.AppResources;
    using LiveScore.Soccer.Odds.Models;
    using Newtonsoft.Json;

    public interface OddsService
    {
        Task<MatchOdds> GetOdds(string matchId, int betTypeId, string language);

        Task<MatchOddsMovement> GetOddsMovement(string matchId, int betTypeId, string bookmakerId, string language);

        Task<bool> InsertOdds(IEnumerable<MatchOdds> matchOdds, bool forceInsert = false);
    }

    public class OddsServiceImpl : OddsService
    {
        private const string Pause = "pause";
        private readonly IDynamicRepository dynamicRepository;
        private readonly IMatchService matchService;
        private readonly IAppSettings appSettings;

        public OddsServiceImpl(
            IDynamicRepository dynamicRepository,
            IMatchService matchService,
            IAppSettings appSettings)
        {
            this.dynamicRepository = dynamicRepository;
            this.matchService = matchService;
            this.appSettings = appSettings;
        }

        public async Task<MatchOdds> GetOdds(string matchId, int betTypeId, string language)
            => new MatchOdds
            {
                MatchId = matchId,
                BetTypeOddsList = await GetBetTypeOddsList(matchId, betTypeId)
            };

        private Task<IEnumerable<BetTypeOdds>> GetBetTypeOddsList(string matchId, int betTypeId)
        {
            var oddsEntities = GetOddsData(matchId, betTypeId)
                .Select(m => JsonConvert.DeserializeObject<BetTypeOdds>(m.Value))
                .GroupBy(o => o.Bookmaker.Id);

            var betTypeOdssList = oddsEntities
                .Select(group =>
                {
                    var first = group.First();
                    var last = group.Last();

                    BetTypeOdds.AssignOpeningData(first.BetOptions, last.BetOptions);

                    return first;
                })
                .OrderBy(bto => bto.Bookmaker.Name)
                .AsEnumerable();

            return Task.FromResult(betTypeOdssList);
        }

        private IList<OddsEntity> GetOddsData(string matchId, int betTypeId)
            => dynamicRepository
                            .Query<OddsEntity>(
                                o => o.MatchId == matchId
                                    && o.BetTypeId == betTypeId
                            )
                            .OrderByDescending(o => o.ModifiedTime)
                            .ToList();

        public async Task<bool> InsertOdds(MatchOdds matchOdds, bool forceInsert = false)
        {
            var insertOddsList = new List<BetTypeOdds>();

            foreach (var betTypeOdds in matchOdds.BetTypeOddsList)
            {
                var lastOddsList = GetOddsData(matchOdds.MatchId, betTypeOdds.Id);
                var lastOdds = lastOddsList.FirstOrDefault(odds => odds.BookmakerId == betTypeOdds.Bookmaker.Id);

                if (lastOdds == null
                    || forceInsert
                    || (lastOdds.ModifiedTime < matchOdds.LastUpdated
                        && !JsonConvert.DeserializeObject<BetTypeOdds>(lastOdds.Value).Equals(betTypeOdds)))
                {
                    insertOddsList.Add(betTypeOdds);
                }
            }

            if (insertOddsList.Any())
            {
                return await InsertOddsByBetType(matchOdds.MatchId, insertOddsList, matchOdds.LastUpdated);
            }

            return true;
        }

        private async Task<bool> InsertOddsByBetType(
            string matchId,
            IEnumerable<BetTypeOdds> BetTypeOddsList,
            DateTime? lastUpdate)
            => await dynamicRepository.InsertRange(BetTypeOddsList.Select(betTypeOdds
               =>
            {
                betTypeOdds.LastUpdatedTime = lastUpdate.HasValue
                       ? lastUpdate.Value
                       : DateTimeOffset.Now;

                var oddsEntity = new OddsEntity
                {
                    Id = Guid.NewGuid().ToString(),
                    BetTypeId = betTypeOdds.Id,
                    BookmakerId = betTypeOdds.Bookmaker.Id,
                    MatchId = matchId,
                    ModifiedTime = betTypeOdds.LastUpdatedTime,
                    Value = JsonConvert.SerializeObject(betTypeOdds),
                    CreatedTime = DateTimeOffset.Now
                };

                return oddsEntity;
            }));

        public async Task<MatchOddsMovement> GetOddsMovement(string matchId, int betTypeId, string bookmakerId, string language)
        {
            var betTypeOddList = dynamicRepository
                            .Query<OddsEntity>(
                                o => o.MatchId == matchId
                                    && o.BetTypeId == betTypeId
                                    && o.BookmakerId == bookmakerId
                            )
                            .OrderBy(o => o.ModifiedTime)
                            .Select(o =>
                            {
                                var bto = JsonConvert.DeserializeObject<BetTypeOdds>(o.Value);
                                bto.LastUpdatedTime = o.ModifiedTime;

                                return bto;
                            })
                            .ToList();

            var firstOdds = betTypeOddList.FirstOrDefault();

            if (firstOdds == null)
            {
                return new MatchOddsMovement();
            }

            return new MatchOddsMovement
            {
                Bookmaker = firstOdds.Bookmaker,
                MatchId = matchId,
                OddsMovements = await BuildOddsMovement(matchId, betTypeOddList, language)
            };
        }

        private static readonly IList<string> OddChangeEventIds
            = new List<string>
            {
                EventTypes.PeriodStart.Value,
                EventTypes.ScoreChange.Value,
                EventTypes.MatchEnded.Value,
                EventTypes.BreakStart.Value
            };

        private async Task<IEnumerable<OddsMovement>> BuildOddsMovement(
            string matchId,
            List<BetTypeOdds> betTypeOddsList,
            string language)
        {
            var oddsMovements = new List<OddsMovement>();
            var match = await matchService.GetMatch(matchId, language);

            if (match == null)
            {
                return oddsMovements;
            }

            var firstBetTypeOdds = betTypeOddsList.First();

            BuildOddMovementBeforeMatchStart(betTypeOddsList, oddsMovements, match, firstBetTypeOdds);

            if (match.TimeLines != null)
            {
                var timelineEvents = GetMainEvents(match);
                BuildOddMovementAfterMatchStarted(betTypeOddsList, oddsMovements, match, timelineEvents);
            }

            CalculateOddsTrend(oddsMovements);

            oddsMovements.Reverse();

            return oddsMovements;
        }

        private static IEnumerable<Timeline> GetMainEvents(Match match)
            => match.TimeLines.Where(tl
#pragma warning disable S1067 // Expressions should not be too complex
                 => tl != null 
                    && OddChangeEventIds.Contains(tl.Type?.ToLowerInvariant())
                    && (string.IsNullOrEmpty(tl.PeriodType) 
                            || tl.PeriodType == PeriodType.RegularPeriod.Value
                            || tl.PeriodType == Pause));
#pragma warning restore S1067 // Expressions should not be too complex

        private static void CalculateOddsTrend(List<OddsMovement> oddsMovements)
        {
            OddsMovement prevOdds = null;
            foreach (var oddsMovement in oddsMovements)
            {
                if (prevOdds == null)
                {
                    prevOdds = oddsMovement;
                }
                else
                {
                    oddsMovement.CalculateOddsTrend(prevOdds.BetOptions);
                    prevOdds = oddsMovement;
                }
            }
        }

        private static void BuildOddMovementAfterMatchStarted(
            List<BetTypeOdds> betTypeOddsList,
            List<OddsMovement> oddsMovements,
            Match match,
            IEnumerable<Timeline> timelineEvents)
        {
            var homeScore = 0;
            var awayScore = 0;
            Timeline firstHalfEvent = null;
            Timeline secondHalfEvent = null;
            Timeline firstBreakStartEvent = null;
            var liveMatchOdds = betTypeOddsList.Where(o => o.LastUpdatedTime >= match.EventDate);
            foreach (var betTypeOdds in liveMatchOdds)
            {
                var timelineEvent = timelineEvents.FirstOrDefault(e => e.Time == betTypeOdds.LastUpdatedTime);
                var oddsMovement = new OddsMovement
                {
                    BetOptions = betTypeOdds.BetOptions,
                    UpdateTime = betTypeOdds.LastUpdatedTime
                };

                CalculateEvents(ref homeScore, ref awayScore, ref firstHalfEvent, ref secondHalfEvent, ref firstBreakStartEvent, timelineEvent, oddsMovement);
                CalculateMatchTime(firstHalfEvent, secondHalfEvent, firstBreakStartEvent, betTypeOdds, oddsMovement);

                if (firstHalfEvent != null)
                {
                    oddsMovement.IsMatchStarted = true;
                }

                oddsMovement.HomeScore = homeScore;
                oddsMovement.AwayScore = awayScore;
                oddsMovements.Add(oddsMovement);
            }
        }

        private static void CalculateEvents(
            ref int homeScore,
            ref int awayScore,
            ref Timeline firstHalfEvent,
            ref Timeline secondHalfEvent,
            ref Timeline firstBreakStartEvent,
            Timeline timelineEvent,
            OddsMovement oddsMovement)
        {
            if (timelineEvent != null)
            {
                const string pause = "pause";
                if (timelineEvent.Type == EventTypes.ScoreChange.Value)
                {
                    homeScore = timelineEvent.HomeScore;
                    awayScore = timelineEvent.AwayScore;
                    oddsMovement.MatchTime = timelineEvent.MatchTime.ToString();
                }
                else if (timelineEvent.Type == EventTypes.BreakStart.Value
                    && timelineEvent.PeriodType.ToLowerInvariant() == pause)
                {
                    firstBreakStartEvent = timelineEvent;
                    oddsMovement.MatchTime = OddsLang.HT;
                }
                else
                {
                    HandlePeriodStartEvent(ref firstHalfEvent, ref secondHalfEvent, timelineEvent, oddsMovement);
                }
            }
        }

        private static void HandlePeriodStartEvent(ref Timeline firstHalfEvent, ref Timeline secondHalfEvent, Timeline timelineEvent, OddsMovement oddsMovement)
        {
            const int firstPeriod = 1;
            const int secondPeriod = 2;
            if (timelineEvent.Type == EventTypes.PeriodStart.Value
                && timelineEvent.Period == firstPeriod
                && timelineEvent.PeriodType == PeriodType.RegularPeriod.Value)
            {
                firstHalfEvent = timelineEvent;
                oddsMovement.MatchTime = OddsLang.KO;
            }

            if (timelineEvent.Type == EventTypes.PeriodStart.Value
                && timelineEvent.Period == secondPeriod
                && timelineEvent.PeriodType == PeriodType.RegularPeriod.Value)
            {
                secondHalfEvent = timelineEvent;
            }
        }

        private static void CalculateMatchTime(
            Timeline firstHalfEvent,
            Timeline secondHalfEvent,
            Timeline firstBreakStartEvent,
            BetTypeOdds betTypeOdds,
            OddsMovement oddsMovement)
        {
            const int startSecondHaft = 46;

            if (string.IsNullOrWhiteSpace(oddsMovement.MatchTime))
            {
                if (secondHalfEvent != null)
                {
                    var totalSecondHalfMinutes = (betTypeOdds.LastUpdatedTime - secondHalfEvent.Time).TotalMinutes;
                    oddsMovement.MatchTime = (totalSecondHalfMinutes + startSecondHaft).ToString("0");
                }
                else if(firstBreakStartEvent != null)
                {
                    oddsMovement.MatchTime = OddsLang.HT;
                }
#pragma warning disable S126 // "if ... else if" constructs should end with "else" clauses
                else if (firstHalfEvent != null)
#pragma warning restore S126 // "if ... else if" constructs should end with "else" clauses
                {
                    var totalFirstHalfMinutes = (betTypeOdds.LastUpdatedTime - firstHalfEvent.Time).TotalMinutes;
                    oddsMovement.MatchTime = totalFirstHalfMinutes.ToString("0");
                }
            }
        }

        private static void BuildOddMovementBeforeMatchStart(
            List<BetTypeOdds> betTypeOddsList,
            List<OddsMovement> oddsMovements,
            Match match,
            BetTypeOdds firstBetTypeOdds)
        {
            var openingOddsMovement = new OddsMovement
            {
                BetOptions = firstBetTypeOdds.BetOptions,
                MatchTime = OddsLang.Opening,
                UpdateTime = firstBetTypeOdds.LastUpdatedTime
            };
            openingOddsMovement.ResetToOpeningOdds();
            oddsMovements.Add(openingOddsMovement);

            var beforeLiveMatchOdds = betTypeOddsList.Where(
                o =>
                    o.LastUpdatedTime < match.EventDate
                    && o.LastUpdatedTime > firstBetTypeOdds.LastUpdatedTime);

            foreach (var betTypeOdds in beforeLiveMatchOdds)
            {
                var oddsMovement = new OddsMovement
                {
                    BetOptions = betTypeOdds.BetOptions,
                    UpdateTime = betTypeOdds.LastUpdatedTime,
                    MatchTime = OddsLang.Live
                };

                oddsMovements.Add(oddsMovement);
            }
        }

        public async Task<bool> InsertOdds(IEnumerable<MatchOdds> matchOdds, bool forceInsert = false)
        {
            var matches = await matchService.GetMatches(
                SportConstants.SoccerId,
                DateTime.UtcNow.Date,
                DateTime.UtcNow.AddDays(appSettings.Jobs.FetchScheduleDateSpan + 1),
                new TimeSpan(),
                Language.En.Value);

            var filteredMatchOdds = matchOdds.Where(m => matches.Any(match => match.Id == m.MatchId));

            foreach (var oddsMatch in filteredMatchOdds)
            {
                await InsertOdds(oddsMatch, forceInsert);
            }

            return true;
        }
    }
}