namespace LiveScore.Soccers.UnitTests.Features.Matches
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using LiveScore.Shared.Database;
    using LiveScore.Shared.Database.Entities;
    using LiveScore.Soccers.Features.Matches.DataAccesses;
    using NSubstitute;
    using Xunit;

    public class MatchDataAccessTests
    {
        private readonly MatchDataAccess matchDataAccess;
        private readonly IDynamicRepository dynamicRepository;

        public MatchDataAccessTests()
        {
            dynamicRepository = Substitute.For<IDynamicRepository>();
            dynamicRepository.Find(Arg.Any<Func<MatchEntity, bool>> ()).Returns(new List<MatchEntity> { StubMatchEntity() });
            dynamicRepository.Find(Arg.Any<Func<MatchResultEntity, bool>>()).Returns(new List<MatchResultEntity>());

            matchDataAccess = new MatchDataAccess(dynamicRepository);
        }

        [Fact]
        public async Task GetMatches_Always_ReturnTeams()
        {
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

            Assert.Equal("sr:competitor:559146", home.Id);
            Assert.Equal("Denton Diablos FC", home.Name);
            Assert.Equal("USA", home.Country);
            Assert.Equal("USA", home.CountryCode);
            Assert.Equal("DEN", home.Abbreviation);
            Assert.True(home.IsHome);

            Assert.Equal("sr:competitor:559156", away.Id);
            Assert.Equal("Dallas City FC", away.Name);
            Assert.Equal("USA", away.Country);
            Assert.Equal("USA", away.CountryCode);
            Assert.Equal("DAL", away.Abbreviation);
            Assert.False(away.IsHome);
        }

        [Fact]
        public async Task GetMatches_Always_ReturnLeagueInformation()
        {
            var matches = await matchDataAccess.GetMatches(
                1,
                DateTime.Now,
                DateTime.Now.AddDays(1),
                "en",
                TimeSpan.Zero);

            var match = matches.First();
            var league = match.League;

            Assert.Equal("sr:tournament:28424", league.Id);
            Assert.Equal("National Premier Soccer League", league.Name);
            Assert.Equal("sr:category:26", league.Category.Id);
            Assert.Equal("USA", league.Category.Name);
        }

        [Fact]
        public async Task GetMatches_Always_ReturnLeagueRound()
        {
            var matches = await matchDataAccess.GetMatches(
                1,
                DateTime.Now,
                DateTime.Now.AddDays(1),
                "en",
                TimeSpan.Zero);

            var match = matches.First();
            var leagueRound = match.LeagueRound;
           
            Assert.Equal("regular season", leagueRound.Phase);
            Assert.Equal("group", leagueRound.Type.Value);
        }

        [Fact]
        public async Task GetMatches_Always_ReturnMatchResult()
        {
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
            Assert.Equal(0, matchResult.Period);
            Assert.Equal(0, matchResult.MatchTime);
        }

        private MatchEntity StubMatchEntity()
        {
            return new MatchEntity
            {
                Id = "sr:match:17809194",
                Value = StubMatchJson(),
                LeagueId = "sr:tournament:28424",
                Language = "en-US",
                EventDate =DateTime.Parse("2019-05-22 00:00:00+00")
            };
        }

        private static string StubMatchJson()
        {
            return @"{
                    'Id': 'sr:match:17809194',
                    'Name': null,
                    'Teams': [
                        {
                            'Id': 'sr:competitor:559146',
                            'Flag': null,
                            'Name': 'Denton Diablos FC',
                            'Coach': null,
                            'IsHome': true,
                            'Country': 'USA',
                            'Players': null,
                            'Formation': null,
                            'Statistic': null,
                            'CountryCode': 'USA',
                            'Abbreviation': 'DEN',
                            'Substitutions': null
                        },
                        {
                            'Id': 'sr:competitor:559156',
                            'Flag': null,
                            'Name': 'Dallas City FC',
                            'Coach': null,
                            'IsHome': false,
                            'Country': 'USA',
                            'Players': null,
                            'Formation': null,
                            'Statistic': null,
                            'CountryCode': 'USA',
                            'Abbreviation': 'DAL',
                            'Substitutions': null
                        }
                    ],
                    'Venue': null,
                    'League': {
                        'Id': 'sr:tournament:28424',
                        'Flag': null,
                        'Name': 'National Premier Soccer League',
                        'Order': 0,
                        'Category': {
                            'Id': 'sr:category:26',
                            'Name': 'USA',
                            'CountryCode': 'USA'
                        }
                    },
                    'Referee': null,
                    'EventDate': '2019-05-22T00:00:00Z',
                    'TimeLines': null,
                    'Attendance': 0,
                    'LeagueRound': {
                        'Name': null,
                        'Type': {
                            'Value': 'group',
                            'DisplayName': 'Group'
                        },
                        'Group': null,
                        'Phase': 'regular season',
                        'Number': 1
                    },
                    'MatchResult': {
                        'Period': 0,
                        'WinnerId': null,
                        'AwayScore': 0,
                        'HomeScore': 0,
                        'MatchTime': 0,
                        'EventStatus': {
                            'Value': 'not_started',
                            'DisplayName': 'NotStarted'
                        },
                        'MatchStatus': {
                            'Value': 'not_started',
                            'DisplayName': 'NotStarted'
                        },
                        'MatchPeriods': null
                    }
                }";
        }
    }


}
