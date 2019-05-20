namespace LiveScore.Jobs
{
    using Hangfire;
    using Hangfire.Common;
    using LiveScore.Entity;
    using LiveScore.Entity.Entities;
    using LiveScore.Shared;
    using LiveScore.Shared.SportRadarApi.Models;
    using System;
    using System.Threading.Tasks;

    public interface DailyScheduleFetchJob
    {
        void Start();

        Task FetchDailySchedule(int sportId, Region region, DateTime date, string language);
    }

    public class DailyScheduleFetchJobImpl : DailyScheduleFetchJob
    {        
        private readonly GenericRepository<MatchEntity> repository;
        private readonly IRecurringJobManager recurringJobManager;
        private readonly JobInstanceFactory instanceFactory;

        public DailyScheduleFetchJobImpl(
            IRecurringJobManager recurringJobManager,         
            GenericRepository<MatchEntity> repository,
            JobInstanceFactory instanceFactory)
        {
            this.instanceFactory = instanceFactory;
            this.repository = repository;
            this.recurringJobManager = recurringJobManager;
        }

        public async Task FetchDailySchedule(int sportId, Region region, DateTime date, string language)
        {
            var dataAccess = instanceFactory.CreateMatchJobDataAccess(sportId);

            var matches = await dataAccess.GetDailySchedule(sportId, region, date, language);

            foreach (var match in matches)
            {
                if (await repository.GetById(match.Id) == null)
                {
                    await repository.Insert(match);
                }
            }

            await repository.Save();
        }

        public void Start()
        {
            //TODO apply multiple languages and regions
            var region = new Region() { Name = "eu", Key = "s37g6cgqfabegn5mu8snw293" };
            var language = "en";
            var date = DateTime.Now;

            recurringJobManager.AddOrUpdate(
                nameof(FetchDailySchedule),
                Job.FromExpression(() => FetchDailySchedule(SportConstants.SoccerId, region, date, language)),
                Cron.Daily());
        }
    }
}
