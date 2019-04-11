namespace LiveScore.Shared.Configurations
{
    public interface IAppSettings
    {
        string AppPath { get; }

        bool EnabledStaticData { get; }

        IDataProviderSettings DataProviderSettings { get; }
    }
}