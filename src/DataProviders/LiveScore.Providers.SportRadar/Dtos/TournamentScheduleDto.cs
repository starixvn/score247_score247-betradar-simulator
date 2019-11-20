namespace LiveScore.Providers.SportRadar.Dtos
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json;

    public class TournamentScheduleDto
    {
        public DateTime generated_at { get; set; }
        public string schema { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string message { get; set; }

        public Tournament tournament { get; set; }

        public IEnumerable<Sport_Event> sport_events { get; set; }
    }
}