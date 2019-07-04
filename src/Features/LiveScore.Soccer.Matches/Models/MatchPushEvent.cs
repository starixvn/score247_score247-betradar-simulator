namespace LiveScore.Soccer.Matches.Models
{    
    using System.Collections.Generic;    

    public class MatchPushEvent
    {        
        public MatchResult MatchResult { get; set; }
        
        public IEnumerable<Timeline> TimeLines { get; set; }
    }
}
