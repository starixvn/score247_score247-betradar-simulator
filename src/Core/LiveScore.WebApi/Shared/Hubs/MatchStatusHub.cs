namespace LiveScore.WebApi.Hubs
{
    using Microsoft.AspNetCore.SignalR;
    using System.Threading.Tasks;

    public class MatchStatusHub : Hub
    {
        public async Task SendMessage(string message)
        {
            await Clients.All.SendAsync("Push", message);
        }
    }
}
