using LiveScore.Providers.SportRadar.Converters;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace LiveScore.Providers.SportRadar.Dtos
{
    public class TournamentInfoDto
    {
        [JsonConverter(typeof(DateFormatConverter), "yyyy-MM-ddTHH:mm:sszzz")]
        public DateTime generated_at { get; set; }

        public string schema { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public Tournament tournament { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public Season season { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public Season_Coverage_Info season_coverage_info { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public TournamentRound round { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public CoverageInfoDto coverage_info { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public IEnumerable<GroupDto> groups { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string message { get; set; }
    }

    public class GroupDto
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string id { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string name { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public IEnumerable<Competitor> teams { get; set; }
    }
}