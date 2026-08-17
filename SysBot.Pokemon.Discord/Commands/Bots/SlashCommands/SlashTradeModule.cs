using Discord;
using Discord.Interactions;
using PKHeX.Core;
using PKHeX.Core.AutoMod;
using SysBot.Base;
using SysBot.Pokemon.Discord.Commands.Bots.Autocomplete;
using SysBot.Pokemon.Discord.Helpers;
using SysBot.Pokemon.Helpers;
using System;
using System.Threading.Tasks;

namespace SysBot.Pokemon.Discord.Commands.Bots.SlashCommands;

/// <summary>
/// Slash command equivalents of the core trade commands:
/// $trade, $hidetrade, $egg and $itemTrade. The prefix module is untouched.
/// The Showdown pipeline is shared rather than reimplemented so a slash trade and a prefix
/// trade produces the same Pokémon from the same input.
/// </summary>
public class SlashTradeModule<T> : InteractionModuleBase<SocketInteractionContext> where T : PKM, new()
{
    private static TradeQueueInfo<T> Info => SysCord<T>.Runner.Hub.Queues.Info;

    // ===========================
    // TRADE  ($trade / $t)
    // ===========================
    [SlashCommand("trade", "Trade a Pokémon from a Showdown set or an uploaded PKM file.")]
    [RequireQueueRoleInteraction(nameof(DiscordManager.RolesTrade))]
    public Task TradeAsync(
        [Summary("set", "Showdown set. Separate lines with ';' e.g. Garchomp @ Life Orb; Jolly Nature; Shiny: Yes")]
        string? set = null,

        [Summary("file", "A PKM file to trade instead of a Showdown set.")]
        IAttachment? file = null,

        [Summary("code", "Link Trade code. Leave blank for a random or your stored code.")]
        [MinValue(0)][MaxValue(99999999)]
        int? code = null,

        [Summary("ignore-autoot", "Skip AutoOT so the Pokémon keeps its original trainer info.")]
        bool ignoreAutoOT = false)
        => RunTradeAsync(set, file, code, ignoreAutoOT, isHiddenTrade:false);

    // ===========================
    // HIDETRADE  ($hidetrade / $ht)
    // ===========================
    [SlashCommand("hidetrade", "Trade a Pokémon without showing the trade details in the channel.")]
    [RequireQueueRoleInteraction(nameof(DiscordManager.RolesTrade))]
    public Task HideTradeAsync(
        [Summary("set", "Showdown set. Separate lines with ';' e.g. Garchomp @ Life Orb; Jolly Nature; Shiny: Yes")]
        string? set = null,

        [Summary("file", "A PKM file to trade instead of a Showdown set.")]
        IAttachment? file = null,

        [Summary("code", "Link Trade code. Leave blank for a random or your stored code.")]
        [MinValue(0)][MaxValue(99999999)]
        int? code = null,

        [Summary("ignore-autoot", "Skip AutoOT so the Pokémon keeps its original trainer info.")]
        bool ignoreAutoOT = false)
        => RunTradeAsync(set, file, code, ignoreAutoOT, isHiddenTrade:true);

    private async Task RunTradeAsync(string? set, IAttachment? file, int? code, bool ignoreAutoOT, bool isHiddenTrade)
    {
        // With neither a set nor a file, open the multi-line modal instead of erroring. This MUST happen
        // before any defer. A modal can only be an interaction's initial response.
        if (string.IsNullOrWhiteSpace(set) && file == null)
        {
            var id = $"fusion_trade:{(isHiddenTrade ? 1 : 0)}:{(ignoreAutoOT ? 1 : 0)}";
            await RespondWithModalAsync<TradeSetModal>(id).ConfigureAwait(false);
            return;
        }

        // Hidden trades defer ephemerally so nothing at all surfaces publicly, not even a placeholder.
        await DeferAsync(ephemeral: isHiddenTrade).ConfigureAwait(false);

        if (!string.IsNullOrWhiteSpace(set) && file != null)
        {
            await FollowupAsync("❌ Provide a Showdown set or a file, not both.", ephemeral: true).ConfigureAwait(false);
            return;
        }

        var tradeCode = code ?? Info.GetRandomTradeCode(Context.User.Id);

        if (file != null)
            await TradeFromFileAsync(file, tradeCode, ignoreAutoOT, isHiddenTrade).ConfigureAwait(false);
        else
            await TradeFromShowdownAsync(SlashShowdownText.ToMultiline(set!), tradeCode, ignoreAutoOT, isHiddenTrade).ConfigureAwait(false);
    }

    /// <summary>
    /// Handles submission. The two wildcard segments carry the hidden and
    /// ignore-AutoOT flags across, since a modal submission is a fresh interaction that
    /// remembers nothing about the command that opened it.
    /// </summary>
    [ModalInteraction("fusion_trade:*:*")]
    [RequireQueueRoleInteraction(nameof(DiscordManager.RolesTrade))]
    public async Task TradeModalAsync(string hiddenFlag, string ignoreAutoOTFlag, TradeSetModal modal)
    {
        bool isHiddenTrade = hiddenFlag == "1";
        bool ignoreAutoOT = ignoreAutoOTFlag == "1";

        await DeferAsync(ephemeral: isHiddenTrade).ConfigureAwait(false);

        if (string.IsNullOrWhiteSpace(modal.Showdown))
        {
            await FollowupAsync("❌ No Showdown set provided.", ephemeral: true).ConfigureAwait(false);
            return;
        }

        int tradeCode;
        if (!string.IsNullOrWhiteSpace(modal.Code))
        {
            if (!int.TryParse(modal.Code.Trim(), out tradeCode) || tradeCode < 0 || tradeCode > 99999999)
            {
                await FollowupAsync("❌ Trade code must be a number between 00000000 and 99999999.", ephemeral: true).ConfigureAwait(false);
                return;
            }
        }
        else
        {
            tradeCode = Info.GetRandomTradeCode(Context.User.Id);
        }

        await TradeFromShowdownAsync(SlashShowdownText.ToMultiline(modal.Showdown), tradeCode, ignoreAutoOT, isHiddenTrade).ConfigureAwait(false);
    }

    /// <summary>
    /// Mirrors "TradeModule.ProcessTradeAttachmentAsync."
    /// </summary>
    private async Task TradeFromFileAsync(IAttachment file, int code, bool ignoreAutoOT, bool isHiddenTrade)
    {
        var download = await NetUtil.DownloadPKMAsync(file).ConfigureAwait(false);
        if (!download.Success)
        {
            await FollowupAsync($"❌ {download.ErrorMessage}", ephemeral: true).ConfigureAwait(false);
            return;
        }

        var converted = EntityConverter.ConvertToType(download.Data!, typeof(T), out _);
        if (converted is not T pk)
        {
            await FollowupAsync($"❌ That file could not be converted to this bot's format ({typeof(T).Name}).", ephemeral: true).ConfigureAwait(false);
            return;
        }

        pk.RefreshChecksum();
        TradeModule<T>.TryApplyEarlyAutoOT(pk, Context.User.Id, ignoreAutoOT);

        await SlashTradeHelper<T>.QueueTradeAsync(Context, code, pk, isHiddenTrade, ignoreAutoOT: ignoreAutoOT).ConfigureAwait(false);
    }

    /// <summary>
    /// Mirrors "TradeModule.ProcessTradeAsync."
    /// Expects real multi-line text.
    /// </summary>
    private async Task TradeFromShowdownAsync(string content, int code, bool ignoreAutoOT, bool isHiddenTrade)
    {
        try
        {
            content = BatchCommandNormalizer.NormalizeBatchCommands(content);

            // Must run AFTER normalization because the validator only matches the .MetDate=YYYYMMDD batch form.
            if (!MetDateValidator.IsValid(content, out var metDateError))
            {
                await FollowupAsync($"❌ {metDateError}", ephemeral: true).ConfigureAwait(false);
                return;
            }

            content = ReusableActions.StripCodeBlock(content);

            // Explicit trainer info in the set overrides AutoOT, same rule as the prefix command.
            bool skipAutoOT = ignoreAutoOT
                || content.Contains("OT:")
                || content.Contains("TID:")
                || content.Contains("SID:");

            if (TradeExtensions<T>.ContainsAdText(content, out _))
            {
                await FollowupAsync("❌ That set contains advertising text and was rejected.", ephemeral: true).ConfigureAwait(false);
                return;
            }

            var processed = await Helpers<T>.ProcessShowdownSetAsync(content, skipAutoOT).ConfigureAwait(false);
            if (processed.Pokemon == null)
            {
                var reason = processed.Error ?? "Unknown error.";
                if (!string.IsNullOrWhiteSpace(processed.LegalizationHint))
                    reason += $"\n\n{processed.LegalizationHint}";
                if (reason.Length > 1800)
                    reason = reason[..1800] + "\n… (truncated)";

                await FollowupAsync($"❌ {reason}", ephemeral: true).ConfigureAwait(false);
                return;
            }

            var pk = processed.Pokemon;
            bool hasExplicitLanguage = content.Contains("Language:", StringComparison.OrdinalIgnoreCase);
            TradeModule<T>.TryApplyEarlyAutoOT(pk, Context.User.Id, skipAutoOT, hasExplicitLanguage);

            await SlashTradeHelper<T>.QueueTradeAsync(
                Context, code, pk, isHiddenTrade,
                lgcode: processed.LgCode,
                ignoreAutoOT: skipAutoOT,
                isNonNative: processed.IsNonNative).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            LogUtil.LogSafe(ex, nameof(SlashTradeModule<T>));
            await FollowupAsync("❌ An unexpected problem happened with that Showdown set. Try `/convert` instead, or remove some information.", ephemeral: true).ConfigureAwait(false);
        }
    }

    // ===========================
    // EGG  ($egg)
    // ===========================
    [SlashCommand("egg", "Trade an egg generated from the provided Pokémon.")]
    [RequireQueueRoleInteraction(nameof(DiscordManager.RolesTrade))]
    public async Task TradeEggAsync(
        [Summary("set", "Species, or a Showdown set. Leave blank to open a larger input box.")]
        string? set = null,

        [Summary("code", "Link Trade code. Leave blank for a random or your stored code.")]
        [MinValue(0)][MaxValue(99999999)]
        int? code = null)
    {
        // A modal must be the initial response, so this has to precede any defer.
        if (string.IsNullOrWhiteSpace(set))
        {
            await RespondWithModalAsync<TradeSetModal>("fusion_egg").ConfigureAwait(false);
            return;
        }

        await DeferAsync(ephemeral: false).ConfigureAwait(false);
        await RunEggAsync(set, code).ConfigureAwait(false);
    }

    /// <summary>
    /// Handles the multi-line egg modal.
    /// </summary>
    [ModalInteraction("fusion_egg")]
    [RequireQueueRoleInteraction(nameof(DiscordManager.RolesTrade))]
    public async Task TradeEggModalAsync(TradeSetModal modal)
    {
        await DeferAsync(ephemeral: false).ConfigureAwait(false);

        if (string.IsNullOrWhiteSpace(modal.Showdown))
        {
            await FollowupAsync("❌ No Showdown set provided.", ephemeral: true).ConfigureAwait(false);
            return;
        }

        int? code = null;
        if (!string.IsNullOrWhiteSpace(modal.Code))
        {
            if (!int.TryParse(modal.Code.Trim(), out var parsed) || parsed < 0 || parsed > 99999999)
            {
                await FollowupAsync("❌ Trade code must be a number between 00000000 and 99999999.", ephemeral: true).ConfigureAwait(false);
                return;
            }
            code = parsed;
        }

        await RunEggAsync(modal.Showdown, code).ConfigureAwait(false);
    }

    private async Task RunEggAsync(string set, int? code)
    {
        var tradeCode = code ?? Info.GetRandomTradeCode(Context.User.Id);

        try
        {
            var content = SlashShowdownText.ToMultiline(set);
            content = BatchCommandNormalizer.NormalizeBatchCommands(content);
            content = ReusableActions.StripCodeBlock(content);

            var showdown = new ShowdownSet(content);

            // GetTemplate parses Ball:/.Scale= into the template's Regen AND consumes those lines from
            // set.InvalidLines, so this exact instance must be the one handed to GenerateEgg. Building a
            // second RegenTemplate here would silently drop the user's ball and batch commands.
            var template = AutoLegalityWrapper.GetTemplate(showdown);
            var sav = AutoLegalityWrapper.GetTrainerInfo<T>();

            var generated = AutoLegalityWrapper.GenerateEgg(sav, template, out var result);
            if (result != LegalizationResult.Regenerated)
            {
                await FollowupAsync(result == LegalizationResult.Timeout
                    ? "❌ Egg generation took too long and the bot timed out."
                    : "❌ Failed to generate an egg from that set. Try removing possible illegal lines and try again.",
                    ephemeral: true).ConfigureAwait(false);
                return;
            }

            generated = EntityConverter.ConvertToType(generated, typeof(T), out _) ?? generated;
            if (generated is not T pk)
            {
                await FollowupAsync("❌ I wasn't able to create an egg for that. Try removing possible illegal lines and try again.", ephemeral: true).ConfigureAwait(false);
                return;
            }

            await SlashTradeHelper<T>.QueueTradeAsync(Context, tradeCode, pk).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            LogUtil.LogSafe(ex, nameof(SlashTradeModule<T>));
            await FollowupAsync("❌ An error occurred while processing that egg request.", ephemeral: true).ConfigureAwait(false);
        }
    }

    // ===========================
    // ITEMTRADE  ($itemTrade / $it / $item)
    // ===========================
    [SlashCommand("itemtrade", "Trade a Pokémon holding the item you request.")]
    [RequireQueueRoleInteraction(nameof(DiscordManager.RolesTrade))]
    public async Task ItemTradeAsync(
        [Summary("item", "The held item you want.")]
        [Autocomplete(typeof(ItemAutocompleteCurrentGameHandler))]
        string item,

        [Summary("code", "Link Trade code. Leave blank for a random or your stored code.")]
        [MinValue(0)][MaxValue(99999999)]
        int? code = null)
    {
        await DeferAsync(ephemeral: false).ConfigureAwait(false);

        var tradeCode = code ?? Info.GetRandomTradeCode(Context.User.Id);

        try
        {
            // Same carrier species the prefix command uses.
            Species species = Info.Hub.Config.Trade.TradeConfiguration.ItemTradeSpecies == Species.None
                ? Species.Diglett
                : Info.Hub.Config.Trade.TradeConfiguration.ItemTradeSpecies;

            var showdown = new ShowdownSet($"{SpeciesName.GetSpeciesNameGeneration((ushort)species, 2, 8)} @ {item.Trim()}");
            var template = AutoLegalityWrapper.GetTemplate(showdown);
            var sav = AutoLegalityWrapper.GetTrainerInfo<T>();
            var generated = sav.GetLegal(template, out var result);

            if (generated == null)
            {
                await FollowupAsync("❌ That set took too long to legalize.", ephemeral: true).ConfigureAwait(false);
                return;
            }

            generated = EntityConverter.ConvertToType(generated, typeof(T), out _) ?? generated;

            if (generated.HeldItem == 0)
            {
                await FollowupAsync($"❌ {Context.User.Username}, the item you entered wasn't recognized.", ephemeral: true).ConfigureAwait(false);
                return;
            }

            if (generated is not T pk)
            {
                var reason = result == "Timeout" ? "That set took too long to generate." : "I wasn't able to create something from that.";
                await FollowupAsync($"❌ {reason}", ephemeral: true).ConfigureAwait(false);
                return;
            }

            TradeModule<T>.TryApplyEarlyAutoOT(pk, Context.User.Id);
            pk.ResetPartyStats();

            await SlashTradeHelper<T>.QueueTradeAsync(Context, tradeCode, pk).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            LogUtil.LogSafe(ex, nameof(SlashTradeModule<T>));
            await FollowupAsync("❌ An error occurred while processing that item trade.", ephemeral: true).ConfigureAwait(false);
        }
    }
}
