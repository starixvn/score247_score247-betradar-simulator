namespace LiveScore.Providers.SportRadar.Dtos
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json;

    public class TeamResultDto
    {
        public DateTime generated_at { get; set; }
        public string schema { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public Competitor team { get; private set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public IEnumerable<Result> results { get; private set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string message { get; set; }
    }
}