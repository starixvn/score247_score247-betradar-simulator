namespace LiveScore.Shared
{
    using System;
    using System.Collections.Generic;
    using LiveScore.Features.Matches;

    public interface InstanceFactory
    {
        MatchDataAccess CreateMatchDataAccess(int sportId);
    }

    public class InstanceFactoryImpl : InstanceFactory
    {
        private readonly IDictionary<int, MatchDataAccess> matchDataAccessContainer;

        public InstanceFactoryImpl(
            IDictionary<int, MatchDataAccess> matchDataAccessContainer)
        {
            this.matchDataAccessContainer = matchDataAccessContainer;
        }

        public MatchDataAccess CreateMatchDataAccess(int sportId)
        {
            if (matchDataAccessContainer == null
                || !matchDataAccessContainer.ContainsKey(sportId))
            {
                throw new InvalidOperationException($"Cannot found implementation of Match Data Access for sport {sportId}");
            }

            return matchDataAccessContainer[sportId];
        }
    }
}