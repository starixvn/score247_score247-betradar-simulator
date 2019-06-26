using System.Collections.Generic;
using Fanex.Logging;
using Fanex.Logging.Extensions.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace LiveScore.BetRadarSimulator.Api.Midlewares
{
    public static class ErrorHandlingMiddleware
    {
        public static void ConfigureExceptionHandler(this IApplicationBuilder app)
        {
            app.UseExceptionHandler(errorApp =>
            {
                errorApp.Run(async context =>
                {
                    var logger = app.ApplicationServices.GetService<ILogger>();
                    var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();
                    var exception = exceptionHandlerPathFeature?.Error;
                    var customInfo = new Dictionary<string, object>();

                   
                    await ExceptionHandler.HandleAsync(exception, context, customInfo);

                    context.Response.StatusCode = 500;
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsync(exception.Message);
                });
            });
        }
    }
}
