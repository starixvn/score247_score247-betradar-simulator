namespace LiveScore.WebApi.Shared.Middlewares
{
    using Hangfire;
    using LiveScore.Jobs;
    using LiveScore.Shared;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.Configuration;
    using LiveScore.Shared.Configurations;
    using Microsoft.Extensions.DependencyInjection;
    using LiveScore.Soccers.Jobs;

    public static class HangfireJobManager
    {
        private const string ResultDateSpanPath = "LiveScores:Jobs:FetchResultDateSpan";
        private const string ScheduleDateSpanPath = "LiveScores:Jobs:FetchScheduleDateSpan";
        private const string ScheduleIntervalCron = " 0 0/6 * * *";

        public static void StartHangfireJobs(this IApplicationBuilder app, IConfiguration Configuration)
        {
            var appSettings = app.ApplicationServices.GetService<IAppSettings>();

            RecurringJob.AddOrUpdate<IAutoUpdateMatchTimeJob>(nameof(IAutoUpdateMatchTimeJob), job => job.UpdateMatchTime(), Cron.Minutely);

            if (!appSettings.EnabledJobs)
            {
                return;
            }

            var resultDateSpan = int.Parse(Configuration.GetSection(ResultDateSpanPath).Value);
            var scheduleDateSpan = int.Parse(Configuration.GetSection(ScheduleDateSpanPath).Value);

            RecurringJob.AddOrUpdate<IDailyScheduleFetchJob>("FetchSchedule", job => job.FetchSchedule(SportConstants.SoccerId, scheduleDateSpan), ScheduleIntervalCron);
            RecurringJob.AddOrUpdate<IDailyScheduleFetchJob>("FetchResult", job => job.FetchResult(SportConstants.SoccerId, resultDateSpan), Cron.Daily);
            RecurringJob.AddOrUpdate<ILiveMatchFetchJob>("FetchLive", job => job.Fetch(SportConstants.SoccerId), Cron.Daily);

            RecurringJob.AddOrUpdate<IMatchPushEventListenerJob>("ListenPushFeed", job => job.StartListeningPushEvent(SportConstants.SoccerId), Cron.Daily);

        }
    }
}
