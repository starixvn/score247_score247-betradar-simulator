namespace LiveScore.Jobs
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using LiveScore.Features.Matches.Models;
    using LiveScore.Shared;
    using LiveScore.Shared.Database;
    using LiveScore.Shared.Database.Entities;
    using LiveScore.Shared.Models;
    using Newtonsoft.Json;

    public interface IDailyScheduleFetchJob
    {
        Task FetchSchedule(int sportId, int dateSpan);

        Task FetchResult(int sportId, int dateSpan);
    }

    public class DailyScheduleFetchJob : IDailyScheduleFetchJob
    {
        private readonly IDynamicRepository repository;
        private readonly DataAccessFactory instanceFactory;

        public DailyScheduleFetchJob(
            IDynamicRepository repository,
            DataAccessFactory instanceFactory)
        {
            this.instanceFactory = instanceFactory;
            this.repository = repository;
        }

        public async Task FetchSchedule(int sportId, int dateSpan)
        {
            var from = DateTime.Now;
            var to = DateTime.Now.AddDays(dateSpan);

            foreach (var lang in Enumeration.GetAll<Language>())
            {
                await FetchDailySchedule(sportId, from, to, lang.Value);
            }
        }

        public async Task FetchResult(int sportId, int dateSpan)
        {
            var from = DateTime.Now.AddDays(-dateSpan);
            var to = DateTime.Now;

            foreach (var lang in Enumeration.GetAll<Language>())
            {
                await FetchDailySchedule(sportId, from, to, lang.Value);
            }
        }

        private async Task FetchDailySchedule(int sportId, DateTime from, DateTime to, string language)
        {
            var liveMatchIds = new List<string>();
            var dataAccess = instanceFactory.CreateMatchDataAccess(sportId, DataProvider.ThirdPartyApi);

            var matches = await dataAccess.GetMatches(sportId, from, to, Enumeration.FromValue<Language>(language).DisplayName.ToLowerInvariant());

            foreach (var match in matches)
            {
                var matchEntity = await repository.GetById<MatchEntity>(match.Id, language);

                if (matchEntity == null)
                {
                    var newMatchEntity = new MatchEntity
                    {
                        Id = match.Id,
                        EventDate = match.EventDate.ToUniversalTime(),
                        LeagueId = match.League.Id,
                        Language = language,
                        SportId = sportId,
                        Value = JsonConvert.SerializeObject(match)
                    };

                    await repository.Insert(newMatchEntity);
                }
                else
                {
                    matchEntity.EventDate = match.EventDate;
                    matchEntity.Value = JsonConvert.SerializeObject(match);

                    repository.Update(matchEntity); 
                }

                if (match.MatchResult.EventStatus == MatchStatus.Closed)
                {
                    ////update live match to closed
                    var liveMatchEntity = await repository.GetById<LiveMatchEntity>(match.Id, language);

                    if (liveMatchEntity != null)
                    {
                        repository.Delete<LiveMatchEntity>(match.Id, language);

                        if (liveMatchIds.Any(x => x.Equals(match.Id)))
                        {
                            liveMatchIds.Add(match.Id);
                        }                        
                    }
                }
            }

            await repository.Save();

            //TODO update cache for schedule and results --> liveMatchIds
        }
    }
}
