namespace LiveScore.Soccers.UnitTests.Features.Leagues
{
    using LiveScore.Features.Leagues;
    using LiveScore.Shared.Models.Dtos;
    using LiveScore.Soccers.Features.Leagues;
    using LiveScore.Soccers.Features.Leagues.DataProviders;
    using NSubstitute;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Xunit;

    public class LeagueDataAccessTests
    {
        private readonly LeagueDataAccess leagueDataAccess;
        private readonly ILeagueApi leagueApi;

        public LeagueDataAccessTests()
        {
            leagueApi = Substitute.For<ILeagueApi>();
            leagueApi
                .GetLeagues(1, "en")
                .Returns(Task.FromResult(StubTournaments()));

            leagueDataAccess = new LeagueDataAccessImpl(leagueApi);
        }

        [Fact]
        public async Task GetLeagues_Always_ReturnLeague()
        {
            var leagues = await leagueDataAccess.GetLeagues(
                1,
                "en");

            var league = leagues.First();

            Assert.Equal("UEFA Champions League", league.Name);
            Assert.Equal("sr:tournament:7", league.Id);
        }

        [Fact]
        public async Task GetLeagues_Always_ReturnLeagueCategory()
        {
            var leagues = await leagueDataAccess.GetLeagues(
                1,
                "en");

            var league = leagues.First();
            var leagueCategory = league.Category;

            Assert.Equal("International Clubs", leagueCategory.Name);
            Assert.Equal("sr:category:393", leagueCategory.Id);
        }

        private Tournaments StubTournaments()
        {
            return new Tournaments
            {
                generated_at = new DateTime(2019, 1, 1),
                schema = "schema",
                tournaments = new List<Tournament>
                {
                    new Tournament
                    {
                        id="sr:tournament:7",
                        name = "UEFA Champions League",
                        sport = new Sport
                        {
                            id="sr:sport:1",
                            name="Soccer"
                        },
                        category = new Category
                        {
                            id="sr:category:393",
                            name="International Clubs",
                            country_code = ""
                        },
                        current_season = new Current_Season
                        {
                            id="sr:season:54533",
                            name="UEFA Champions League 18/19",
                            start_date="2018-06-26",
                            end_date="2019-06-02",
                            year="18/19"
                        },
                        season_coverage_info = new Season_Coverage_Info
                        {
                            season_id = "sr:season:54533",
                            scheduled = 217,
                            played = 216,
                            max_coverage_level = "platinum",
                            max_covered = 124,
                            min_coverage_level = "bronze"
                        }
                    }
                }
            };
        }
    }
}
