using hhreg.business.domain.common;
using hhreg.business.utilities;

namespace hhreg.business.domain;

public class Settings : BaseEntity<Settings>
{
    public double InitialBalance { get; set; } // in minutes
    public double WorkDay { get; set; } // in minutes
    public string StartCalculationsAt { get; set; } = string.Empty;

    public override string[] ExtractRow()
    {
        return new string[] {
            InitialBalance.ToTimeString(),
            WorkDay.ToTimeString(),
            DateTime.Parse(StartCalculationsAt).ToString("dd/MM/yyyy")
        };
    }
}
