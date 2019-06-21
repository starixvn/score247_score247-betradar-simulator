namespace LiveScore.WebApi.Shared.Configurations
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using LiveScore.Shared.Configurations;
    using Microsoft.Extensions.Configuration;

    public class AppSettings : IAppSettings
    {
        private readonly IConfiguration configuration;

        public AppSettings(            
            IConfiguration configuration,
            IDataProviderSettings dataProviderSettings,
            IDictionary<int, string> timeZoneSettings,
            string appPath)
        {           
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));                     
            DataProviderSettings = dataProviderSettings;
            TimeZoneSettings = timeZoneSettings;
            EnabledJobs = GetValue<bool>($"LiveScores:{nameof(EnabledJobs)}");
            EnabledListenPush = GetValue<bool>($"LiveScores:{nameof(EnabledListenPush)}");
            EnabledDatabaseMigration = GetValue<bool>($"LiveScores:{nameof(EnabledDatabaseMigration)}");
            AppPath = appPath;
        }

        public IDataProviderSettings DataProviderSettings { get; }

        public IDictionary<int, string> TimeZoneSettings { get; }

        public bool EnabledJobs { get; }

        public bool EnabledListenPush { get; }

        public bool EnabledDatabaseMigration { get; }

        public string AppPath { get; }

        public T GetValue<T>(string key)
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