namespace LiveScore.Features.Odds.Models
{
    using System.Collections.Generic;
    using System.Linq;
    using LiveScore.Soccer.Models;

    public class BetTypeOdds : Entity<int, string>
    {
        public Bookmaker Bookmaker { get; set; }

        public IEnumerable<BetOptionOdds> BetOptions { get; set; }

        public void AssignOpeningData(IEnumerable<BetOptionOdds> openingBetOptions)
        {
            if(openingBetOptions.Count() != BetOptions.Count())
            {
                return;
            }

            var totalBetOptions = openingBetOptions.Count();

            for (int i = 0; i < totalBetOptions; i++)
            {
                BetOptions.ElementAt(i).AssginOpeningData(openingBetOptions.ElementAt(i));
            }
        }

        public override bool Equals(object obj)
        {
            var betTypeOdds = obj as BetTypeOdds;

#pragma warning disable S1067 // Expressions should not be too complex
            if (betTypeOdds == null
                || betTypeOdds.Bookmaker == null
                || Bookmaker == null
                || !betTypeOdds.Bookmaker.Equals(Bookmaker)
                || BetOptions == null
                || betTypeOdds.BetOptions == null
                || BetOptions.Count() != betTypeOdds.BetOptions.Count())
#pragma warning restore S1067 // Expressions should not be too complex
            {
                return false;
            }

            var totalOptions = BetOptions.Count();
            for (int i = 0; i < totalOptions; i++)
            {
                if(!BetOptions.ElementAt(i).Equals(betTypeOdds.BetOptions.ElementAt(i)))
                {
                    return false;
                }
            }

            return true;
        }

        public override int GetHashCode()
            => Id.GetHashCode() 
                + (Bookmaker == null ? 0 : Bookmaker.GetHashCode()) 
                + (BetOptions == null ? 0 : BetOptions.GetHashCode());
    }
}