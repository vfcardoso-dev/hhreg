namespace hhreg.business;

public static class HhregMessages
{
    // Common
    public const string CouldNotParseAsAValidDateFormat = "Could not parse '{0}' as a valid date format.";
    public const string CouldNotParseAsAValidTimeFormat = "Could not parse '{0}' as a valid time format.";
    public const string CouldNotParseAsAValidIntegerFormat = "Could not parse '{0}' as a valid integer format";
    public const string InvalidInputFormatOnValue = "Invalid input format on value '{0}'";
    
    // Config
    public const string DatabaseLocationTitle = "Database location";
    
    // Entry
    public const string YouShouldInformADayToLog = "You should inform a day to log (or set entry as today with -t).";
    public const string YouShouldInformAtLeastOneTimeEntryOrSetAJustificative = "You should inform at least one time entry or set a justification with -j.";
    public const string EntryTimesMustBePositive = "Entry times must be positives.";
    public const string CannotOverrideANotYetCreatedDay = "Cannot override a not yet created day '{0}'";

    // Init
    public const string YouShouldInformInitialBalance = "You should inform initial balance.";
    public const string YouShouldInformWorkday = "You should inform workday.";
    public const string YouShouldInformStartCalculationsAt = "You should inform your a date to start balance calculations.";
    public const string SettingsAlreadyInitialized = "Settings already initialized. You can change it with 'config edit'";
}