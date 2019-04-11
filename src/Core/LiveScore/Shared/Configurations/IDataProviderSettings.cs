namespace LiveScore.Shared.Configurations
{
    using System.Collections.Generic;
    using LiveScore.Shared.SportRadarApi.Models;

    public interface IDataProviderSettings
    {
#pragma warning disable S3996 // URI properties should not be strings
        string ApiUrl { get; }
#pragma warning restore S3996 // URI properties should not be strings

        IEnumerable<SportSetting> Sports { get; }
    }
}