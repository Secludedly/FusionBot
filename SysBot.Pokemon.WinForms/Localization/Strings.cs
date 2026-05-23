using System.Globalization;
using System.Resources;

namespace SysBot.Pokemon.WinForms.Localization;

public static class Strings
{
    private static ResourceManager? _rm;

    private static ResourceManager ResourceManager =>
        _rm ??= new ResourceManager("SysBot.Pokemon.WinForms.Localization.Strings", typeof(Strings).Assembly);

    public static string Get(string key) =>
        ResourceManager.GetString(key, CultureInfo.CurrentUICulture) ?? key;

    public static string Get(string key, string fallback) =>
        ResourceManager.GetString(key, CultureInfo.CurrentUICulture) ?? fallback;
}
