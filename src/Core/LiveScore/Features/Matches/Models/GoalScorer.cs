using LiveScore.Shared.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace LiveScore.Features.Matches.Models
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
