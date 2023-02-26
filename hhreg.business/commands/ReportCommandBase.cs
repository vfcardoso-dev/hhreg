using Spectre.Console;
using Spectre.Console.Cli;

namespace hhreg.business;

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
        var invalidDays = _timeRepository.GetInvalidDayEntries();
        if (!invalidDays.Any()) return;

        foreach(var dayEntry in invalidDays) {
            var day = DateOnly.Parse(dayEntry.Day!);
            _logger.WriteLine($@"[orange1]VALIDATION:[/] Odd number of time entries ([orange1]{dayEntry.TimeEntries.Count()}[/]) at [orange1]{day.ToString("dd/MM/yyyy")}[/]! ({string
                .Join(" / ", dayEntry.TimeEntries.Select(x => x.Time))})");
        }

        _logger.WriteLine("[purple_1]DISCLAIMER:[/] There are day entries with an odd count of time entries, whose count should've been even.");

        throw new HhregException("Please fix these days before generating new reports.");
    }
}