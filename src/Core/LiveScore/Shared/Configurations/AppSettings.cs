namespace LiveScore.Shared.Configurations
{
    using System.Collections.Generic;

    public interface IAppSettings
    {
        IDataProviderSettings DataProviderSettings { get; }

        IDictionary<int, string> TimeZoneSettings { get; }

        bool EnabledJobs { get; }
    }
}