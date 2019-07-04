namespace LiveScore.Soccer.Matches.Models
{
    using LiveScore.Soccer.Models;

    public class MatchFunction : Entity<string, string>
    {
        public string Abbreviation { get; set; }
    }
}