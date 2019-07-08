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

        bool EnabledOddsJob { get; }

        string AppPath { get; }

        bool EnabledListenLogging { get; }

        int NumberOfTopMatches { get; }

        Jobs Jobs { get; } 
    }

    public class Jobs
    {
        public int FetchResultDateSpan { get; set; }

        public int FetchScheduleDateSpan { get; set; }
    }
}