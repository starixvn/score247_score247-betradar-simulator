namespace LiveScore.Soccer.Matches.Models
{
    using LiveScore.Soccer.Shared.Models;

    public class MatchFunction : Entity<string, string>
    {
        public string Abbreviation { get; set; }
    }
}