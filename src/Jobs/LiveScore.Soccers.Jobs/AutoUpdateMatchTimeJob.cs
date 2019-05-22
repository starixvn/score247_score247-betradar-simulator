namespace LiveScore.Soccers.Jobs
{
    using System.Linq;
    using System.Threading.Tasks;
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
        private readonly IRecurringJobManager recurringJobManager;
        private readonly IHubContext<MatchHub> matchHub;
        private readonly IDynamicRepository dynamicRepository;

        public AutoUpdateMatchTimeJob(
             IRecurringJobManager recurringJobManager,
             IMatchService matchService,
             IHubContext<MatchHub> matchHub,
             IDynamicRepository dynamicRepository)
        {
            this.recurringJobManager = recurringJobManager;
            this.matchService = matchService;
            this.matchHub = matchHub;
            this.dynamicRepository = dynamicRepository;
        }

        public void Start()
        {
            recurringJobManager.AddOrUpdate(nameof(AutoUpdateMatchTimeJob), Job.FromExpression(() => UpdateMatchTime()), Cron.Minutely());
        }

        public async Task UpdateMatchTime()
        {
            const int MaxTime = 120;
            var liveMatches = await matchService.GetLiveMatches(SportConstants.SoccerId, "en-US");

            foreach (var match in liveMatches)
            {
                var matchTime = match.MatchResult.MatchTime;
                var matchStatus = match.MatchResult.MatchStatus;

                if (matchTime == 45 && matchStatus.IsFirstHalf()
                    || matchStatus.IsHalfTime()
                    || (matchTime == 90 && matchStatus.IsSecondHalf())
                    || (matchTime == 105 && matchStatus.IsFirstHaftExtra())
                    || (matchTime == 120 && matchStatus.IsSecondHalfExtra()))
                {
                    return;
                }

                matchTime++;

                await matchHub.Clients.All.SendAsync("PushMatchTime", SportConstants.SoccerId.ToString(), match.Id, matchTime);

                await UpdateMatchResult(match, matchTime);
            }
        }

        private async Task UpdateMatchResult(IMatch match, int matchTime)
        {
            var matchResultEntity = (await dynamicRepository.Find<MatchResultEntity>(t => t.MatchId == match.Id))
                .OrderBy(r => r.Id)
                .LastOrDefault();

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
