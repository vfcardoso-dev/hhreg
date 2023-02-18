namespace hhreg.business;

public interface IAppSettings {
    string DatabaseName { get; }
    string AppDataFolder { get; }
    string DatabaseFilePath { get; }
    string ConnectionString { get; }
}

public class AppSettings : IAppSettings
{
    public string DatabaseName { get; set; } = "hhreg";
    public string AppDataFolder => Environment.ExpandEnvironmentVariables(@"%LOCALAPPDATA%\hhreg");
    public string DatabaseFilePath => $@"{AppDataFolder}\{DatabaseName}.db";
    public string ConnectionString => $@"Data Source={DatabaseFilePath}";
}