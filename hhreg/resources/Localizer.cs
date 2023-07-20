
using System.Globalization;
using hhreg.business.infrastructure;
using Microsoft.Extensions.Localization;

namespace hhreg.resources;

public interface ILocalizer {
    void SetCulture(string culture);
    string GetCulture();

    string Get(string key);
}

public sealed class Localizer : ILocalizer {

    private readonly IStringLocalizer<Localizer> _localizer;

    public Localizer(
        IStringLocalizer<Localizer> localizer, 
        ILocaleSettings localeSettings)
    {
        _localizer = localizer;
        
        SetCulture(localeSettings.Language ?? "en");
    }

    public void SetCulture(string culture) {
        CultureInfo.CurrentUICulture = new CultureInfo(culture, false);
    }

    public string GetCulture() {
        return CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
    }

    public string Get(string key) {
        var found = _localizer[key];
        return found ?? key;
    }
}