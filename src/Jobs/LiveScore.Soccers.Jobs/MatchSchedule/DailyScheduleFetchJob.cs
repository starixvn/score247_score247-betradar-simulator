namespace LiveScore.Soccer.Jobs
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using LiveScore.Soccer.Matches.DataAccesses;
    using LiveScore.Soccer.Matches.Models;
    using LiveScore.Soccer.Shared;
    using LiveScore.Soccer.Shared.Database;
    using LiveScore.Soccer.Shared.Database.Entities;
    using LiveScore.Soccer.Shared.Enumerations;
    using LiveScore.Soccer.Shared.Models;
    using Newtonsoft.Json;

    public interface IDailyScheduleFetchJob
    {
        Task FetchSchedule(int sportId, int dateSpan);

        Task FetchResult(int sportId, int dateSpan);
    }

    public class DailyScheduleFetchJob : IDailyScheduleFetchJob
    {
        private readonly IDynamicRepository repository;
        private readonly Func<int, DataProvider, IMatchDataAccess> matchDataAccessFactory;

        public DailyScheduleFetchJob(
            IDynamicRepository repository,
            Func<int, DataProvider, IMatchDataAccess> matchDataAccessFactory)
        {
            this.matchDataAccessFactory = matchDataAccessFactory;
            this.repository = repository;
        }

        public async Task FetchSchedule(int sportId, int dateSpan)
        {
            var from = DateTime.Now;
            var to = DateTime.Now.AddDays(dateSpan);

            foreach (var lang in Enumeration.GetAll<Language>())
            {
                await FetchDailySchedule(sportId, from, to, lang.Value);
            }
        }

        public async Task FetchResult(int sportId, int dateSpan)
        {
            var from = DateTime.Now.AddDays(-dateSpan);
            var to = DateTime.Now;

            foreach (var lang in Enumeration.GetAll<Language>())
            {
                await FetchDailySchedule(sportId, from, to, lang.Value);
            }
        }

        private async Task FetchDailySchedule(int sportId, DateTime from, DateTime to, string language)
        {
            var dataAccess = matchDataAccessFactory(sportId, DataProvider.ThirdPartyApi);

            var matches = await dataAccess.GetMatches(
                sportId,
                from.ToUniversalTime(),
                to.ToUniversalTime(),
                Enumeration.FromValue<Language>(language).DisplayName.ToLowerInvariant());

            var newMatchEntities = new List<MatchEntity>();
            var existingMatchEntities = new List<MatchEntity>();

            foreach (var match in matches)
            {
                var matchEntity = await repository.GetById<MatchEntity>(match.Id, language);

                if (matchEntity == null)
                {
                    var newMatchEntity = CreateMatchEntity(sportId, language, match);
                    newMatchEntities.Add(newMatchEntity);
                }
                else
                {
                    matchEntity.EventDate = match.EventDate.ToUniversalTime();
                    matchEntity.Value = JsonConvert.SerializeObject(match);

                    existingMatchEntities.Add(matchEntity);
                }

                if (match.MatchResult.EventStatus == MatchStatus.Closed)
                {
                    await repository.Delete<LiveMatchEntity>(match.Id, language);
                }
            }

            if (newMatchEntities.Any())
            {
                await repository.InsertRange(newMatchEntities);
            }

            if (existingMatchEntities.Any())
            {
                await repository.UpdateRange(existingMatchEntities);
            }

            //TODO update cache for schedule and results --> liveMatchIds
        }

        private static MatchEntity CreateMatchEntity(int sportId, string language, IMatch match)
        => new MatchEntity
        {
            Id = match.Id,
            EventDate = match.EventDate.ToUniversalTime(),
            LeagueId = match.League.Id,
            Language = language,
            SportId = sportId,
            Region = match.Region,
            Value = JsonConvert.SerializeObject(match)
        };
    }
}