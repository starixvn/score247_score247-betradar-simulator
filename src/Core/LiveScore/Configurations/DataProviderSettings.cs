namespace LiveScore.Soccer.Configurations
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public interface IDataProviderSettings
    {
#pragma warning disable S3996 // URI properties should not be strings
        string ApiUrl { get; }
#pragma warning restore S3996 // URI properties should not be strings

        IEnumerable<SportSetting> Sports { get; }

        SportSetting GetSportSetting(int Id);
    }

    public class DataProviderSettings : IDataProviderSettings
    {
#pragma warning disable S3996 // URI properties should not be strings
        public string ApiUrl { get; set; }
#pragma warning restore S3996 // URI properties should not be strings

        public IEnumerable<SportSetting> Sports { get; set; }

        public SportSetting GetSportSetting(int Id)
        {
            var sportSetting = Sports.FirstOrDefault(sport => sport.Id == Id);

            if (sportSetting == null)
            {
                throw new InvalidOperationException($"Missing Setting for sportId {Id}");
            }

            return sportSetting;
        }
    }

    public class SportSetting
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public IEnumerable<EndPoint> EndPoints { get; set; }

        public IEnumerable<Region> Regions { get; set; }

        public int MaxDaysStoredData { get; set; }

        public IEnumerable<EndPointSetting> GetEndPointSetting(string key)
        {
            var endpoint = EndPoints.FirstOrDefault(x => x.Name.Equals(key));

            if (endpoint == null)
            {
                throw new InvalidOperationException($"Missing endpoint settings for {key}");
            }

            return endpoint.EndPointSettings;
        }
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

        public string PushKey { get; set; }
    }
}