namespace LiveScore.WebApi.Features.Notifications
{
    using LiveScore.Features.Matches.Models;
    using LiveScore.Shared.Configurations;
    using Microsoft.AspNetCore.SignalR.Client;
    using System.Threading.Tasks;

    public interface PushService
    {
        Task Connect();

        Task Disconnect();

        Task Send(string message);

        Task Send(string matchId, TimeLine timelineEvent);
    }

    public class PushServiceImpl : PushService
    {
        private readonly HubConnection hubConnection;

        public PushServiceImpl(IAppSettings appSettings)
        {
            hubConnection = new HubConnectionBuilder()
                .WithUrl($"{appSettings.Host}/hubs/MatchHub")
                .Build();
        }

        public Task Connect() => hubConnection.StartAsync();

        public Task Disconnect() => hubConnection.StopAsync();

        public Task Send(string message) => hubConnection.InvokeAsync("SendMessage", message);

        public Task Send(string matchId, TimeLine timelineEvent)
        {
            return hubConnection.InvokeAsync("SendMatchEvent", matchId, timelineEvent);
        }
    }
}
