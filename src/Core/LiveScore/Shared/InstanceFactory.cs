namespace LiveScore.Shared
{
    using System;
    using System.Collections.Generic;
    using LiveScore.Features.Leagues;
    using LiveScore.Features.Matches;

    public interface InstanceFactory
    {
        MatchDataAccess CreateMatchDataAccess(int sportId);

        LeagueDataAccess CreateLeagueDataAccess(int sportId);
    }

    public class InstanceFactoryImpl : InstanceFactory
    {
        private readonly IDictionary<int, MatchDataAccess> matchDataAccessContainer;
        private readonly IDictionary<int, LeagueDataAccess> leagueDataAccessContainer;

        public InstanceFactoryImpl(
            IDictionary<int, MatchDataAccess> matchDataAccessContainer,
            IDictionary<int, LeagueDataAccess> leagueDataAccessContainer)
        {
            this.matchDataAccessContainer = matchDataAccessContainer;
            this.leagueDataAccessContainer = leagueDataAccessContainer;
        }

        public LeagueDataAccess CreateLeagueDataAccess(int sportId)
        {
            if (leagueDataAccessContainer == null
                || !leagueDataAccessContainer.ContainsKey(sportId))
            {
                throw new InvalidOperationException($"Cannot found implementation of League Data Access for sport {sportId}");
            }

            return leagueDataAccessContainer[sportId];
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