namespace LiveScore.Jobs
{
    using LiveScore.Features.Matches.Models;
    using LiveScore.Shared;
    using LiveScore.Shared.Database;
    using LiveScore.Shared.Database.Entities;
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

        private readonly IDynamicRepository repository;
        private readonly InstanceFactory instanceFactory;

        public LiveMatchFetchJob(
            IDynamicRepository repository,
            InstanceFactory instanceFactory)
        {
            this.instanceFactory = instanceFactory;
            this.repository = repository;
        }

        public async Task Fetch(int sportId)
        {
            var liveMatchIds = new List<string>();
            var closedMatchIds = new List<string>();

            var dataAccess = instanceFactory.CreateMatchDataAccess(sportId, DataProvider.ThirdPartyApi);
                        
            foreach (var lang in Enumeration.GetAll<Language>())
            {
                var matches = await dataAccess.GetLiveMatches(sportId, Enumeration.FromValue<Language>(lang.Value).DisplayName.ToLowerInvariant());

                liveMatchIds.AddRange(matches.Where(x => x.MatchResult.EventStatus == MatchStatus.Live).Select(x => x.Id).ToList());
                closedMatchIds.AddRange(matches.Where(x => x.MatchResult.EventStatus == MatchStatus.Closed).Select(x => x.Id).ToList());
            }

            // status == not_started -> incomming matches?
                        
            if (liveMatchIds.Any())
            {
                await UpdateLiveMatches(liveMatchIds);
            }

            if (closedMatchIds.Any())
            {
                await UpdateClosedMatches(closedMatchIds);
            }
        }

        private async Task UpdateLiveMatches(List<string> matchIds)
        {
            // status == live && not exist in live table -> insert to live table -> update cache

            foreach (var id in matchIds)
            {
                var liveMatchEntity = await repository.GetById<LiveMatchEntity>(id);

                if (liveMatchEntity == null)
                {
                    var matchEntity = await repository.GetById<MatchEntity>(id);

                    liveMatchEntity = new LiveMatchEntity
                    {
                        Id = matchEntity.Id,
                        SportId = matchEntity.SportId,
                        Language = matchEntity.Language,
                        Value = matchEntity.Value,
                        LeagueId = matchEntity.LeagueId,
                        EventDate = matchEntity.EventDate.ToUniversalTime()
                    };

                    await repository.Insert(liveMatchEntity);
                }
            }

            await repository.Save();

        }

        private async Task UpdateClosedMatches(List<string> matchIds)
        {
            // status == closed && exist in live table -> delete in live table ->  update cache

            foreach (var id in matchIds)
            {
                //TODO update get id pair
                var matchEntity = await repository.GetById<LiveMatchEntity>(id);

                if (matchEntity != null)
                {
                    repository.Delete<LiveMatchEntity>(matchEntity);
                }
            }

            //TODO update result of matches?

            await repository.Save();
        }
    }
}
