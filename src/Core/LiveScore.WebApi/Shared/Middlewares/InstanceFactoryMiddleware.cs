namespace LiveScore.WebApi.Shared.Middlewares
{
    using System.Collections.Generic;
    using LiveScore.Features.Matches;
    using LiveScore.Shared;
    using LiveScore.Shared.Configurations;
    using LiveScore.Shared.Database;
    using LiveScore.Shared.Helpers;
    using LiveScore.Shared.Hubs;
    using Microsoft.AspNetCore.SignalR;
    using Microsoft.Extensions.DependencyInjection;
    using LiveScore.Soccers.Features.Matches;
    using Fanex.Logging;
    using LiveScore.Shared.Factories;

    public static class InstanceFactoryMiddleware
    {
        public static void RegisterDataAccessFactory(this IServiceCollection services)
        {
            var matchDataAccessContainer = BuildMatchDataAccessContainer(services);

            services.AddSingleton((DataAccessFactory)new DataAccessFactoryImpl(matchDataAccessContainer));
        }

        public static void RegisterPushHandlerFactory(this IServiceCollection services)
        {
            var serviceProvider = services.BuildServiceProvider();
            var hubContext = serviceProvider.GetService<IHubContext<MatchHub>>();
            var matchService = serviceProvider.GetService<IMatchService>();
            var logger = serviceProvider.GetService<ILogger>();

            var pushHandlerContainer = new Dictionary<int, IMatchPushEventHandler>
            {
                { SportConstants.SoccerId, new MatchPushEventHandler(matchService, hubContext, logger) }
            };

            services.AddSingleton((PushHandlerFactory)new PushHandlerFactoryImpl(pushHandlerContainer));
        }

        public static void RegisterPushListenerFactory(this IServiceCollection services)
        {
            var serviceProvider = services.BuildServiceProvider();            
            var pushHandlerFactory = serviceProvider.GetService<PushHandlerFactory>();

            var pushListenerContainer = new Dictionary<int, IMatchPushEventListener>
            {
                { SportConstants.SoccerId, new MatchPushEventListener(pushHandlerFactory) }
            };

            services.AddSingleton((PushListenerFactory)new PushListenerFactoryImpl(pushListenerContainer));
        }

        private static Dictionary<int, IDictionary<DataProvider, IMatchDataAccess>> BuildMatchDataAccessContainer(IServiceCollection services)
        {
            var matchDataAccessContainer = new Dictionary<int, IDictionary<DataProvider, IMatchDataAccess>>
                {
                    { SportConstants.SoccerId, GetMatchDataAccess(services)},
                };

            return matchDataAccessContainer;
        }

        private static Dictionary<DataProvider, IMatchDataAccess> GetMatchDataAccess(IServiceCollection services)
        {
            var serviceProvider = services.BuildServiceProvider();
            var repository = serviceProvider.GetService<IDynamicRepository>();
            var appSettings = serviceProvider.GetService<IAppSettings>();
            var restClient = serviceProvider.GetService<IRestClient>();

            return new Dictionary<DataProvider, IMatchDataAccess>
            {
                { DataProvider.Database, new Soccers.Features.Matches.DataAccesses.MatchDataAccess(repository) },
                { DataProvider.ThirdPartyApi, new Soccers.Features.Matches.DataAccesses.SportRadarMatchDataAccess(appSettings, restClient) },
            };
        }
    }
}
