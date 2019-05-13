namespace LiveScore.Soccers.Features.Leagues.DataProviders
{
    using LiveScore.Shared.Models.Dtos;
    using System.Threading.Tasks;

    public interface ILeagueApi
    {
        Task<Tournaments> GetLeagues(
            int sportId,
            string language);
    }
}
