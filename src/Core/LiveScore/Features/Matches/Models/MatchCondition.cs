namespace LiveScore.Features.Matches.Models
{
    public interface IMatchCondition
    {
        int Attendance { get; }

        IVenue Venue { get; }

        string Referee { get; }
    }

    public class MatchCondition : IMatchCondition
    {
        public int Attendance { get; set; }

        public IVenue Venue { get; set; }

        public string Referee { get; set; }
    }
}