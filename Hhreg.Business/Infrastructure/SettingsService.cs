using Hhreg.Business.Domain;
using Hhreg.Business.Exceptions;
using System.Text.Json;
using static System.Environment;

namespace Hhreg.Business.Infrastructure;

public interface ISettingsService
{
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
    private readonly string _appDataFolder = GetFolderPath(SpecialFolder.LocalApplicationData, SpecialFolderOption.DoNotVerify);
    private readonly string _settingsFilename = "settings.json";
    private string _settingsFilePath => $@"{AppDataFolder}\{_settingsFilename}";

    public string AppName => _appName;
    public string DatabaseName => _appName;
    public string AppDataFolder => $@"{_appDataFolder}\{_appName}";
    public string DatabaseFilePath => $@"{AppDataFolder}\{DatabaseName}.db";
    public string ConnectionString => $@"Data Source={DatabaseFilePath}";

    public Settings GetSettings()
    {
        if (!IsInitialized()) throw new HhregException(HhregMessages.SettingsNotYetInitialized);

        using (var sr = new StreamReader(_settingsFilePath))
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