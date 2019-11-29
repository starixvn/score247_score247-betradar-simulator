namespace LiveScore.Providers.SportRadar.Dtos
{
    using System;
    using System.Collections.Generic;
    using LiveScore.Providers.SportRadar.Converters;
    using Newtonsoft.Json;

    public class TournamentScheduleDto
    {
        [JsonConverter(typeof(DateFormatConverter), "yyyy-MM-ddTHH:mm:sszzz")]
        public DateTime generated_at { get; set; }

        public string schema { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string message { get; set; }

        public Tournament tournament { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public IEnumerable<Sport_Event> sport_events { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public IEnumerable<Result> results { get; set; }
    }
}