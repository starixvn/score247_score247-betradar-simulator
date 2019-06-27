namespace LiveScore.WebApi.Shared.Middlewares
{
    using System;
    using System.Collections.Generic;
    using LiveScore.Features.Matches;
    using LiveScore.Shared;
    using LiveScore.Soccers.Features.Matches;
    using Microsoft.Extensions.DependencyInjection;

    public static class PushEventListenerFactoryMiddleware
    {
        public static void RegisterPushEventListenerFactory(this IServiceCollection services)
        {
            services.AddScoped<MatchPushEventListener>();
            services.AddScoped<Func<int, IMatchPushEventListener>>(serviceProvider => (sportId) =>
            {
                switch (sportId)
                {
                    case SportConstants.SoccerId:
                        return serviceProvider.GetService<MatchPushEventListener>();

                    case SportConstants.BasketballId:
                        throw new NotImplementedException("Not implement PushEventListener for Basketball");

                    default:
                        throw new KeyNotFoundException("Not found implementation of " + sportId);
                }
            });
        }
    }
}