using System;
using System.IO;
using System.Threading.Tasks;
using LiveScore.BetRadarSimulator.Api.Shared;
using LiveScore.Providers.SportRadar.Dtos;
using Newtonsoft.Json;

namespace LiveScore.BetRadarSimulator.Api.Features.Teams
{
    public interface ITeamService
    {
        Task<H2HDto> GetH2H(string path);

        Task<TeamResultDto> GetResults(string path);
    }

    public class TeamService : ITeamService
    {
        private readonly IFileService fileService;

        public TeamService(IFileService fileService)
        {
            this.fileService = fileService;
        }

        public async Task<H2HDto> GetH2H(string path)
        {
            var fileContent = await fileService.GetFile(path);
            if (string.IsNullOrWhiteSpace(fileContent))
            {
                return new H2HDto { generated_at = DateTime.Now, schema = "", message = "No data." };
            }
            else
            {
                var h2HDto = JsonConvert.DeserializeObject<H2HDto>(fileContent, new JsonSerializerSettings { Error = HandleDeserializationError });
                h2HDto.generated_at = DateTime.Now;

                return h2HDto;
            }
        }

        public async Task<TeamResultDto> GetResults(string path)
        {
            var fileContent = await fileService.GetFile(path);
            if (string.IsNullOrWhiteSpace(fileContent))
            {
                return new TeamResultDto { generated_at = DateTime.Now, schema = "", message = "No data." };
            }
            else
            {
                var resultDto = JsonConvert.DeserializeObject<TeamResultDto>(fileContent);
                resultDto.generated_at = DateTime.Now;

                return resultDto;
            }
        }

        private void HandleDeserializationError(object sender, Newtonsoft.Json.Serialization.ErrorEventArgs errorArgs)
        {
            var currentError = errorArgs.ErrorContext.Error.Message;
            errorArgs.ErrorContext.Handled = true;
        }
    }
}