using System.Globalization;
using System.Resources;

namespace SysBot.Pokemon.Localization;

/// <summary>
/// Lookup helper for the SysBot.Pokemon-side resource file used by the Hub PropertyGrid.
/// Distinct from the WinForms <c>Strings</c> file since this assembly cannot reference WinForms.
/// Both pick up the same <c>CurrentUICulture</c>, which is set once at program startup.
/// </summary>
public static class HubStrings
{
    private static ResourceManager? _rm;

    private static ResourceManager ResourceManager =>
        _rm ??= new ResourceManager("SysBot.Pokemon.Localization.HubStrings", typeof(HubStrings).Assembly);

    public static string Get(string key) =>
        ResourceManager.GetString(key, CultureInfo.CurrentUICulture) ?? key;

    public static string Get(string key, string fallback) =>
        ResourceManager.GetString(key, CultureInfo.CurrentUICulture) ?? fallback;
}
