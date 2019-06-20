using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LiveScore.Features.Matches.Models;
using Microsoft.AspNetCore.SignalR.Client;
using Newtonsoft.Json;

namespace LiveScore.HubClientSample
{
    class Program
    {
        static HubConnection connection;

        static void Main(string[] args)
        {
            connection = new HubConnectionBuilder()
                .WithUrl("http://localhost:64296/hubs/matchhub")
                .Build();

            connection.Closed += async (error) =>
            {
                await Task.Delay(new Random().Next(0, 5) * 1000);
                await connection.StartAsync();
            };

            connection.On<string, Dictionary<string, MatchPushEvent>>("PushMatchEvent", (sportId, data) =>
            {
                Console.WriteLine($"{sportId}: {JsonConvert.SerializeObject(data, Formatting.Indented)}");
            });

            connection.On<string>("TestEvent", (text) =>
            {
                Console.WriteLine(text);
            });

            connection.StartAsync().GetAwaiter().GetResult();

            Console.WriteLine("Start Listen");

            Console.ReadLine();
        }
    }
}
