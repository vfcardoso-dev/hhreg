namespace hhreg.business.domain;

public class Settings
{
    public double InitialBalance { get; set; } // in minutes
    public double WorkDay { get; set; } // in minutes
    public double LunchTime { get; set; } = 60; // in minutes
}
