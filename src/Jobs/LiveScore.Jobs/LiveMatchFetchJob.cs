namespace LiveScore.Jobs
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using LiveScore.Features.Matches;
    using LiveScore.Features.Matches.Models;
    using LiveScore.Shared;
    using LiveScore.Shared.Database;
    using LiveScore.Shared.Database.Entities;
    using LiveScore.Shared.Enumerations;
    using LiveScore.Shared.Models;
    using Newtonsoft.Json;

    public interface ILiveMatchFetchJob
    {
        Task Fetch(int sportId);
    }

    public class LiveMatchFetchJob : ILiveMatchFetchJob
    {
        private readonly IDynamicRepository repository;
        private readonly Func<int, DataProvider, IMatchDataAccess> matchDataAccessFactory;

        public LiveMatchFetchJob(
            IDynamicRepository repository,
            Func<int, DataProvider, IMatchDataAccess> matchDataAccessFactory)
        {
            this.repository = repository;
            this.matchDataAccessFactory = matchDataAccessFactory;
        }

        public async Task Fetch(int sportId)
        {
            var liveMatches = new List<IMatch>();
            var closedMatches = new List<IMatch>();

            var dataAccess = matchDataAccessFactory(sportId, DataProvider.ThirdPartyApi);

            foreach (var lang in Enumeration.GetAll<Language>())
            {
                var language = Enumeration.FromValue<Language>(lang.Value).DisplayName.ToLowerInvariant();
                var matches = await dataAccess.GetLiveMatches(sportId, language);

                liveMatches.AddRange(matches.Where(x => x.MatchResult.EventStatus == MatchStatus.Live).ToList());
                closedMatches.AddRange(matches.Where(x => x.MatchResult.EventStatus == MatchStatus.Closed).ToList());

                if (closedMatches.Any())
                {
                    await DeleteLiveMatches(closedMatches, language);
                }

                if (liveMatches.Any())
                {
                    await InsertLiveMatches(liveMatches, language);
                }

                // status == not_started -> incomming matches?
            }

            //TODO update cache
        }

        private async Task DeleteLiveMatches(List<IMatch> matches, string language)
        {
            var liveMatchEntities = new List<LiveMatchEntity>();
            var matchEntities = new List<MatchEntity>();

            // status == closed && exist in live table -> delete in live table ->  update cache
            foreach (var match in matches)
            {
                var liveMatchEntity = await repository.GetById<LiveMatchEntity>(match.Id, language);

                if (liveMatchEntity != null)
                {
                    liveMatchEntities.Add(liveMatchEntity);

                    var matchEntity = await repository.GetById<MatchEntity>(match.Id, language);
                    matchEntity.Value = JsonConvert.SerializeObject(match);
                    matchEntities.Add(matchEntity);
                }
            }

            if (liveMatchEntities.Any())
            {
                await repository.DeleteRange(liveMatchEntities);
            }

            if (matchEntities.Any())
            {
                await repository.UpdateRange(matchEntities);
            }
        }

        private async Task InsertLiveMatches(List<IMatch> matches, string language)
        {
            var liveMatchEntities = new List<LiveMatchEntity>();
            // status == live && not exist in live table -> insert to live table -> update cache
            foreach (var match in matches)
            {
                var liveMatchEntity = await repository.GetById<LiveMatchEntity>(match.Id, language);

                if (liveMatchEntity == null)
                {
                    var matchEntity = await repository.GetById<MatchEntity>(match.Id, language);

                    if (matchEntity != null)
                    {
                        liveMatchEntities.Add(CreateLiveMatchEntity(matchEntity));
                    }
                }
            }

            if (liveMatchEntities.Any())
            {
                await repository.InsertRange(liveMatchEntities);
            }
        }

        private static LiveMatchEntity CreateLiveMatchEntity(MatchEntity matchEntity)
        => new LiveMatchEntity
        {
            Id = matchEntity.Id,
            SportId = matchEntity.SportId,
            Language = matchEntity.Language,
            Value = matchEntity.Value,
            LeagueId = matchEntity.LeagueId,
            EventDate = matchEntity.EventDate.ToUniversalTime(),
            CreatedTime = matchEntity.CreatedTime.ToUniversalTime(),
            ModifiedTime = matchEntity.ModifiedTime.ToUniversalTime()
        };
    }
}