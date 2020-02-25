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

        Task<RegionTournamentDto> GetRegionTournaments(string region);

        Task<TournamentScheduleDto> GetTournamentSchedule(string tournamentId);

        Task<TournamentScheduleDto> GetTournamentResult(string tournamentId);

        Task<TournamentInfoDto> GetTournamentInfo(string tournamentId);
    }

    public class TournamentService : ITournamentService
    {
        private readonly IFileService fileService;

        public TournamentService(IFileService fileService)
        {
            this.fileService = fileService;
        }

        public async Task<RegionTournamentDto> GetRegionTournaments(string region)
        {
            var path = $"{AppDomain.CurrentDomain.BaseDirectory}/App_Data/staticdata/{region}-tournaments.json";

            if (System.IO.File.Exists(path))
            {
                var jsonData = await System.IO.File.ReadAllTextAsync(path);

                return JsonConvert.DeserializeObject<RegionTournamentDto>(jsonData);
            }

            return new RegionTournamentDto();
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

        public async Task<TournamentScheduleDto> GetTournamentSchedule(string tournamentId)
        {
            var path = $"tournaments/schedule/{tournamentId}.json";

            var fileContent = await fileService.GetFile(path);
            if (string.IsNullOrWhiteSpace(fileContent))
            {
                return new TournamentScheduleDto { generated_at = DateTime.Now, message = "No schedule" };
            }
            else
            {
                var resultDto = JsonConvert.DeserializeObject<TournamentScheduleDto>(fileContent);

                resultDto.generated_at = DateTime.Now;

                return resultDto;
            }
        }

        public async Task<TournamentScheduleDto> GetTournamentResult(string tournamentId)
        {
            var path = $"{AppDomain.CurrentDomain.BaseDirectory}/App_Data/staticdata/tournament-result.json";

            if (!System.IO.File.Exists(path))
            {
                return new TournamentScheduleDto();
            }

            var jsonData = await System.IO.File.ReadAllTextAsync(path);

            var results = JsonConvert.DeserializeObject<TournamentScheduleDto>(jsonData);
            results.tournament.id = tournamentId;
            foreach (var result in results.results)
            {
                result.sport_event.tournament.id = tournamentId;
                result.sport_event.season.id = tournamentId;
            }

            return results;
        }

        public async Task<TournamentInfoDto> GetTournamentInfo(string tournamentId)
        {
            var path = $"tournaments/info/{tournamentId}.json";

            var fileContent = await fileService.GetFile(path);
            if (string.IsNullOrWhiteSpace(fileContent))
            {
                var staticPath = $"{AppDomain.CurrentDomain.BaseDirectory}/App_Data/staticdata/tournament-info.json";
                fileContent = await System.IO.File.ReadAllTextAsync(staticPath);
            }

            var resultDto = JsonConvert.DeserializeObject<TournamentInfoDto>(fileContent);

            resultDto.generated_at = DateTime.Now;

            return resultDto;
        }
    }
}