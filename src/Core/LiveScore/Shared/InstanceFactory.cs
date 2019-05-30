namespace LiveScore.Shared
{
    using System;
    using System.Collections.Generic;
    using LiveScore.Features.Matches;

    public enum DataProvider
    {
        Database,
        ThirdPartyApi
    }

    public interface InstanceFactory
    {
        IMatchDataAccess CreateMatchDataAccess(int sportId, DataProvider dataProvider = DataProvider.Database);

        IPushEventHandler CreatePushEventHandler(int sportId);
    }

    public class InstanceFactoryImpl : InstanceFactory
    {
        private readonly IDictionary<int, IDictionary<DataProvider, IMatchDataAccess>> matchDataAccessContainer;
        private readonly IDictionary<int, IPushEventHandler> pushEventHandlerContainer;

        public InstanceFactoryImpl(
            IDictionary<int, IDictionary<DataProvider, IMatchDataAccess>> matchDataAccessContainer,
            IDictionary<int, IPushEventHandler> pushEventHandlerContainer)
        {
            this.matchDataAccessContainer = matchDataAccessContainer;
            this.pushEventHandlerContainer = pushEventHandlerContainer;
        }


        public IMatchDataAccess CreateMatchDataAccess(int sportId, DataProvider dataProvider = DataProvider.Database)
        {
            if (matchDataAccessContainer == null
                || !matchDataAccessContainer.ContainsKey(sportId))
            {
                throw new InvalidOperationException($"Cannot found implementation of Match Data Access for sport {sportId}");
            }

            return matchDataAccessContainer[sportId][dataProvider];
        }

        public IPushEventHandler CreatePushEventHandler(int sportId)
        {
            if (pushEventHandlerContainer == null || !pushEventHandlerContainer.ContainsKey(sportId))
            {
                throw new InvalidOperationException($"Cannot found implementation of Push Event Handler for sport {sportId}");
            }

            return pushEventHandlerContainer[sportId];
        }
    }
}