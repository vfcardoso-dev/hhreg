using System.ComponentModel;
using Spectre.Console.Cli;
using Spectre.Console;
using System.Diagnostics.CodeAnalysis;

namespace hhreg.business;

public sealed class EntryNowCommand : Command<EntryNowCommand.Settings>
{
    private readonly ITimeRepository _timeRepository;

    public EntryNowCommand(ITimeRepository timeRepository)
    {
        _timeRepository = timeRepository;
    }

    public sealed class Settings : CommandSettings {}

    public override int Execute([NotNull] CommandContext context, [NotNull] Settings settings)
    {
        var inputNow = DateTime.Now;
        var date = DateOnly.FromDateTime(inputNow);
        var time = TimeOnly.FromDateTime(inputNow);

        var dayEntry = _timeRepository.GetOrCreateDay(date.ToString("yyyy-MM-dd"));

        _timeRepository.CreateTime(dayEntry.Id, time.ToTimeString());
        
        AnsiConsole.MarkupLineInterpolated($@"Day entry [green]SUCCESSFULLY[/] created!");
        AnsiConsole.MarkupLineInterpolated($"[yellow]{date}[/]: {time.ToTimeString()}");
        return 0;
    }
}