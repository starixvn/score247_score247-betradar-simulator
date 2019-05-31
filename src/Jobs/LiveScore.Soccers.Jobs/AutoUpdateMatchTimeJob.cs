namespace LiveScore.Soccers.Jobs
{
    using System.Linq;
    using System.Threading.Tasks;
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
        Task UpdateMatchTime();
    }

    public class AutoUpdateMatchTimeJob : IAutoUpdateMatchTimeJob
    {
        private const int MaxMatchTime = 120;
        private readonly IMatchService matchService;
        private readonly IHubContext<MatchHub> matchHub;
        private readonly IDynamicRepository dynamicRepository;

        public AutoUpdateMatchTimeJob(
             IMatchService matchService,
             IHubContext<MatchHub> matchHub,
             IDynamicRepository dynamicRepository)
        {
            this.matchService = matchService;
            this.matchHub = matchHub;
            this.dynamicRepository = dynamicRepository;
        }

        public async Task UpdateMatchTime()
        {
            var liveMatches = await matchService.GetLiveMatches(SportConstants.SoccerId, "en-US");

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

                await matchHub.Clients.All.SendAsync("PushMatchTime", SportConstants.SoccerId.ToString(), match.Id, matchTime);

                await UpdateMatchResult(match, matchTime);
            }
        }

        private async Task UpdateMatchResult(IMatch match, int matchTime)
        {
            var matchResultEntity = (await dynamicRepository.Query<MatchResultEntity>(t => t.MatchId == match.Id))
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
