namespace LiveScore.Features.Sports.Models.Soccers
{
    using LiveScore.Features.Teams.Models;

    public class SoccerTeamStatistic : ITeamStatistic
    {
        public int Possession { get; set; }

        public int FreeKicks { get; set; }

        public int ThrowIns { get; set; }

        public int GoalKicks { get; set; }

        public int ShotsBlocked { get; set; }

        public int ShotsOnTarget { get; set; }

        public int ShotsOffTarget { get; set; }

        public int CornerKicks { get; set; }

        public int Fouls { get; set; }

        public int ShotsSaved { get; set; }

        public int Offsides { get; set; }

        public int YellowCards { get; set; }

        public int Injuries { get; set; }
    }
}