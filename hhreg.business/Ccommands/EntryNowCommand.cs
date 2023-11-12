using System.Diagnostics.CodeAnalysis;
using hhreg.business.infrastructure;
using hhreg.business.repositories;
using hhreg.business.utilities;
using Spectre.Console.Cli;

namespace hhreg.business.commands;

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
        var date = inputNow.ToDateOnly();
        var time = inputNow.ToTimeOnly();

        var dayEntry = _timeRepository.GetOrCreateDay(date);

        _timeRepository.CreateTime(dayEntry.Id, time.ToTimeString());
        
        _logger.WriteLine($@"Day entry [green]SUCCESSFULLY[/] created!");
        _logger.WriteLine($"[yellow]{date}[/]: {time.ToTimeString()}");
        return 0;
    }
}