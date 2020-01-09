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
        private const int firstPeriod = 1;
        private const int secondPeriod = 2;
        private const int startSecondHaft = 46;
        private const string pause = "pause";
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

                    first.AssignOpeningData(last.BetOptions);

                    return first;
                })
                .OrderBy(bto => bto.Bookmaker.Name)
                .AsEnumerable();

            return Task.FromResult(betTypeOdssList);
        }

        private IList<OddsEntity> GetOddsData(string matchId, int betTypeId)
            => dynamicRepository.Query<OddsEntity>(
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
            var betTypeOddsList = GetBookmakerOddsListByBetType(matchId, betTypeId, bookmakerId);
            var firstOdds = betTypeOddsList.FirstOrDefault();

            if (firstOdds == null)
            {
                return new MatchOddsMovement();
            }

            return new MatchOddsMovement
            {
                Bookmaker = firstOdds.Bookmaker,
                MatchId = matchId,
                OddsMovements = await BuildOddsMovement(matchId, betTypeOddsList, language)
            };
        }

        private List<BetTypeOdds> GetBookmakerOddsListByBetType(string matchId, int betTypeId, string bookmakerId)
            => dynamicRepository
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
            var match = await matchService.GetMatch(matchId, language);

            if (match == null)
            {
                return Enumerable.Empty<OddsMovement>();
            }

            var oddsMovements = BuildOddMovementBeforeMatchStart(betTypeOddsList, match)
                .Concat(BuildOddMovementAfterMatchStarted(betTypeOddsList, match));

            CalculateOddsTrend(oddsMovements);

            oddsMovements = oddsMovements.Reverse();

            return oddsMovements;
        }

        private static IEnumerable<Timeline> GetMainEvents(Match match)
            => match.TimeLines.Where(tl
#pragma warning disable S1067 // Expressions should not be too complex
                 => tl != null
                    && OddChangeEventIds.Contains(tl.Type?.ToLowerInvariant())
                    && (string.IsNullOrEmpty(tl.PeriodType)
                            || tl.PeriodType == PeriodType.RegularPeriod.Value
                            || tl.PeriodType == pause));

#pragma warning restore S1067 // Expressions should not be too complex

        private static void CalculateOddsTrend(IEnumerable<OddsMovement> oddsMovements)
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

        private static IEnumerable<OddsMovement> BuildOddMovementAfterMatchStarted(
            IEnumerable<BetTypeOdds> betTypeOddsList,
            Match match)
        {
            if (match.TimeLines == null)
            {
                return Enumerable.Empty<OddsMovement>();
            }

            var homeScore = 0;
            var awayScore = 0;
            Timeline currentEvent = null;
            var timelineEvents = GetMainEvents(match);
            var oddsMovements = new List<OddsMovement>();
            var matchLiveOdds = betTypeOddsList.Where(o => o.LastUpdatedTime >= match.EventDate);

            foreach (var betTypeOdds in matchLiveOdds)
            {
                var timelineEvent = timelineEvents.FirstOrDefault(e => e.Time == betTypeOdds.LastUpdatedTime);
                var oddsMovement = BuildOddsMovementEvent(ref homeScore, ref awayScore, ref currentEvent, timelineEvent, betTypeOdds);

                oddsMovements.Add(oddsMovement);
            }

            return oddsMovements;
        }

        private static OddsMovement BuildOddsMovementEvent(
            ref int homeScore,
            ref int awayScore,
            ref Timeline currentEvent,
            Timeline timelineEvent,
            BetTypeOdds betTypeOdds)
        {
            var oddsMovement = new OddsMovement
            {
                BetOptions = betTypeOdds.BetOptions,
                UpdateTime = betTypeOdds.LastUpdatedTime
            };

            if (IsScoreChange(timelineEvent))
            {
                homeScore = (int)timelineEvent.HomeScore;
                awayScore = (int)timelineEvent.AwayScore;
                oddsMovement.MatchTime = $"{timelineEvent.MatchTime}'";
            }

            if (IsHalfTimeBreakStart(timelineEvent))
            {
                currentEvent = timelineEvent;
                oddsMovement.MatchTime = OddsLang.HT;
            }

            if (IsFirstPeriodStart(timelineEvent))
            {
                currentEvent = timelineEvent;
                oddsMovement.MatchTime = OddsLang.KO;
            }

            if (IsSecondPeriodStart(timelineEvent))
            {
                currentEvent = timelineEvent;
                currentEvent.Time = currentEvent.Time.AddMinutes(-startSecondHaft);
            }

            if (string.IsNullOrWhiteSpace(oddsMovement.MatchTime))
            {
                var totalMinutes = (oddsMovement.UpdateTime - currentEvent.Time).TotalMinutes;
                oddsMovement.MatchTime = totalMinutes.ToString("0") + "'";
            }

            oddsMovement.IsMatchStarted = currentEvent != null;
            oddsMovement.HomeScore = homeScore;
            oddsMovement.AwayScore = awayScore;

            return oddsMovement;
        }

        private static bool IsScoreChange(Timeline timelineEvent)
            => timelineEvent != null
                && timelineEvent.Type == EventTypes.ScoreChange.Value;

        private static bool IsHalfTimeBreakStart(Timeline timelineEvent)
            => timelineEvent != null
                && timelineEvent.Type == EventTypes.BreakStart.Value
                && timelineEvent.PeriodType.ToLowerInvariant() == pause;

        private static bool IsSecondPeriodStart(Timeline timelineEvent)
            => timelineEvent != null
                && timelineEvent.Type == EventTypes.PeriodStart.Value
                && timelineEvent.Period == secondPeriod
                && timelineEvent.PeriodType == PeriodType.RegularPeriod.Value;

        private static bool IsFirstPeriodStart(Timeline timelineEvent)
            => timelineEvent != null
                && timelineEvent.Type == EventTypes.PeriodStart.Value
                && timelineEvent.Period == firstPeriod
                && timelineEvent.PeriodType == PeriodType.RegularPeriod.Value;

        private static IEnumerable<OddsMovement> BuildOddMovementBeforeMatchStart(
            List<BetTypeOdds> betTypeOddsList,
            Match match)
        {
            var oddsMovements = new List<OddsMovement>();
            var firstBetTypeOdds = betTypeOddsList.FirstOrDefault();
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

            return oddsMovements;
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