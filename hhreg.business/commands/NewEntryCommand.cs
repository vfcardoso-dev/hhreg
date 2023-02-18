using System.ComponentModel;
using Spectre.Console.Cli;
using Spectre.Console;
using System.Diagnostics.CodeAnalysis;

namespace hhreg.business;

public sealed class NewEntryCommand : Command<NewEntryCommand.Settings>
{
    private readonly ITimeRepository _timeRepository;

    public NewEntryCommand(ITimeRepository timeRepository)
    {
        _timeRepository = timeRepository;
    }

    public sealed class Settings : CommandSettings 
    {
        [Description("Sets entry day as today")]
        [CommandOption("-t|--today")]
        public bool IsToday { get; init; }

        [Description("Defines a justification")]
        [CommandOption("-j|--justification")]
        public string? Justification { get; init; }

        [Description("Defines the day")]
        [CommandOption("-d|--day")]
        public string? Day { get; init; }

        [Description("Defines time entries")]
        [CommandArgument(0, "[entries]")]
        public string[] Entries { get; init; } = new string[]{};


        public override ValidationResult Validate()
        {
            if (!IsToday && Day == null) {
                return ValidationResult.Error("You should inform a day to log (or set entry as today with -t).");
            }

            if (!IsToday && !DateTime.TryParse(Day, out var _)) {
                return ValidationResult.Error($"Could not parse '{Day}' as a valid date format.");
            }
                
            if (Entries.Length == 0 && Justification == null) {
                return ValidationResult.Error("You should inform at least one time entry or set a justification with -j.");
            }

            foreach(var entry in Entries) {
                if (!TimeSpan.TryParse(entry, out var time)) {
                    return ValidationResult.Error($"Could not parse '{entry}' as a valid time format.");
                } 
            }
            
            return ValidationResult.Success();
        }
    }

    public override int Execute([NotNull] CommandContext context, [NotNull] Settings settings)
    {
        var inputDay = settings.IsToday ? DateOnly.FromDateTime(DateTime.Today) : DateOnly.Parse(settings.Day!);
        var dayEntry = _timeRepository.GetOrCreateDay(inputDay.ToString("yyyy-MM-dd"), settings.Justification);

        // var firstPersistedTime = dayEntry.TimeEntries.OrderBy(x => x.Time).First();
        // if (settings.Entries.Any(x => x < firstPersistedTime.Time)) {
        //     // If any of entries informed is before first persisted entry, then its forbidden
        //     throw new HhregException("You cannot inform a time entry minor than the first already informed.");
        // }

        _timeRepository.CreateTime(dayEntry.Id, settings.Entries);
        
        AnsiConsole.MarkupLineInterpolated($@"Day entry [green]SUCCESSFULLY[/] created!");
        AnsiConsole.MarkupLineInterpolated($"[yellow]{inputDay}[/]: {string.Join(", ", settings.Entries)}");
        return 0;
    }
}