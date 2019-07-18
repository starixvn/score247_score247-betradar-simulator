namespace LiveScore.Features.Odds
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using LiveScore.Database.Entities;
    using LiveScore.Soccer.Odds.Models;
    using LiveScore.Soccer.Database;
    using Newtonsoft.Json;
    using LiveScore.Soccer.Matches;
    using LiveScore.Soccer;
    using LiveScore.Soccer.Matches.Models;
    using LiveScore.Soccer.Odds.AppResources;

    public interface OddsService
    {
        Task<MatchOdds> GetOdds(string matchId, int betTypeId, string language);

        Task<MatchOddsMovement> GetOddsMovement(string matchId, int betTypeId, string bookmakerId, string language);

        Task<bool> InsertOdds(MatchOdds matchOdds);
    }

    public class OddsServiceImpl : OddsService
    {
        private readonly IDynamicRepository dynamicRepository;
        private readonly IMatchService matchService;

        public OddsServiceImpl(
            IDynamicRepository dynamicRepository,
            IMatchService matchService)
        {
            this.dynamicRepository = dynamicRepository;
            this.matchService = matchService;
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
                .Select(group => {
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
            => dynamicRepository
                            .Query<OddsEntity>(
                                o => o.MatchId == matchId
                                    && o.BetTypeId == betTypeId
                            )
                            .OrderByDescending(o => o.ModifiedTime)
                            .ToList();

        public async Task<bool> InsertOdds(MatchOdds matchOdds)
        {
            var insertOddsList = new List<BetTypeOdds>();

            foreach (var betTypeOdds in matchOdds.BetTypeOddsList)
            {
                var lastOddsList = GetOddsData(matchOdds.MatchId, betTypeOdds.Id);
                var lastBookMakerOdds = lastOddsList.FirstOrDefault(odds => odds.BookmakerId == betTypeOdds.Bookmaker.Id);

                if (lastBookMakerOdds == null
                    || (lastBookMakerOdds.ModifiedTime < matchOdds.LastUpdated
                        && !JsonConvert.DeserializeObject<BetTypeOdds>(lastBookMakerOdds.Value).Equals(betTypeOdds)))
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

            if(firstOdds == null)
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
                EventTypes.MatchEnded.Value
            };


        private async Task<IEnumerable<OddsMovement>> BuildOddsMovement(string matchId, List<BetTypeOdds> betTypeOddsList, string language)
        {
            var oddsMovements = new List<OddsMovement>();
            var match = await matchService.GetMatch(matchId, language);

            if(match.TimeLines == null)
            {
                return oddsMovements;
            }

            var timelineEvents = match.TimeLines.Where(tl 
                => OddChangeEventIds.Contains(tl.Type.ToLowerInvariant()) 
                    && (string.IsNullOrEmpty(tl.PeriodType) || tl.PeriodType == PeriodType.RegularPeriod.Value));

            var homeScore = 0;
            var awayScore = 0;
            Timeline firstHalfEvent = null;
            Timeline matchStartedEvent = null;
            Timeline secondHalfEvent = null;
            var liveMatchOdds = betTypeOddsList.Where(o => o.LastUpdatedTime >= match.EventDate);

            foreach (var betTypeOdds in liveMatchOdds)
            {
                var timelineEvent = timelineEvents.FirstOrDefault(e => e.Time == betTypeOdds.LastUpdatedTime);
                var oddsMovement = new OddsMovement
                {
                    BetOptions = betTypeOdds.BetOptions,
                    UpdateTime = betTypeOdds.LastUpdatedTime
                };

                if(timelineEvent != null)
                {
                    if(timelineEvent.Type == EventTypes.ScoreChange.Value)
                    {
                        homeScore = timelineEvent.HomeScore;
                        awayScore = timelineEvent.AwayScore;
                    }

                    if (timelineEvent.Type == EventTypes.PeriodStart.Value
                        && timelineEvent.Period == 1
                        && timelineEvent.PeriodType == PeriodType.RegularPeriod.Value)
                    {
                        firstHalfEvent = timelineEvent;
                        oddsMovement.MatchTime = OddsLang.KO;
                    }

                    if (timelineEvent.Type == EventTypes.PeriodStart.Value
                        && timelineEvent.Period == 2
                        && timelineEvent.PeriodType == PeriodType.RegularPeriod.Value)
                    {
                        secondHalfEvent = timelineEvent;
                    }
                }

                const int startSecondHaft = 46;

                if(secondHalfEvent != null)
                {
                    oddsMovement.MatchTime = ((betTypeOdds.LastUpdatedTime - secondHalfEvent.Time).TotalMinutes + startSecondHaft).ToString("0");
                }
                else if (firstHalfEvent != null && string.IsNullOrWhiteSpace(oddsMovement.MatchTime))
                {
                    oddsMovement.MatchTime = (betTypeOdds.LastUpdatedTime - firstHalfEvent.Time).TotalMinutes.ToString("0");
                }

                oddsMovement.HomeScore = homeScore;
                oddsMovement.AwayScore = awayScore;

                oddsMovements.Add(oddsMovement);
            }

            oddsMovements.Reverse();

            var todayOdds = betTypeOddsList.FirstOrDefault(o => o.LastUpdatedTime.Date == match.EventDate.Date);

            if (todayOdds != null)
            {
                oddsMovements.Add(new OddsMovement
                {
                    BetOptions = todayOdds.BetOptions,
                    MatchTime = OddsLang.Today,
                    UpdateTime = todayOdds.LastUpdatedTime
                });
            }

            var firstBetTypeOdds = betTypeOddsList.First();
            oddsMovements.Add(new OddsMovement
            {
                BetOptions = firstBetTypeOdds.BetOptions,
                MatchTime = OddsLang.Opening,
                UpdateTime = firstBetTypeOdds.LastUpdatedTime
            });

            return oddsMovements;
        }
    }
}