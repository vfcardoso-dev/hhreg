using Spectre.Console;
using Spectre.Console.Rendering;

namespace hhreg.business.domain;

public class Settings : BaseEntity<Settings>
{
    public double InitialBalance { get; set; } // in minutes
    public double WorkDay { get; set; } // in minutes
    public double LunchTime { get; set; } = 60; // in minutes

    public override IRenderable[] CreateRenderableRow()
    {
        return new Text[] {
            new Text(InitialBalance.ToTimeString()),
            new Text(WorkDay.ToTimeString()),
            new Text(LunchTime.ToTimeString())
        };
    }
}
