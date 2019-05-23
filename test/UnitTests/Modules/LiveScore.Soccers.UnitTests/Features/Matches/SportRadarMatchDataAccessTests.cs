namespace LiveScore.Soccers.UnitTests.Features.Matches
{
    using LiveScore.Shared.Configurations;
    using LiveScore.Shared.Helpers;
    using LiveScore.Soccers.Features.Matches.DataAccesses;
    using NSubstitute;
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

        }
    }
}
