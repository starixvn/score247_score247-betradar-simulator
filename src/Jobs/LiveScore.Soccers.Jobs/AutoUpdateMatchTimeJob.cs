namespace LiveScore.Soccers.Jobs
{
    using System.Threading.Tasks;
    using Fanex.Caching;
    using Hangfire;
    using Hangfire.Common;
    using LiveScore.Features.Matches;
    using LiveScore.Features.Matches.Models;
    using LiveScore.Shared;
    using LiveScore.Shared.Database;
    using LiveScore.Shared.Database.Entities;
    using LiveScore.Shared.Hubs;
    using Microsoft.AspNetCore.SignalR;
    using Newtonsoft.Json;

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
        private readonly IDynamicRepository dynamicRepository;

        public AutoUpdateMatchTimeJob(
             IRecurringJobManager recurringJobManager,
             IMatchService matchService,
             ICacheService cacheService,
             IHubContext<MatchHub> matchHub,
             IDynamicRepository dynamicRepository)
        {
            this.recurringJobManager = recurringJobManager;
            this.matchService = matchService;
            this.cacheService = cacheService;
            this.matchHub = matchHub;
            this.dynamicRepository = dynamicRepository;
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
                var matchTime = match.MatchResult.MatchTime;

                if (matchTime < 120)
                {
                    matchTime++;
                }

                await matchHub.Clients.All.SendAsync("PushMatchTime", SportConstants.SoccerId.ToString(), match.Id, matchTime);

                await UpdateMatchResult(match, matchTime);
            }
        }

        private async Task UpdateMatchResult(IMatch match, int matchTime)
        {
            var matchResultEntity = await dynamicRepository.Last<MatchResultEntity>(m => m.MatchId == match.Id);

            if (matchResultEntity != null)
            {
                var matchResult = JsonConvert.DeserializeObject<MatchResult>(matchResultEntity.Value);
                matchResult.MatchTime = matchTime;
                matchResultEntity.Value = JsonConvert.SerializeObject(matchResult);

                dynamicRepository.Update(matchResultEntity);
                await dynamicRepository.Save();
            }
        }
    }
}
