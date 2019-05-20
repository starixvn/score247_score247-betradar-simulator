namespace LiveScore.Soccers.Features.Matches.DataProviders
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using LiveScore.Entity;
    using LiveScore.Entity.Entities;
    using LiveScore.Features.Matches.Models;
    using Newtonsoft.Json;

    public class DatabaseMatchApi : IMatchApi
    {
        private readonly GenericRepository<MatchEntity> repository;

        public DatabaseMatchApi(GenericRepository<MatchEntity> repository)
        {
            this.repository = repository;
        }

        public async Task<IEnumerable<IMatch>> GetDailySchedules(int sportId, DateTime from, DateTime to, string language)
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

        public Task<IEnumerable<IMatch>> GetLiveSchedules(int sportId, string language)
        {
            throw new NotImplementedException();
        }
    }
}
