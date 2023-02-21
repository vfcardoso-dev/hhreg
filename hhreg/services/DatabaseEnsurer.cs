using hhreg.business;
using Microsoft.Extensions.Logging;
using Spectre.Console;

namespace hhreg;

public interface IDatabaseEnsurer {
    void Ensure();
}

public class DatabaseEnsurer : IDatabaseEnsurer {

    private readonly IAppSettings _appSettings;
    private readonly IUnitOfWork _unitOfWork;

    public DatabaseEnsurer(
        IUnitOfWork unitOfWork, 
        IAppSettings appSettings) {
            _unitOfWork = unitOfWork;
            _appSettings = appSettings;
    }

    public void Ensure() {
        if (!Directory.Exists(_appSettings.AppDataFolder)) 
        {
            Directory.CreateDirectory(_appSettings.AppDataFolder);
        }
        
        if (!File.Exists(_appSettings.DatabaseFilePath)) 
        {
            var f = File.Create(_appSettings.DatabaseFilePath);
            AnsiConsole.MarkupLineInterpolated($"[darkblue]INFO:[/] Database file did not exists. Created on '[yellow]{_appSettings.DatabaseFilePath}[/]'.");
            f.Close();
        }

        var tableExists = _unitOfWork.QuerySingle<bool>(
            @"SELECT case when count(*) == 0 then false else true end 
            FROM sqlite_master WHERE type='table' AND name='Settings';");

        if (!tableExists) 
        {
            AnsiConsole.MarkupLineInterpolated($"[darkblue]INFO:[/] Ensuring database tables on '[yellow]{_appSettings.ConnectionString}[/]'...");

            var sql = $@"
                PRAGMA foreign_keys = ON;

                CREATE TABLE IF NOT EXISTS DayEntry (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Day TEXT NOT NULL UNIQUE,
                        DayType TEXT NOT NULL,
                        Justification TEXT NULL,
                        TotalMinutes INTEGER NOT NULL DEFAULT (0.0)
                    );
                
                CREATE TABLE IF NOT EXISTS TimeEntry (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Time TEXT NOT NULL,
                        DayEntryId INTEGER NOT NULL,
                        FOREIGN KEY(DayEntryId) REFERENCES DayEntry(Id)
                    );
                    
                CREATE TABLE IF NOT EXISTS Settings (
                        InitialBalance INTEGER NOT NULL,
                        WorkDay INTEGER NOT NULL
                    );";

            _unitOfWork.Execute(sql);
        }       
    }
}