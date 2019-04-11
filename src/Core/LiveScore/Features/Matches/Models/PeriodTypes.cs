namespace LiveScore.Features.Matches.Models
{
    using LiveScore.Shared.Models;

    public class PeriodTypes : Enumeration
    {
        public static readonly PeriodTypes RegularPeriod = new PeriodTypes("regular_period", nameof(RegularPeriod));
        public static readonly PeriodTypes Overtime = new PeriodTypes("overtime", nameof(Overtime));
        public static readonly PeriodTypes Penalties = new PeriodTypes("penalties", nameof(Penalties));

        public PeriodTypes()
        {
        }

        public PeriodTypes(string value, string displayName)
            : base(value, displayName)
        {
        }

        public PeriodTypes(string value)
            : base(value, value)
        {
        }
    }
}