using Discord;
using Discord.WebSocket;
using Newtonsoft.Json.Linq;
using PKHeX.Core;
using PKHeX.Core.AutoMod;
using SysBot.Base;
using SysBot.Pokemon.Discord.Helpers;
using SysBot.Pokemon.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace SysBot.Pokemon.Discord;

public static class AutoLegalityExtensionsDiscord
{
    public static async Task ReplyWithLegalizedSetAsync(this ISocketMessageChannel channel, ITrainerInfo sav, ShowdownSet set, Dictionary<string, bool>? userHTPreferences = null, byte requestedLanguage = 0, TrainerOverride? trainerOverride = null)
    {
        if (set.Species <= 0)
        {
            await channel.SendMessageAsync(
                "Oops! I wasn't able to interpret your message! If you intended to convert something, please double check what you're pasting!"
            ).ConfigureAwait(false);
            return;
        }

        try
        {
            // Check if this is an egg request based on nickname
            bool isEggRequest = set.Nickname.Equals("egg", StringComparison.CurrentCultureIgnoreCase)
                                && Breeding.CanHatchAsEgg(set.Species);

            PKM pkm;
            string result;
            IBattleTemplate? template = null;

            if (isEggRequest)
            {
                // Wrap the ShowdownSet directly in a RegenTemplate
                var regenTemplate = new RegenTemplate(set);

                // Generate egg (also applies the user's batch commands, e.g. .Scale=)
                pkm = AutoLegalityWrapper.GenerateEgg(sav, regenTemplate, out var eggResult);
                result = eggResult.ToString();
            }
            else
            {
                // Generate normally
                template = AutoLegalityWrapper.GetTemplate(set);
                pkm = sav.GetLegal(template, out result);
            }

            if (pkm == null)
            {
                await channel.SendMessageAsync("Failed to generate Pokémon from your set.").ConfigureAwait(false);
                return;
            }

            // Apply requested language now, before legality checks, so the analysis
            // sees the correct language. ALM only applies RegenTemplate language when
            // OT/TID/SID are also present; we must set it explicitly otherwise.
            if (requestedLanguage != 0)
                ApplyLanguageToSet(pkm, set, requestedLanguage);

            // Apply user-supplied OT/TID/SID from the convert command before legality
            // checks. ALM generates with the bot's configured sav, so the result keeps
            // the bot's default trainer info unless we explicitly override it here.
            if (trainerOverride is not null && trainerOverride.HasAny)
            {
                LogUtil.LogInfo($"Convert TrainerOverride = Requested OT: {trainerOverride.OT} | Requested TID: {trainerOverride.TID} | Requested SID: {trainerOverride.SID} | Requested OTGender: {(trainerOverride.OTGender is null ? "none" : trainerOverride.OTGender == 0 ? "Male" : "Female")} | Species: {pkm.Species} | Before OT: {pkm.OriginalTrainerName} | Before TID: {pkm.TrainerTID7} | Before SID: {pkm.TrainerSID7} | Before OTGender: {(pkm.OriginalTrainerGender == 0 ? "Male" : "Female")}", "TrainerOverride");
                ApplyTrainerOverride(pkm, trainerOverride);
                LogUtil.LogInfo($"Convert TrainerOverride = Final OT: {pkm.OriginalTrainerName} | Final TID: {pkm.TrainerTID7} | Final SID: {pkm.TrainerSID7} | Final OTGender: {(pkm.OriginalTrainerGender == 0 ? "Male" : "Female")} | Legal: {new LegalityAnalysis(pkm).Valid}", "TrainerOverride");
            }
            else
            {
                LogUtil.LogInfo($"Convert TrainerOverride = NO OVERRIDE requested in content. ALM's defaults consequentially applied. Trainer Override: {(trainerOverride is null ? "null" : "empty")}", "TrainerOverride");
            }

            var la = new LegalityAnalysis(pkm);
            var spec = GameInfo.Strings.Species[set.Species];

            // If Z-A generation failed and we have a PA9, try every HOME-supported game
            // before giving up — mirrors the same fallback used by the trade path.
            if (!la.Valid && !isEggRequest && pkm is PA9 && template != null)
            {
                var fallback = TryGetAsHomePa9(template, spec);
                if (fallback != null)
                {
                    pkm = fallback;
                    if (requestedLanguage != 0)
                        ApplyLanguageToSet(pkm, set, requestedLanguage);
                    if (trainerOverride is not null && trainerOverride.HasAny)
                        ApplyTrainerOverride(pkm, trainerOverride);
                    la = new LegalityAnalysis(pkm);
                }
            }

            if (!la.Valid)
            {
                var reason = result switch
                {
                    "Timeout" => $"That {spec} set took too long to generate.",
                    "VersionMismatch" => "Request refused: PKHeX and Auto-Legality Mod version mismatch.",
                    _ => $"I wasn't able to create a {spec} from that set."
                };

                var imsg = $"Oops! {reason}";
                if (result == "Failed" && !isEggRequest)
                    imsg += $"\n{AutoLegalityWrapper.GetLegalizationHint(set, sav, pkm)}";

                await channel.SendMessageAsync(imsg).ConfigureAwait(false);
                return;
            }

            var msg = isEggRequest
                ? $"Here's your ({result}) legalized egg for {spec}!"
                : $"Here's your ({result}) legalized PKM & Showdown Set for {spec} ({la.EncounterOriginal.Name})!";
            await channel.SendPKMAsync(pkm, msg + $"\n{ReusableActions.GetFormattedShowdownText(pkm)}").ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            LogUtil.LogSafe(ex, nameof(AutoLegalityExtensionsDiscord));
            var msg = $"Oops! An unexpected problem happened with this Showdown Set:\n```{string.Join("\n", set.GetSetLines())}```\nError: {ex.Message}";
            await channel.SendMessageAsync(msg).ConfigureAwait(false);
        }
    }

    public static async Task ReplyWithLegalizedSetAsync(this ISocketMessageChannel channel, string content, byte gen)
    {
        content = BatchCommandNormalizer.NormalizeBatchCommands(content);
        if (!MetDateValidator.IsValid(content, out var metDateError))
        {
            await channel.SendMessageAsync(metDateError!).ConfigureAwait(false);
            return;
        }
        var userHTPreferences = ParseHyperTrainingCommandsPublic(content);
        content = ReusableActions.StripCodeBlock(content);
        byte requestedLanguage = ExtractAndStripLanguage(ref content);
        var trainerOverride = ExtractAndStripTrainerInfo(ref content);
        var set = new ShowdownSet(content);
        var sav = AutoLegalityWrapper.GetTrainerInfo(gen);
        await channel.ReplyWithLegalizedSetAsync(sav, set, userHTPreferences, requestedLanguage, trainerOverride).ConfigureAwait(false);
    }

    public static async Task ReplyWithLegalizedSetAsync<T>(this ISocketMessageChannel channel, string content) where T : PKM, new()
    {
        content = BatchCommandNormalizer.NormalizeBatchCommands(content);
        if (!MetDateValidator.IsValid(content, out var metDateError))
        {
            await channel.SendMessageAsync(metDateError!).ConfigureAwait(false);
            return;
        }
        var userHTPreferences = ParseHyperTrainingCommandsPublic(content);
        content = ReusableActions.StripCodeBlock(content);
        byte requestedLanguage = ExtractAndStripLanguage(ref content);
        var trainerOverride = ExtractAndStripTrainerInfo(ref content);
        var set = new ShowdownSet(content);
        var sav = AutoLegalityWrapper.GetTrainerInfo<T>();
        await channel.ReplyWithLegalizedSetAsync(sav, set, userHTPreferences, requestedLanguage, trainerOverride).ConfigureAwait(false);
    }

    private static byte ExtractAndStripLanguage(ref string content)
    {
        byte lang = LanguageHelper.GetFinalLanguage(content, null, 0, _ => 0);
        if (lang == 0)
            return 0;
        var lines = content.Split('\n').Where(l => !l.TrimStart().StartsWith("Language:", StringComparison.OrdinalIgnoreCase));
        content = string.Join('\n', lines);
        return lang;
    }

    /// Parses OT/TID/SID lines out of the convert command content and returns them
    /// for explicit application after ALM generation. ShowdownSet does not preserve
    /// these as standard fields, and ALM's RegenSet does not reliably apply them
    /// onto a PKM generated against the bot's configured sav — so we strip and
    /// apply them ourselves.
    private static TrainerOverride? ExtractAndStripTrainerInfo(ref string content)
    {
        string? ot = null;
        uint? tid = null;
        uint? sid = null;
        byte? otGender = null;
        var kept = new List<string>();

        foreach (var raw in content.Split('\n'))
        {
            var trimmed = raw.TrimStart();
            // Check OTGender before OT so the shorter "OT:" prefix doesn't swallow it.
            if (TryConsumePrefix(trimmed, "OTGender:", out var genderVal) && TryParseTrainerGender(genderVal, out var genderParsed))
            {
                otGender = genderParsed;
                continue;
            }
            if (TryConsumePrefix(trimmed, "OT:", out var otVal))
            {
                ot = otVal;
                continue;
            }
            if (TryConsumePrefix(trimmed, "TID:", out var tidVal) && uint.TryParse(tidVal, out var tidParsed))
            {
                tid = tidParsed;
                continue;
            }
            if (TryConsumePrefix(trimmed, "SID:", out var sidVal) && uint.TryParse(sidVal, out var sidParsed))
            {
                sid = sidParsed;
                continue;
            }
            kept.Add(raw);
        }

        if (ot is null && tid is null && sid is null && otGender is null)
            return null;

        content = string.Join('\n', kept);
        return new TrainerOverride(ot, tid, sid, otGender);
    }

    /// Parses an OTGender value into PKHeX's byte convention (0 = Male, 1 = Female).
    /// Accepts "Male"/"M"/"0" and "Female"/"F"/"1", case-insensitive.
    private static bool TryParseTrainerGender(string value, out byte gender)
    {
        switch (value.Trim().ToLowerInvariant())
        {
            case "male" or "m" or "0":
                gender = 0;
                return true;
            case "female" or "f" or "1":
                gender = 1;
                return true;
            default:
                gender = 0;
                return false;
        }
    }

    private static bool TryConsumePrefix(string line, string prefix, out string value)
    {
        if (line.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
        {
            value = line[prefix.Length..].Trim();
            return true;
        }
        value = string.Empty;
        return false;
    }

    /// Applies user-supplied OT/TID/SID onto a generated PKM. Mirrors the trade
    /// flow's TryApplyEarlyAutoOT: captures shiny state first, rebuilds the PID
    /// against the new IDs to preserve the original Square/Star shiny type, and
    /// fully reverts if the result is illegal so we don't ship a broken file.
    private static void ApplyTrainerOverride(PKM pkm, TrainerOverride o)
    {
        var backup = pkm.Clone();
        bool wasShiny = backup.IsShiny;
        uint originalShinyXor = backup.ShinyXor;

        if (o.OT is not null)
        {
            // Clear the trash buffer first — PKHeX's OriginalTrainerName setter
            // writes the new chars + null terminator but does NOT zero out bytes
            // past the new null. Replacing "FreeMons.Org" (12) with "Chris" (5)
            // leaves "ns.Org\0" in the buffer, which the Trainer legality check
            // flags as invalid trash.
            pkm.OriginalTrainerTrash.Clear();
            pkm.OriginalTrainerName = o.OT;
        }
        if (o.TID is not null)
            pkm.TrainerTID7 = o.TID.Value;
        if (o.SID is not null)
            pkm.TrainerSID7 = o.SID.Value;
        if (o.OTGender is not null)
            pkm.OriginalTrainerGender = o.OTGender.Value;

        if (wasShiny && (o.TID is not null || o.SID is not null))
            pkm.PID = (uint)((pkm.TID16 ^ pkm.SID16 ^ (pkm.PID & 0xFFFF) ^ originalShinyXor) << 16) | (pkm.PID & 0xFFFF);

        pkm.RefreshChecksum();

        var la = new LegalityAnalysis(pkm);
        if (!la.Valid)
        {
            var fails = string.Join("; ", la.Results.Where(r => !r.Valid).Select(r => $"{r.Identifier}"));
            LogUtil.LogInfo($"Convert TrainerOverride: REVERT — legality failed: {fails}", "TrainerOverride");
            pkm.OriginalTrainerTrash.Clear();
            backup.OriginalTrainerTrash.CopyTo(pkm.OriginalTrainerTrash);
            pkm.TrainerTID7 = backup.TrainerTID7;
            pkm.TrainerSID7 = backup.TrainerSID7;
            pkm.OriginalTrainerGender = backup.OriginalTrainerGender;
            pkm.PID = backup.PID;
            pkm.RefreshChecksum();
        }
    }

    public sealed record TrainerOverride(string? OT, uint? TID, uint? SID, byte? OTGender)
    {
        public bool HasAny => OT is not null || TID is not null || SID is not null || OTGender is not null;
    }

    public static async Task ReplyWithLegalizedSetAsync(this ISocketMessageChannel channel, IAttachment att)
    {
        var download = await NetUtil.DownloadPKMAsync(att).ConfigureAwait(false);
        if (!download.Success)
        {
            await channel.SendMessageAsync(download.ErrorMessage).ConfigureAwait(false);
            return;
        }

        var pkm = download.Data!;
        if (new LegalityAnalysis(pkm).Valid)
        {
            await channel.SendMessageAsync($"{download.SanitizedFileName}: Already legal.").ConfigureAwait(false);
            return;
        }

        var legal = pkm.LegalizePokemon();
        if (!new LegalityAnalysis(legal).Valid)
        {
            await channel.SendMessageAsync($"{download.SanitizedFileName}: Unable to legalize.").ConfigureAwait(false);
            return;
        }

        legal.RefreshChecksum();

        var msg = $"Here's your legalized PKM for {download.SanitizedFileName}!\n{ReusableActions.GetFormattedShowdownText(legal)}";
        await channel.SendPKMAsync(legal, msg).ConfigureAwait(false);
    }

    /// <summary>
    /// Checks if the normalized content contains hypertrain-related batch commands.
    /// Returns a dictionary of which stats were specified and their values.
    /// If null, no HT commands were specified.
    /// If dictionary contains "ALL" key with value 0, HyperTrainFlags=0 was specified (no HT at all).
    /// </summary>
    public static Dictionary<string, bool>? ParseHyperTrainingCommandsPublic(string content)
    {
        var htFlags = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);

        // Check for .HyperTrainFlags=0 which means disable all hypertraining
        if (content.Contains(".HyperTrainFlags=0", StringComparison.OrdinalIgnoreCase))
        {
            htFlags["ALL"] = false;
            return htFlags;
        }

        // Check for individual HT flags
        if (content.Contains(".HT_HP=", StringComparison.OrdinalIgnoreCase))
        {
            htFlags["HP"] = !content.Contains(".HT_HP=False", StringComparison.OrdinalIgnoreCase);
        }
        if (content.Contains(".HT_ATK=", StringComparison.OrdinalIgnoreCase))
        {
            htFlags["ATK"] = !content.Contains(".HT_ATK=False", StringComparison.OrdinalIgnoreCase);
        }
        if (content.Contains(".HT_DEF=", StringComparison.OrdinalIgnoreCase))
        {
            htFlags["DEF"] = !content.Contains(".HT_DEF=False", StringComparison.OrdinalIgnoreCase);
        }
        if (content.Contains(".HT_SPA=", StringComparison.OrdinalIgnoreCase))
        {
            htFlags["SPA"] = !content.Contains(".HT_SPA=False", StringComparison.OrdinalIgnoreCase);
        }
        if (content.Contains(".HT_SPD=", StringComparison.OrdinalIgnoreCase))
        {
            htFlags["SPD"] = !content.Contains(".HT_SPD=False", StringComparison.OrdinalIgnoreCase);
        }
        if (content.Contains(".HT_SPE=", StringComparison.OrdinalIgnoreCase))
        {
            htFlags["SPE"] = !content.Contains(".HT_SPE=False", StringComparison.OrdinalIgnoreCase);
        }

        return htFlags.Count > 0 ? htFlags : null;
    }

    /// Sets language on a generated PKM, including the Asian OT truncation that
    /// PKHeX requires when the configured OT exceeds 6 characters (the limit
    /// PKHeX enforces for Japanese/Korean/Chinese Pokémon).
    private static void ApplyLanguageToSet(PKM pkm, ShowdownSet set, byte language)
    {
        pkm.Language = (int)language;

        bool isAsian = language is
            (byte)LanguageID.Japanese or
            (byte)LanguageID.Korean or
            (byte)LanguageID.ChineseS or
            (byte)LanguageID.ChineseT;

        if (isAsian && pkm.OriginalTrainerName.Length > 6)
        {
            const string shortOT = "王犬米";
            pkm.OriginalTrainerName = shortOT;
            // Simple property assignment leaves stale trash bytes from the previous
            // longer OT, which PKHeX's Trainer check flags as invalid. Clear them
            // explicitly (same approach used in the trade path's PrepareForTrade).
            var trashBuf = new byte[pkm.TrashCharCountTrainer * 2];
            int trashLen = pkm.SetString(trashBuf, shortOT.AsSpan(), pkm.TrashCharCountTrainer, StringConverterOption.ClearZero);
            pkm.OriginalTrainerTrash.Clear();
            trashBuf.AsSpan(0, trashLen).CopyTo(pkm.OriginalTrainerTrash);
        }

        if (string.IsNullOrEmpty(set.Nickname))
        {
            pkm.Nickname = SpeciesName.GetSpeciesNameGeneration(pkm.Species, pkm.Language, pkm.Format);
            pkm.IsNicknamed = false;
        }
        pkm.RefreshChecksum();
    }

    /// <summary>
    /// Mirrors the HOME fallback in Helpers.TryGetAsHomePa9.
    /// Tries every PKM format HOME supports (newest first) and returns the first
    /// result that converts to a legally valid PA9.
    /// </summary>
    private static PA9? TryGetAsHomePa9(IBattleTemplate template, string speciesName)
    {
        (Func<ITrainerInfo> GetTrainer, string Name)[] sources =
        [
            (() => AutoLegalityWrapper.GetTrainerInfo<PK9>(),  "SV"),
            (() => AutoLegalityWrapper.GetTrainerInfo<PK8>(),  "SWSH"),
            (() => AutoLegalityWrapper.GetTrainerInfo<PA8>(),  "PLA"),
            (() => AutoLegalityWrapper.GetTrainerInfo<PB8>(),  "BDSP"),
            (() => AutoLegalityWrapper.GetTrainerInfo<PK7>(),  "USUM/SM"),
            (() => AutoLegalityWrapper.GetTrainerInfo<PB7>(),  "LGPE"),
            (() => AutoLegalityWrapper.GetTrainerInfo<PK6>(),  "ORAS/XY"),
            (() => AutoLegalityWrapper.GetTrainerInfo<PK5>(),  "BW/B2W2"),
            (() => AutoLegalityWrapper.GetTrainerInfo<PK4>(),  "DPPt/HGSS"),
            (() => AutoLegalityWrapper.GetTrainerInfo<PK3>(),  "RSE/FRLG"),
        ];

        foreach (var (getTrainer, name) in sources)
        {
            try
            {
                var trainerInfo = getTrainer();
                var generated = trainerInfo.GetLegal(template, out _);
                if (generated == null)
                    continue;

                var converted = EntityConverter.ConvertToType(generated, typeof(PA9), out _);
                if (converted is not PA9 pa9)
                    continue;

                if (!new LegalityAnalysis(pa9).Valid)
                    continue;

                LogUtil.LogInfo(
                    $"{speciesName}: HOME fallback succeeded from {name} (Version={pa9.Version})",
                    "PA9HomeFallback");
                return pa9;
            }
            catch { }
        }

        return null;
    }
}
