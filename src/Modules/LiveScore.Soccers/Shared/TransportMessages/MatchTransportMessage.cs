namespace LiveScore.Soccers.Shared.TransportMessages
{
    using LiveScore.Features.Matches.Models;
    using System.Collections.Generic;

    public class MatchTransportMessage
    {
        public int SportId { get; set; }

        public IEnumerable<Dictionary<string, MatchEvent>> MatchEvents { get; set; }
    }

    public class MatchEvent
    {
        public MatchResult MatchResult { get; set; }

        public IEnumerable<TimeLine> TimeLines { get; set; }
    }
}
