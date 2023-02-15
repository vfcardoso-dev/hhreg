namespace hhreg.business.domain;

public class Settings
{
    public int InitialBalance { get; set; } // in minutes
    public int WorkDay { get; set; } // in minutes
    public int LunchTime { get; set; } = 60; // in minutes
}
