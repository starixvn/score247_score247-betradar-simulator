namespace LiveScore.Features.Teams.Models
{
    using LiveScore.Shared.Models;

    public interface ICoach : IEntity<int, string>
    {
        string Nationality { get; }

        string CountryCode { get; }
    }

    public class Coach : Entity<int, string>, ICoach
    {
        public string Nationality { get; set; }

        public string CountryCode { get; set; }
    }
}