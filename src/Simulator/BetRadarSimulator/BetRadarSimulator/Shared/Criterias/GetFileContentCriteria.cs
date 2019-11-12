namespace LiveScore.BetRadarSimulator.Api.Shared.Criterias
{
    using System;
    using Fanex.Data.Repository;

    public class GetFileContentCriteria : CriteriaBase
    {
        public GetFileContentCriteria(string filePath, string environment)
        {
            FilePath = filePath;
            Environment = environment;
        }

        public string FilePath { get; }
        public string Environment { get; }

        public override string GetSettingKey()
            => "SimulatorFile_GetFile";

        public override bool IsValid()
            => !string.IsNullOrWhiteSpace(FilePath) && !string.IsNullOrWhiteSpace(Environment);
    }
}