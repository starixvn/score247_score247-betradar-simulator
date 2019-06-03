namespace LiveScore.Jobs
{
    using LiveScore.Features.Matches.Models;
    using LiveScore.Shared;
    using LiveScore.Shared.Models;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public interface ILiveMatchFetchJob
    {
        Task Fetch(int sportId);
    }

    public class LiveMatchFetchJob : ILiveMatchFetchJob
    {
        private readonly DataAccessFactory instanceFactory;

        public LiveMatchFetchJob(
            DataAccessFactory instanceFactory)
        {
            this.instanceFactory = instanceFactory;
        }

        public async Task Fetch(int sportId)
        {
            var liveMatchIds = new List<string>();
            var closedMatchIds = new List<string>();

            var dataAccess = instanceFactory.CreateMatchDataAccess(sportId, DataProvider.ThirdPartyApi);
                        
            foreach (var lang in Enumeration.GetAll<Language>())
            {
                var language = Enumeration.FromValue<Language>(lang.Value).DisplayName.ToLowerInvariant();
                var matches = await dataAccess.GetLiveMatches(sportId, language);

                liveMatchIds.AddRange(matches.Where(x => x.MatchResult.EventStatus == MatchStatus.Live).Select(x => x.Id).ToList());
                closedMatchIds.AddRange(matches.Where(x => x.MatchResult.EventStatus == MatchStatus.Closed).Select(x => x.Id).ToList());

                // status == not_started -> incomming matches?
            }

            if (liveMatchIds.Any())
            {
                //TODO enqueue job to insert new live matches
            }

            if (closedMatchIds.Any())
            {
                //TODO enqueue job to delete live matches
            }
        }
    }
}
