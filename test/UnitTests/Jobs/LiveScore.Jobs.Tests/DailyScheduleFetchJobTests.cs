namespace LiveScore.Jobs.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using LiveScore.Features.Leagues.Models;
    using LiveScore.Features.Matches;
    using LiveScore.Features.Matches.Models;
    using LiveScore.Soccer.Shared;
    using LiveScore.Soccer.Shared.Database;
    using LiveScore.Soccer.Shared.Database.Entities;
    using LiveScore.Soccer.Shared.Enumerations;
    using LiveScore.Soccer.Jobs;
    using NSubstitute;
    using Xunit;

    public class DailyScheduleFetchJobTests
    {
        private readonly IDynamicRepository repository;
        private readonly IMatchDataAccess dataAccess;
        private readonly Func<int, DataProvider, IMatchDataAccess> matchDataAccessFactory;
        private readonly DailyScheduleFetchJob job;

        public DailyScheduleFetchJobTests()
        {
            repository = Substitute.For<IDynamicRepository>();
            dataAccess = Substitute.For<IMatchDataAccess>();
            matchDataAccessFactory = (_, __) => dataAccess;
            job = new DailyScheduleFetchJob(repository, matchDataAccessFactory);
        }

        [Fact]
        public async Task FetchSchedule_Always_GetMatches()
        {
            await job.FetchSchedule(SportConstants.SoccerId, 2);

            await dataAccess.Received(1).GetMatches(SportConstants.SoccerId, Arg.Any<DateTime>(), Arg.Any<DateTime>(), Arg.Any<string>());
        }

        [Fact]
        public async Task FetchSchedule_HasMatches_ShouldInsert()
        {
            dataAccess.GetMatches(SportConstants.SoccerId, Arg.Any<DateTime>(), Arg.Any<DateTime>(), Arg.Any<string>()).Returns(new List<Match>
            {
                new Match
                {
                    Id = "sr:match:1",
                    EventDate = DateTime.Now,
                    MatchResult = new MatchResult{ EventStatus = MatchStatus.NotStarted },
                    League = new League{ Id= "league1"  }
                }
            });

            await job.FetchSchedule(SportConstants.SoccerId, 2);

            await repository.InsertRange(Arg.Is<IEnumerable<MatchEntity>>(x => x.First(m => m.Id == "sr:match:1" && m.LeagueId == "league1") != null)); ;
        }

        [Fact]
        public async Task FetchSchedule_HasMatchesButExist_ShouldUpdate()
        {
            dataAccess.GetMatches(SportConstants.SoccerId, Arg.Any<DateTime>(), Arg.Any<DateTime>(), Arg.Any<string>()).Returns(new List<Match>
            {
                new Match
                {
                    Id = "sr:match:1",
                    EventDate = DateTime.Now,
                    MatchResult = new MatchResult{ EventStatus = MatchStatus.NotStarted },
                    League = new League{ Id= "league1"  }
                }
            });

            repository.GetById<MatchEntity>("sr:match:1", Arg.Any<string>()).Returns(new MatchEntity { Id = "sr:match:1", EventDate = DateTime.Now });

            await job.FetchSchedule(SportConstants.SoccerId, 2);

            await repository.UpdateRange(Arg.Is<IEnumerable<MatchEntity>>(x => x.First(m => m.Id == "sr:match:1") != null)); ;
        }

        [Fact]
        public async Task FetchResult_Always_GetMatches()
        {
            await job.FetchResult(SportConstants.SoccerId, 2);

            await dataAccess.Received(1).GetMatches(SportConstants.SoccerId, Arg.Any<DateTime>(), Arg.Any<DateTime>(), Arg.Any<string>());
        }

        [Fact]
        public async Task FetchResult_HasMatchesAndExistInLive_ShouldDelete()
        {
            dataAccess.GetMatches(SportConstants.SoccerId, Arg.Any<DateTime>(), Arg.Any<DateTime>(), Arg.Any<string>()).Returns(new List<Match>
            {
                new Match
                {
                    Id = "sr:match:1",
                    EventDate = DateTime.Now,
                    MatchResult = new MatchResult{ EventStatus = MatchStatus.Closed },
                    League = new League{ Id= "league1"  }
                }
            });

            repository.GetById<LiveMatchEntity>("sr:match:1", Arg.Any<string>()).Returns(new LiveMatchEntity { Id = "sr:match:1", EventDate = DateTime.Now });

            await job.FetchResult(SportConstants.SoccerId, 2);

            await repository.Received(1).Delete<LiveMatchEntity>("sr:match:1", Arg.Any<string>());
        }
    }
}