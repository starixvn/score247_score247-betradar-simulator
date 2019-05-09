namespace LiveScore.WebApi.Features.Matches
{
    using LiveScore.Shared.Configurations;
    using Microsoft.AspNetCore.SignalR.Client;
    using System.Threading.Tasks;

    public interface MatchStatusService
    {
        Task Connect();

        Task Disconnect();

        Task Send(string message);
    }

    public class MatchStatusServiceImpl : MatchStatusService
    {
        private readonly HubConnection hubConnection;

        public MatchStatusServiceImpl(IAppSettings appSettings)
        {
            hubConnection = new HubConnectionBuilder()
                .WithUrl($"{appSettings.Host}/hubs/MatchStatusHub")
                .Build();
        }

        public Task Connect() => hubConnection.StartAsync();

        public Task Disconnect() => hubConnection.StopAsync();

        public Task Send(string message) => hubConnection.InvokeAsync("SendMessage", message);
    }
}
