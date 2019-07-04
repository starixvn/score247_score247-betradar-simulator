namespace LiveScore.Soccer.Configurations
{
    using System.Collections.Generic;

    public interface IAppSettings
    {
        IDataProviderSettings DataProviderSettings { get; }

        IDictionary<int, string> TimeZoneSettings { get; }

        bool EnabledJobs { get; }

        bool EnabledListenPush { get; }

        bool EnabledDatabaseMigration { get; }

        string AppPath { get; }

        bool EnabledListenLogging { get; }

        int NumberOfTopMatches { get; }
    }
}