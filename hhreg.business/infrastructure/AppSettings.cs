public class AppSettings 
{
    public string DatabaseFile { get; set; } = "hhreg.db";
    public string ConnectionString => $"Data Source={DatabaseFile}";
}