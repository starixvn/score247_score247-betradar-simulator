namespace LiveScore.Jobs.Tests
{
    using LiveScore.Features.Matches;
    using LiveScore.Features.Matches.Models;
    using LiveScore.Shared;
    using LiveScore.Shared.Database;
    using LiveScore.Shared.Database.Entities;
    using NSubstitute;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Xunit;

    public class LiveMatchFetchJobTests
    {
        private readonly IDynamicRepository repository;
        private readonly DataAccessFactory instanceFactory;
        private readonly IMatchDataAccess dataAccess;

        private readonly LiveMatchFetchJob job;

        public LiveMatchFetchJobTests()
        {
            repository = Substitute.For<IDynamicRepository>();
            dataAccess = Substitute.For<IMatchDataAccess>();
            instanceFactory = Substitute.For<DataAccessFactory>();
            instanceFactory.CreateMatchDataAccess(SportConstants.SoccerId, DataProvider.ThirdPartyApi).Returns(dataAccess);

            job = new LiveMatchFetchJob(repository, instanceFactory);
        }

        [Fact]
        public async Task Fetch_Empty_ShouldNotThrowsException()
        {
            await job.Fetch(SportConstants.SoccerId);
        }

        [Fact]
        public async Task Fetch_HasLiveMatchesAndNotExist_ShouldInsert()
        {
            dataAccess.GetLiveMatches(SportConstants.SoccerId, Arg.Any<string>()).Returns(new List<Match>
            {
                new Match { Id = "sr:match:1", MatchResult = new MatchResult{ EventStatus = MatchStatus.Live } }
            });

            repository.GetById<MatchEntity>("sr:match:1", Arg.Any<string>()).Returns(new MatchEntity
            {
                Id = "sr:match:1",
                EventDate = DateTime.Now,
                CreatedTime = DateTime.Now,
                ModifiedTime = DateTime.Now
            });

            await job.Fetch(SportConstants.SoccerId);

            await repository.Received(1).InsertRange(Arg.Any<IEnumerable<LiveMatchEntity>>());
        }

        [Fact]
        public async Task Fetch_HasLiveMatchesAndExist_ShouldNotInsert()
        {
            dataAccess.GetLiveMatches(SportConstants.SoccerId, Arg.Any<string>()).Returns(new List<Match>
            {
                new Match { Id = "sr:match:1", MatchResult = new MatchResult{ EventStatus = MatchStatus.Live } }
            });

            repository.GetById<LiveMatchEntity>("sr:match:1", Arg.Any<string>()).Returns(new LiveMatchEntity
            {
                Id = "sr:match:1",
                EventDate = DateTime.Now,
                CreatedTime = DateTime.Now,
                ModifiedTime = DateTime.Now
            });

            await job.Fetch(SportConstants.SoccerId);

            await repository.DidNotReceive().InsertRange(Arg.Any<IEnumerable<LiveMatchEntity>>());
        }

        [Fact]
        public async Task Fetch_HasClosedMatchesAndExistInLive_ShouldDelete()
        {
            dataAccess.GetLiveMatches(SportConstants.SoccerId, Arg.Any<string>()).Returns(new List<Match>
            {
                new Match { Id = "sr:match:1", MatchResult = new MatchResult{ EventStatus = MatchStatus.Closed } }
            });

            repository.GetById<MatchEntity>("sr:match:1", Arg.Any<string>()).Returns(new MatchEntity
            {
                Id = "sr:match:1",
                EventDate = DateTime.Now,
                CreatedTime = DateTime.Now,
                ModifiedTime = DateTime.Now
            });

            repository.GetById<LiveMatchEntity>("sr:match:1", Arg.Any<string>()).Returns(new LiveMatchEntity
            {
                Id = "sr:match:1",
                EventDate = DateTime.Now,
                CreatedTime = DateTime.Now,
                ModifiedTime = DateTime.Now
            });

            await job.Fetch(SportConstants.SoccerId);

            await repository.Received(1).DeleteRange(Arg.Any<IEnumerable<LiveMatchEntity>>());
            await repository.Received(1).UpdateRange(Arg.Any<IEnumerable<MatchEntity>>());
        }

        [Fact]
        public async Task Fetch_HasClosedMatchesButNotExistInLive_ShouldNotDelete()
        {
            dataAccess.GetLiveMatches(SportConstants.SoccerId, Arg.Any<string>()).Returns(new List<Match>
            {
                new Match { Id = "sr:match:1", MatchResult = new MatchResult{ EventStatus = MatchStatus.Closed } }
            });

            await job.Fetch(SportConstants.SoccerId);

            await repository.DidNotReceive().DeleteRange(Arg.Any<IEnumerable<LiveMatchEntity>>());
            await repository.DidNotReceive().UpdateRange(Arg.Any<IEnumerable<LiveMatchEntity>>());
        }
    }
}
