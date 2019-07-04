using LiveScore.Soccer.Models;

namespace LiveScore.Soccer.Matches.Models
{
    public interface IGoalScorer : IEntity<string, string>
    {
        string Method { get; }
    }

    public class GoalScorer : Entity<string, string>, IGoalScorer
    {
        public string Method { get; set; }
    }
}
