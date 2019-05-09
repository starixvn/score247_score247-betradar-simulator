namespace LiveScore.WebApi.Hubs
{
    using LiveScore.Features.Matches.Models;
    using Microsoft.AspNetCore.SignalR;
    using System.Threading.Tasks;

    public class MatchHub : Hub
    {
        public async Task SendMessage(string message)
        {
            await Clients.All.SendAsync("Push", message);
        }

        public async Task SendMatchEvent(string matchId, TimeLine timeLine)
        {
            await Clients.All.SendAsync("PushMatchEvent", matchId, timeLine);
        }
    }
}
