namespace LiveScore.Jobs
{
    using Hangfire;
    using Hangfire.Common;
    using LiveScore.Entity;
    using LiveScore.Entity.Entities;
    using LiveScore.Shared;
    using Newtonsoft.Json;
    using System;
    using System.Threading.Tasks;

    public interface IDailyScheduleFetchJob
    {
        void Start();

        Task FetchDailySchedule(int sportId, DateTime from, DateTime to, string language);
    }

    public class DailyScheduleFetchJob : IDailyScheduleFetchJob
    {
        private readonly IGenericRepository<MatchEntity> repository;
        private readonly IRecurringJobManager recurringJobManager;
        private readonly InstanceFactory instanceFactory;

        public DailyScheduleFetchJob(
            IRecurringJobManager recurringJobManager,
            IGenericRepository<MatchEntity> repository,
            InstanceFactory instanceFactory)
        {
            this.instanceFactory = instanceFactory;
            this.repository = repository;
            this.recurringJobManager = recurringJobManager;
        }

        public async Task FetchDailySchedule(int sportId, DateTime from, DateTime to, string language)
        {
            var dataAccess = instanceFactory.CreateMatchDataAccess(sportId, DataProvider.ThirdPartyApi);

            var matches = await dataAccess.GetMatches(sportId, from, to, language);

            foreach (var match in matches)
            {
                if (await repository.GetById(match.Id) == null)
                {
                    var matchEntity = new MatchEntity
                    {
                        Id = match.Id,
                        EventDate = match.EventDate,
                        SeasonId = string.Empty,
                        Language = language,
                        SportId = sportId,
                        Value = JsonConvert.SerializeObject(match)                        
                    };

                    await repository.Insert(matchEntity);
                }
            }

            await repository.Save();
        }

        public void Start()
        {
            //TODO apply multiple languages
            var language = "en";
            var date = DateTime.Now;

            recurringJobManager.AddOrUpdate(
                nameof(FetchDailySchedule),
                Job.FromExpression(() => FetchDailySchedule(SportConstants.SoccerId, date, date, language)),
                Cron.Daily());
        }
    }
}
