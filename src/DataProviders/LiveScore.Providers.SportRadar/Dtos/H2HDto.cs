namespace LiveScore.Providers.SportRadar.Dtos
{
    using System;
    using System.Collections.Generic;
    using LiveScore.Providers.SportRadar.Converters;
    using Newtonsoft.Json;

    public class H2HDto
    {
        [JsonConverter(typeof(DateFormatConverter), "yyyy-MM-ddTHH:mm:sszzz")]
        public DateTime generated_at { get; set; }

        public string schema { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public IEnumerable<Competitor> teams { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public LastMeetings last_meetings { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public IEnumerable<Result> next_meetings { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string message { get; set; }
    }

    public class LastMeetings
    {
        public IEnumerable<Result> results { get; set; }
    }
}