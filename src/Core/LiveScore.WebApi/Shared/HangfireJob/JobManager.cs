namespace LiveScore.WebApi.Shared.HangfireJob
{
    using Hangfire;
    using LiveScore.Jobs;
    using LiveScore.Shared;
    using LiveScore.Soccers.Jobs;
    using Microsoft.Extensions.Configuration;
    using System;

    public static class JobManager
    {
        private const string ResultDateSpanPath = "LiveScores:Jobs:FetchResultDateSpan";
        private const string ScheduleDateSpanPath = "LiveScores:Jobs:FetchScheduleDateSpan";

        private const string ScheduleIntervalCron = " 0 0/6 * * *";

        public static void Register(IConfiguration Configuration)
        {
            var resultDateSpan = Int32.Parse(Configuration.GetSection(ResultDateSpanPath).Value);
            var scheduleDateSpan = Int32.Parse(Configuration.GetSection(ScheduleDateSpanPath).Value);

            RecurringJob.AddOrUpdate<IAutoUpdateMatchTimeJob>(nameof(AutoUpdateMatchTimeJob), job => job.UpdateMatchTime(), Cron.Minutely);
            RecurringJob.AddOrUpdate<IDailyScheduleFetchJob>("FetchSchedule", job => job.FetchSchedule(SportConstants.SoccerId, scheduleDateSpan), ScheduleIntervalCron);
            RecurringJob.AddOrUpdate<IDailyScheduleFetchJob>("FetchResult", job => job.FetchResult(SportConstants.SoccerId, resultDateSpan), Cron.Daily);
            BackgroundJob.Enqueue<IMatchPushEventListenerJob>(job => job.StartListeningPushEvent(SportConstants.SoccerId));
        }
    }
}
