using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using Spectre.Console;
using Spectre.Console.Cli;

namespace hhreg.business;

public sealed class ReportBalanceCommand : ReportCommandBase<ReportBalanceCommand.Settings>
{
    private readonly ITimeRepository _timeRepository;
    private readonly ISettingsRepository _settingsRepository;

    public ReportBalanceCommand(
        ITimeRepository timeRepository, 
        ISettingsRepository settingsRepository) : base(timeRepository) 
    {
        _timeRepository = timeRepository;
        _settingsRepository = settingsRepository;
    }

    public sealed class Settings : CommandSettings {
        [Description("Retrieve last N days")]
        [CommandOption("-t|--tail")]
        [DefaultValue(5)]
        public int Tail { get; init; }
    }

    public override int Execute([NotNull] CommandContext context, [NotNull] Settings settings)
    {
        CheckInvalidTimeEntries();

        var cfg = _settingsRepository.Get()!;

        
        var table = new Table();
        
        
        AnsiConsole.Write(table);
        return 0;
    }
}