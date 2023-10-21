namespace hhreg.business.exceptions;

public static class HhregMessages
{
    // Common
    public const string CouldNotParseAsAValidDateFormat = "Could not parse '{0}' as a valid date format.";
    public const string CouldNotParseAsAValidTimeFormat = "Could not parse '{0}' as a valid time format.";
    public const string CouldNotParseAsAValidIntegerFormat = "Could not parse '{0}' as a valid integer format";
    public const string InvalidInputFormatOnValue = "Invalid input format on value '{0}'";
    public const string SettingsNotYetInitialized = "CLI ainda não foi inicializado.";

    // Config
    public const string DatabaseLocationTitle = "Database location";

    // Update
    public const string UnknownPlatform = "UnknownPlatform";
    public const string LastestReleaseUnavailable = "LastestReleaseUnavailable";
    public const string UpdateArtifactNotFound = "UpdateArtifactNotFound";
    
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

    // Report
    public const string YouShouldInformADay = "You should inform a day.";
    public const string YouShouldInformAMonth = "You should inform a month (MM/yyyy).";
    public const string TailMustHaveAPositiveValue = "Tail must have a positive value.";
    public const string ThereAreDayEntriesWithAnOddCountOfTimeEntries = "[purple_1]DISCLAIMER:[/] There are day entries with an odd count of time entries, whose count should have been even.";
    public const string PleaseFixTheseDaysBeforeGeneratingNewReports = "Please fix these days before generating new reports.";
    public const string ConfigurationIsSetToStartBalanceCalculationsAfterTheOffsetDate = "Configuration is set to start balance calculations after the offset date. StartCalculationsAt: {0}; OffsetDate: {1}";
    public const string InformedDayIsNotRegistered = "Informed day is not registered.";
}