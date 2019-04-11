namespace LiveScore.WebApi.Shared.Middlewares
{
    using System;
    using System.Net;
    using System.Threading.Tasks;
    using Fanex.Logging.Extensions.AspNetCore;
    using Microsoft.AspNetCore.Http;

    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate next;

        public ErrorHandlingMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        public async Task Invoke(HttpContext context /* other dependencies */)
        {
            try
            {
                await next(context);
            }
            catch (Exception ex)
            {
                await ExceptionHandler.HandleAsync(ex, context).ConfigureAwait(false);
                await HandleExceptionAsync(context).ConfigureAwait(false);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context)
        {
            const HttpStatusCode code = HttpStatusCode.InternalServerError; // 500 if unexpected

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)code;
            return context.Response.WriteAsync("System Error!");
        }
    }
}