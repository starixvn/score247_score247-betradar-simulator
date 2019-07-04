namespace LiveScore.Soccer.Matches.Models
{
    using LiveScore.Soccer.Shared.Models;

    public class Venue : Entity<string, string>
    {
        public int Capacity { get; set; }

        public string CityName { get; set; }

        public string CountryName { get; set; }

        public string CountryCode { get; set; }
    }
}