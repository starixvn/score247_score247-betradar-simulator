using BetRadarSimulator.Results;
using BetRadarSimulator.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Threading;
using System.Timers;

namespace BetRadarSimulator.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventsController : ControllerBase
    {
        private const int DefaultInterval = 5 * 1000;
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

        private void OnStreamAvailabe(Stream stream, CancellationToken requestAborted)
        {
            var wait = requestAborted.WaitHandle;
            var client = new StreamWriter(stream);
            notificationService.Clients.Add(client);

            wait.WaitOne();

            StreamWriter ignore;
            notificationService.Clients.TryTake(out ignore);
        }

        private async void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            foreach (var client in notificationService.Clients)
            {
                try
                {
                    var data = $"heartbeat: {DefaultInterval} \n";
                    await client.WriteAsync(data);
                    await client.FlushAsync();
                }
                catch (Exception)
                {
                    StreamWriter ignore;
                    notificationService.Clients.TryTake(out ignore);
                }
            }
        }
    }
}
