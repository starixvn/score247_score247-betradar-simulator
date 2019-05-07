namespace LiveScore.WebApi.Shared.Configurations
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using LiveScore.Shared.Configurations;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;

    public class AppSettings : IAppSettings
    {
        private readonly IConfiguration configuration;
        private readonly IHostingEnvironment hostingEnvironment;

        public AppSettings(
            IHostingEnvironment hostingEnvironment, 
            IConfiguration configuration,
            IDataProviderSettings dataProviderSettings,
            IDictionary<int, string> timeZoneSettings)
        {
            this.hostingEnvironment = hostingEnvironment ?? throw new ArgumentNullException(nameof(hostingEnvironment));
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            EnabledStaticData = GetValue<bool>("LiveScores:EnabledStaticData");
            AutoGenerateStatus = GetValue<bool>("LiveScores:AutoGenerateStatus");
            DataProviderSettings = dataProviderSettings;
            TimeZoneSettings = timeZoneSettings;
        }

        public string AppPath => hostingEnvironment.ContentRootPath;

        public bool EnabledStaticData { get; }

        public IDataProviderSettings DataProviderSettings { get; }

        public IDictionary<int, string> TimeZoneSettings { get; }

        public bool AutoGenerateStatus { get; }

        private T GetValue<T>(string key)
        {
            try
            {
                var value = configuration[key];

                TypeConverter typeConverter = TypeDescriptor.GetConverter(typeof(T));

                return (T)typeConverter.ConvertFromString(value);
            }
            catch (Exception ex)
            {
                throw new InvalidCastException($"Key: {key}", ex);
            }
        }
    }
}