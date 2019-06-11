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

    public interface DataAccessFactory
    {
        IMatchDataAccess CreateMatchDataAccess(int sportId, DataProvider dataProvider = DataProvider.Database);
    }

    public class DataAccessFactoryImpl : DataAccessFactory
    {
        private readonly IDictionary<int, IDictionary<DataProvider, IMatchDataAccess>> matchDataAccessContainer;

        public DataAccessFactoryImpl(
            IDictionary<int, IDictionary<DataProvider, IMatchDataAccess>> matchDataAccessContainer)
        {
            this.matchDataAccessContainer = matchDataAccessContainer;
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
    }
}