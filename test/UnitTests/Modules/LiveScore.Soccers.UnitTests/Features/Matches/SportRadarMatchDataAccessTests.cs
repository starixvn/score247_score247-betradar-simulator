namespace LiveScore.Soccers.UnitTests.Features.Matches
{
    using LiveScore.Shared.Configurations;
    using LiveScore.Shared.Helpers;
    using LiveScore.Shared.Models.Dtos;
    using LiveScore.Shared.SportRadarApi.Models;
    using LiveScore.Soccers.Features.Matches.DataAccesses;
    using NSubstitute;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Xunit;

    public class SportRadarMatchDataAccessTests
    {
        private readonly SportRadarMatchDataAccess matchDataAccess;
        private readonly IAppSettings appSettings;
        private readonly IRestClient restClient;

        public SportRadarMatchDataAccessTests()
        {
            appSettings = Substitute.For<IAppSettings>();
            restClient = Substitute.For<IRestClient>();

            matchDataAccess = new SportRadarMatchDataAccess(appSettings, restClient);
        }

        [Fact]
        public async Task GetMatches_EmptySportSettings_ThrowsException()
        {

            await Assert.ThrowsAsync<InvalidOperationException>(() => matchDataAccess.GetMatches(
                1,
                DateTime.Now,
                DateTime.Now.AddDays(1),
                "en",
                TimeSpan.Zero
                ));
        }

        [Fact]
        public async Task GetMatches_EmptyRegionSettings_ReturnEmpty()
        {
            appSettings.DataProviderSettings.Returns(new StubDataProviderSettings());

            var matches = await matchDataAccess.GetMatches(
                2,
                DateTime.Now,
                DateTime.Now.AddDays(1),
                "en",
                TimeSpan.Zero
                );

            Assert.Empty(matches);
        }

        [Fact]
        public async Task GetMatches_EmptyDailySchedule_ReturnEmpty()
        {
            appSettings.DataProviderSettings.Returns(new StubDataProviderSettings());
            restClient.ExecuteGetAsync<DailyScheduleDto>(Arg.Any<string>()).Returns(new DailyScheduleDto());

            var matches = await matchDataAccess.GetMatches(
                1,
                DateTime.Now,
                DateTime.Now.AddDays(1),
                "en",
                TimeSpan.Zero
                );

            Assert.Empty(matches);
        }
    }

    public class StubDataProviderSettings : IDataProviderSettings
    {
        public string ApiUrl => "api.com";

        public IEnumerable<SportSetting> Sports => new List<SportSetting> {
                new SportSetting
                {
                    Id = 1,
                    Name ="Soccer",
                    EndPoints = new List<EndPoint>
                    {
                        new EndPoint { Name = "MatchSchedule", EndPointSettings = new List<EndPointSetting>{ } }
                    },
                    Regions = new List<Region>
                    {
                        new Region{ Name="eu", Key="eu-key" }
                    }
                },
                new SportSetting
                {
                    Id = 2,
                    Name ="Basketball",
                    EndPoints = new List<EndPoint>
                    {
                        new EndPoint { Name = "MatchSchedule", EndPointSettings = new List<EndPointSetting>{ } }
                    },
                    Regions = new List<Region>{ }
                }
        };
    }
}
