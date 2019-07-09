namespace LiveScore.WebApi
{
    using System;
    using Fanex.Caching.Memory;
    using Fanex.Logging;
    using LiveScore.Soccer.Matches.Hubs;
    using LiveScore.WebApi.Shared.Middlewares;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;

    public class Startup
    {
        private readonly IHostingEnvironment hostingEnvironment;

        public Startup(IConfiguration configuration, IHostingEnvironment hostingEnvironment)
        {
            Configuration = configuration;
            this.hostingEnvironment = hostingEnvironment;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            try
            {
                services.AddCors(options =>
                {
                    options.AddPolicy("CorsPolicy",
                        builder => builder.AllowAnyOrigin()
                            .AllowAnyMethod()
                            .AllowAnyHeader()
                            .AllowCredentials());
                });
                services.AddLogging(Configuration);
                services.AddSettings(Configuration, hostingEnvironment);
                services.AddOptions();
                services.AddSignalR();
                services.AddLogging(b => b.AddConsole());
                services.AddMemoryCache(Configuration.GetSection("fanex.caching"));
                services.AddDatabase(Configuration);
                services.AddServices();
                services.AddHangfireJobs(Configuration);
                services.RegisterPushEventListenerFactory();
                services.AddSwagger();
                services.AddMvc()
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

#pragma warning disable S2325 // Methods and properties that don't access instance data should be static
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
#pragma warning restore S2325 // Methods and properties that don't access instance data should be static
        {
            app.UseSignalR(routes =>
            {
                routes.MapHub<MatchHub>("/hubs/MatchHub");
            });
            app.UseDatabase();
            app.UseHangfire();
            app.ConfigureExceptionHandler();
            app.ConfigureSwagger();
        }
    }
}