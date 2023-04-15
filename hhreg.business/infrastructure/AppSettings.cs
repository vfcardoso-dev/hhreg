using static System.Environment;

namespace hhreg.business.infrastructure;

public interface IAppSettings {
    string DatabaseName { get; }
    string AppDataFolder { get; }
    string DatabaseFilePath { get; }
    string ConnectionString { get; }
}

public class AppSettings : IAppSettings
{
    private readonly string _appName = "hhreg";

    public string DatabaseName { get; set; } = "hhreg";
    public string AppDataFolder => GetAppDataFolder();
    public string DatabaseFilePath => $@"{AppDataFolder}\{DatabaseName}.db";
    public string ConnectionString => $@"Data Source={DatabaseFilePath}";

    private string GetAppDataFolder()
    {
        var folderPath = Environment.GetFolderPath(SpecialFolder.LocalApplicationData, SpecialFolderOption.DoNotVerify);
        return Path.Combine(folderPath, _appName);
    }
}