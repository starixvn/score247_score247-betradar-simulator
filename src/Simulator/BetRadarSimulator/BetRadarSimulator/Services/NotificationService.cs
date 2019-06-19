using LiveScore.BetRadarSimulator.Api.Services;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;

namespace LiveScore.BetRadarSimulator.Services
{
    public interface INotificationService
    {
        void AddClient(IServerSentEventsClient client);

        void RemoveClient(IServerSentEventsClient client);

        IReadOnlyCollection<IServerSentEventsClient> GetClients();
    }

    public class NotificationService : INotificationService
    {
        private ConcurrentBag<IServerSentEventsClient> Clients;

        public NotificationService()
        {
            Clients = new ConcurrentBag<IServerSentEventsClient>();
        }

        public IReadOnlyCollection<IServerSentEventsClient> GetClients()
        => Clients.ToArray();

        public void AddClient(IServerSentEventsClient client)
        {
            Clients.Add(client);
        }

        public void RemoveClient(IServerSentEventsClient client)
        {
            Clients.TryTake(out client);
        }
    }
}
