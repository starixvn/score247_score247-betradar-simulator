using LiveScore.BetRadarSimulator.Api.Services;
using LiveScore.BetRadarSimulator.Results;
using LiveScore.BetRadarSimulator.Services;
using LiveScore.Soccers.Shared.Dtos;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace LiveScore.BetRadarSimulator.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventsController : ControllerBase
    {
        private const int DefaultInterval = 5 * 1000;
        private const string heartbeat = "{\"heartbeat\":{\"interval\":5000}} \n";

        private static System.Timers.Timer timer;
        private INotificationService notificationService;

        public EventsController(INotificationService notificationService)
        {
            this.notificationService = notificationService;
        }

        [HttpGet(nameof(Subscribe))]
        public IActionResult Subscribe()
        {
            timer = new System.Timers.Timer();
            timer.Interval = DefaultInterval;
            timer.AutoReset = true;
            timer.Elapsed += TimerElapsed;
            timer.Start();

            return new PushStreamResult(OnStreamAvailabe, HttpContext.RequestAborted);
        }

        [HttpPost(nameof(Push))]
        public async Task<IActionResult> Push(string eventStr)
        {
            await SendEvent(eventStr);

            return Ok();
        }

        [HttpPost(nameof(PushEvent))]
        public async Task<IActionResult> PushEvent([FromBody] MatchPushEventDto pushEvent)
        {
            var eventStr = JsonConvert.SerializeObject(pushEvent) + Environment.NewLine;

            await SendEvent(eventStr);

            return Ok();
        }

        private void OnStreamAvailabe(Stream stream, CancellationToken requestAborted)
        {
            var wait = requestAborted.WaitHandle;
            var client = new ServerSentEventsClient(stream);
            notificationService.AddClient(client);

            wait.WaitOne();

            notificationService.RemoveClient(client);
        }

        private async void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            foreach (var client in notificationService.GetClients())
            {
                try
                {
                    await client.SendEventAsync(heartbeat);
                }
                catch (Exception)
                {
                    notificationService.RemoveClient(client);
                }
            }
        }

        private async Task SendEvent(string eventstr)
        {
            foreach (var client in notificationService.GetClients())
            {
                var data = $"{eventstr} {Environment.NewLine}";

                await client.SendEventAsync(data);
            }
        }
    }
}
