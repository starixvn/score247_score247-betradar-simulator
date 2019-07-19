namespace LiveScore.Soccer.Odds.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using LiveScore.Soccer.Models;

    public class OddsMovement : Entity<int, string>
    {
        public IEnumerable<BetOptionOdds> BetOptions { get; set; }

        public string MatchTime { get; set; }

        public int HomeScore { get; set; }

        public int AwayScore { get; set; }

        public bool IsMatchStarted { get; set; }

        public DateTimeOffset UpdateTime { get; set; }

        public void CalculateOddsTrend(IEnumerable<BetOptionOdds> prevBetOptions)
        {
            if (prevBetOptions.Count() != BetOptions.Count())
            {
                return;
            }

            var totalBetOptions = prevBetOptions.Count();

            for (int i = 0; i < totalBetOptions; i++)
            {
                BetOptions.ElementAt(i).ReCalculateOddsTrend(prevBetOptions.ElementAt(i).LiveOdds);
            }
        }

        public void ResetToOpeningOdds()
        {
            foreach (var betOption in BetOptions)
            {
                betOption.ResetOpeningOdds();
            }
        }
    }
}