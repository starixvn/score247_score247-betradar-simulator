namespace LiveScore.WebApi.Shared.Middlewares
{
    using System.Collections.Generic;
    using LiveScore.Features.Matches;
    using LiveScore.Shared;
    using LiveScore.Shared.Configurations;
    using LiveScore.Shared.Database;
    using LiveScore.Shared.Helpers;
    using LiveScore.Shared.Hubs;
    using LiveScore.Soccers.Features.Matches;
    using Microsoft.AspNetCore.SignalR;
    using Microsoft.Extensions.DependencyInjection;

    public static class InstanceFactoryMiddleware
    {
        public static void RegisterInstanceFactory(this IServiceCollection services)
        {
            var matchDataAccessContainer = BuildMatchDataAccessContainer(services);
            var pushEventHandlerContainer = BuildPushEventHandlerContainer(services, matchDataAccessContainer);

            services.AddSingleton((InstanceFactory)new InstanceFactoryImpl(matchDataAccessContainer, pushEventHandlerContainer));
        }

        private static Dictionary<int, IDictionary<DataProvider, IMatchDataAccess>> BuildMatchDataAccessContainer(IServiceCollection services)
        {
            var matchDataAccessContainer = new Dictionary<int, IDictionary<DataProvider, IMatchDataAccess>>
                {
                        { SportConstants.SoccerId, GetMatchDataAccess(services)},
                    ////{ SportConstants.BasketballId, new LiveScore.Basketballs.Features.Matches.MatchDataAccessImpl() }
                };

            return matchDataAccessContainer;
        }

        private static Dictionary<int, IPushEventHandler> BuildPushEventHandlerContainer(IServiceCollection services, Dictionary<int, IDictionary<DataProvider, IMatchDataAccess>> matchDataAccessContainer)
        {
            var hubContext = services.BuildServiceProvider().GetService<IHubContext<PushMatchEventHub>>();

            var pushEventHandlerContainer = new Dictionary<int, IPushEventHandler>
            {
                { SportConstants.SoccerId, new PushEventHandler(matchDataAccessContainer[SportConstants.SoccerId][DataProvider.Database], hubContext) }
            };

            return pushEventHandlerContainer;
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
