using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace LiveScore.Providers.SportRadar.Dtos
{
    public class LineUpsDto
    {
        public DateTime generated_at { get; set; }
        public string schema { get; set; }
        public Sport_Event sport_event { get; set; }
        public IEnumerable<LineUp> lineups { get; set; }
    }

    public class LineUp
    {
        public string team { get; set; }
        public string formation { get; set; }
        public Manager manager { get; set; }
        public Jersey jersey { get; set; }
        public IEnumerable<LineupPlayer> starting_lineup { get; set; }
        public IEnumerable<LineupPlayer> substitutes { get; set; }
    }

    public class Manager
    {
        public string id { get; set; }
        public string name { get; set; }
        public string nationality { get; set; }
        public string country_code { get; set; }
    }

    public class Jersey
    {
        public string type { get; set; }

        [JsonProperty("base")]
        public string jersey_base { get; set; }

        public string sleeve { get; set; }
        public string number { get; set; }
        public string squares { get; set; }
        public string stripes { get; set; }
        public string stripes_color { get; set; }
        public string horizontal_stripes { get; set; }
        public string split { get; set; }
        public string shirt_type { get; set; }
        public string sleeve_detail { get; set; }
    }

    public class LineupPlayer
    {
        public string id { get; set; }
        public string name { get; set; }
        public string type { get; set; }
        public string jersey_number { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string position { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? order { get; set; }
    }
}