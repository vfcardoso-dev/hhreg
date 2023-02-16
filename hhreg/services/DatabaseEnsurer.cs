using hhreg.business;
using Microsoft.Extensions.Logging;

namespace hhreg;

public interface IDatabaseEnsurer {
    void Ensure();
}

public class DatabaseEnsurer : IDatabaseEnsurer {

    private readonly AppSettings _appSettings;
    private readonly ILogger<DatabaseEnsurer> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public DatabaseEnsurer(
        IUnitOfWork unitOfWork, 
        ILoggerFactory loggerFactory, 
        AppSettings appSettings) {
            _unitOfWork = unitOfWork;
            _logger = loggerFactory.CreateLogger<DatabaseEnsurer>();
            _appSettings = appSettings;
    }

    public void Ensure() {
        if (!File.Exists(_appSettings.DatabaseFile)) {
            File.Create(_appSettings.DatabaseFile);
            _logger.LogInformation("Arquivo de banco de dados não existia. Criado em '{databaseFile}'", _appSettings.DatabaseFile);
        }

        var tableExists = _unitOfWork.QuerySingle<bool>(
            @"SELECT case when count(*) == 0 then false else true end 
            FROM sqlite_master WHERE type='table' AND name='Settings';");

        if (!tableExists) {
            _logger.LogInformation("Garantindo tabelas do banco de dados em '{connectionString}'", _appSettings.ConnectionString);

            var sql = $@"
                PRAGMA foreign_keys = ON;

                CREATE TABLE IF NOT EXISTS DayEntry (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Day TEXT NOT NULL UNIQUE,
                        Justification TEXT NULL
                    );
                
                CREATE TABLE IF NOT EXISTS TimeEntry (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Time TEXT NOT NULL,
                        DayEntryId INTEGER NOT NULL,
                        FOREIGN KEY(DayEntryId) REFERENCES DayEntry(Id)
                    );
                    
                CREATE TABLE IF NOT EXISTS Settings (
                        InitialBalance DOUBLE NOT NULL,
                        WorkDay DOUBLE NOT NULL,
                        LunchTime DOUBLE NOT NULL
                    );";

            _unitOfWork.Execute(sql);
        }       
    }
}