namespace LiveScore.Features.Matches.Models
{
    using LiveScore.Soccer.Shared.Models;

    public interface IMatchFunction : IEntity<string, string>
    {
        string Abbreviation { get; }
    }

    public class MatchFunction : Entity<string, string>, IMatchFunction
    {
        public string Abbreviation { get; set; }
    }
}