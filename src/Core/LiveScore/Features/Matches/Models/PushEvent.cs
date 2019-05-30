namespace LiveScore.Features.Matches.Models
{
    using System.Collections.Generic;
    using LiveScore.Shared.Helpers;
    using Newtonsoft.Json;

    public class PushEvent
    {
        [JsonConverter(typeof(JsonConcreteTypeConverter<MatchResult>))]
        public IMatchResult MatchResult { get; set; }

        [JsonConverter(typeof(JsonConcreteTypeConverter<IEnumerable<Timeline>>))]
        public IEnumerable<ITimeline> TimeLines { get; set; }
    }
}
