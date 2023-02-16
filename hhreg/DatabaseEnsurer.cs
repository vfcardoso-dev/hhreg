using Dapper;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace hhreg;

public interface IDatabaseEnsurer {
    void Ensure();
}

public class DatabaseEnsurer : IDatabaseEnsurer {

    private readonly IConfiguration _config;
    private readonly ILogger<DatabaseEnsurer> _logger;

    public DatabaseEnsurer(IConfiguration config, ILoggerFactory loggerFactory) {
        _logger = loggerFactory.CreateLogger<DatabaseEnsurer>();
        _config = config;
    }

    public void Ensure() {
        var connectionString = _config.GetValue<string>("ConnectionStrings:Default") ?? "Data Source=hhreg.db";

        _logger.LogInformation("Ensuring database exists at connection string '{connectionString}'", connectionString);

        using var db = new SqliteConnection(connectionString);
        
        var sql = $@"
                    PRAGMA foreign_keys = ON;

                    CREATE TABLE IF NOT EXISTS DayEntry (
                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                            Date TEXT NOT NULL UNIQUE,
                            Justification TEXT NULL
                        );
                 
                    CREATE TABLE IF NOT EXISTS TimeEntry (
                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                            Time TEXT NOT NULL,
                            DayEntryId INTEGER NOT NULL,
                            FOREIGN KEY(DayEntryId) REFERENCES DayEntry(Id)
                        );
                        
                    CREATE TABLE IF NOT EXISTS Settings (
                            InitialBalance INTEGER NOT NULL,
                            WorkDay INTEGER NOT NULL,
                            LunchTime INTEGER NOT NULL
                        );";

        db.Execute(sql);
    }
}