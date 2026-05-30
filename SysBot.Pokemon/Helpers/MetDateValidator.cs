using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace SysBot.Pokemon.Helpers;

/// <summary>
/// Rejects user-requested MetDate values that fall beyond today + a small
/// timezone leniency. Without this, a request like .MetDate=20770420 silently
/// produces a PKM with a future MetDate that PKHeX flags as illegal and that
/// makes no sense for an in-game trade.
/// </summary>
public static class MetDateValidator
{
    /// <summary>Days of slack past today to cover timezone differences (UTC-12 to UTC+14 spans ~26 hours).</summary>
    private const int LeniencyDays = 2;

    public const string FutureDateError = "The MetDate for this Pokémon cannot be set to a future date.";

    private static readonly Regex MetDatePattern = new(@"\.MetDate=(\d{8})", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    /// <summary>
    /// Scans normalized showdown content for any .MetDate=YYYYMMDD value
    /// that exceeds today (UTC) + LeniencyDays. Run AFTER BatchCommandNormalizer
    /// — the regex only matches the normalized batch form.
    /// </summary>
    public static bool IsValid(string content, out string? error)
    {
        error = null;
        var matches = MetDatePattern.Matches(content);
        if (matches.Count == 0)
            return true;

        var maxAllowed = DateTime.UtcNow.Date.AddDays(LeniencyDays);
        foreach (Match m in matches)
        {
            if (!DateTime.TryParseExact(m.Groups[1].Value, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var requested))
                continue;
            if (requested.Date > maxAllowed)
            {
                error = FutureDateError;
                return false;
            }
        }
        return true;
    }
}
