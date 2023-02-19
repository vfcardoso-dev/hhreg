using Spectre.Console;
using Spectre.Console.Cli;

namespace hhreg.business;

public abstract class ReportCommandBase<T> : Command<T> where T : CommandSettings
{
    private readonly ITimeRepository _timeRepository;

    public ReportCommandBase(ITimeRepository timeRepository)
    {
        _timeRepository = timeRepository;
    }

    protected void CheckInvalidTimeEntries()
    {
        var invalidDays = _timeRepository.GetInvalidDayEntries();
        if (invalidDays.Count() == 0) return;

        foreach(var dayEntry in invalidDays) {
            var day = DateOnly.Parse(dayEntry.Day!);
            AnsiConsole.MarkupLineInterpolated($@"[orange1]VALIDATION:[/] Odd number of time entries ([orange1]{dayEntry.TimeEntries.Count()}[/]) at [orange1]{day.ToString("dd/MM/yyyy")}[/]! ({string
                .Join(" / ", dayEntry.TimeEntries.Select(x => x.Time))})");
        }

        AnsiConsole.MarkupLine("[purple_1]DISCLAIMER:[/] There are day entries with an odd count of time entries, whose count should've been even.");

        throw new HhregException("Please fix these days before generating new reports.");
    }
}