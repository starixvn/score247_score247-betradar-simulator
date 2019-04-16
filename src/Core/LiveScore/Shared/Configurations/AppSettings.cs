namespace LiveScore.Shared.Configurations
{
    using System.Collections.Generic;

    public interface IAppSettings
    {
        string AppPath { get; }

        bool EnabledStaticData { get; }

        IDataProviderSettings DataProviderSettings { get; }

        IDictionary<int, string> TimeZoneSettings { get; }
    }
}