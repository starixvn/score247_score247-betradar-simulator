namespace LiveScore.Providers.SportRadar.Dtos
{
    using System;
    using System.Collections.Generic;
    using LiveScore.Providers.SportRadar.Converters;
    using Newtonsoft.Json;

    public class RegionTournamentDto
    {
        [JsonConverter(typeof(DateFormatConverter), "yyyy-MM-ddTHH:mm:sszzz")]
        public DateTime generated_at { get; set; }

        public string schema { get; set; }
        public IEnumerable<Tournament> tournaments { get; set; }
    }
}