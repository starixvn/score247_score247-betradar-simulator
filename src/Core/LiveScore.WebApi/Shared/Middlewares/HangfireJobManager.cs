using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.SignalR;
using LiveScore.Shared.Hubs;
using LiveScore.Soccers.Features.Matches;
using LiveScore.Shared.Configurations;
using Hangfire.Common;
namespace LiveScore.WebApi.Shared.Middlewares
{
    using Hangfire;
    using LiveScore.Jobs;
    using LiveScore.Shared;
    using LiveScore.Soccers.Jobs;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.Configuration;

    public static class HangfireJobManager
    {
        private const string ResultDateSpanPath = "LiveScores:Jobs:FetchResultDateSpan";
        private const string ScheduleDateSpanPath = "LiveScores:Jobs:FetchScheduleDateSpan";
        private const string ScheduleIntervalCron = " 0 0/6 * * *";

        public static void StartHangfireJobs(this IApplicationBuilder app, IConfiguration Configuration)
        {
            var resultDateSpan = int.Parse(Configuration.GetSection(ResultDateSpanPath).Value);
            var scheduleDateSpan = int.Parse(Configuration.GetSection(ScheduleDateSpanPath).Value);

            //RecurringJob.AddOrUpdate<IAutoUpdateMatchTimeJob>(nameof(AutoUpdateMatchTimeJob), job => job.UpdateMatchTime(), Cron.Minutely);

            RecurringJob.AddOrUpdate<IDailyScheduleFetchJob>("FetchSchedule", job => job.FetchSchedule(SportConstants.SoccerId, scheduleDateSpan), ScheduleIntervalCron);
            RecurringJob.AddOrUpdate<IDailyScheduleFetchJob>("FetchResult", job => job.FetchResult(SportConstants.SoccerId, resultDateSpan), Cron.Daily);
            RecurringJob.AddOrUpdate<ILiveMatchFetchJob>("FetchLive", job => job.Fetch(SportConstants.SoccerId), Cron.Daily);

            var pushHandler = app.ApplicationServices.GetService<IMatchPushEventHandler>();
            var appSettings = app.ApplicationServices.GetService<IAppSettings>();
            var soccerMatchPushEventListener = new MatchPushEventListener(pushHandler, appSettings);

            BackgroundJob.Enqueue<IMatchPushEventListenerJob>(job => job.StartListeningPushEvent(SportConstants.SoccerId, soccerMatchPushEventListener));
        }
    }
}
