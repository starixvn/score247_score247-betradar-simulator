namespace LiveScore.WebApi.Shared.Configurations
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using LiveScore.Soccer.Shared.Configurations;
    using Microsoft.Extensions.Configuration;

    public class AppSettings : IAppSettings
    {
        private readonly IConfiguration configuration;
        private const string LiveScore = "LiveScores:";

        public AppSettings(
            IConfiguration configuration,
            DataProviderSettings dataProviderSettings,
            IDictionary<int, string> timeZoneSettings,
            string appPath)
        {
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            DataProviderSettings = dataProviderSettings;
            TimeZoneSettings = timeZoneSettings;
            EnabledJobs = GetValue<bool>($"{LiveScore}{nameof(EnabledJobs)}");
            EnabledListenPush = GetValue<bool>($"{LiveScore}{nameof(EnabledListenPush)}");
            EnabledDatabaseMigration = GetValue<bool>($"{LiveScore}{nameof(EnabledDatabaseMigration)}");
            EnabledListenLogging = GetValue<bool>($"{LiveScore}{nameof(EnabledListenLogging)}");
            NumberOfTopMatches = GetValue<int>($"{LiveScore}{nameof(NumberOfTopMatches)}");
            if (NumberOfTopMatches <= 0)
            {
                NumberOfTopMatches = int.MaxValue;
            }

            AppPath = appPath;
        }

        public IDataProviderSettings DataProviderSettings { get; }

        public IDictionary<int, string> TimeZoneSettings { get; }

        public bool EnabledJobs { get; }

        public bool EnabledListenPush { get; }

        public bool EnabledListenLogging { get; }

        public bool EnabledDatabaseMigration { get; }

        public string AppPath { get; }

        public int NumberOfTopMatches { get; }

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