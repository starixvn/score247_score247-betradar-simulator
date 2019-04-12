namespace LiveScore.Features.Matches.Models
{
    using LiveScore.Shared.Models;

    public class PeriodType : Enumeration
    {
        public static readonly PeriodType RegularPeriod = new PeriodType("regular_period", nameof(RegularPeriod));
        public static readonly PeriodType Overtime = new PeriodType("overtime", nameof(Overtime));
        public static readonly PeriodType Penalties = new PeriodType("penalties", nameof(Penalties));

        public PeriodType()
        {
        }

        public PeriodType(string value, string displayName)
            : base(value, displayName)
        {
        }

        public PeriodType(string value)
            : base(value, value)
        {
        }
    }
}