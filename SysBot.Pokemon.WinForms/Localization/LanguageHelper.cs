using System;
using System.Globalization;
using System.IO;
using System.Text.Json;
using System.Threading;

namespace SysBot.Pokemon.WinForms.Localization;

public static class LanguageHelper
{
    public static string ToCultureName(UILanguage language) => language switch
    {
        UILanguage.Japanese => "ja",
        UILanguage.French => "fr",
        UILanguage.Italian => "it",
        UILanguage.German => "de",
        UILanguage.Spanish => "es",
        UILanguage.Korean => "ko",
        UILanguage.ChineseSimplified => "zh-Hans",
        UILanguage.ChineseTraditional => "zh-Hant",
        _ => "en",
    };

    public static void Apply(UILanguage language)
    {
        var culture = new CultureInfo(ToCultureName(language));

        // DefaultThreadCurrentUICulture seeds every new thread; CurrentUICulture covers the
        // current (UI) thread. Set both so background workers picked up later don't fall back
        // to the system culture and bypass our resx selection.
        CultureInfo.DefaultThreadCurrentUICulture = culture;
        Thread.CurrentThread.CurrentUICulture = culture;
    }

    /// <summary>
    /// Reads only the <c>Hub.Language</c> field from the config file without deserializing the
    /// full <c>ProgramConfig</c>. Used before <c>Main</c> is constructed so the culture is
    /// already set when forms call <c>InitializeComponent</c> and load resx strings.
    /// </summary>
    public static UILanguage ReadConfiguredLanguage(string configPath)
    {
        try
        {
            if (!File.Exists(configPath))
                return UILanguage.English;

            using var doc = JsonDocument.Parse(File.ReadAllText(configPath));
            if (!doc.RootElement.TryGetProperty("Hub", out var hub))
                return UILanguage.English;
            if (!hub.TryGetProperty("Language", out var lang))
                return UILanguage.English;

            return lang.ValueKind switch
            {
                JsonValueKind.Number when lang.TryGetInt32(out var n) && Enum.IsDefined(typeof(UILanguage), n) => (UILanguage)n,
                JsonValueKind.String when Enum.TryParse<UILanguage>(lang.GetString(), ignoreCase: true, out var parsed) => parsed,
                _ => UILanguage.English,
            };
        }
        catch
        {
            return UILanguage.English;
        }
    }
}
