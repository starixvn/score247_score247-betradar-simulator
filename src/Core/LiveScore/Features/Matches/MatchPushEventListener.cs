namespace LiveScore.Features.Matches
{
    using System.Threading.Tasks;
    using LiveScore.Shared.Hubs;
    using Microsoft.AspNetCore.SignalR;

    public interface IMatchPushEventListener
    {
        Task<bool> Start(byte sportId, IHubContext<MatchHub> hubContext);
    }
}
