namespace LiveScore.Soccers.Jobs
{
    using System.Linq;
    using System.Threading.Tasks;
    using Hangfire;
    using LiveScore.Features.Matches;
    using LiveScore.Features.Matches.Models;
    using LiveScore.Shared;
    using LiveScore.Shared.Hubs;
    using LiveScore.Shared.Models;
    using Microsoft.AspNetCore.SignalR;

    public interface IAutoUpdateMatchTimeJob
    {
        Task UpdateMatchTime();
    }

    [AutomaticRetry(OnAttemptsExceeded = AttemptsExceededAction.Delete)]
    public class AutoUpdateMatchTimeJob : IAutoUpdateMatchTimeJob
    {
        private const int MaxMatchTime = 120;
        private readonly IMatchService matchService;
        private readonly IHubContext<MatchHub> matchHub;

        public AutoUpdateMatchTimeJob(
             IMatchService matchService,
             IHubContext<MatchHub> matchHub)
        {
            this.matchService = matchService;
            this.matchHub = matchHub;
        }

        public async Task UpdateMatchTime()
        {
            foreach (var language in Enumeration.GetAll<Language>())
            {
                var liveMatches = await matchService.GetLiveMatches(SportConstants.SoccerId, language.Value);

                foreach (var match in liveMatches)
                {
                    var matchTime = match.MatchResult.MatchTime;
                    var matchEvent = match.LatestTimeline ?? match.TimeLines?.LastOrDefault();
                    var injuryTime = matchEvent?.InjuryTimeAnnounced ?? 0;

                    if (matchTime == (MaxMatchTime + injuryTime))
                    {
                        continue;
                    }

                    matchTime++;

                    match.MatchResult.MatchTime = matchTime;

                    await matchHub.Clients.All.SendAsync("PushMatchTime", SportConstants.SoccerId.ToString(), match.Id, matchTime);
                }

                await matchService.UpdateLiveMatches(SportConstants.SoccerId, language.Value, liveMatches);
            }
        }
    }
}
