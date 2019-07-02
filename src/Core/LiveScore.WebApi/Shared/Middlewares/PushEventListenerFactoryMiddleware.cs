namespace LiveScore.WebApi.Shared.Middlewares
{
    using System;
    using System.Collections.Generic;
    using LiveScore.Features.Matches;
    using LiveScore.Soccer.Jobs;
    using LiveScore.Soccer.Shared;
    using LiveScore.Soccer.SignalRHub.Match.EventHandling;
    using Microsoft.Extensions.DependencyInjection;

    public static class PushEventListenerFactoryMiddleware
    {
        public static void RegisterPushEventListenerFactory(this IServiceCollection services)
        {
            services.AddScoped<IMatchPushEventHandler, MatchPushEventHandler>();
            services.AddScoped<IMatchPushEventListener, MatchPushEventListener>();
            services.AddSingleton<IPenaltyEventHandler, PenaltyEventHandler>();
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