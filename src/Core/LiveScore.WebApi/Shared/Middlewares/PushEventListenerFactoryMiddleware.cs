namespace LiveScore.WebApi.Shared.Middlewares
{
    using System;
    using System.Collections.Generic;
    using LiveScore.Soccer.Jobs;
    using LiveScore.Soccer.Matches.Handlers;
    using LiveScore.Soccer.Matches.Hubs.EventHandling;
    using LiveScore.Soccer.Shared;
    using Microsoft.Extensions.DependencyInjection;

    public static class PushEventListenerFactoryMiddleware
    {
        public static void RegisterPushEventListenerFactory(this IServiceCollection services)
        {
            services.AddScoped<IMatchPushEventHandler, MatchPushEventHandler>();
            services.AddScoped<IMatchPushEventListener, MatchPushEventListener>();
            services.AddSingleton<IPenaltyEventHandler, PenaltyEventHandler>();
            services.AddScoped<IPushEventHandler, PushEventHandler>();
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