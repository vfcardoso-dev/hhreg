using hhreg.business.domain;
using hhreg.business.exceptions;
using System.Text.Json;
using static System.Environment;

namespace hhreg.business.infrastructure;

public interface ISettingsService {
    string AppName { get; }
    string AppDataFolder { get; }
    string DatabaseName { get; }
    string DatabaseFilePath { get; }
    string ConnectionString { get; }
    Settings GetSettings();
    void SaveSettings(Settings settings);
    bool IsInitialized();
}

public class SettingsService : ISettingsService
{
    private readonly string _appName = "hhreg";
    private readonly string _appDataFolder = Environment.GetFolderPath(SpecialFolder.LocalApplicationData, SpecialFolderOption.DoNotVerify);
    private readonly string _settingsFilename = "settings.json";
    private string _settingsFilePath => $@"{_appDataFolder}\{_settingsFilename}.db";

    public string AppName => _appName;
    public string DatabaseName => _appName;
    public string AppDataFolder => _appDataFolder;
    public string DatabaseFilePath => $@"{_appDataFolder}\{DatabaseName}.db";
    public string ConnectionString => $@"Data Source={DatabaseFilePath}";

    public Settings GetSettings()
    {
        if (!IsInitialized()) throw new HhregException(HhregMessages.SettingsNotYetInitialized);

        using(var sr = new StreamReader(_settingsFilePath))
        {
            var rawSettings = sr.ReadToEnd();
            var settings = JsonSerializer.Deserialize<Settings>(rawSettings);
            return settings!;
        }
    }
    public void SaveSettings(Settings settings)
    {
        var rawSettings = JsonSerializer.Serialize(settings);
        using (var outputFile = new StreamWriter(_settingsFilePath, false))
        {
            outputFile.Write(rawSettings);
        }
    }

    public bool IsInitialized()
    {
        return File.Exists(_settingsFilePath) && new FileInfo(_settingsFilePath).Length > 0;
    }
}