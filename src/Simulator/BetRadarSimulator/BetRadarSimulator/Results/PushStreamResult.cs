using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BetRadarSimulator.Results
{
    public class PushStreamResult : IActionResult
    {
        private readonly Action<Stream, CancellationToken> onStreamAvailabe;
        private readonly CancellationToken cts;

        public PushStreamResult(Action<Stream, CancellationToken> onStreamAvailabe, CancellationToken cts)
        {
            this.onStreamAvailabe = onStreamAvailabe;
            this.cts = cts;
        }

        public Task ExecuteResultAsync(ActionContext context)
        {
            var stream = context.HttpContext.Response.Body;

            onStreamAvailabe(stream, cts);

            return Task.CompletedTask;
        }
    }
}
