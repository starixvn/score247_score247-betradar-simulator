namespace LiveScore.Soccers.Features.Matches
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Fanex.Caching;
    using LiveScore.Features.Matches.Models;

    public interface IPenaltyEventHandler
    {
        Task<Timeline> Handle(string matchId, Timeline shootoutEvent);
    }

    public class PenaltyEventHandler : IPenaltyEventHandler
    {
        private const int minNumOfSettlePenalties = 6;

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
            var latestShootout = await GetLatestShoot(shootoutEvent, latestEventCacheKey);

            if (shootoutEvent != null)
            {
                if (latestShootout == null)
                {
                    SetFirstShoot(shootoutEvent);
                }

                cachedPenaltyEvents.Add(shootoutEvent);

                SetScore(cachedPenaltyEvents, shootoutEvent);

                if (latestShootout != null || cachedPenaltyEvents.Count > minNumOfSettlePenalties)
                {
                    return await HandleLatestPenaltyEvent(matchEventsCacheKey, latestEventCacheKey, cachedPenaltyEvents, shootoutEvent, latestShootout);
                }

                await CachePenaltyEvents(matchEventsCacheKey, cachedPenaltyEvents);

                return await Task.FromResult(shootoutEvent);
            }

            return await Task.FromResult(default(Timeline));
        }

        private static void SetFirstShoot(Timeline shootoutEvent)
        {
            shootoutEvent.IsFirstShoot = true;

            if (shootoutEvent.IsHome)
            {
                shootoutEvent.HomeShootoutPlayer = shootoutEvent.Player;
            }
            else
            {
                shootoutEvent.AwayShootoutPlayer = shootoutEvent.Player;
            }
        }

        private async Task<Timeline> GetLatestShoot(Timeline shootoutEvent, string latestEventCacheKey)
        {
            var latestShootout = (await cacheService.GetAsync<Timeline>(latestEventCacheKey));
            if (latestShootout == null)
            {
                await cacheService.SetAsync(latestEventCacheKey, shootoutEvent, MatchPenaltyCacheOptions);
            }

            return latestShootout;
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
                    shootoutEvent.HomeShootoutPlayer = shootoutEvent.Player;
                }
                else
                {
                    shootoutEvent.HomeShootoutPlayer = latestPenalty.HomeShootoutPlayer;
                    shootoutEvent.IsHomeShootoutScored = latestPenalty.IsHomeShootoutScored;
                    shootoutEvent.AwayShootoutPlayer = shootoutEvent.Player;
                }

                await CachePenaltyEvents(cacheKey, cachedPenaltyEvents);
                await cacheService.RemoveAsync(latestEventCacheKey);
            }

            return await Task.FromResult(shootoutEvent);
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
    }
}