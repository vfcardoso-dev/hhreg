public class AppSettings 
{
    public string DatabaseName { get; set; } = "hhreg";
    public string AppDataFolder => Environment.ExpandEnvironmentVariables(@"%LOCALAPPDATA%\hhreg");
    public string DatabaseFilePath => $@"{AppDataFolder}\{DatabaseName}.db";
    public string ConnectionString => $@"Data Source={DatabaseFilePath}";
}