namespace hhreg.business.infrastructure;

public interface ILocaleSettings {
    string? Language { get; }
}

public class LocaleSettings : ILocaleSettings
{
    public string? Language { get; set; }
}