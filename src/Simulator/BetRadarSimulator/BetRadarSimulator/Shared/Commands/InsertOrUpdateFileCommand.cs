namespace LiveScore.BetRadarSimulator.Api.Shared.Commands
{
    using Fanex.Data.Repository;

    public class InsertOrUpdateFileCommand : NonQueryCommand
    {
        public InsertOrUpdateFileCommand(string filePath, string evironment, string fileConent)
        {
            FilePath = filePath;
            Environment = evironment;
            FileConent = fileConent;
        }

        public string FilePath { get; }
        public string Environment { get; }
        public string FileConent { get; }

        public override string GetSettingKey()
            => "SimulatorFile_InsertOrUpdate";

        public override bool IsValid()
            => !string.IsNullOrWhiteSpace(FilePath) && !string.IsNullOrWhiteSpace(Environment);
    }
}