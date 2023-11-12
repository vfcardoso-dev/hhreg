using Hhreg.Business.Exceptions;
using Hhreg.Business.Infrastructure;
using Hhreg.Business.Repositories;
using Hhreg.Business.Utilities;
using Spectre.Console.Cli;

namespace Hhreg.Business.Commands;

public abstract class ReportCommandBase<T> : Command<T> where T : CommandSettings
{
    private readonly ITimeRepository _timeRepository;
    private readonly ILogger _logger;

    public ReportCommandBase(ITimeRepository timeRepository, ILogger logger)
    {
        _timeRepository = timeRepository;
        _logger = logger;
    }

    protected void CheckInvalidTimeEntries()
    {
        var invalidDays = _timeRepository.GetInvalidDayEntries().ToList();
        if (!invalidDays.Any()) return;

        foreach (var dayEntry in invalidDays)
        {
            var day = dayEntry.Day!.ToDateOnly();
            _logger.WriteLine($@"[orange1]VALIDATION:[/] Odd number of time entries ([orange1]{dayEntry.TimeEntries.Count()}[/]) at [orange1]{day:dd/MM/yyyy}[/]! ({string
                .Join(" / ", dayEntry.TimeEntries.Select(x => x.Time))})");
        }

        _logger.WriteLine(HhregMessages.ThereAreDayEntriesWithAnOddCountOfTimeEntries);

        // throw new HhregException(HhregMessages.PleaseFixTheseDaysBeforeGeneratingNewReports);
    }
}