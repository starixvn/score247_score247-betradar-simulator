namespace LiveScore.Features.Matches
{
    using LiveScore.Shared.Hubs;
    using Microsoft.AspNetCore.SignalR;
    using System.Threading.Tasks;

    public interface IMatchPushEventListener
    {
        Task Start(byte sportId, IHubContext<MatchHub> hubContext);
    }
}