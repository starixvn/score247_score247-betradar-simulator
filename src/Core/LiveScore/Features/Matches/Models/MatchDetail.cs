namespace LiveScore.Features.Matches.Models
{
    public interface IMatchDetail
    {
        ITimeLine TimeLine { get; }

        int Attendance { get; }

        IVenue Venue { get; }

        string Referee { get; }
    }

    public class MatchDetail : Match, IMatchDetail
    {        
        public ITimeLine TimeLine { get; set; }

        public int Attendance { get; set; }

        public IVenue Venue { get; set; }

        public string Referee { get; set; }
    }
}
