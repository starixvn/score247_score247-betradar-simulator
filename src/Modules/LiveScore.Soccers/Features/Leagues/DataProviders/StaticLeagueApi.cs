namespace LiveScore.Soccers.Features.Leagues.DataProviders
{
    using System.IO;
    using System.Threading.Tasks;
    using LiveScore.Shared.Configurations;
    using LiveScore.Shared.Models.Dtos;
    using Newtonsoft.Json;

    public class StaticLeagueApi : ILeagueApi
    {        
        private readonly IAppSettings appSettings;

        public StaticLeagueApi(IAppSettings appSettings)
        {
            this.appSettings = appSettings;
        }

        public Task<Tournaments> GetLeagues(int sportId, string language)
        {
            // Temporary for hardcode, please ignore it
            var jsonData = File.ReadAllText($"{appSettings.AppPath}/App_Data/staticdata/tournaments.json");
            var leaguesDto = JsonConvert.DeserializeObject<Tournaments>(jsonData);

            return Task.FromResult(leaguesDto);
        }
    }
}
