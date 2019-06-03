namespace LiveScore.Soccers.Jobs
{
    using System.Linq;
    using System.Threading.Tasks;
    using LiveScore.Features.Matches;
    using LiveScore.Features.Matches.Models;
    using LiveScore.Shared;
    using LiveScore.Shared.Hubs;
    using Microsoft.AspNetCore.SignalR;

    public interface IAutoUpdateMatchTimeJob
    {
        Task UpdateMatchTime();
    }

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
            foreach (var language in Language.SupportedLanguages)
            {
                var liveMatches = await matchService.GetAllLiveMatches(SportConstants.SoccerId, language.Value);

                foreach (var match in liveMatches)
                {
                    var matchTime = match.MatchResult.MatchTime;
                    var matchEvent = match.TimeLines?.FirstOrDefault();
                    var maxMatchTime = MaxMatchTime + ((matchEvent?.Type == EventTypes.InjuryTimeShown.Value) ? matchEvent?.InjuryTimeAnnounced : 0);

                    if (matchTime == maxMatchTime)
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
