using hhreg.business.infrastructure;
using Spectre.Console;

namespace hhreg.services;

public interface IDatabaseEnsurer {
    void Ensure();
}

public class DatabaseEnsurer : IDatabaseEnsurer {

    private readonly ISettingsService _settingsService;
    private readonly IUnitOfWork _unitOfWork;

    public DatabaseEnsurer(
        IUnitOfWork unitOfWork, 
        ISettingsService settingsService) {
            _unitOfWork = unitOfWork;
            _settingsService = settingsService;
    }

    public void Ensure() {
        if (!Directory.Exists(_settingsService.AppDataFolder)) 
        {
            Directory.CreateDirectory(_settingsService.AppDataFolder);
        }
        
        if (!File.Exists(_settingsService.DatabaseFilePath)) 
        {
            var f = File.Create(_settingsService.DatabaseFilePath);
            AnsiConsole.MarkupLineInterpolated($"[darkblue]INFO:[/] Arquivo de banco de dados não existe. '[yellow]{_dbSettings.DatabaseFilePath}[/]'.");
            f.Close();
        }

        var tableExists = _unitOfWork.QuerySingle<bool>(
            @"SELECT 
                case when count(*) == 0 then false else true end 
              FROM sqlite_master 
              WHERE type='table' AND name='Settings';");

        if (!tableExists) 
        {
            AnsiConsole.MarkupLineInterpolated($"[darkblue]INFO:[/] Garantindo que tabelas existem em '[yellow]{_dbSettings.ConnectionString}[/]'...");

            var sql = $@"
                PRAGMA foreign_keys = ON;

                CREATE TABLE IF NOT EXISTS DayEntry (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Day TEXT NOT NULL UNIQUE,
                        DayType TEXT NOT NULL,
                        Justification TEXT NULL,
                        TotalMinutes INTEGER NOT NULL DEFAULT (0)
                    );
                
                CREATE TABLE IF NOT EXISTS TimeEntry (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Time TEXT NOT NULL,
                        DayEntryId INTEGER NOT NULL,
                        FOREIGN KEY(DayEntryId) REFERENCES DayEntry(Id)
                    );";

            _unitOfWork.Execute(sql);
        }       
    }
}