namespace LiveScore.Features.Leagues.Models
{
    using LiveScore.Shared.Models;

    public class LeagueRoundTypes : Enumeration
    {
        public const string Cup = "cup";
        public const string Group = "group";
        public const string PlayOff = "playoff";
        public const string Qualifier = "qualification";
        public const string Variable = "variable";

        public static readonly LeagueRoundTypes CupRound = new LeagueRoundTypes(Cup, nameof(Cup));
        public static readonly LeagueRoundTypes GroupRound = new LeagueRoundTypes(Group, nameof(Group));
        public static readonly LeagueRoundTypes PlayOffRound = new LeagueRoundTypes(PlayOff, nameof(PlayOff));
        public static readonly LeagueRoundTypes QualifierRound = new LeagueRoundTypes(Qualifier, nameof(Qualifier));
        public static readonly LeagueRoundTypes VariableRound = new LeagueRoundTypes(Variable, nameof(Variable));

        public LeagueRoundTypes()
        {
        }

        public LeagueRoundTypes(string value, string displayName)
            : base(value, displayName)
        {
        }
    }
}