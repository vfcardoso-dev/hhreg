using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Json;
using Hhreg.Business.Domain;
using Hhreg.Business.Domain.Dtos;
using Hhreg.Business.Exceptions;
using Hhreg.Business.Infrastructure;
using Hhreg.Business.Repositories;
using Hhreg.Business.Utilities;
using Spectre.Console;
using Spectre.Console.Cli;
using TextCopy;

namespace Hhreg.Business.Commands;

public sealed class ReportMyDrakeCommand : ReportCommandBase<ReportMyDrakeCommand.Settings>
{
    private readonly ITimeRepository _timeRepository;
    private readonly IClipboard _clipboard;
    private readonly ILogger _logger;
    private readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public ReportMyDrakeCommand(
        ITimeRepository timeRepository,
        IClipboard clipboard,
        ILogger logger) : base(timeRepository, logger)
    {
        _timeRepository = timeRepository;
        _clipboard = clipboard;
        _logger = logger;
    }

    public sealed class Settings : CommandSettings
    {
        [Description("First day on export period (format: dd/MM/yyyy).")]
        [CommandArgument(0, "<start>")]
        public string Start { get; init; } = string.Empty;

        [Description("Last day on export period (format: dd/MM/yyyy). Optional.")]
        [CommandArgument(1, "[end]")]
        public string? End { get; init; }

        [Description("Last day on export period (format: dd/MM/yyyy). Optional.")]
        [CommandOption("-v|--verbose")]
        public bool Verbose { get; init; }

        public override ValidationResult Validate()
        {
            if (!DateOnly.TryParse(Start, out var _))
            {
                return ValidationResult.Error(string.Format(HhregMessages.CouldNotParseAsAValidDateFormat, Start));
            }

            if (End != null && !DateOnly.TryParse(End, out var _))
            {
                return ValidationResult.Error(string.Format(HhregMessages.CouldNotParseAsAValidDateFormat, End));
            }

            return ValidationResult.Success();
        }
    }

    public override int Execute([NotNull] CommandContext context, [NotNull] Settings settings)
    {
        CheckInvalidTimeEntries();

        var startDate = settings.Start.ToDateOnly();
        var endDate = settings.End?.ToDateOnly() ?? DateTime.Today.ToDateOnly();

        _logger.WriteLine($"Exporting entries from {startDate:dd/MM/yyyy} to {endDate:dd/MM/yyyy}...");

        var dayEntries = _timeRepository.GetDayEntriesByType(startDate, endDate, DayType.Work);
        var myDrakeEvents = new List<MyDrakeEvent>();

        foreach (var dayEntry in dayEntries)
        {
            if (dayEntry.DayType != DayType.Work)
            {
                var day = DateOnly.Parse(dayEntry.Day!).ToString("dd/MM/yyyy");
                _logger.WriteLine($"[darkblue]INFO:[/] Bypassing day [yellow]{day}[/] because it's not a work day. Type: {dayEntry.DayType}. Justification: {dayEntry.Justification}");
                continue;
            }

            for (int i = 0, j = 1; j < dayEntry.TimeEntries.OrderBy(x => x.Time).Count(); i = j + 1, j += 2)
            {
                var first = TimeSpan.Parse(dayEntry.TimeEntries.ElementAt(i).Time!).ToTimeString();
                var second = TimeSpan.Parse(dayEntry.TimeEntries.ElementAt(j).Time!).ToTimeString();

                var evt = new MyDrakeEvent
                {
                    Id = Guid.NewGuid(),
                    Start = $"{dayEntry.Day}T{first}:00",
                    End = $"{dayEntry.Day}T{second}:00",
                    Justification = dayEntry.Justification,
                    Event = new MyDrakeInnerEvent
                    {
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

        if (settings.Verbose)
        {
            var panel = new Panel("Copy the code below and paste on [green]hhreg.chrome[/] extension");
            _logger.Write(panel);

            _logger.WriteLine(encoded);
        }
        else
        {
            _clipboard.SetText(encoded);

            var panel = new Panel("Code generated and copied to clipboard. You can now paste it on [green]hhreg.chrome[/] extension");
            _logger.Write(panel);
        }


        return 0;
    }
}