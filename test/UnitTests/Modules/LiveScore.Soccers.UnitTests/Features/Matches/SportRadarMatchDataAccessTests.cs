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
    using System.Linq;
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

        [Fact]
        public async Task GetMatches_Always_ReturnTeams()
        {
            appSettings.DataProviderSettings.Returns(new StubDataProviderSettings());
            restClient.ExecuteGetAsync<DailyScheduleDto>(Arg.Any<string>()).Returns(StubDailySchedule());

            var matches = await matchDataAccess.GetMatches(
                1,
                DateTime.Now,
                DateTime.Now.AddDays(1),
                "en",
                TimeSpan.Zero);

            var match = matches.First();
            var home = match.Teams.ElementAt(0);
            var away = match.Teams.ElementAt(1);

            Assert.Equal(2, match.Teams.Count());

            Assert.Equal("sr:competitor:142194", home.Id);
            Assert.Equal("Kaya FC-Iloilo", home.Name);
            Assert.Equal("Philippines", home.Country);
            Assert.Equal("PHL", home.CountryCode);
            Assert.Equal("KAY", home.Abbreviation);
            Assert.True(home.IsHome);

            Assert.Equal("sr:competitor:135864", away.Id);
            Assert.Equal("PSM Makassar", away.Name);
            Assert.Equal("Indonesia", away.Country);
            Assert.Equal("IDN", away.CountryCode);
            Assert.Equal("MAK", away.Abbreviation);
            Assert.False(away.IsHome);
        }

        [Fact]
        public async Task GetMatches_Always_ReturnLeagueInformation()
        {
            appSettings.DataProviderSettings.Returns(new StubDataProviderSettings());
            restClient.ExecuteGetAsync<DailyScheduleDto>(Arg.Any<string>()).Returns(StubDailySchedule());

            var matches = await matchDataAccess.GetMatches(
                1,
                DateTime.Now,
                DateTime.Now.AddDays(1),
                "en",
                TimeSpan.Zero);

            var match = matches.First();
            var league = match.League;

            Assert.Equal("sr:tournament:668", league.Id);
            Assert.Equal("AFC Cup", league.Name);
            Assert.Equal("sr:category:393", league.Category.Id);
            Assert.Equal("International Clubs", league.Category.Name);
        }

        [Fact]
        public async Task GetMatches_Always_ReturnLeagueRound()
        {
            appSettings.DataProviderSettings.Returns(new StubDataProviderSettings());
            restClient.ExecuteGetAsync<DailyScheduleDto>(Arg.Any<string>()).Returns(StubDailySchedule());

            var matches = await matchDataAccess.GetMatches(
                1,
                DateTime.Now,
                DateTime.Now.AddDays(1),
                "en",
                TimeSpan.Zero);

            var match = matches.First();
            var leagueRound = match.LeagueRound;

            Assert.Equal("RoundLeague", leagueRound.Name);
            Assert.Equal("H", leagueRound.Group);
            Assert.Equal(4, leagueRound.Number);
            Assert.Equal("phase", leagueRound.Phase);
            Assert.Equal("group", leagueRound.Type.Value);
        }

        [Fact]
        public async Task GetMatches_Always_ReturnMatchResult()
        {
            appSettings.DataProviderSettings.Returns(new StubDataProviderSettings());
            restClient.ExecuteGetAsync<DailyScheduleDto>(Arg.Any<string>()).Returns(StubDailySchedule());

            var matches = await matchDataAccess.GetMatches(
                1,
                DateTime.Now,
                DateTime.Now.AddDays(1),
                "en",
                TimeSpan.Zero);

            var match = matches.First();
            var matchResult = match.MatchResult;

            Assert.Equal("not_started", matchResult.EventStatus.Value);
            Assert.Equal("not_started", matchResult.MatchStatus.Value);
            Assert.Equal(0, matchResult.HomeScore);
            Assert.Equal(0, matchResult.AwayScore);            
            Assert.Equal(0, matchResult.MatchTime);
        }


        private DailyScheduleDto StubDailySchedule()
        {
            return new DailyScheduleDto
            {
                generated_at = new DateTime(2019, 1, 1),
                schema = "schema",
                sport_events = new List<Sport_Event>
                {
                     new Sport_Event
                        {
                            tournament_round = new Tournament_Round
                            {
                                    type = "group",
                                    number = 4,
                                    group = "H",
                                    name = "RoundLeague",
                                    phase = "phase"
                            },
                            tournament = new Tournament
                            {
                                id = "sr:tournament:668",
                                name = "AFC Cup",
                                sport = new Sport
                                {
                                    id = "sr:sport:1",
                                    name = "Soccer"
                                },
                                category = new Category
                                {
                                    id = "sr:category:393",
                                    name = "International Clubs"
                                }
                            },
                            season = new Season
                            {
                                id= "sr:season:62299",
                                  name= "AFC Cup 2019",
                                  start_date = "2019-02-05",
                                  end_date = "2019-11-03",
                                  year = "2019",
                                  tournament_id = "sr:tournament:668"
                            },
                            competitors = new List<Competitor>
                            {
                                new Competitor
                                {
                                    id = "sr:competitor:142194",
                                    name = "Kaya FC-Iloilo",
                                    country = "Philippines",
                                    country_code = "PHL",
                                    abbreviation = "KAY",
                                    qualifier = "home"
                                },
                                new Competitor
                                {
                                    id = "sr:competitor:135864",
                                    name = "PSM Makassar",
                                    country = "Indonesia",
                                    country_code = "IDN",
                                    abbreviation = "MAK",
                                    qualifier = "away"
                                }
                            },
                            venue = new Venue
                            {
                              id= "sr:venue:38696",
                              name= "Iloilo Sports Complex",
                              capacity= 7000,
                              city_name= "Iloilo",
                              country_name= "Philippines",
                              map_coordinates= "10.72, 122.57",
                              country_code= "PHL"
                            }
                        }

                    }

            };
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
                        new EndPoint {
                            Name = "MatchSchedule",
                            EndPointSettings = new List<EndPointSetting>
                            {
                                new EndPointSetting { Path = "soccer-t3/{0}/{1}/schedules/{2}/schedule.json?api_key={3}" }
                            } }
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
