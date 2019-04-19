namespace LiveScore.WebApi
{
    using System;
    using System.Collections.Generic;
    using Fanex.Logging;
    using Fanex.Logging.Sentry;
    using LiveScore.Features.Matches;
    using LiveScore.Shared;
    using LiveScore.Shared.Configurations;
    using LiveScore.Shared.Helpers;
    using LiveScore.Shared.SportRadarApi.Models;
    using LiveScore.Soccers.Features.Matches;
    using LiveScore.Soccers.Features.Matches.DataProviders;
    using LiveScore.WebApi.Shared.Configurations;
    using LiveScore.WebApi.Shared.Middlewares;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Newtonsoft.Json;
    using Sentry;
    using Swashbuckle.AspNetCore.Swagger;

    public class Startup
    {
        private const string SportRadarSettingPath = "LiveScores:SportRadarSetting";
        private const string TimeZoneSettingsPath = "LiveScores:TimeZoneSettings";
        private readonly IHostingEnvironment hostingEnvironment;

        public Startup(IConfiguration configuration, IHostingEnvironment hostingEnvironment)
        {
            Configuration = configuration;
            this.hostingEnvironment = hostingEnvironment;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            InitLogging(services);

            services.AddOptions();

            try
            {
                var sportRadarDataProviderSettings = new SportRadarDataProviderSettings();
                Configuration.GetSection(SportRadarSettingPath).Bind(sportRadarDataProviderSettings);

                var timeZoneSettings = new Dictionary<int, string>();
                Configuration.GetSection(TimeZoneSettingsPath).Bind(timeZoneSettings);

                var appSettings = new AppSettings(hostingEnvironment, Configuration, sportRadarDataProviderSettings, timeZoneSettings);
                services.AddSingleton<IAppSettings>(appSettings);
                services.AddSingleton<MatchService, MatchServiceImpl>();
                services.AddSingleton(CreateRestSharpClient(sportRadarDataProviderSettings));
                services.AddSingleton((InstanceFactory)new InstanceFactoryImpl(
                    new Dictionary<int, MatchDataAccess>
                    {
                        { SportConstants.SoccerId, new MatchDataAccessImpl(GetMatchApiInstance(appSettings)) },
                        { SportConstants.BasketballId, new LiveScore.Basketballs.Features.Matches.MatchDataAccessImpl() }
                    }
                    ));

                //
                services.AddSingleton(GetMatchApiInstance(appSettings));

                services.AddSwaggerGen(c =>
                {
                    c.SwaggerDoc("v1", new Info { Title = "Live Scores API", Version = "v1" });
                });
                services
                    .AddMvc()
                    .AddJsonOptions(options =>
                    {
                        options.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Unspecified;
                    })
                    .SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
            }
            catch (Exception ex)
            {
                LogService.Error(ex.ToString());
            }

            LogService.Info("LiveScore API has been started.");
        }

        private static IRestClient CreateRestSharpClient(IDataProviderSettings providerSettings)
            => new RestClient(new RestSharp.RestClient
            {
                BaseUrl = new Uri(providerSettings.ApiUrl)
            });

        private void InitLogging(IServiceCollection services)
        {
            LogManager
                    .SetDefaultLogCategory(Configuration["Fanex.Logging:DefaultCategory"])
                    .Use(new SentryLogging(new SentryEngineOptions
                    {
                        Dsn = new Dsn(Configuration["Fanex.Logging:SentryUrl"])
                    }));

            services.AddSingleton(Logger.Log);
        }

        private static IMatchApi GetMatchApiInstance(AppSettings appSettings)
            => appSettings.EnabledStaticData
                                    ? (IMatchApi)new StaticMatchApi(appSettings)
                                    : (IMatchApi)new SportRadarMatchApi(appSettings, CreateRestSharpClient(appSettings.DataProviderSettings));

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
#pragma warning disable S2325 // Methods and properties that don't access instance data should be static

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
#pragma warning restore S2325 // Methods and properties that don't access instance data should be static
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app
                .UseSwagger()
                .UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint($"../swagger/v1/swagger.json", "Chatbot API Docs");
                })
                .UseMvc(routes =>
                {
                    routes.MapRoute(
                        name: "default",
                        template: "{controller=Home}/{action=Index}");
                })
                .UseMiddleware(typeof(ErrorHandlingMiddleware));
        }
    }
}