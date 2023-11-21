using Hhreg.Business.Infrastructure;
using Hhreg.Business.Domain.Dtos;
using Spectre.Console;
using Hhreg.Business.Domain;
using Hhreg.Business.Exceptions;

namespace Hhreg.Services;

public interface IDatabaseEnsurer
{
    void Ensure();
}

public class DatabaseEnsurer : IDatabaseEnsurer
{

    private readonly ISettingsService _settingsService;
    private readonly IUnitOfWork _unitOfWork;

    public DatabaseEnsurer(
        IUnitOfWork unitOfWork,
        ISettingsService settingsService)
    {
        _unitOfWork = unitOfWork;
        _settingsService = settingsService;
    }

    public void Ensure()
    {
        if (!Directory.Exists(_settingsService.AppDataFolder))
        {
            Directory.CreateDirectory(_settingsService.AppDataFolder);
        }

        if (!File.Exists(_settingsService.DatabaseFilePath))
        {
            var f = File.Create(_settingsService.DatabaseFilePath);
            AnsiConsole.MarkupLineInterpolated($"[darkblue]INFO:[/] Arquivo de banco de dados não existe. '[yellow]{_settingsService.DatabaseFilePath}[/]'.");
            f.Close();
        }

        var tableExists = _unitOfWork.QuerySingle<bool>(
            @"SELECT 
                case when count(*) == 0 then false else true end 
              FROM sqlite_master 
              WHERE type='table' AND name='DayEntry';");

        if (!tableExists)
        {
            AnsiConsole.MarkupLineInterpolated($"[darkblue]INFO:[/] Garantindo que tabelas existem em '[yellow]{_settingsService.ConnectionString}[/]'...");

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

        var settingsExists = _unitOfWork.QuerySingle<bool>(
            @"SELECT 
                case when count(*) == 0 then false else true end 
              FROM sqlite_master 
              WHERE type='table' AND name='Settings';");

        if (settingsExists) {
             AnsiConsole.MarkupLineInterpolated($"[darkblue]INFO:[/] Migrando configurações para novo formato...");

             try {
                var legacySettings = _unitOfWork.QuerySingle<LegacySettings>(@"select * from Settings");

                var newSettings = new Settings {
                    StartBalanceInMinutes = legacySettings.InitialBalance,
                    WorkDayInMinutes = legacySettings.WorkDay,
                    LastBalanceCutoff = legacySettings.StartCalculationsAt!,
                    EntryToleranceInMinutes = 0
                };

                _settingsService.SaveSettings(newSettings);

                _unitOfWork.Execute(@"drop table Settings");
             } catch (Exception e) {
                throw new HhregException("Falha ao tentar migrar configurações", e);
             }
        }
    }
}