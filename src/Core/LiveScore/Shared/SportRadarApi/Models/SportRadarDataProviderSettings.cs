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

        public IEnumerable<EndPoint> EndPoints { get; set; }

        public IEnumerable<Region> Regions { get; set; }

        public int MaxDaysStoredData { get; set; }
    }

    public class EndPoint
    {
        public string Name { get; set; }

        public IEnumerable<EndPointSetting> EndPointSettings { get; set; }
    }

    public class EndPointSetting
    {
        public int FormatId { get; set; }

        public string Path { get; set; }

        public bool IsPast { get; set; }
    }

    public class Region
    {
        public string Name { get; set; }

        public string Key { get; set; }
    }
}