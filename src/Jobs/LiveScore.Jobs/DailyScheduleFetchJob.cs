namespace LiveScore.Jobs
{
    using System;
    using System.Threading.Tasks;
    using LiveScore.Shared;
    using LiveScore.Shared.Database;
    using LiveScore.Shared.Database.Entities;
    using LiveScore.Shared.Models;
    using Newtonsoft.Json;

    public interface IDailyScheduleFetchJob
    {
        Task FetchDailySchedule(int sportId);
    }

    public class DailyScheduleFetchJob : IDailyScheduleFetchJob
    {
        private readonly IDynamicRepository repository;
        private readonly InstanceFactory instanceFactory;

        public DailyScheduleFetchJob(
            IDynamicRepository repository,
            InstanceFactory instanceFactory)
        {
            this.instanceFactory = instanceFactory;
            this.repository = repository;
        }

        public async Task FetchDailySchedule(int sportId)
        {
            var from = DateTime.Now;
            var to = DateTime.Now.AddDays(1);

            foreach (var lang in Enumeration.GetAll<Language>())
            {
                await FetchDailySchedule(sportId, from, to, lang.Value);
            }
        }

        public async Task FetchDailySchedule(int sportId, DateTime from, DateTime to, string language)
        {
            var dataAccess = instanceFactory.CreateMatchDataAccess(sportId, DataProvider.ThirdPartyApi);

            var matches = await dataAccess.GetMatches(sportId, from, to, Enumeration.FromValue<Language>(language).DisplayName.ToLowerInvariant(), TimeSpan.Zero);

            foreach (var match in matches)
            {
                if (await repository.GetById<MatchEntity>(match.Id) == null)
                {
                    var matchEntity = new MatchEntity
                    {
                        Id = match.Id,
                        EventDate = match.EventDate.ToUniversalTime(),
                        LeagueId = match.League.Id,
                        Language = language,
                        SportId = sportId,
                        Value = JsonConvert.SerializeObject(match)
                    };

                    await repository.Insert(matchEntity);
                }
            }

            await repository.Save();
        }


    }
}
