using System.Reflection;
using hhreg.business;
using hhreg.business.infrastructure;
using hhreg.resources;
using Spectre.Console;

namespace hhreg.services;

public interface IDatabaseEnsurer {
    void Ensure();
}

public class DatabaseEnsurer : IDatabaseEnsurer {

    private readonly IDbSettings _dbSettings;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILocalizer _localizer;

    public DatabaseEnsurer(
        IUnitOfWork unitOfWork, 
        IDbSettings dbSettings,
        ILocalizer localizer) {
            _unitOfWork = unitOfWork;
            _dbSettings = dbSettings;
            _localizer = localizer;
    }

    public void Ensure() {
        if (!Directory.Exists(_dbSettings.AppDataFolder)) 
        {
            Directory.CreateDirectory(_dbSettings.AppDataFolder);
        }
        
        if (!File.Exists(_dbSettings.DatabaseFilePath)) 
        {
            var f = File.Create(_dbSettings.DatabaseFilePath);
            AnsiConsole.MarkupLineInterpolated($"[darkblue]INFO:[/] {_localizer.Get("DatabaseFileDidNotExists")} '[yellow]{_dbSettings.DatabaseFilePath}[/]'.");
            f.Close();
        }

        var tableExists = _unitOfWork.QuerySingle<bool>(
            @"SELECT 
                case when count(*) == 0 then false else true end 
              FROM sqlite_master 
              WHERE type='table' AND name='Settings';");

        if (!tableExists) 
        {
            AnsiConsole.MarkupLineInterpolated($"[darkblue]INFO:[/] {_localizer.Get("EnsuringDatabaseTablesExistsOn")} '[yellow]{_dbSettings.ConnectionString}[/]'...");

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
                    );
                    
                CREATE TABLE IF NOT EXISTS Settings (
                        InitialBalance INTEGER NOT NULL,
                        WorkDay INTEGER NOT NULL,
                        StartCalculationsAt TEXT NOT NULL
                    );";

            _unitOfWork.Execute(sql);
        }       
    }
}