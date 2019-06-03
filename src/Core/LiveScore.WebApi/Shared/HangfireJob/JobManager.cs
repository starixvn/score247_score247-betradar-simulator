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
        private const string JobDateSpanSettingsPath = "Jobs:DefaultDateSpan";

        public static void Register(IConfiguration Configuration)
        {
            var dateSpan = Int32.Parse(Configuration.GetSection(JobDateSpanSettingsPath).Value);

            RecurringJob.AddOrUpdate<IAutoUpdateMatchTimeJob>(nameof(AutoUpdateMatchTimeJob), job => job.UpdateMatchTime(), Cron.Minutely);
            RecurringJob.AddOrUpdate<IDailyScheduleFetchJob>("FetchSchedule", job => job.FetchSchedule(SportConstants.SoccerId, dateSpan), Cron.Daily);
            RecurringJob.AddOrUpdate<IDailyScheduleFetchJob>("FetchResult", job => job.FetchResult(SportConstants.SoccerId, dateSpan), Cron.Daily);
            BackgroundJob.Enqueue<IMatchPushEventListenerJob>(job => job.StartListeningPushEvent(SportConstants.SoccerId));
        }
    }
}
