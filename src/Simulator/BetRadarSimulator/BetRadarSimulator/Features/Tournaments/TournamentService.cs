using System;
using System.Threading.Tasks;
using LiveScore.BetRadarSimulator.Api.Shared;
using LiveScore.Providers.SportRadar.Dtos;
using Newtonsoft.Json;

namespace LiveScore.BetRadarSimulator.Api.Features.Tournaments
{
    public interface ITournamentService
    {
        Task<TournamentStandingDto> GetTournamentStandings(string path);
    }

    public class TournamentService : ITournamentService
    {
        private readonly IFileService fileService;

        public TournamentService(IFileService fileService)
        {
            this.fileService = fileService;
        }

        public async Task<TournamentStandingDto> GetTournamentStandings(string path)
        {
            var fileContent = await fileService.GetFile(path);
            if (string.IsNullOrWhiteSpace(fileContent))
            {
                return new TournamentStandingDto { generated_at = DateTime.Now, schema = "", message = "No live standings for this tournament." };
            }
            else
            {
                var resultDto = JsonConvert.DeserializeObject<TournamentStandingDto>(fileContent);
                resultDto.generated_at = DateTime.Now;

                return resultDto;
            }
        }
    }
}