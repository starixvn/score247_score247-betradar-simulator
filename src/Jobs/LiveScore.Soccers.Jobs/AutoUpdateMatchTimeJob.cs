namespace LiveScore.Soccers.Jobs
{
    using System;
    using System.Threading.Tasks;
    using Fanex.Caching;
    using Hangfire;
    using Hangfire.Common;
    using LiveScore.Features.Matches;
    using LiveScore.Shared;
    using LiveScore.Shared.Hubs;
    using Microsoft.AspNetCore.SignalR;

    public interface IAutoUpdateMatchTimeJob
    {
        void Start();
    }

    public class AutoUpdateMatchTimeJob : IAutoUpdateMatchTimeJob
    {
        private readonly IMatchService matchService;
        private readonly ICacheService cacheService;
        private readonly IRecurringJobManager recurringJobManager;
        private readonly IHubContext<MatchHub> matchHub;

        public AutoUpdateMatchTimeJob(
            IRecurringJobManager recurringJobManager,
             IMatchService matchService,
             ICacheService cacheService,
             IHubContext<MatchHub> matchHub)
        {
            this.recurringJobManager = recurringJobManager;
            this.matchService = matchService;
            this.cacheService = cacheService;
            this.matchHub = matchHub;
        }

        public void Start()
        {
            recurringJobManager.AddOrUpdate(nameof(AutoUpdateMatchTimeJob), Job.FromExpression(() => UpdateMatchTime()), Cron.Minutely());
        }

        public async Task UpdateMatchTime()
        {
            var liveMatches = await matchService.GetLiveMatches(SportConstants.SoccerId, "en-US");

            foreach (var match in liveMatches)
            {
                var cacheKey = "MatchTime-" + match.Id;
                var matchClock = await cacheService.GetOrSetAsync(
                        cacheKey,
                        () => string.IsNullOrEmpty(match.MatchResult.MatchTime) ? 1 : Convert.ToInt32(match.MatchResult.MatchTime.Split(':')[0]),
                        new CacheItemOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(120)));

                if (matchClock < 120)
                {
                    matchClock++;
                }

                await cacheService.SetAsync(cacheKey, matchClock);

                await matchHub.Clients.All.SendAsync("PushMatchTime", SportConstants.SoccerId.ToString(), match.Id, matchClock);
            }
        }
    }
}
