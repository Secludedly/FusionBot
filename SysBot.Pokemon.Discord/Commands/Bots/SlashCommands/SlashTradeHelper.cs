using Discord;
using Discord.Interactions;
using Discord.Net;
using Discord.WebSocket;
using PKHeX.Core;
using SysBot.Pokemon.Discord.Helpers;
using SysBot.Pokemon.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DiscordColor = Discord.Color;

namespace SysBot.Pokemon.Discord.Commands.Bots.SlashCommands;

/// <summary>
/// Slash command counterpart to "QueueHelper{T}".
/// Why this exists instead of reusing QueueHelper directly?
/// Every public entry point on "QueueHelper{T}" takes a "Discord.Commands.SocketCommandContext," which can
/// only be built from a "SocketUserMessage." A slash command has no backing message, and the type
/// cannot be constructed from an interaction, so the queue-and-announce wrapper has to be mirrored here.
/// Nothing in the message pipeline is modified or called in a new way by this file. All the
/// heavy lifting is delegated to the existing context-free helpers: "DetailsExtractor,"
/// "QueueHelper.PrepareEmbedDetails," "QueueHelper.CreateLGLinkCodeSpriteEmbed,"
/// "EmbedHelper," "TradeQueueInfo.AddToTradeQueue" so embed layout, queue behavior and
/// trade code delivery stay identical to the prefix commands.
/// </summary>
public static class SlashTradeHelper<T> where T : PKM, new()
{
    private const uint MaxTradeCode = 9999_9999;

    private static TradeQueueInfo<T> Info => SysCord<T>.Runner.Hub.Queues.Info;

    /// <summary>
    /// Queues a "special" trade (Clone, Dump, FixOT or SeedCheck) where the user shows the bot a
    /// Pokémon rather than supplying one. "pk" is a blank "T"
    /// for these routines, matching what the prefix modules pass.
    /// </summary>
    public static async Task<bool> QueueSpecialTradeAsync(
        SocketInteractionContext context,
        int code,
        PokeRoutineType routine,
        PokeTradeType tradeType,
        T? pk = null,
        List<Pictocodes>? lgcode = null)
    {
        if ((uint)code > MaxTradeCode)
        {
            await RespondErrorAsync(context, "Trade code should be 00000000-99999999!").ConfigureAwait(false);
            return false;
        }

        var trader = context.User;
        var userID = trader.Id;

        if (Info.IsUserInQueue(userID))
        {
            await RespondErrorAsync(context, "You already have an existing trade in the queue. Please wait until it is processed.").ConfigureAwait(false);
            return false;
        }

        pk ??= new T();
        lgcode ??= Info.GetRandomLGTradeCode();
        var sig = trader.GetFavor();

        try
        {
            // Trade code delivery: LGPE gets the pictocode sprite sheet, everything else the embed.
            // Same branch QueueHelper.AddToQueueAsync uses.
            if (pk is PB7 && lgcode != null)
            {
                var (spriteFile, spriteEmbed) = QueueHelper<T>.CreateLGLinkCodeSpriteEmbed(lgcode);
                await trader.SendFileAsync(spriteFile, "Your trade code will be.", embed: spriteEmbed).ConfigureAwait(false);
            }
            else
            {
                await EmbedHelper.SendTradeCodeEmbedAsync(trader, code).ConfigureAwait(false);
            }
        }
        catch (HttpException)
        {
            await RespondErrorAsync(context, $"{trader.Mention} - I could not DM you your trade code. Please enable DMs from server members and try again.").ConfigureAwait(false);
            return false;
        }

        var trainer = new PokeTradeTrainerInfo(trader.Username, userID);
        var notifier = new DiscordTradeNotifier<T>(pk, trainer, code, trader, 1, 1, false, lgcode: lgcode!);

        int uniqueTradeID = GenerateUniqueTradeID();

        var detail = new PokeTradeDetail<T>(pk, trainer, notifier, tradeType, code,
            sig == RequestSignificance.Favored, lgcode, 1, 1, false, false, uniqueTradeID);

        var entry = new TradeEntry<T>(detail, userID, PokeRoutineType.LinkTrade, trader.Username, uniqueTradeID);
        var added = Info.AddToTradeQueue(entry, userID, false, sig == RequestSignificance.Owner);

        if (added != QueueResultAdd.AlreadyInQueue && added != QueueResultAdd.NotAllowedItem)
        {
            // Keep the notifier's ID in sync with the queued entry, otherwise the DM position lookup
            // reports the wrong slot. Mirrors the same call in QueueHelper.AddToTradeQueue.
            notifier.UpdateUniqueTradeID(uniqueTradeID);
            await notifier.SendInitialQueueUpdate().ConfigureAwait(false);
        }

        switch (added)
        {
            case QueueResultAdd.AlreadyInQueue:
                await RespondErrorAsync(context, $"{trader.Mention} - You are already in the queue!").ConfigureAwait(false);
                return false;

            case QueueResultAdd.QueueFull:
                var maxCount = SysCord<T>.Runner.Config.Queues.MaxQueueCount;
                var fullEmbed = new EmbedBuilder()
                    .WithColor(DiscordColor.Red)
                    .WithTitle("🚫 Queue Full")
                    .WithDescription($"The queue is currently full ({maxCount}/{maxCount}). Please try again later when space becomes available.")
                    .WithFooter("Queue will open up as trades are completed")
                    .WithTimestamp(DateTimeOffset.Now)
                    .Build();
                await context.Interaction.FollowupAsync(embed: fullEmbed, ephemeral: true).ConfigureAwait(false);
                return false;

            case QueueResultAdd.NotAllowedItem:
                var held = pk.HeldItem;
                var itemName = held > 0 ? GameInfo.GetStrings("en").Item[held] : "(none)";
                await RespondErrorAsync(context, $"{trader.Mention} - Trade blocked: the held item '{itemName}' cannot be traded in PLZA.").ConfigureAwait(false);
                return false;
        }

        await AnnounceAsync(context, pk, trader, routine, uniqueTradeID, isHiddenTrade: false, isNonNative: false).ConfigureAwait(false);
        return true;
    }

    /// <summary>
    /// Queues a normal Link Trade of an actual Pokémon. Mirrors the validation gates in
    /// "Helpers{T}.AddTradeToQueueAsync" tradability, blocked held items, legality (with the
    /// nickname auto-fix), non-native and HOME-tracker rules, etc., then queues and announces.
    /// </summary>
    public static async Task<bool> QueueTradeAsync(
        SocketInteractionContext context,
        int code,
        T pk,
        bool isHiddenTrade = false,
        List<Pictocodes>? lgcode = null,
        bool ignoreAutoOT = false,
        bool isNonNative = false)
    {
        if ((uint)code > MaxTradeCode)
        {
            await RespondErrorAsync(context, "Trade code should be 00000000-99999999!").ConfigureAwait(false);
            return false;
        }

        var trader = context.User;
        var userID = trader.Id;

        if (Info.IsUserInQueue(userID))
        {
            await RespondErrorAsync(context, "You already have an existing trade in the queue. Please wait until it is processed.").ConfigureAwait(false);
            return false;
        }

        lgcode ??= Helpers<T>.GenerateRandomPictocodes(3);

        if (!pk.CanBeTraded())
        {
            await RespondErrorAsync(context, "Provided Pokémon content is blocked from trading!").ConfigureAwait(false);
            return false;
        }

        if (TradeExtensions<T>.IsItemBlocked(pk))
        {
            var blockedItem = pk.HeldItem > 0 ? GameInfo.GetStrings("en").Item[pk.HeldItem] : "(none)";
            await RespondErrorAsync(context, $"Trade blocked: The held item '{blockedItem}' cannot be traded.").ConfigureAwait(false);
            return false;
        }

        var la = new LegalityAnalysis(pk);

        // Auto-fix nickname-only issues by clearing the nickname and re-validating, same as the prefix path.
        if (!la.Valid && la.Results.Any(r => r.Identifier is CheckIdentifier.Nickname))
        {
            var clone = (T)pk.Clone();
            _ = clone.ClearNickname();
            var laNick = new LegalityAnalysis(clone);
            if (laNick.Valid)
            {
                pk = clone;
                la = laNick;
            }
        }

        if (!la.Valid)
        {
            string speciesName = SpeciesName.GetSpeciesName(pk.Species, (int)LanguageID.English);
            string report = la.Report();
            if (report.Length > 1500)
                report = report[..1500] + "\n… (truncated)";

            var message = pk.IsEgg
                ? $"Invalid Showdown Set for the {speciesName} egg. Please review your information and try again.\n\nLegality Report:\n```\n{report}\n```"
                : $"{speciesName} is not legal, and cannot be traded!\n\nLegality Report:\n```\n{report}\n```";

            await RespondErrorAsync(context, message).ConfigureAwait(false);
            return false;
        }

        if (Info.Hub.Config.Legality.DisallowNonNatives && isNonNative)
        {
            string speciesName = SpeciesName.GetSpeciesName(pk.Species, (int)LanguageID.English);
            await RespondErrorAsync(context, $"This **{speciesName}** is not native to this game, and cannot be traded! Trade with the correct bot, then trade to HOME.").ConfigureAwait(false);
            return false;
        }

        if (Info.Hub.Config.Legality.DisallowTracked && pk is IHomeTrack { HasTracker: true })
        {
            string speciesName = SpeciesName.GetSpeciesName(pk.Species, (int)LanguageID.English);
            await RespondErrorAsync(context, $"This {speciesName} file is tracked by HOME, and cannot be traded!").ConfigureAwait(false);
            return false;
        }

        var sig = trader.GetFavor();

        try
        {
            if (pk is PB7 && lgcode != null)
            {
                var (spriteFile, spriteEmbed) = QueueHelper<T>.CreateLGLinkCodeSpriteEmbed(lgcode);
                await trader.SendFileAsync(spriteFile, "Your trade code will be.", embed: spriteEmbed).ConfigureAwait(false);
            }
            else
            {
                await EmbedHelper.SendTradeCodeEmbedAsync(trader, code).ConfigureAwait(false);
            }
        }
        catch (HttpException)
        {
            await RespondErrorAsync(context, $"{trader.Mention} - I could not DM you your trade code. Please enable DMs from server members and try again.").ConfigureAwait(false);
            return false;
        }

        var trainer = new PokeTradeTrainerInfo(trader.Username, userID);
        var notifier = new DiscordTradeNotifier<T>(pk, trainer, code, trader, 1, 1, false, lgcode: lgcode!);

        int uniqueTradeID = GenerateUniqueTradeID();

        var detail = new PokeTradeDetail<T>(pk, trainer, notifier, PokeTradeType.Specific, code,
            sig == RequestSignificance.Favored, lgcode, 1, 1, false, isHiddenTrade, uniqueTradeID, ignoreAutoOT);

        var entry = new TradeEntry<T>(detail, userID, PokeRoutineType.LinkTrade, trader.Username, uniqueTradeID);
        var added = Info.AddToTradeQueue(entry, userID, false, sig == RequestSignificance.Owner);

        if (added != QueueResultAdd.AlreadyInQueue && added != QueueResultAdd.NotAllowedItem)
        {
            notifier.UpdateUniqueTradeID(uniqueTradeID);
            await notifier.SendInitialQueueUpdate().ConfigureAwait(false);
        }

        switch (added)
        {
            case QueueResultAdd.AlreadyInQueue:
                await RespondErrorAsync(context, $"{trader.Mention} - You are already in the queue!").ConfigureAwait(false);
                return false;

            case QueueResultAdd.QueueFull:
                var maxCount = SysCord<T>.Runner.Config.Queues.MaxQueueCount;
                var fullEmbed = new EmbedBuilder()
                    .WithColor(DiscordColor.Red)
                    .WithTitle("🚫 Queue Full")
                    .WithDescription($"The queue is currently full ({maxCount}/{maxCount}). Please try again later when space becomes available.")
                    .WithFooter("Queue will open up as trades are completed")
                    .WithTimestamp(DateTimeOffset.Now)
                    .Build();
                await context.Interaction.FollowupAsync(embed: fullEmbed, ephemeral: true).ConfigureAwait(false);
                return false;

            case QueueResultAdd.NotAllowedItem:
                var held = pk.HeldItem;
                var itemName = held > 0 ? GameInfo.GetStrings("en").Item[held] : "(none)";
                await RespondErrorAsync(context, $"{trader.Mention} - Trade blocked: the held item '{itemName}' cannot be traded in PLZA.").ConfigureAwait(false);
                return false;
        }

        await AnnounceAsync(context, pk, trader, PokeRoutineType.LinkTrade, uniqueTradeID, isHiddenTrade, isNonNative).ConfigureAwait(false);
        return true;
    }

    /// <summary>
    /// Builds and posts the queue embed. Field layout and image URLs are taken from the same
    /// "DetailsExtractor" calls the prefix path uses, so both surfaces look identical.
    /// </summary>
    private static async Task AnnounceAsync(SocketInteractionContext context, T pk, SocketUser trader, PokeRoutineType routine, int uniqueTradeID, bool isHiddenTrade, bool isNonNative)
    {
        bool isClone = routine == PokeRoutineType.Clone;
        bool isDump = routine == PokeRoutineType.Dump;
        bool isFixOT = routine == PokeRoutineType.FixOT;
        bool isSeedCheck = routine == PokeRoutineType.SeedCheck;
        bool isSpecial = isClone || isDump || isFixOT || isSeedCheck;

        int totalTradeCount = 0;
        TradeCodeStorage.TradeCodeDetails? tradeDetails = null;
        if (SysCord<T>.Runner.Config.Trade.TradeConfiguration.StoreTradeCodes)
        {
            var storage = new TradeCodeStorage();
            totalTradeCount = storage.GetTradeCount(trader.Id);
            tradeDetails = storage.GetTradeDetails(trader.Id);
        }

        var embedData = DetailsExtractor<T>.ExtractPokemonDetails(
            pk, trader, false, isClone, isDump, isFixOT, isSeedCheck, false, 1, 1);

        try
        {
            (string embedImageUrl, DiscordColor embedColor) = await QueueHelper<T>.PrepareEmbedDetails(pk).ConfigureAwait(false);

            embedData.EmbedImageUrl =
                isDump ? "https://raw.githubusercontent.com/Secludedly/ZE-FusionBot-Sprite-Images/main/Dumping.png?raw=true&width=300&height=300" :
                isClone ? "https://raw.githubusercontent.com/Secludedly/ZE-FusionBot-Sprite-Images/main/Cloning.png?raw=true&width=300&height=300" :
                isSeedCheck ? "https://raw.githubusercontent.com/Secludedly/ZE-FusionBot-Sprite-Images/main/Seeding.png?raw=true&width=300&height=300" :
                isFixOT ? "https://raw.githubusercontent.com/Secludedly/ZE-FusionBot-Sprite-Images/main/FixOTing.png?raw=true&width=300&height=300" :
                embedImageUrl;

            embedData.HeldItemUrl = string.Empty;
            if (!string.IsNullOrWhiteSpace(embedData.HeldItem))
            {
                string heldItemName = embedData.HeldItem.ToLower().Replace(" ", "");
                embedData.HeldItemUrl = $"https://serebii.net/itemdex/sprites/{heldItemName}.png";
            }

            embedData.IsLocalFile = File.Exists(embedData.EmbedImageUrl);

            var position = Info.CheckPosition(trader.Id, uniqueTradeID, routine);
            var botct = Info.Hub.Bots.Count;
            var baseEta = position.Position > botct ? Info.Hub.Config.Queues.EstimateDelay(position.Position, botct) : 0;

            string footerText = $"Current Queue Position: {(position.Position == -1 ? 1 : position.Position)}";
            string userDetailsText = DetailsExtractor<T>.GetUserDetails(totalTradeCount, tradeDetails, trader.Mention);
            if (!string.IsNullOrEmpty(userDetailsText))
                footerText += $"\n{userDetailsText}";
            footerText += $"\nWait Estimate: {baseEta:F1} min(s) for trade.";
            footerText += $"\nFusionBot {TradeBot.Version}";

            var embedBuilder = new EmbedBuilder()
                .WithColor(embedColor)
                .WithImageUrl(embedData.IsLocalFile ? $"attachment://{Path.GetFileName(embedData.EmbedImageUrl)}" : embedData.EmbedImageUrl)
                .WithFooter(footerText)
                .WithAuthor(new EmbedAuthorBuilder()
                    .WithName(embedData.AuthorName)
                    .WithIconUrl(trader.GetAvatarUrl() ?? trader.GetDefaultAvatarUrl())
                    .WithUrl("https://zepkm.com/pokecreator"));

            DetailsExtractor<T>.AddAdditionalText(embedBuilder);

            if (isSpecial)
                DetailsExtractor<T>.AddSpecialTradeFields(embedBuilder, false, isSeedCheck, isClone, isFixOT, trader.Mention);
            else
                DetailsExtractor<T>.AddNormalTradeFields(embedBuilder, embedData, trader.Mention, pk);

            // Non-native / HOME tracker notices, matching QueueHelper.AddToTradeQueue.
            if (pk is IHomeTrack homeTrack)
            {
                if (homeTrack.HasTracker && isNonNative)
                    AddNotice(embedBuilder, "**__Notice__**: **This Pokemon is Non-Native & Has Home Tracker.**", "*AutoOT not applied.*");
                else if (homeTrack.HasTracker)
                    AddNotice(embedBuilder, "**__Notice__**: **Home Tracker Detected.**", "*AutoOT not applied.*");
                else if (isNonNative)
                    AddNotice(embedBuilder, "**__Notice__**: **This Pokemon is Non-Native.**", "*Cannot enter HOME & AutoOT not applied.*");
            }
            else if (isNonNative)
            {
                AddNotice(embedBuilder, "**__Notice__**: **This Pokemon is Non-Native.**", "*Cannot enter HOME & AutoOT not applied.*");
            }

            DetailsExtractor<T>.AddThumbnails(embedBuilder, isClone, isSeedCheck, embedData.HeldItemUrl);

            // A hidden trade (or embeds turned off) must not post trade details to the channel. The prefix
            // command posts a stripped-down text block; an ephemeral followup is strictly more private,
            // since only the requester can see it at all.
            if (isHiddenTrade || !SysCord<T>.Runner.Config.Trade.TradeEmbedSettings.UseEmbeds)
            {
                var summary = $"▹𝗦𝗨𝗖𝗖𝗘𝗦𝗦𝗙𝗨𝗟𝗟𝗬 𝗔𝗗𝗗𝗘𝗗◃\n" +
                              $"//【𝐏𝐎𝐒𝐈𝐓𝐈𝐎𝐍: {position.Position}】\n";
                if (embedData.SpeciesName != "---")
                    summary += $"//【𝐏𝐎𝐊𝐄𝐌𝐎𝐍: ||{embedData.SpeciesName}||】\n";
                summary += $"//【𝐄𝐓𝐀: {baseEta:F1} Min(s)】";

                await context.Interaction.FollowupAsync(summary, ephemeral: true).ConfigureAwait(false);
                if (embedData.IsLocalFile)
                    await QueueHelper<T>.ScheduleFileDeletion(embedData.EmbedImageUrl, 0).ConfigureAwait(false);
                return;
            }

            var embed = embedBuilder.Build();

            if (embedData.IsLocalFile)
            {
                // A followup cannot carry a local attachment referenced by attachment:// from the channel,
                // so acknowledge the interaction and post the embed + file to the channel like the prefix path.
                await context.Interaction.FollowupAsync("✅ Added to the queue! Check your DMs for the trade code.", ephemeral: true).ConfigureAwait(false);
                await context.Channel.SendFileAsync(embedData.EmbedImageUrl, embed: embed).ConfigureAwait(false);
                await QueueHelper<T>.ScheduleFileDeletion(embedData.EmbedImageUrl, 0).ConfigureAwait(false);
            }
            else
            {
                await context.Interaction.FollowupAsync(embed: embed).ConfigureAwait(false);
            }
        }
        catch (HttpException ex)
        {
            await RespondErrorAsync(context, $"Discord returned an error while posting the trade details: {ex.Message}").ConfigureAwait(false);
        }
    }

    private static void AddNotice(EmbedBuilder builder, string name, string value)
    {
        builder.Footer.IconUrl = "https://raw.githubusercontent.com/Secludedly/ZE-FusionBot-Sprite-Images/main/exclamation.gif";
        builder.AddField(name, value);
    }

    /// <summary>
    /// Responds to the interaction whether or not it has already been deferred or replied to.
    /// </summary>
    private static async Task RespondErrorAsync(SocketInteractionContext context, string message)
    {
        if (context.Interaction.HasResponded)
            await context.Interaction.FollowupAsync(message, ephemeral: true).ConfigureAwait(false);
        else
            await context.Interaction.RespondAsync(message, ephemeral: true).ConfigureAwait(false);
    }

    /// <summary>
    /// Matches "QueueHelper.GenerateUniqueTradeID", which is private to that class.
    /// </summary>
    private static int GenerateUniqueTradeID()
    {
        long timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        int randomValue = Random.Shared.Next(1000);
        return (int)((timestamp % int.MaxValue) * 1000 + randomValue);
    }
}
