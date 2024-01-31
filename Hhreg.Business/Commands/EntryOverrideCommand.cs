using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using Hhreg.Business.Domain;
using Hhreg.Business.Exceptions;
using Hhreg.Business.Infrastructure;
using Hhreg.Business.Repositories;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Hhreg.Business.Commands;

public sealed class EntryOverrideCommand : Command<EntryOverrideCommand.Settings>
{
    private readonly ITimeRepository _timeRepository;
    private readonly ILogger _logger;

    public EntryOverrideCommand(ITimeRepository timeRepository, ILogger logger)
    {
        _timeRepository = timeRepository;
        _logger = logger;
    }

    public sealed class Settings : CommandSettings
    {
        [Description("Define a data da entrada como hoje")]
        [CommandOption("-t|--today")]
        public bool IsToday { get; init; }

        [Description("Define o tipo do dia (Work,Weekend,Sick,Holiday,Vacation)")]
        [CommandOption("-y|--day-type")]
        [DefaultValue(DayType.Work)]
        public DayType DayType { get; init; }

        [Description("Define uma justificativa")]
        [CommandOption("-j|--justification")]
        public string? Justification { get; init; }

        [Description("Define a data da entrada (formato: dd/MM/yyyy)")]
        [CommandOption("-d|--day")]
        public string? Day { get; init; }

        [Description("Define as marcações (formato: HH:mm)")]
        [CommandArgument(0, "[entries]")]
        public string[] Entries { get; init; } = Array.Empty<string>();


        public override ValidationResult Validate()
        {
            if (!IsToday && Day == null)
            {
                return ValidationResult.Error(HhregMessages.YouShouldInformADayToLog);
            }

            if (!IsToday && !DateOnly.TryParse(Day, out var _))
            {
                return ValidationResult.Error(string.Format(HhregMessages.CouldNotParseAsAValidDateFormat, Day));
            }

            if (Entries.Length == 0 && Justification == null)
            {
                return ValidationResult.Error(HhregMessages.YouShouldInformAtLeastOneTimeEntryOrSetAJustificative);
            }

            foreach (var entry in Entries)
            {
                if (TimeSpan.TryParse(entry, out var time))
                {
                    if (time < TimeSpan.Zero)
                    {
                        return ValidationResult.Error(HhregMessages.EntryTimesMustBePositive);
                    }
                }
                else
                {
                    return ValidationResult.Error(string.Format(HhregMessages.CouldNotParseAsAValidTimeFormat, entry));
                }
            }

            return ValidationResult.Success();
        }
    }

    public override int Execute([NotNull] CommandContext context, [NotNull] Settings settings)
    {
        var inputDay = settings.IsToday
            ? DateOnly.FromDateTime(DateTime.Today)
            : DateOnly.Parse(settings.Day!);

        var dayEntry = _timeRepository.GetDayEntry(inputDay);
        if (dayEntry == null) throw new HhregException(string.Format(HhregMessages.CannotOverrideANotYetCreatedDay, 
            inputDay.ToString("dd/MM/yyyy")));

        _timeRepository.OverrideDayEntry(dayEntry.Id, settings.Justification, settings.DayType, settings.Entries);

        var dayText = settings.DayType == DayType.Work ? string.Join(" / ", settings.Entries) : settings.Justification;
        _logger.WriteLine($@"Marcações sobrescritas com [green]SUCESSO[/]!");
        _logger.WriteLine($"[yellow]{inputDay}[/]: {dayText}");
        return 0;
    }
}