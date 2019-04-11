using LiveScore.Shared.Models;

namespace LiveScore.Features.Matches.Models
{
    public interface IVenue : IEntity<string, string>
    {
        int Capacity { get; }

        string CityName { get; }

        string CountryName { get; }
    }

    public class Venue : Entity<string, string>, IVenue
    {
        public int Capacity { get; set; }

        public string CityName { get; set; }

        public string CountryName { get; set; }
    }
}