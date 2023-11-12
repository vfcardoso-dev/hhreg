using Hhreg.Business.Domain.Common;
using Hhreg.Business.Utilities;

namespace Hhreg.Business.Domain;

public class Settings : BaseEntity<Settings>
{
    public double StartBalanceInMinutes { get; set; }
    public double WorkDayInMinutes { get; set; }
    public double EntryToleranceInMinutes { get; set; }
    public string LastBalanceCutoff { get; set; } = string.Empty; // Date

    public override string[] ExtractRow()
    {
        return new string[] {
            StartBalanceInMinutes.ToTimeString(),
            WorkDayInMinutes.ToTimeString(),
            EntryToleranceInMinutes.ToTimeString(),
            DateTime.Parse(LastBalanceCutoff).ToString("dd/MM/yyyy")
        };
    }
}
