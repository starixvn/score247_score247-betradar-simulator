namespace LiveScore.WebApi.Shared.Middlewares
{
    using System.Collections.Generic;
    using Fanex.Logging;
    using LiveScore.Features.Matches;
    using LiveScore.Shared;
    using LiveScore.Shared.Configurations;
    using LiveScore.Soccers.Features.Matches;
    using Microsoft.Extensions.DependencyInjection;

    public static class PushEventListenerFactoryMiddleware
    {
        public static void RegisterPushEventListenerFactory(this IServiceCollection services)
        {
            var matchDataAccessContainer = BuildPushEventListenerContainer(services);

            services.AddSingleton((PushEventListenerFactory)new PushEventListenerFactoryImpl(matchDataAccessContainer));
        }

        private static Dictionary<int, IMatchPushEventListener> BuildPushEventListenerContainer(IServiceCollection services)
        {
            var serviceProvider = services.BuildServiceProvider();
            var pushHandler = serviceProvider.GetService<IMatchPushEventHandler>();
            var appSettings = serviceProvider.GetService<IAppSettings>();
            var logger = serviceProvider.GetService<ILogger>();

            var pushEventListenerContainer = new Dictionary<int, IMatchPushEventListener>
                {
                    { SportConstants.SoccerId, new MatchPushEventListener(pushHandler, appSettings, logger) },
                };

            return pushEventListenerContainer;
        }
    }
}
