namespace LiveScore.Soccers.UnitTests.Features.Matches
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using LiveScore.Soccer.Shared.Configurations;
    using LiveScore.Soccer.Shared.Helpers;
    using LiveScore.Soccer.Shared.SportRadarApi.Models;
    using LiveScore.Soccers.Features.Matches.DataAccesses;
    using LiveScore.Soccers.UnitTests.Shared;
    using LiveScore.Soccer.Dtos;
    using NSubstitute;
    using Xunit;
    using LiveScore.Features.Matches;

    public class SportRadarMatchDataAccessTests
    {
        private readonly SportRadarMatchDataAccess matchDataAccess;
        private readonly IAppSettings appSettings;
        private readonly IRestClient restClient;
        private readonly IPenaltyEventHandler penaltyEventHandler;

        public SportRadarMatchDataAccessTests()
        {
            appSettings = Substitute.For<IAppSettings>();
            restClient = Substitute.For<IRestClient>();
            penaltyEventHandler = Substitute.For<IPenaltyEventHandler>();

            matchDataAccess = new SportRadarMatchDataAccess(appSettings, restClient, penaltyEventHandler);
        }

        [Fact]
        public async Task GetMatches_EmptySportSettings_ThrowsException()
        {
            appSettings.DataProviderSettings.Returns(new StubDataProviderSettings());

            await Assert.ThrowsAsync<InvalidOperationException>(() => matchDataAccess.GetMatches(
                3,
                DateTime.Now,
                DateTime.Now.AddDays(1),
                "en"
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
                "en"
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
                "en"
                );

            Assert.Empty(matches);
        }

        [Fact]
        public async Task GetMatches_Schedule_ReturnTeams()
        {
            appSettings.DataProviderSettings.Returns(new StubDataProviderSettings());
            restClient.ExecuteGetAsync<DailyScheduleDto>(Arg.Any<string>()).Returns(StubDailySchedule());

            var matches = await matchDataAccess.GetMatches(
                1,
                DateTime.Now,
                DateTime.Now.AddDays(1),
                "en");

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
        public async Task GetMatches_Schedule_ReturnLeagueInformation()
        {
            appSettings.DataProviderSettings.Returns(new StubDataProviderSettings());
            restClient.ExecuteGetAsync<DailyScheduleDto>(Arg.Any<string>()).Returns(StubDailySchedule());

            var matches = await matchDataAccess.GetMatches(
                1,
                DateTime.Now,
                DateTime.Now.AddDays(1),
                "en");

            var match = matches.First();
            var league = match.League;

            Assert.Equal("sr:tournament:668", league.Id);
            Assert.Equal("AFC Cup", league.Name);
            Assert.Equal("sr:category:393", league.Category.Id);
            Assert.Equal("International Clubs", league.Category.Name);
        }

        [Fact]
        public async Task GetMatches_Schedule_ReturnLeagueRound()
        {
            appSettings.DataProviderSettings.Returns(new StubDataProviderSettings());
            restClient.ExecuteGetAsync<DailyScheduleDto>(Arg.Any<string>()).Returns(StubDailySchedule());

            var matches = await matchDataAccess.GetMatches(
                1,
                DateTime.Now,
                DateTime.Now.AddDays(1),
                "en");

            var match = matches.First();
            var leagueRound = match.LeagueRound;

            Assert.Equal("RoundLeague", leagueRound.Name);
            Assert.Equal("H", leagueRound.Group);
            Assert.Equal(4, leagueRound.Number);
            Assert.Equal("phase", leagueRound.Phase);
            Assert.Equal("group", leagueRound.Type.Value);
        }

        [Fact]
        public async Task GetMatches_Schedule_ReturnMatchResult()
        {
            appSettings.DataProviderSettings.Returns(new StubDataProviderSettings());
            restClient.ExecuteGetAsync<DailyScheduleDto>(Arg.Any<string>()).Returns(StubDailySchedule());

            var matches = await matchDataAccess.GetMatches(
                1,
                DateTime.Now,
                DateTime.Now.AddDays(1),
                "en");

            var match = matches.First();
            var matchResult = match.MatchResult;

            Assert.Equal("not_started", matchResult.EventStatus.Value);
            Assert.Equal("not_started", matchResult.MatchStatus.Value);
            Assert.Equal(0, matchResult.HomeScore);
            Assert.Equal(0, matchResult.AwayScore);
            Assert.Equal(0, matchResult.MatchTime);
        }

        [Fact]
        public async Task GetMatches_Result_ReturnTeams()
        {
            appSettings.DataProviderSettings.Returns(new StubDataProviderSettings());
            restClient.ExecuteGetAsync<DailyResultDto>(Arg.Any<string>()).Returns(StubDailyResult());

            var matches = await matchDataAccess.GetMatches(
                1,
                DateTime.Now.AddDays(-2),
                DateTime.Now.AddDays(-1),
                "en");

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
        public async Task GetMatches_Result_ReturnLeagueInformation()
        {
            appSettings.DataProviderSettings.Returns(new StubDataProviderSettings());
            restClient.ExecuteGetAsync<DailyResultDto>(Arg.Any<string>()).Returns(StubDailyResult());

            var matches = await matchDataAccess.GetMatches(
                1,
                DateTime.Now.AddDays(-2),
                DateTime.Now.AddDays(-1),
                "en");

            var match = matches.First();
            var league = match.League;

            Assert.Equal("sr:tournament:668", league.Id);
            Assert.Equal("AFC Cup", league.Name);
            Assert.Equal("sr:category:393", league.Category.Id);
            Assert.Equal("International Clubs", league.Category.Name);
        }

        [Fact]
        public async Task GetMatches_Result_ReturnLeagueRound()
        {
            appSettings.DataProviderSettings.Returns(new StubDataProviderSettings());
            restClient.ExecuteGetAsync<DailyResultDto>(Arg.Any<string>()).Returns(StubDailyResult());

            var matches = await matchDataAccess.GetMatches(
                1,
                DateTime.Now.AddDays(-2),
                DateTime.Now.AddDays(-1),
                "en");

            var match = matches.First();
            var leagueRound = match.LeagueRound;

            Assert.Equal("RoundLeague", leagueRound.Name);
            Assert.Equal("H", leagueRound.Group);
            Assert.Equal(4, leagueRound.Number);
            Assert.Equal("phase", leagueRound.Phase);
            Assert.Equal("group", leagueRound.Type.Value);
        }

        [Fact]
        public async Task GetMatches_Result_ReturnMatchResult()
        {
            appSettings.DataProviderSettings.Returns(new StubDataProviderSettings());
            restClient.ExecuteGetAsync<DailyResultDto>(Arg.Any<string>()).Returns(StubDailyResult());

            var matches = await matchDataAccess.GetMatches(
                1,
                DateTime.Now.AddDays(-2),
                DateTime.Now.AddDays(-1),
                "en");

            var match = matches.First();
            var matchResult = match.MatchResult;
            var matchPeriod = match.MatchResult.MatchPeriods.First();

            Assert.Equal("live", matchResult.EventStatus.Value);
            Assert.Equal("1st_half", matchResult.MatchStatus.Value);
            Assert.Equal(0, matchResult.HomeScore);
            Assert.Equal(2, matchResult.AwayScore);
            Assert.Equal(29, matchResult.MatchTime);
            Assert.Equal(1, matchResult.Period);
            Assert.NotEmpty(matchResult.MatchPeriods);
            Assert.Equal(0, matchPeriod.HomeScore);
            Assert.Equal(2, matchPeriod.AwayScore);
            Assert.Equal(1, matchPeriod.Number);
            Assert.Equal("regular_period", matchPeriod.PeriodType.Value);
        }

        [Fact]
        public async Task GetLiveMatches_EmptyResult_ReturnEmpty()
        {
            appSettings.DataProviderSettings.Returns(new StubDataProviderSettings());
            restClient.ExecuteGetAsync<DailyResultDto>(Arg.Is<string>(x => x.Contains("schedules/live/results.json")))
                .Returns(new DailyResultDto());

            var matches = await matchDataAccess.GetLiveMatches(
                1,
                "en"
                );

            Assert.Empty(matches);
        }

        [Fact]
        public async Task GetLiveMatches_Result_ReturnMatchResult()
        {
            appSettings.DataProviderSettings.Returns(new StubDataProviderSettings());
            restClient.ExecuteGetAsync<DailyResultDto>(Arg.Is<string>(x => x.Contains("schedules/live/results.json")))
                .Returns(StubDailyResult());

            var matches = await matchDataAccess.GetLiveMatches(
                1,
                "en");

            var match = matches.First();
            var matchResult = match.MatchResult;
            var matchPeriod = match.MatchResult.MatchPeriods.First();

            Assert.Equal("live", matchResult.EventStatus.Value);
            Assert.Equal("1st_half", matchResult.MatchStatus.Value);
            Assert.Equal(0, matchResult.HomeScore);
            Assert.Equal(2, matchResult.AwayScore);
            Assert.Equal(29, matchResult.MatchTime);
            Assert.Equal(1, matchResult.Period);
            Assert.NotEmpty(matchResult.MatchPeriods);
            Assert.Equal(0, matchPeriod.HomeScore);
            Assert.Equal(2, matchPeriod.AwayScore);
            Assert.Equal(1, matchPeriod.Number);
            Assert.Equal("regular_period", matchPeriod.PeriodType.Value);
        }

        [Fact]
        public async Task GetTimelines_EmptySportSettings_ThrowsException()
        {
            appSettings.DataProviderSettings.Returns(new StubDataProviderSettings());

            await Assert.ThrowsAsync<InvalidOperationException>(() => matchDataAccess.GetTimelines(
                2,
                "eu",
                "sr:match:1",
                "en"
                ));
        }

        [Fact]
        public async Task GetTimelines_EmptyRegionSettings_ReturnEmpty()
        {
            appSettings.DataProviderSettings.Returns(new StubDataProviderSettings());

            var timelines = await matchDataAccess.GetTimelines(
                1,
                "eu",
                "sr:match:1",
                "en"
                );

            Assert.Empty(timelines);
        }

        [Fact]
        public async Task GetTimelines_EmptyMatchTimelineDto_ReturnEmpty()
        {
            appSettings.DataProviderSettings.Returns(new StubDataProviderSettings());
            restClient.ExecuteGetAsync<MatchTimelineDto>(Arg.Any<string>()).Returns(new MatchTimelineDto());

            var timelines = await matchDataAccess.GetTimelines(
                1,
                "eu",
                "sr:match:1",
                "en"
                );

            Assert.Empty(timelines);
        }

        [Fact]
        public async Task GetTimelines_MatchTimelineDto_ReturnTimelines()
        {
            appSettings.DataProviderSettings.Returns(new StubDataProviderSettings());
            restClient.ExecuteGetAsync<MatchTimelineDto>(Arg.Any<string>()).Returns(StubTimelines.StubMatchTimeLineDto());

            var timelines = await matchDataAccess.GetTimelines(
                1,
                "eu",
                "sr:match:1",
                "en"
                );

            Assert.NotEmpty(timelines);
            Assert.Equal(8, timelines.Count());
            Assert.Equal("match_started", timelines.First().Type);
            Assert.Equal("match_ended", timelines.Last().Type);
        }

        [Fact]
        public async Task GetTimelines_MatchTimelineDto_ContainsScoreChange()
        {
            appSettings.DataProviderSettings.Returns(new StubDataProviderSettings());
            restClient.ExecuteGetAsync<MatchTimelineDto>(Arg.Any<string>()).Returns(StubTimelines.StubMatchTimeLineDto());

            var timelines = await matchDataAccess.GetTimelines(
                1,
                "eu",
                "sr:match:1",
                "en"
                );

            var scoreChange = timelines.First(x => x.Type.Equals("score_change"));

            Assert.NotNull(scoreChange);
            Assert.NotEmpty(scoreChange.Commentaries);
            Assert.NotNull(scoreChange.GoalScorer);
            Assert.NotNull(scoreChange.Assist);
            Assert.Equal(1, scoreChange.AwayScore);
            Assert.Equal(0, scoreChange.HomeScore);
        }

        [Fact]
        public async Task GetTimelines_MatchTimelineDto_ContainsYellowCard()
        {
            appSettings.DataProviderSettings.Returns(new StubDataProviderSettings());
            restClient.ExecuteGetAsync<MatchTimelineDto>(Arg.Any<string>()).Returns(StubTimelines.StubMatchTimeLineDto());

            var timelines = await matchDataAccess.GetTimelines(
                1,
                "eu",
                "sr:match:1",
                "en"
                );

            var yellowCard = timelines.First(x => x.Type.Equals("yellow_card"));

            Assert.NotNull(yellowCard);
            Assert.NotEmpty(yellowCard.Commentaries);
            Assert.NotNull(yellowCard.Player);
            Assert.Equal("home", yellowCard.Team);
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
                            venue = new VenueDto
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

        private DailyResultDto StubDailyResult()
        {
            return new DailyResultDto
            {
                generated_at = new DateTime(2019, 1, 1),
                schema = "schema",
                results = new List<Result>
                {
                    new Result{
                        sport_event =  new Sport_Event
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
                            venue = new VenueDto
                            {
                              id= "sr:venue:38696",
                              name= "Iloilo Sports Complex",
                              capacity= 7000,
                              city_name= "Iloilo",
                              country_name= "Philippines",
                              map_coordinates= "10.72, 122.57",
                              country_code= "PHL"
                            }
                        },
                        sport_event_status = new Sport_Event_Status
                        {
                            status="live",
                            match_status = "1st_half",
                            home_score = 0,
                            away_score = 2,
                            period = 1,
                            clock = new Clock{
                                match_time = "29:17"
                            },
                            period_scores = new List<Period_Scores>{
                                new Period_Scores{
                                    home_score = 0,
                                    away_score = 2,
                                    type = "regular_period",
                                    number = 1
                                }
                            }
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
                                new EndPointSetting { Path = "soccer-t3/{0}/{1}/schedules/{2}/schedule.json?api_key={3}" },
                                new EndPointSetting { Path = "soccer-t3/{0}/{1}/schedules/{2}/results.json?api_key={3}", IsPast=true }
                            } },
                        new EndPoint {
                            Name = "LiveMatch",
                            EndPointSettings = new List<EndPointSetting>
                            {
                                new EndPointSetting { Path = "soccer-t3/{0}/{1}/schedules/live/results.json?api_key={2}" }
                            } },
                        new EndPoint {
                            Name = "Timeline",
                            EndPointSettings = new List<EndPointSetting>
                            {
                                new EndPointSetting { Path = "soccer-xt3/{0}/{1}/matches/{2}/timeline.json?api_key={3}" }
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
