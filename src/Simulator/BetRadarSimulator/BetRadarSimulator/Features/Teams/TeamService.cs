using System;
using System.IO;
using System.Threading.Tasks;
using LiveScore.Providers.SportRadar.Dtos;
using Newtonsoft.Json;

namespace LiveScore.BetRadarSimulator.Api.Features.Teams
{
    public interface ITeamService
    {
        Task<H2HDto> GetH2H(string path);
    }

    public class TeamService : ITeamService
    {
        public async Task<H2HDto> GetH2H(string path)
        {
            if (File.Exists(path))
            {
                var jsonData = await File.ReadAllTextAsync(path);
                var h2HDto = JsonConvert.DeserializeObject<H2HDto>(jsonData);
                h2HDto.generated_at = DateTime.Now;

                return h2HDto;
            }
            else
            {
                return new H2HDto { generated_at = DateTime.Now, schema = "", message = "No data." };
            }
        }
    }
}