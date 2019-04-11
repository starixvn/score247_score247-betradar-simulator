using LiveScore.Shared.Models;

namespace LiveScore.Features.Teams.Models
{
    public interface IPlayer : IEntity<int, string>
    {
        string Type { get; }

        int JerseyNumber { get; }

        string Position { get; }

        int Order { get; }
    }

    public class Player : Entity<int, string>, IPlayer
    {
        public string Type { get; set; }

        public int JerseyNumber { get; set; }

        public string Position { get; set; }

        public int Order { get; set; }
    }
}