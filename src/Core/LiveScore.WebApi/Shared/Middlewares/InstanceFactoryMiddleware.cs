namespace LiveScore.WebApi.Shared.Middlewares
{
    using System.Collections.Generic;
    using Fanex.Logging;
    using LiveScore.Features.Matches;
    using LiveScore.Shared;
    using LiveScore.Shared.Configurations;
    using LiveScore.Shared.Database;
    using LiveScore.Shared.Helpers;
    using LiveScore.Soccers.Features.Matches;
    using Microsoft.Extensions.DependencyInjection;

    public static class InstanceFactoryMiddleware
    {
        public static void RegisterDataAccessFactory(this IServiceCollection services)
        {
            var matchDataAccessContainer = BuildMatchDataAccessContainer(services);

            services.AddSingleton((DataAccessFactory)new DataAccessFactoryImpl(matchDataAccessContainer));
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
            var logger = serviceProvider.GetService<ILogger>();
            var penaltyEventHandler = serviceProvider.GetService<IPenaltyEventHandler>();

            return new Dictionary<DataProvider, IMatchDataAccess>
            {
                { DataProvider.Database, new Soccers.Features.Matches.DataAccesses.MatchDataAccess(repository, logger) },
                { DataProvider.ThirdPartyApi, new Soccers.Features.Matches.DataAccesses.SportRadarMatchDataAccess(appSettings, restClient, penaltyEventHandler) },
            };
        }
    }
}
