namespace LiveScore.Features.Odds.Models
{
    using LiveScore.Soccer.Shared.Models;

    public class OddsTrend : Enumeration
    {
        public static readonly OddsTrend Neutral = new OddsTrend("neutral", nameof(Neutral));
        public static readonly OddsTrend Up = new OddsTrend("up", nameof(Up));
        public static readonly OddsTrend Down = new OddsTrend("down", nameof(Down));

        public OddsTrend()
        {
        }

        public OddsTrend(string value, string displayName)
            : base(value, displayName)
        {
        }

        public OddsTrend(string value)
            : base(value, value)
        {
        }
    }
}