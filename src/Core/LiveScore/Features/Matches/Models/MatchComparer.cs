namespace LiveScore.Features.Matches.Models
{
    using System.Collections.Generic;

    public class MatchComparer : IEqualityComparer<IMatch>
    {
        public bool Equals(IMatch x, IMatch y)
        {
            return x.Id == y.Id;
        }

        public int GetHashCode(IMatch match)
        {
            return match.Id.GetHashCode();
        }
    }
}
