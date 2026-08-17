using SysBot.Pokemon.Discord.Helpers;
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace SysBot.Pokemon.Discord.Commands.Bots.SlashCommands;

/// <summary>
/// Discord's slash command string options are single-line and won't accept a newline in
/// an option box, and flattens the newlines out of anything pasted in. The problem is Showdown sets are
/// naturally multi-line. This restores the line structure before the text reaches the existing
/// parsing pipeline, so batch commands typed straight into a set survive either.
/// "TradeSetModal" is the richer path (a paragraph input keeps real newlines) and this covers
/// users who paste or type a set into the single-line option.
/// </summary>
public static class SlashShowdownText
{
    /// <summary>
    /// Turns whatever a user managed to get into a slash option back into a proper multi-line set.
    /// Handles three shapes: real newlines (from a modal), semicolon(;) separators, and a set that was
    /// pasted into a single-line option and had its newlines flattened to spaces by the Discord client.
    /// </summary>
    public static string ToMultiline(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        // Literal backslash-n first: users pasting from a text editor often end up with it escaped.
        var text = input.Replace("\\n", "\n").Replace(';', '\n');

        // Nothing separated it, so this is likely a set that was pasted into a single-line
        // option. Discord replaces the newlines with spaces on paste, so rebuild them from structure.
        if (!text.Contains('\n'))
            text = Reflow(text);

        // Collapse the blank lines that trailing or doubled separators would otherwise produce,
        // since the Showdown parser treats a blank line as a set boundary.
        var lines = text.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        for (int i = 0; i < lines.Length; i++)
            lines[i] = lines[i].Trim();

        return string.Join('\n', lines);
    }

    /// <summary>
    /// Rebuilds line breaks in a Showdown set whose newlines were flattened into spaces.
    /// The newline information is genuinely gone by the time the interaction reaches us, so the breaks
    /// are recovered from Showdown's grammar instead. A line starts at a move hyphen marker (- ), a
    /// recognized "Key:" attribute, a ".Key=" or "~=" batch command, or an
    /// "[BLANK] Nature" declaration. Everything before the first is the species line.
    /// </summary>
    public static string Reflow(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        // Break before the next token, consuming the whitespace that used to be the newline.
        return TokenBoundary.Replace(input.Trim(), "\n").Trim();
    }

    /// <summary>
    /// Core Showdown attributes, plus a few the normalizer does not itself list.
    /// </summary>
    private static readonly string[] CoreShowdownKeys =
    [
        "Ability", "Level", "Shiny", "Happiness", "Friendship", "EVs", "IVs", "Ball",
        "Tera Type", "TeraType", "Gigantamax", "GMax", "Dynamax Level", "Nickname",
        "OT", "OTGender", "OT Gender", "TID", "SID", "Language", "Alpha", "Form",
        "Met Date", "Met Level", "Met Location", "Hidden Power", "Type", "Size",
    ];

    private static readonly Regex TokenBoundary = BuildTokenBoundary();

    private static Regex BuildTokenBoundary()
    {
        // Longest first so "Met Location:" wins over a hypothetical "Met:" prefix match.
        var keys = BatchCommandNormalizer.CommandProcessors.Keys
            .Concat(BatchCommandNormalizer.EqualCommandKeys)
            .Concat(BatchCommandNormalizer.BatchCommandAliasMap.Keys)
            .Concat(CoreShowdownKeys)
            .Where(k => !string.IsNullOrWhiteSpace(k))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderByDescending(k => k.Length)
            .Select(Regex.Escape);

        var keyList = keys.ToArray();
        var keyAlternation = string.Join("|", keyList);

        // Words that begin a multi-word key ("Tera" from "Tera Type", "Met" from "Met Location", ...).
        // Their trailing words are often standalone keys too -- "Type:" and "Level:" both are -- so
        // without this guard "Tera Type: Steel" would break twice, into "Tera" and "Type: Steel".
        var leadIns = BatchCommandNormalizer.CommandProcessors.Keys
            .Concat(BatchCommandNormalizer.EqualCommandKeys)
            .Concat(BatchCommandNormalizer.BatchCommandAliasMap.Keys)
            .Concat(CoreShowdownKeys)
            .Where(k => k.Contains(' '))
            .SelectMany(k =>
            {
                var words = k.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                // Every proper leading prefix of the key, so three-word keys are covered too.
                return Enumerable.Range(1, words.Length - 1).Select(n => string.Join(' ', words.Take(n)));
            })
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderByDescending(w => w.Length)
            .Select(Regex.Escape)
            .ToArray();

        var noBreakInsideKey = leadIns.Length == 0
            ? string.Empty
            : @"(?<!\b(?:" + string.Join("|", leadIns) + @")\s)";

        // A break occurs at whitespace immediately followed by one of:
        //   "- "            a move entry
        //   ".Key="         direct batch command
        //   "~=Key="        equals-form batch command
        //   "Known Key:"    a recognised Showdown/batch attribute
        //   "Word Nature"   the nature declaration
        var pattern =
            @"\s+(?=" +
            @"-\s" +
            @"|\.[A-Za-z_]\w*\s*=" +
            @"|~=" +
            @"|" + noBreakInsideKey + @"(?:" + keyAlternation + @")\s*:" +
            @"|[A-Za-z]+\s+Nature\b" +
            @")";

        return new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);
    }

}
