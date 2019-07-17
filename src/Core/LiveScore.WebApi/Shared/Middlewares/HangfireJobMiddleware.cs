namespace LiveScore.WebApi.Shared.Middlewares
{
    using System;
    using System.Linq;
    using Hangfire;
    using Hangfire.Dashboard;
    using Hangfire.PostgreSql;
    using LiveScore.Soccer;
    using LiveScore.Soccer.Configurations;
    using LiveScore.Soccer.Jobs;
    using LiveScore.Soccer.Odds.Jobs;
    using LiveScore.Soccers.Jobs;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    public static class HangfireJobManager
    {
        private const string ScheduleIntervalCron = " 0 0/6 * * *";
        private const int OneYearDays = 365;
        private const int NumOfWorkers = 2;

        public static void AddHangfireJobs(this IServiceCollection services, IConfiguration configuration)
        {
            
            services.AddHangfire(x => x.UsePostgreSqlStorage(configuration.GetConnectionString("hangfireConnection"),
                   new PostgreSqlStorageOptions
                   {
                       //change this
                       InvisibilityTimeout = TimeSpan.FromDays(OneYearDays)
                   }));

            services.AddScoped<IAutoUpdateMatchTimeJob, AutoUpdateMatchTimeJob>();
            services.AddScoped<IDailyScheduleFetchJob, DailyScheduleFetchJob>();
            services.AddScoped<ILiveMatchFetchJob, LiveMatchFetchJob>();
            services.AddScoped<ISportRadarOddsJob, SportRadarOddsJob>();
            services.AddScoped<MatchPushEventListenerJob, MatchPushEventListenerJob>();
        }

        public static void UseHangfire(this IApplicationBuilder app)
        {
            
            app.UseHangfireDashboard(options: new DashboardOptions
            {
                Authorization = Enumerable.Empty<IDashboardAuthorizationFilter>()
            }).UseHangfireServer(options: new BackgroundJobServerOptions
            {
                WorkerCount = NumOfWorkers
            }).StartHangfireJobs();
        }

        private static void StartHangfireJobs(this IApplicationBuilder app)
        {
            var appSettings = app.ApplicationServices.GetService<IAppSettings>();

            RecurringJob.AddOrUpdate<IAutoUpdateMatchTimeJob>(nameof(IAutoUpdateMatchTimeJob), job => job.UpdateMatchTime(), Cron.Minutely);

            if (appSettings.EnabledListenPush)
            {
                BackgroundJob.Enqueue<MatchPushEventListenerJob>(job => job.StartListeningPushEvent(SportConstants.SoccerId));
            }

            if (appSettings.EnabledOddsJob)
            {
                RecurringJob.AddOrUpdate<ISportRadarOddsJob>("FetchOdds", job => job.FetchDaily(), ScheduleIntervalCron);
                RecurringJob.AddOrUpdate<ISportRadarOddsJob>("FetchOdds", job => job.FetchDaily(), ScheduleIntervalCron);
            }

            if (!appSettings.EnabledJobs)
            {
                return;
            }

            var resultDateSpan = appSettings.Jobs.FetchResultDateSpan;
            var scheduleDateSpan = appSettings.Jobs.FetchScheduleDateSpan;

            RecurringJob.AddOrUpdate<IDailyScheduleFetchJob>("FetchSchedule", job => job.FetchSchedule(SportConstants.SoccerId, scheduleDateSpan), ScheduleIntervalCron);
            RecurringJob.AddOrUpdate<IDailyScheduleFetchJob>("FetchResult", job => job.FetchResult(SportConstants.SoccerId, resultDateSpan), Cron.Daily);
            RecurringJob.AddOrUpdate<ILiveMatchFetchJob>("FetchLive", job => job.Fetch(SportConstants.SoccerId), Cron.Daily);
        }
    }
}