using Fanex.Caching;
namespace LiveScore.Features.Matches
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using LiveScore.Entity;
    using LiveScore.Entity.Entities;
    using LiveScore.Features.Matches.Models;
    using LiveScore.Shared;
    using Newtonsoft.Json;

    public interface MatchService
    {
        Task<IEnumerable<IMatch>> GetMatches(int sportId, DateTime from, DateTime to, TimeSpan timeZone, string language);

        Task<IEnumerable<IMatch>> GetLiveMatches(int sportId, string language);
    }

    public class MatchServiceImpl : MatchService
    {
        private readonly InstanceFactory instanceFactory;
        private readonly ICacheService cacheService;
        private readonly GenericRepository<MatchEntity> repository;

        public MatchServiceImpl(InstanceFactory instanceFactory, ICacheService cacheService, GenericRepository<MatchEntity> repository)
        {
            this.instanceFactory = instanceFactory;
            this.cacheService = cacheService;
            this.repository = repository;
        }

        public async Task<IEnumerable<IMatch>> GetMatches(
            int sportId,
            DateTime from,
            DateTime to,
            TimeSpan timeZone,
            string language)
        {
            if (sportId <= 0
                || from > to
                || from == DateTime.MinValue
                || to == DateTime.MinValue)
            {
                return await Task.FromResult(Enumerable.Empty<IMatch>());
            }

            //var matchDataAccess = instanceFactory.CreateMatchDataAccess(sportId);

            //var matches = (await matchDataAccess
            //    .GetMatches(sportId, from, to, language))
            //    .Select(match =>
            //    {
            //        if (match.MatchResult.EventStatus.IsLive())
            //        {
            //            var matchTime = cacheService.Get<int>("MatchTime-" + match.Id);

            //            if (matchTime > 0)
            //            {
            //                match.MatchResult.MatchTime = $"{matchTime}:00";
            //            }
            //        }

            //        match.ChangeEventDateByTimeZone(timeZone);

            //        return match;
            //    });

            var matches = new List<Match>();
            var matchEntities = await repository.GetAll();

            foreach (var entity in matchEntities)
            {
                matches.Add(JsonConvert.DeserializeObject<Match>(entity.Value));
            }

            return matches;
        }

        public async Task<IEnumerable<IMatch>> GetLiveMatches(int sportId, string language)
        {
            var matchDataAccess = instanceFactory.CreateMatchDataAccess(sportId);

            return await matchDataAccess.GetLiveMatches(sportId, language);
        }
    }
}