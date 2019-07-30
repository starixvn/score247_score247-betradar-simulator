using System.Linq;
using Fanex.Logging;
using Fanex.Logging.Sentry;
using LiveScore.BetRadarSimulator.Api.Features.Matches;
using LiveScore.BetRadarSimulator.Api.Midlewares;
using LiveScore.BetRadarSimulator.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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

            services.AddResponseCompression(options =>
            {
                options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[] { "text/event-stream" });
            });

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info { Title = "Sport Radar Simulator", Version = "v1" });
            });

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
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

            app.UseSwagger()
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
