using System;
using System.Linq;
using Fanex.Data;
using Fanex.Data.MySql;
using Fanex.Logging;
using Fanex.Logging.Sentry;
using LiveScore.BetRadarSimulator.Api.Features.Matches;
using LiveScore.BetRadarSimulator.Api.Features.Teams;
using LiveScore.BetRadarSimulator.Api.Features.Tournaments;
using LiveScore.BetRadarSimulator.Api.Midlewares;
using LiveScore.BetRadarSimulator.Api.Shared;
using LiveScore.BetRadarSimulator.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Sentry;
using Swashbuckle.AspNetCore.Swagger;

namespace LiveScoreSimulator
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            InitLogging(services);
            services.AddSingleton<INotificationService, NotificationService>();
            services.AddSingleton<IMatchService, MatchService>();
            services.AddSingleton<ITeamService, TeamService>();
            services.AddSingleton<IFileService, FileService>();
            services.AddSingleton<ITournamentService, TournamentService>();

            services.AddResponseCompression(options =>
            {
                options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[] { "text/event-stream" });
            });

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Sport Radar Simulator", Version = "v1" });
            });

            services.AddMvc()
                .AddJsonOptions(options =>
                {
                    options.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Utc;
                })
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        }

#pragma warning disable S2325 // Methods and properties that don't access instance data should be static
#pragma warning disable RECS0154 // Parameter is never used

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
#pragma warning restore RECS0154 // Parameter is never used
        {
            app.ConfigureExceptionHandler();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            var connections = Configuration
                 .GetSection("ConnectionStrings")
                 .GetChildren()
                 .ToDictionary(connection => connection.Key,
                               connection => new ConnectionConfiguration(connection.Key, connection.Value));

            DbSettingProviderManager
               .StartNewSession()
               .Use(connections)
               .WithMySql(resourcePath: Configuration["AppDataPath"])
               .Run();

            var basePath = Configuration["HostingVirtualPath"];
            app.UseSwagger(c =>
            {
                c.PreSerializeFilters.Add((swagger, httpReq) =>
                {
                    swagger.Servers.Add(new OpenApiServer { Url = basePath });
                });
            })
                .UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint($"../swagger/v1/swagger.json", "Sport Radar Simulator Docs");
            }).UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}");
            });
        }

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
    }
}