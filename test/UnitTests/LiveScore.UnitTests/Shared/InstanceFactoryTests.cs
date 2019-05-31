namespace LiveScore.UnitTests.Shared
{
    using LiveScore.Features.Matches;
    using LiveScore.Shared;
    using NSubstitute;
    using System;
    using System.Collections.Generic;
    using Xunit;

    public class InstanceFactoryTests
    {
        private readonly IMatchDataAccess matchDataAccess;

        private readonly InstanceFactory instanceFactory;

        public InstanceFactoryTests()
        {
            matchDataAccess = Substitute.For<IMatchDataAccess>();
            var dataAccessDictionary = new Dictionary<DataProvider, IMatchDataAccess> { { DataProvider.Database, matchDataAccess } };

            var matches = new Dictionary<int, IDictionary<DataProvider, IMatchDataAccess>>
            {
                { SportConstants.SoccerId, dataAccessDictionary },
                { SportConstants.BasketballId, null }
            };

            instanceFactory = new InstanceFactoryImpl(matches, null);
        }

        [Fact]
        public void CreateMatchDataAccess_SportIdNotExist_ThrowsException()
        {
            Assert.Throws<InvalidOperationException>(() => instanceFactory.CreateMatchDataAccess(3));
        }

        [Fact]
        public void CreateMatchDataAccess_HasContainer_ReturnsDataAccess()
        {
            var actualDataAccess = instanceFactory.CreateMatchDataAccess(SportConstants.SoccerId);

            Assert.NotNull(actualDataAccess);
        }
    }
}
