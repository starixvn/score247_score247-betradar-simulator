namespace LiveScore.WebApi.Shared.Middlewares
{
    using Hangfire;
    using LiveScore.Jobs;
    using LiveScore.Shared;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.Configuration;
    using LiveScore.Soccers.Features.Matches;
    using LiveScore.Shared.Configurations;
    using Microsoft.Extensions.DependencyInjection;

    public static class HangfireJobManager
    {
        private const string ResultDateSpanPath = "LiveScores:Jobs:FetchResultDateSpan";
        private const string ScheduleDateSpanPath = "LiveScores:Jobs:FetchScheduleDateSpan";
        private const string ScheduleIntervalCron = " 0 0/6 * * *";

        public static void StartHangfireJobs(this IApplicationBuilder app, IConfiguration Configuration)
        {
            var appSettings = app.ApplicationServices.GetService<IAppSettings>();

            if(!appSettings.EnabledJobs)
            {
                return;
            }

            var resultDateSpan = int.Parse(Configuration.GetSection(ResultDateSpanPath).Value);
            var scheduleDateSpan = int.Parse(Configuration.GetSection(ScheduleDateSpanPath).Value);
            var isEnableHangfireJob = Configuration.GetValue<bool>("EnableHangfireJob");

            if (isEnableHangfireJob)
            {
                RecurringJob.AddOrUpdate<IDailyScheduleFetchJob>("FetchSchedule", job => job.FetchSchedule(SportConstants.SoccerId, scheduleDateSpan), ScheduleIntervalCron);
                RecurringJob.AddOrUpdate<IDailyScheduleFetchJob>("FetchResult", job => job.FetchResult(SportConstants.SoccerId, resultDateSpan), Cron.Daily);
                RecurringJob.AddOrUpdate<ILiveMatchFetchJob>("FetchLive", job => job.Fetch(SportConstants.SoccerId), Cron.Daily);

                var pushHandler = app.ApplicationServices.GetService<IMatchPushEventHandler>();
                var soccerMatchPushEventListener = new MatchPushEventListener(pushHandler, appSettings);

                BackgroundJob.Enqueue<IMatchPushEventListenerJob>(job => job.StartListeningPushEvent(SportConstants.SoccerId, soccerMatchPushEventListener));
            }
        }
    }
}
