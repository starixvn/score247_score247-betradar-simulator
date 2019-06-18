using System.Collections.Concurrent;
using System.IO;

namespace BetRadarSimulator.Services
{
    public interface INotificationService
    {
        ConcurrentBag<StreamWriter> Clients { get; set; }
    }

    public class NotificationService : INotificationService
    {
        public ConcurrentBag<StreamWriter> Clients { get; set; }

        public NotificationService()
        {
            Clients = new ConcurrentBag<StreamWriter>();
        }
    }
}
