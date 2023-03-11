namespace hhreg.business;

public static class HhregMessages
{
    public static class Common
    {
        public const string CouldNotParseAsAValidDateFormat = "Could not parse '{0}' as a valid date format.";
        public const string CouldNotParseAsAValidTimeFormat = "Could not parse '{0}' as a valid time format.";
        public const string CouldNotParseAsAValidIntegerFormat = "Could not parse '{0}' as a valid integer format";
        public const string InvalidInputFormatOnValue = "Invalid input format on value '{0}'";
    }

    public static class Config
    {
        public static class Database
        {
            public const string DatabaseLocationTitle = "Database location";
        }
    }

    public static class Entry
    {
        public static class New
        {
            public const string YouShouldInformADayToLog = "You should inform a day to log (or set entry as today with -t).";
            public const string YouShouldInformAtLeastOneTimeEntryOrSetAJustificative = "You should inform at least one time entry or set a justification with -j.";
            public const string EntryTimesMustBePositive = "Entry times must be positives.";

        }
    }
}