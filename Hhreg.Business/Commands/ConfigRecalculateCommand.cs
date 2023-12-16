using System.Diagnostics.CodeAnalysis;
using Hhreg.Business.Infrastructure;
using Hhreg.Business.Repositories;
using Hhreg.Business.Utilities;
using Spectre.Console.Cli;

namespace Hhreg.Business.Commands;

public sealed class ConfigRecalculateCommand : Command<ConfigRecalculateCommand.Settings>
{
    private readonly ISettingsService _settingsService;
    private readonly ITimeRepository _timeRepository;
    private readonly ILogger _logger;

    public ConfigRecalculateCommand(ILogger logger, ITimeRepository timeRepository, ISettingsService settingsService)
    {
        _logger = logger;
        _timeRepository = timeRepository;
        _settingsService = settingsService;
    }

    public sealed class Settings : CommandSettings { }

    public override int Execute([NotNull] CommandContext context, [NotNull] Settings settings)
    {
        var cfg = _settingsService.GetSettings();
        var dayEntries = _timeRepository.GetAllDayEntries();

        foreach ( var dayEntry in dayEntries )
        {
            var entries = dayEntry.TimeEntries.Select(x => x.Time!);

            var newTotal = Calculations.GetTotalMinutes(entries, dayEntry.DayType, cfg.EntryToleranceInMinutes, cfg.WorkDayInMinutes);
            _timeRepository.UpdateDayEntryTotalMinutes(dayEntry.Id, newTotal);
        }

        return 0;
    }
}