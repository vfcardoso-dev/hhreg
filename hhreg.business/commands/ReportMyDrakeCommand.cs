using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Json;
using Spectre.Console;
using Spectre.Console.Cli;

namespace hhreg.business;

public sealed class ReportMyDrakeCommand : ReportCommandBase<ReportMyDrakeCommand.Settings>
{
    private readonly ITimeRepository _timeRepository;
    private readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    public ReportMyDrakeCommand(ITimeRepository timeRepository) : base(timeRepository) 
    {
        _timeRepository = timeRepository;
    }

    public sealed class Settings : CommandSettings {
        [Description("First day on export period (format: dd/MM/yyyy).")]
        [CommandArgument(0, "<start>")]
        public string Start { get; init; } = string.Empty;

        [Description("Last day on export period (format: dd/MM/yyyy). Optional.")]
        [CommandArgument(1, "[end]")]
        public string? End { get; init; }

        public override ValidationResult Validate()
        {
            if (!DateOnly.TryParse(Start, out var _)) {
                return ValidationResult.Error($"Could not parse '{Start}' as a valid date format.");
            }

            if (End != null && !DateOnly.TryParse(End, out var _)) {
                return ValidationResult.Error($"Could not parse '{End}' as a valid date format.");
            }
            
            return ValidationResult.Success();
        }
    }

    public override int Execute([NotNull] CommandContext context, [NotNull] Settings settings)
    {
        CheckInvalidTimeEntries();

        var startDate = DateOnly.Parse(settings.Start);
        var endDate = settings.End != null ? DateOnly.Parse(settings.Start) : DateOnly.FromDateTime(DateTime.Today);

        AnsiConsole.MarkupInterpolated($"Exporting entries from {startDate.ToString("dd/MM/yyyy")} to {endDate.ToString("dd/MM/yyyy")}...");

        var dayEntries = _timeRepository.GetDayEntriesByType(startDate.ToString("yyyy-MM-dd"), endDate.ToString("yyyy-MM-dd"), DayType.Work);
        var myDrakeEvents = new List<MyDrakeEvent>();

        foreach(var dayEntry in dayEntries)
        {
            if (dayEntry.DayType != DayType.Work) {
                var day = DateOnly.Parse(dayEntry.Day!).ToString("dd/MM/yyyy");
                AnsiConsole.MarkupLineInterpolated($"[darkblue]INFO:[/] Bypassing day [yellow]{day}[/] because it's not a work day. Type: {dayEntry.DayType}. Justification: {dayEntry.Justification}");
                continue;
            }

            for(int i=0,j=1; j < dayEntry.TimeEntries.OrderBy(x => x.Time).Count(); i=j+1,j=j+2)
            {
                var first = TimeSpan.Parse(dayEntry.TimeEntries.ElementAt(i).Time!).ToTimeString();
                var second = TimeSpan.Parse(dayEntry.TimeEntries.ElementAt(j).Time!).ToTimeString();

                var evt = new MyDrakeEvent {
                    Id = Guid.NewGuid(),
                    Start = $"{dayEntry.Day}T{first}:00",
                    End = $"{dayEntry.Day}T{second}:00",
                    Justification = dayEntry.Justification,
                    Event = new MyDrakeInnerEvent {
                        StartDate = $"{dayEntry.Day}T{first}:00.000Z",
                        EndDate = $"{dayEntry.Day}T{second}:00.000Z",
                        Occurrence = new Option(),
                        OperationalUnit = new Option(),
                        CostCenter = new Option(),
                        Reason = dayEntry.Justification
                    }
                };

                myDrakeEvents.Add(evt);
            }
        }

        var json = JsonSerializer.Serialize(myDrakeEvents, _jsonSerializerOptions);
        var bytes = Encoding.UTF8.GetBytes(json);
        var encoded = Convert.ToBase64String(bytes);
        
        var panel = new Panel("Copy the code below and paste on [green]hhreg.chrome[/] extension");
        AnsiConsole.Write(panel);

        AnsiConsole.WriteLine(encoded);

        return 0;
    }
}