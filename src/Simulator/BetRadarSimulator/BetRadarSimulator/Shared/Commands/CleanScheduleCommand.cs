namespace LiveScore.BetRadarSimulator.Api.Shared.Commands
{
    using Fanex.Data.Repository;

    public class CleanScheduleCommand : NonQueryCommand
    {
        public string Environment { get; set; }

        public override string GetSettingKey()
            => "SimulatorFile_CleanFiles";

        public override bool IsValid()
            => true;
    }
}