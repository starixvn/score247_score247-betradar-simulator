namespace LiveScore.Shared.Configurations
{
    using System.Collections.Generic;

    public interface IAppSettings
    {
        string AppPath { get; }

        bool EnabledStaticData { get; }

        bool AutoGenerateStatus { get; }

        IDataProviderSettings DataProviderSettings { get; }

        IDictionary<int, string> TimeZoneSettings { get; }
    }
}