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
                if (latestPenalty != null)
                {
                    return await HandleLatestPenaltyEvent(matchEventsCacheKey, latestEventCacheKey, cachedPenaltyEvents, shootoutEvent, latestPenalty);
                }

                await CachePenaltyEvents(shootoutEvent, matchEventsCacheKey, cachedPenaltyEvents);
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
            if (latestPenalty != null)
            {
                if (shootoutEvent.IsHome && !latestPenalty.IsHome)
                {
                    shootoutEvent.AwayShootoutPlayer = latestPenalty.AwayShootoutPlayer;
                    shootoutEvent.IsAwayShootoutScored = latestPenalty.IsAwayShootoutScored;
                }
                else if (!shootoutEvent.IsHome && latestPenalty.IsHome)
                {
                    shootoutEvent.HomeShootoutPlayer = latestPenalty.AwayShootoutPlayer;
                    shootoutEvent.IsHomeShootoutScored = latestPenalty.IsAwayShootoutScored;
                }

                await CachePenaltyEvents(shootoutEvent, cacheKey, cachedPenaltyEvents);

                SetScore(cachedPenaltyEvents, shootoutEvent);

                await cacheService.RemoveAsync(latestEventCacheKey);

                return await Task.FromResult(shootoutEvent);
            }

            return await Task.FromResult(default(Timeline));
        }

        private async Task CachePenaltyEvents(Timeline shootoutEvent, string cacheKey, IList<Timeline> cachedPenaltyEvents)
        {
            cachedPenaltyEvents.Add(shootoutEvent);
            await cacheService.SetAsync(cacheKey, cachedPenaltyEvents, MatchPenaltyCacheOptions);
        }

        private static void SetScore(IList<Timeline> shootoutEvents, Timeline shoutoutTimeline)
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

            shoutoutTimeline.ShootoutHomeScore = homeScore;
            shoutoutTimeline.ShootoutAwayScore = awayScore;
        }
    }
}