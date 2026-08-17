using Discord.Interactions;
using PKHeX.Core;
using SysBot.Base;
using SysBot.Pokemon.Helpers;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SysBot.Pokemon.Discord.Commands.Bots.SlashCommands;

/// <summary>
/// Slash command equivalents of the random "generate something and trade it" commands:
/// $mysteryegg, $mysterymon and $dittoTrade. The prefix modules are untouched.
/// Generation is delegated to the existing generators of "MysteryEggModule.GenerateLegalMysteryEgg"
/// and "MysteryMonModule.GenerateMysteryMon," so the randomization rules stay
/// in one place. Only the Ditto Showdown set is rebuilt, because the prefix version parses it out
/// of an open form text argument string that slash options replace easily.
/// </summary>
public class SlashGeneratorModule<T> : InteractionModuleBase<SocketInteractionContext> where T : PKM, new()
{
    private static TradeQueueInfo<T> Info => SysCord<T>.Runner.Hub.Queues.Info;

    // ===========================
    // MYSTERY EGG  ($mysteryegg / $me)
    // ===========================
    [SlashCommand("mysteryegg", "Trade a random shiny 6IV egg.")]
    [RequireQueueRoleInteraction(nameof(DiscordManager.RolesTrade))]
    public async Task MysteryEggAsync(
        [Summary("code", "Link Trade code. Leave blank for a random or your stored code.")]
        [MinValue(0)][MaxValue(99999999)]
        int? code = null)
    {
        await DeferAsync(ephemeral: false).ConfigureAwait(false);

        // LGPE has no breeding, so there is nothing to generate.
        if (typeof(T) == typeof(PB7))
        {
            await FollowupAsync("❌ Mystery Eggs are not available for Let's Go Pikachu/Eevee, as the game does not support breeding.", ephemeral: true).ConfigureAwait(false);
            return;
        }

        try
        {
            var pk = MysteryEggModule<T>.GenerateLegalMysteryEgg();
            if (pk == null)
            {
                await FollowupAsync("❌ I couldn't generate a mystery egg right now. Please try again.", ephemeral: true).ConfigureAwait(false);
                return;
            }

            await SlashTradeHelper<T>.QueueTradeAsync(
                Context,
                code ?? Info.GetRandomTradeCode(Context.User.Id),
                pk).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            LogUtil.LogSafe(ex, nameof(SlashGeneratorModule<T>));
            await FollowupAsync("❌ An error occurred while generating your mystery egg.", ephemeral: true).ConfigureAwait(false);
        }
    }

    // ===========================
    // MYSTERY MON  ($mysterymon / $mm / $mystery / $surprise)
    // ===========================
    [SlashCommand("mysterymon", "Trade a completely random Pokémon.")]
    [RequireQueueRoleInteraction(nameof(DiscordManager.RolesTrade))]
    public async Task MysteryMonAsync(
        [Summary("code", "Link Trade code. Leave blank for a random or your stored code.")]
        [MinValue(0)][MaxValue(99999999)]
        int? code = null)
    {
        await DeferAsync(ephemeral: false).ConfigureAwait(false);

        try
        {
            // Same 6 second budget the prefix command allows.
            using var cancel = new CancellationTokenSource(TimeSpan.FromSeconds(6));
            var pk = MysteryMonModule<T>.GenerateMysteryMon(cancel.Token);

            if (pk == null)
            {
                await FollowupAsync("Please try to find your Mystery Pokémon again! Whatever it is, it's still waiting for you!", ephemeral: true).ConfigureAwait(false);
                return;
            }

            await SlashTradeHelper<T>.QueueTradeAsync(
                Context,
                code ?? Info.GetRandomTradeCode(Context.User.Id),
                pk).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            LogUtil.LogSafe(ex, nameof(SlashGeneratorModule<T>));
            await FollowupAsync("Please try to find your Mystery Pokémon again! Whatever it is, it's still waiting for you!", ephemeral: true).ConfigureAwait(false);
        }
    }

    // ===========================
    // DITTO  ($dittoTrade / $dt / $ditto)
    // ===========================
    [SlashCommand("dittotrade", "Trade a Ditto with the stats, language and nature you choose.")]
    [RequireQueueRoleInteraction(nameof(DiscordManager.RolesTrade))]
    public async Task DittoTradeAsync(
        [Summary("ivs", "IV spread as HP/Atk/Def/SpA/SpD/Spe, e.g. 31/0/31/31/31/0. Defaults to 6IV.")]
        string? ivs = null,

        [Summary("language", "Ditto's language.")]
        [Choice("Japanese", "Japanese")][Choice("English", "English")][Choice("French", "French")]
        [Choice("Italian", "Italian")][Choice("German", "German")][Choice("Spanish", "Spanish")]
        [Choice("Korean", "Korean")][Choice("Chinese (Simplified)", "ChineseS")][Choice("Chinese (Traditional)", "ChineseT")]
        string language = "Japanese",

        [Summary("nature", "Ditto's nature.")]
        [Autocomplete(typeof(Autocomplete.NatureAutocompleteHandler))]
        string nature = "Timid",

        [Summary("shiny", "Should the Ditto be shiny? Defaults to yes.")]
        bool shiny = true,

        [Summary("code", "Link Trade code. Leave blank for a random or your stored code.")]
        [MinValue(0)][MaxValue(99999999)]
        int? code = null)
    {
        await DeferAsync(ephemeral: false).ConfigureAwait(false);

        int[] iv = [31, 31, 31, 31, 31, 31];
        bool userSetIVs = false;

        if (!string.IsNullOrWhiteSpace(ivs))
        {
            var parts = ivs.Split('/', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 6)
            {
                await FollowupAsync("❌ IVs must be six values separated by `/`, e.g. `31/0/31/31/31/0`.", ephemeral: true).ConfigureAwait(false);
                return;
            }

            for (int i = 0; i < 6; i++)
            {
                if (!int.TryParse(parts[i].Trim(), out iv[i]) || iv[i] < 0 || iv[i] > 31)
                {
                    await FollowupAsync($"❌ Invalid IV `{parts[i].Trim()}`. Each value must be 0–31.", ephemeral: true).ConfigureAwait(false);
                    return;
                }
            }
            userSetIVs = true;
        }

        try
        {
            // Mirrors the set built by "TradeModule.ProcessDittoTradeAsync," including the fixed
            // OT/TID/SID the prefix command defaults to.
            var sb = new StringBuilder();
            sb.AppendLine("Ditto @ Destiny Knot");
            sb.AppendLine("Level: 100");
            if (shiny)
                sb.AppendLine("Shiny: Yes");
            sb.AppendLine($"{nature} Nature");
            sb.AppendLine($"Language: {language}");
            sb.AppendLine("OT: Ditto");
            sb.AppendLine("TID: 143319");
            sb.AppendLine("SID: 2551");
            sb.AppendLine("OTGender: Male");
            sb.AppendLine($".IV_HP={iv[0]}");
            sb.AppendLine($".IV_ATK={iv[1]}");
            sb.AppendLine($".IV_DEF={iv[2]}");
            sb.AppendLine($".IV_SPA={iv[3]}");
            sb.AppendLine($".IV_SPD={iv[4]}");
            sb.AppendLine($".IV_SPE={iv[5]}");
            sb.AppendLine($"~=Version={GetDefaultDittoVersion()}");

            var set = new ShowdownSet(sb.ToString());
            var template = AutoLegalityWrapper.GetTemplate(set);
            var sav = AutoLegalityWrapper.GetTrainerInfo<T>();
            var generated = sav.GetLegal(template, out _);

            if (generated is not T raw)
            {
                await FollowupAsync("❌ That Ditto took too long to legalize.", ephemeral: true).ConfigureAwait(false);
                return;
            }

            // Applies the Ditto-specific met location / ball handling, preserving requested IVs.
            var pk = TradeExtensions<T>.DittoTrade(raw, userSetIVs ? iv : null);

            await SlashTradeHelper<T>.QueueTradeAsync(
                Context,
                code ?? Info.GetRandomTradeCode(Context.User.Id),
                pk).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            LogUtil.LogSafe(ex, nameof(SlashGeneratorModule<T>));
            await FollowupAsync("❌ An error occurred while creating your Ditto.", ephemeral: true).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Mirrors "TradeModule.GetDefaultDittoVersion," which is private to that module.
    /// </summary>
    private static string GetDefaultDittoVersion()
    {
        if (typeof(T) == typeof(PK9)) return "50";
        if (typeof(T) == typeof(PB8)) return "48";
        return "45"; // PK8 (SWSH) and any other fallback
    }
}
