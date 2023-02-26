using System.ComponentModel;
using Spectre.Console.Cli;
using Spectre.Console;
using System.Diagnostics.CodeAnalysis;

namespace hhreg.business;

public sealed class EntryNowCommand : Command<EntryNowCommand.Settings>
{
    private readonly ITimeRepository _timeRepository;
    private readonly ILogger _logger;

    public EntryNowCommand(ITimeRepository timeRepository, ILogger logger)
    {
        _timeRepository = timeRepository;
        _logger = logger;
    }

    public sealed class Settings : CommandSettings {}

    public override int Execute([NotNull] CommandContext context, [NotNull] Settings settings)
    {
        var inputNow = DateTime.Now;
        var date = DateOnly.FromDateTime(inputNow);
        var time = TimeOnly.FromDateTime(inputNow);

        var dayEntry = _timeRepository.GetOrCreateDay(date.ToString("yyyy-MM-dd"));

        _timeRepository.CreateTime(dayEntry.Id, time.ToTimeString());
        
        _logger.WriteLine($@"Day entry [green]SUCCESSFULLY[/] created!");
        _logger.WriteLine($"[yellow]{date}[/]: {time.ToTimeString()}");
        return 0;
    }
}