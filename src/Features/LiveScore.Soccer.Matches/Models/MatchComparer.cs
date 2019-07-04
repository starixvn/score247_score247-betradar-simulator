namespace LiveScore.Soccer.Matches.Models
{
    using System.Collections.Generic;

    public class MatchComparer : IEqualityComparer<IMatch>
    {
        public bool Equals(IMatch x, IMatch y)
        {
            return x.Id == y.Id;
        }

        public int GetHashCode(IMatch obj)
        {
            return obj == null ? 0 : obj.Id.GetHashCode();
        }
    }
}
