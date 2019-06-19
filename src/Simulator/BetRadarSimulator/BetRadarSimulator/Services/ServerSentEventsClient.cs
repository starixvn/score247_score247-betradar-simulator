using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LiveScore.BetRadarSimulator.Api.Services
{
    public interface IServerSentEventsClient
    {
        Guid Id { get; }

        bool IsConnected { get; }

        Task SendEventAsync(string text);

        Task SendEventAsync(string text, CancellationToken cancellationToken);
    }

    public class ServerSentEventsClient : IServerSentEventsClient
    {
        private readonly StreamWriter streamWiter;

        public ServerSentEventsClient(Stream stream)
        {
            streamWiter = new StreamWriter(stream);
        }

        public Guid Id => throw new NotImplementedException();

        public bool IsConnected => throw new NotImplementedException();

        public async Task SendEventAsync(string text)
        {
            await streamWiter.WriteAsync(text);
            await streamWiter.FlushAsync();
        }

        public Task SendEventAsync(string text, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
