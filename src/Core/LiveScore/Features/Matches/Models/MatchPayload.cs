namespace LiveScore.Features.Matches.Models
{
    using LiveScore.Shared.Helpers;
    using Newtonsoft.Json;
    using System.Collections.Generic;
   
    public class MatchPayload 
    {   
        [JsonConverter(typeof(JsonConcreteTypeConverter<MatchResult>))]
        public IMatchResult MatchResult { get; set; }

        [JsonConverter(typeof(JsonConcreteTypeConverter<IEnumerable<TimeLine>>))]
        public IEnumerable<ITimeline> TimeLines { get; set; }
    }
}
