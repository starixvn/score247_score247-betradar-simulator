namespace LiveScore.Jobs
{
    using System;
    using System.Collections.Generic;
    using LiveScore.Jobs.Features.Matches;

    public interface JobInstanceFactory
    {
        MatchJobDataAccess CreateMatchJobDataAccess(int sportId);
    }

    public class JobInstanceFactoryImpl : JobInstanceFactory
    {
        private readonly IDictionary<int, MatchJobDataAccess> matchDataAccessContainer;

        public JobInstanceFactoryImpl(
            IDictionary<int, MatchJobDataAccess> matchDataAccessContainer)
        {
            this.matchDataAccessContainer = matchDataAccessContainer;
        }

        public MatchJobDataAccess CreateMatchJobDataAccess(int sportId)
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