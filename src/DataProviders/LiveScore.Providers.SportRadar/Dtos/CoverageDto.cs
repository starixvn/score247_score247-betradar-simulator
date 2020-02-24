using Newtonsoft.Json;

namespace LiveScore.Providers.SportRadar.Dtos
{
    public class CoverageDto
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool basic_score { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool key_events { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool detailed_events { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool lineups { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool commentary { get; set; }
    }

    public class CoverageInfoDto
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string level { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool live_coverage { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public CoverageDto coverage { get; set; }
    }
}