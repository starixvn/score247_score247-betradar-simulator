namespace LiveScore.Features.Teams.Models
{
    public interface ITeamStatistic
    {
        int Possession { get; }

        int Fouls { get; }

        int Injuries { get; }
    }
}