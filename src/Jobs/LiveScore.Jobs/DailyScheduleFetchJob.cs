namespace LiveScore.Jobs
{
    using System;
    using System.Threading.Tasks;
    using Hangfire;
    using Hangfire.Common;
    using LiveScore.Shared;
    using LiveScore.Shared.Database;
    using LiveScore.Shared.Database.Entities;
    using LiveScore.Shared.Models;
    using Newtonsoft.Json;

    public interface IDailyScheduleFetchJob
    {
        void Start();

        Task FetchDailySchedule(int sportId, DateTime from, DateTime to, string language);
    }

    public class DailyScheduleFetchJob : IDailyScheduleFetchJob
    {
        private readonly IDynamicRepository repository;
        private readonly IRecurringJobManager recurringJobManager;
        private readonly InstanceFactory instanceFactory;

        public DailyScheduleFetchJob(
            IRecurringJobManager recurringJobManager,
            IDynamicRepository repository,
            InstanceFactory instanceFactory)
        {
            this.instanceFactory = instanceFactory;
            this.repository = repository;
            this.recurringJobManager = recurringJobManager;
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

        public void Start()
        {   
            var from = DateTime.Now;
            var to = DateTime.Now;

            foreach (var lang in Enumeration.GetAll<Language>())
            {
                recurringJobManager.AddOrUpdate(
                nameof(FetchDailySchedule),
                Job.FromExpression(() => FetchDailySchedule(SportConstants.SoccerId, from, to, lang.Value)),
                Cron.Daily());

                //TODO foreach sport type
            }
        }
    }
}
