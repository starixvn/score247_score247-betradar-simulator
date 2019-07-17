namespace LiveScore.Features.Odds.Models
{
    using System;
    using LiveScore.Soccer.Models;

    public class BetOptionOdds : Entity<int, string>
    {
        public string Type { get; set; }

        public decimal LiveOdds { get; set; }

        public decimal OpeningOdds { get; set; }

        public string OptionValue { get; set; }

        public string OpeningOptionValue { get; set; }

        public OddsTrend OddsTrend
            => OpeningOdds == LiveOdds
                ? OddsTrend.Neutral
#pragma warning disable S3358 // Ternary operators should not be nested
                : OpeningOdds > LiveOdds
                    ? OddsTrend.Down
                    : OddsTrend.Up;
#pragma warning restore S3358 // Ternary operators should not be nested

        public DateTime LastUpdatedTime { get; set; }

        public void AssginOpeningData(BetOptionOdds openingBetOption)
        {
            if(openingBetOption == null)
            {
                return;
            }

            OpeningOptionValue = string.IsNullOrWhiteSpace(openingBetOption.OpeningOptionValue) 
                ? openingBetOption.OptionValue
                : openingBetOption.OpeningOptionValue;
            OpeningOdds = openingBetOption.OpeningOdds == 0 
                    ? openingBetOption.LiveOdds
                    : openingBetOption.OpeningOdds;
        }

        public override bool Equals(object obj)
        {
            var betOptionOdds = obj as BetOptionOdds;
            if(betOptionOdds == null)
            {
                return false;
            }

            return Type == betOptionOdds.Type
                && OptionValue == betOptionOdds.OptionValue
                && LiveOdds == betOptionOdds.LiveOdds
                && OpeningOdds == betOptionOdds.OpeningOdds;
        }

        public override int GetHashCode()
            => (Type ?? string.Empty).GetHashCode()
                + (OptionValue ?? string.Empty).GetHashCode()
                + LiveOdds.GetHashCode()
                + OpeningOdds.GetHashCode();
    }
}