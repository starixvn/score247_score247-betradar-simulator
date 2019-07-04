namespace LiveScore.Soccer.Matches.Models
{
    using System.Collections.Generic;

    public class MatchComparer : IEqualityComparer<Match>
    {
        public bool Equals(Match x, Match y)
        {
            return x.Id == y.Id;
        }

        public int GetHashCode(Match obj)
        {
            return obj == null ? 0 : obj.Id.GetHashCode();
        }
    }
}
