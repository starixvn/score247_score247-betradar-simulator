namespace LiveScore.BetRadarSimulator.Api.Shared
{
    using System.Threading.Tasks;
    using Fanex.Data.Repository;
    using LiveScore.BetRadarSimulator.Api.Shared.Commands;
    using LiveScore.BetRadarSimulator.Api.Shared.Criterias;
    using Microsoft.Extensions.Configuration;

    public interface IFileService
    {
        Task<int> SaveFile(string path, string value);

        Task<string> GetFile(string path);

        Task CleanMatches();
    }

    public class FileService : IFileService
    {
        private readonly IDynamicRepository repository;
        private readonly string enviroment;

        public FileService(IConfiguration configuration)
        {
            enviroment = configuration.GetValue<string>("DevelopmentEnvironment");
            repository = new DynamicRepository();
        }

        public Task CleanMatches()
        {
            var command = new CleanScheduleCommand();

            return repository.ExecuteAsync(command);
        }

        public Task<string> GetFile(string path)
        {
            var criteria = new GetFileContentCriteria(path, enviroment);

            return repository.GetAsync<string>(criteria);
        }

        public Task<int> SaveFile(string path, string value)
        {
            var command = new InsertOrUpdateFileCommand(path, enviroment, value);

            return repository.ExecuteAsync(command);
        }
    }
}