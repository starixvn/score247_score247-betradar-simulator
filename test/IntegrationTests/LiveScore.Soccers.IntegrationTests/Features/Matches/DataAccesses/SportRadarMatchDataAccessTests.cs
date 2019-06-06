namespace LiveScore.Soccers.IntegrationTests.Features.Matches.DataAccesses
{
    using LiveScore.Shared;
    using LiveScore.Shared.Configurations;
    using LiveScore.Shared.Database;
    using LiveScore.Shared.Helpers;
    using LiveScore.Shared.SportRadarApi.Models;
    using LiveScore.Soccers.Features.Matches.DataAccesses;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Xunit;

    public class SportRadarMatchDataAccessTests
    {
        private readonly SportRadarMatchDataAccess dataAccess;

        public SportRadarMatchDataAccessTests()
        {
            var restClient = new RestClient(new RestSharp.RestClient
            {
                BaseUrl = new Uri("https://api.sportradar.us/")
            });

            //dataAccess = new SportRadarMatchDataAccess(new StubAppSettings(), restClient);
        }

        [Fact]
        public async Task GetTimelines_ShouldNotEmpty()
        {
            //var timelines = await dataAccess.GetTimelines(SportConstants.SoccerId, "eu", "sr:match:18297893", "en");

            //Assert.NotEmpty(timelines);
        }
    }

    public class StubAppSettings : IAppSettings
    {
        public IDataProviderSettings DataProviderSettings => new StubDataProviderSettings();

        public IDictionary<int, string> TimeZoneSettings => new Dictionary<int, string>();
    }

    public class StubDataProviderSettings : IDataProviderSettings
    {
        public string ApiUrl => "https://api.sportradar.us/";

        public IEnumerable<SportSetting> Sports => new List<SportSetting> {
                new SportSetting
                {
                    Id = 1,
                    Name ="Soccer",
                    EndPoints = new List<EndPoint>
                    {                        
                        new EndPoint {
                            Name = "Timeline",
                            EndPointSettings = new List<EndPointSetting>
                            {
                                new EndPointSetting { Path = "soccer-xt3/{0}/{1}/matches/{2}/timeline.json?api_key={3}" }
                            } }
                    },
                    Regions = new List<Region>
                    {
                        new Region{ Name="eu", Key="s37g6cgqfabegn5mu8snw293" }
                    }
                }
        };

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
}
