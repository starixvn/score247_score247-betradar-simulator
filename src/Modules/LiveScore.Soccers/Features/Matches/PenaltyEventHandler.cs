namespace LiveScore.Soccers.Features.Matches
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Fanex.Caching;
    using LiveScore.Features.Matches.Models;

    public interface IPenaltyEventHandler
    {
        Task<Timeline> Handle(string matchId, Timeline shootoutEvent);
    }

    public class PenaltyEventHandler : IPenaltyEventHandler
    {
        const int minNumOfSettlePenalties = 6;

        private static readonly CacheItemOptions MatchPenaltyCacheOptions = new CacheItemOptions
        {
            SlidingExpiration = TimeSpan.FromMinutes(10),
        };

        private readonly ICacheService cacheService;

        public PenaltyEventHandler(ICacheService cacheService)
        {
            this.cacheService = cacheService;
        }

        public async Task<Timeline> Handle(string matchId, Timeline shootoutEvent)
        {
            var matchEventsCacheKey = $"Penalty_Match_{matchId}";
            var latestEventCacheKey = $"Penalty_Match_{matchId}_Latest_Event";
            var cachedPenaltyEvents = (await cacheService.GetAsync<IList<Timeline>>(matchEventsCacheKey)) ?? new List<Timeline>();
            
            var latestPenalty = (await cacheService.GetAsync<Timeline>(latestEventCacheKey));

            if (latestPenalty == null)
            {
                await cacheService.SetAsync(latestEventCacheKey, shootoutEvent, MatchPenaltyCacheOptions);
            }

            if (shootoutEvent != null)
            {
                cachedPenaltyEvents.Add(shootoutEvent);

                if (latestPenalty != null || cachedPenaltyEvents.Count > minNumOfSettlePenalties)
                {
                    return await HandleLatestPenaltyEvent(matchEventsCacheKey, latestEventCacheKey, cachedPenaltyEvents, shootoutEvent, latestPenalty);
                }

                await CachePenaltyEvents(matchEventsCacheKey, cachedPenaltyEvents);
            }

            return await Task.FromResult(default(Timeline));
        }

        private async Task<Timeline> HandleLatestPenaltyEvent(
            string cacheKey, 
            string latestEventCacheKey, 
            IList<Timeline> cachedPenaltyEvents, 
            Timeline shootoutEvent, 
            Timeline latestPenalty)
        {
            SetScore(cachedPenaltyEvents, shootoutEvent);

            if (IsPenaltyCompleted(cachedPenaltyEvents.Count, shootoutEvent.ShootoutHomeScore, shootoutEvent.ShootoutAwayScore))
            {
                return await Task.FromResult(shootoutEvent);
            }

            if (latestPenalty != null)
            {
                if (shootoutEvent.IsHome && !latestPenalty.IsHome)
                {
                    shootoutEvent.AwayShootoutPlayer = latestPenalty.AwayShootoutPlayer;
                    shootoutEvent.IsAwayShootoutScored = latestPenalty.IsAwayShootoutScored;
                    shootoutEvent.HomeShootoutPlayer = shootoutEvent.Player;
                }
                else
                {
                    shootoutEvent.HomeShootoutPlayer = latestPenalty.AwayShootoutPlayer;
                    shootoutEvent.IsHomeShootoutScored = latestPenalty.IsAwayShootoutScored;
                    shootoutEvent.AwayShootoutPlayer = shootoutEvent.Player;
                }

                await CachePenaltyEvents(cacheKey, cachedPenaltyEvents);

                await cacheService.RemoveAsync(latestEventCacheKey);

                return await Task.FromResult(shootoutEvent);
            }

            return await Task.FromResult(default(Timeline));
        }

        private async Task CachePenaltyEvents(string cacheKey, IList<Timeline> cachedPenaltyEvents)
        {
            await cacheService.SetAsync(cacheKey, cachedPenaltyEvents, MatchPenaltyCacheOptions);
        }

        private static void SetScore(IList<Timeline> shootoutEvents, Timeline shootoutTimeline)
        {
            var homeScore = 0;
            var awayScore = 0;

            foreach (var shootoutEvent in shootoutEvents)
            {
                if (shootoutEvent != null)
                {
                    homeScore += shootoutEvent.IsHome && shootoutEvent.IsHomeShootoutScored ? 1 : 0;
                    awayScore += !shootoutEvent.IsHome && shootoutEvent.IsAwayShootoutScored ? 1 : 0;
                }
            }

            shootoutTimeline.ShootoutHomeScore = homeScore;
            shootoutTimeline.ShootoutAwayScore = awayScore;
        }

        private static bool IsPenaltyCompleted(
            int totalShootout,
            int shootoutHomeScore,
            int shootoutAwayScore)
        {
            if (totalShootout > minNumOfSettlePenalties)
            {
                var differenceScore = Math.Abs(shootoutHomeScore - shootoutAwayScore);
                const int threeGapScore = 3;
                if (differenceScore >= threeGapScore)
                {
                    return true;
                }

                const int twoGapScore = 2;
                const int eightShoots = 8;
                if (totalShootout >= eightShoots && differenceScore >= twoGapScore)
                {
                    return true;
                }

                const int oneGapScore = 1;
                const int tenShoots = 10;
                if (totalShootout >= tenShoots && totalShootout % twoGapScore == 0 && differenceScore >= oneGapScore)
                {
                    return true;
                }
            }

            return false;
        }
    }
}