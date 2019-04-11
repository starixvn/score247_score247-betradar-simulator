namespace LiveScore.Shared.SportRadarApi.Models
{
    using System.Collections.Generic;
    using LiveScore.Shared.Configurations;

    public class SportRadarDataProviderSettings : IDataProviderSettings
    {
#pragma warning disable S3996 // URI properties should not be strings
        public string ApiUrl { get; set; }
#pragma warning restore S3996 // URI properties should not be strings

        public IEnumerable<SportSetting> Sports { get; set; }
    }

    public class SportSetting
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public IEnumerable<string> EndPoints { get; set; }

        public int MaxDaysStoredData { get; set; }
    }
}