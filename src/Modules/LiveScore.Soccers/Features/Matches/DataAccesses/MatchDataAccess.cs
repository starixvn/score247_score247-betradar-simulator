namespace LiveScore.Soccers.Features.Matches.DataAccesses
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using LiveScore.Entity;
    using LiveScore.Entity.Entities;
    using LiveScore.Features.Matches;
    using LiveScore.Features.Matches.Models;
    using Newtonsoft.Json;

    public class MatchDataAccess : IMatchDataAccess
    {
        private readonly IGenericRepository<MatchEntity> repository;

        public MatchDataAccess(IGenericRepository<MatchEntity> repository)
        {
            this.repository = repository;
        }

        public async Task<IEnumerable<IMatch>> GetMatches(int sportId, DateTime from, DateTime to, string language)
        {
            var matches = new List<Match>();

            //TODO implement daterange
            var matchEntities = (await repository.GetAll()).Where(x => x.SportId == sportId && x.Language.Equals(language, StringComparison.OrdinalIgnoreCase));

            foreach (var entity in matchEntities)
            {
                matches.Add(JsonConvert.DeserializeObject<Match>(entity.Value));
            }

            return matches;
        }

        public async Task<IEnumerable<IMatch>> GetLiveMatches(int sportId, string language)
        {
            return (await GetMatches(sportId, DateTime.Today, DateTime.Today, language))
                .Where(m => m.MatchResult.EventStatus.IsLive());
        }
    }
}
