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
using System.Threading.Tasks;
using DiscordColor = Discord.Color;

namespace SysBot.Pokemon.Discord.Commands.Bots.SlashCommands;

/// <summary>
/// Slash-command counterpart to the three batch-container methods on QueueHelper{T}.
/// Batch command logic for each counterpart is tied to SocketCommandContext.
/// They only differ is in the presentation.
/// </summary>
public static class SlashBatchHelper<T> where T : PKM, new()
{
    private const uint MaxTradeCode = 9999_9999;

    private static TradeQueueInfo<T> Info => SysCord<T>.Runner.Hub.Queues.Info;
    
    /// <summary>
    /// Optional single-embed summary, used instead of per-Pokémon embeds.
    /// </summary>
    public sealed record BatchSummary(string AuthorTitle, string Description, string ImageUrl, string? ThumbnailUrl = null);

    /// <summary>
    /// Queues a batch of Pokémon as one container trade, matching the prefix behavior:
    /// One trade code DM'd once, one queue entry carrying every Pokémon.
    /// </summary>
    public static async Task<bool> QueueBatchAsync(
        SocketInteractionContext context,
        int code,
        List<T> allTrades,
        bool isMysteryEgg = false,
        BatchSummary? summary = null)
    {
        if (allTrades.Count == 0)
        {
            await RespondErrorAsync(context, "No Pokémon to trade.").ConfigureAwait(false);
            return false;
        }

        if ((uint)code > MaxTradeCode)
        {
            await RespondErrorAsync(context, "Trade code should be 00000000-99999999!").ConfigureAwait(false);
            return false;
        }

        var trader = context.User;
        var userID = trader.Id;
        var totalBatchTrades = allTrades.Count;
        var firstTrade = allTrades[0];
        var sig = trader.GetFavor();

        var trainerInfo = new PokeTradeTrainerInfo(trader.Username, userID);
        var notifier = new DiscordTradeNotifier<T>(firstTrade, trainerInfo, code, trader, 1, totalBatchTrades, isMysteryEgg, lgcode: []);

        int uniqueTradeID = GenerateUniqueTradeID();

        var detail = new PokeTradeDetail<T>(firstTrade, trainerInfo, notifier, PokeTradeType.Batch, code,
            sig == RequestSignificance.Favored, null, 1, totalBatchTrades, isMysteryEgg)
        {
            BatchTrades = allTrades,
        };

        var entry = new TradeEntry<T>(detail, userID, PokeRoutineType.Batch, trader.Username, uniqueTradeID: uniqueTradeID);
        var added = Info.AddToTradeQueue(entry, userID, false, sig == RequestSignificance.Owner);

        try
        {
            await EmbedHelper.SendTradeCodeEmbedAsync(trader, code).ConfigureAwait(false);
        }
        catch (HttpException)
        {
            await RespondErrorAsync(context, $"{trader.Mention} - I could not DM you your trade code. Please enable DMs from server members and try again.").ConfigureAwait(false);
            return false;
        }

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
                await context.Interaction.FollowupAsync(embed: new EmbedBuilder()
                    .WithColor(DiscordColor.Red)
                    .WithTitle("🚫 Queue Full")
                    .WithDescription($"The queue is currently full ({maxCount}/{maxCount}). Please try again later when space becomes available.")
                    .WithFooter("Queue will open up as trades are completed")
                    .WithTimestamp(DateTimeOffset.Now)
                    .Build(), ephemeral: true).ConfigureAwait(false);
                return false;

            case QueueResultAdd.NotAllowedItem:
                var held = firstTrade.HeldItem;
                var itemName = held > 0 ? GameInfo.GetStrings("en").Item[held] : "(none)";
                await RespondErrorAsync(context, $"{trader.Mention} - Trade blocked: the held item '{itemName}' cannot be traded in PLZA.").ConfigureAwait(false);
                return false;
        }

        var position = Info.CheckPosition(userID, uniqueTradeID, PokeRoutineType.Batch);
        var botct = Info.Hub.Bots.Count;
        var baseEta = position.Position > botct ? Info.Hub.Config.Queues.EstimateDelay(position.Position, botct) : 0;

        int totalTradeCount = 0;
        TradeCodeStorage.TradeCodeDetails? tradeDetails = null;
        if (SysCord<T>.Runner.Config.Trade.TradeConfiguration.StoreTradeCodes)
        {
            var storage = new TradeCodeStorage();
            totalTradeCount = storage.GetTradeCount(userID);
            tradeDetails = storage.GetTradeDetails(userID);
        }

        await context.Interaction.FollowupAsync(
            $"{trader.Mention} - Added batch trade with {totalBatchTrades} Pokémon to the queue! Position: {position.Position}. Estimated: {baseEta:F1} min(s).").ConfigureAwait(false);

        if (!SysCord<T>.Runner.Config.Trade.TradeEmbedSettings.UseEmbeds)
            return true;

        if (summary != null)
            await PostSummaryEmbedAsync(context, trader, summary, position.Position, baseEta, totalBatchTrades, totalTradeCount, tradeDetails).ConfigureAwait(false);
        else
            await PostPerPokemonEmbedsAsync(context, trader, allTrades, position.Position, baseEta, totalTradeCount, tradeDetails).ConfigureAwait(false);

        return true;
    }

    private static async Task PostSummaryEmbedAsync(SocketInteractionContext context, SocketUser trader,
        BatchSummary summary, int position, double baseEta, int totalBatchTrades, int totalTradeCount,
        TradeCodeStorage.TradeCodeDetails? tradeDetails)
    {
        string footerText = $"Batch of {totalBatchTrades} | Current Queue Position: {position}";
        var userDetailsText = DetailsExtractor<T>.GetUserDetails(totalTradeCount, tradeDetails, trader.Mention);
        if (!string.IsNullOrEmpty(userDetailsText))
            footerText += $"\n{userDetailsText}";
        footerText += $"\nWait Estimate: {baseEta:F1} min(s) for batch";

        var builder = new EmbedBuilder()
            .WithColor(DiscordColor.Blue)
            .WithDescription(summary.Description)
            .WithImageUrl(summary.ImageUrl)
            .WithFooter(footerText)
            .WithAuthor(new EmbedAuthorBuilder()
                .WithName($"{trader.Username}'s {summary.AuthorTitle}")
                .WithIconUrl(trader.GetAvatarUrl() ?? trader.GetDefaultAvatarUrl())
                .WithUrl("https://zepkm.com/pokecreator"));

        if (!string.IsNullOrWhiteSpace(summary.ThumbnailUrl))
            builder.WithThumbnailUrl(summary.ThumbnailUrl);

        DetailsExtractor<T>.AddAdditionalText(builder);

        await context.Interaction.FollowupAsync(embed: builder.Build()).ConfigureAwait(false);
    }

    private static async Task PostPerPokemonEmbedsAsync(SocketInteractionContext context, SocketUser trader,
        List<T> allTrades, int position, double baseEta, int totalTradeCount,
        TradeCodeStorage.TradeCodeDetails? tradeDetails)
    {
        for (int i = 0; i < allTrades.Count; i++)
        {
            var pk = allTrades[i];
            int batchTradeNumber = i + 1;

            var embedData = DetailsExtractor<T>.ExtractPokemonDetails(
                pk, trader, false, false, false, false, false, true, batchTradeNumber, allTrades.Count);

            try
            {
                (string embedImageUrl, DiscordColor embedColor) = await QueueHelper<T>.PrepareEmbedDetails(pk).ConfigureAwait(false);

                embedData.EmbedImageUrl = embedImageUrl;
                embedData.HeldItemUrl = string.Empty;
                if (!string.IsNullOrWhiteSpace(embedData.HeldItem))
                {
                    string heldItemName = embedData.HeldItem.ToLower().Replace(" ", "");
                    embedData.HeldItemUrl = $"https://serebii.net/itemdex/sprites/{heldItemName}.png";
                }

                embedData.IsLocalFile = File.Exists(embedData.EmbedImageUrl);

                string footerText = $"Batch Trade {batchTradeNumber} of {allTrades.Count}";
                if (i == 0)
                {
                    footerText += $" | Current Queue Position: {position}";
                    var userDetailsText = DetailsExtractor<T>.GetUserDetails(totalTradeCount, tradeDetails, trader.Mention);
                    if (!string.IsNullOrEmpty(userDetailsText))
                        footerText += $"\n{userDetailsText}";
                    footerText += $"\nWait Estimate: {baseEta:F1} min(s) for batch";
                }

                var builder = new EmbedBuilder()
                    .WithColor(embedColor)
                    .WithImageUrl(embedData.IsLocalFile ? $"attachment://{Path.GetFileName(embedData.EmbedImageUrl)}" : embedData.EmbedImageUrl)
                    .WithFooter(footerText)
                    .WithAuthor(new EmbedAuthorBuilder()
                        .WithName(embedData.AuthorName)
                        .WithIconUrl(trader.GetAvatarUrl() ?? trader.GetDefaultAvatarUrl())
                        .WithUrl("https://zepkm.com/pokecreator"));

                DetailsExtractor<T>.AddAdditionalText(builder);
                DetailsExtractor<T>.AddNormalTradeFields(builder, embedData, trader.Mention, pk);

                if (pk is IHomeTrack { HasTracker: true })
                {
                    builder.Footer.IconUrl = "https://raw.githubusercontent.com/Secludedly/ZE-FusionBot-Sprite-Images/main/exclamation.gif";
                    builder.AddField("**__Notice__**: **Home Tracker Detected.**", "*AutoOT not applied.*");
                }

                DetailsExtractor<T>.AddThumbnails(builder, false, false, embedData.HeldItemUrl);

                var embed = builder.Build();

                // Followups can't carry a channel-relative attachment:// reference, so a locally
                // rendered sprite has to go to the channel directly, same as the prefix path.
                if (embedData.IsLocalFile)
                {
                    await context.Channel.SendFileAsync(embedData.EmbedImageUrl, embed: embed).ConfigureAwait(false);
                    await QueueHelper<T>.ScheduleFileDeletion(embedData.EmbedImageUrl, 0).ConfigureAwait(false);
                }
                else
                {
                    await context.Interaction.FollowupAsync(embed: embed).ConfigureAwait(false);
                }

                // Small delay between embeds to avoid rate limiting.
                if (i < allTrades.Count - 1)
                    await Task.Delay(500).ConfigureAwait(false);
            }
            catch (HttpException ex)
            {
                await RespondErrorAsync(context, $"Discord returned an error posting trade {batchTradeNumber}: {ex.Message}").ConfigureAwait(false);
            }
        }
    }

    /// <summary>Posts the per-set validation failures, mirroring <c>BatchHelpers.SendBatchErrorEmbedAsync</c>.</summary>
    public static async Task SendBatchErrorsAsync(SocketInteractionContext context, List<BatchTradeError> errors, int totalTrades)
    {
        var embed = new EmbedBuilder()
            .WithTitle("❌ Batch Trade Validation Failed")
            .WithColor(DiscordColor.Red)
            .WithDescription($"{errors.Count} out of {totalTrades} Pokémon could not be processed.")
            .WithFooter("Please fix the invalid sets and try again.");

        // Discord allows up to25 fields per embed.
        foreach (var error in errors.Count > 25 ? errors.GetRange(0, 25) : errors)
        {
            var fieldValue = $"**Error:** {error.ErrorMessage}";
            if (!string.IsNullOrEmpty(error.LegalizationHint))
                fieldValue += $"\n💡 **Hint:** {error.LegalizationHint}";

            if (!string.IsNullOrEmpty(error.ShowdownSet))
            {
                var lines = error.ShowdownSet.Split('\n');
                fieldValue += $"\n**Set:** {string.Join(" | ", lines.Length > 2 ? lines[..2] : lines)}...";
            }

            if (fieldValue.Length > 1024)
                fieldValue = fieldValue[..1021] + "...";

            embed.AddField($"Trade #{error.TradeNumber} - {error.SpeciesName}", fieldValue);
        }

        if (errors.Count > 25)
            embed.WithFooter($"Showing 25 of {errors.Count} errors. Please fix the invalid sets and try again.");

        await context.Interaction.FollowupAsync(embed: embed.Build(), ephemeral: true).ConfigureAwait(false);
    }

    /// <summary>
    /// Shared entry gate for every batch command: the feature toggle, and the "one-trade-per-user" rule to prevent abuse.
    /// </summary>
    public static async Task<bool> EnsureBatchAllowedAsync(SocketInteractionContext context)
    {
        if (!SysCord<T>.Runner.Config.Trade.TradeConfiguration.AllowBatchTrades)
        {
            var app = await context.Client.GetApplicationInfoAsync().ConfigureAwait(false);
            await RespondErrorAsync(context, $"Batch trades are currently disabled by the bot administrator, @{app.Owner}.").ConfigureAwait(false);
            return false;
        }

        if (Info.IsUserInQueue(context.User.Id))
        {
            await RespondErrorAsync(context, "You already have an existing trade in the queue. Please wait until it is processed.").ConfigureAwait(false);
            return false;
        }

        return true;
    }

    /// <summary>
    /// The batch size honors MaxPkmsPerTrade with the prebuilthard cap of 6.
    /// </summary>
    public static int MaxBatchSize()
    {
        var configured = SysCord<T>.Runner.Config.Trade.TradeConfiguration.MaxPkmsPerTrade;
        return Math.Clamp(configured <= 0 ? 1 : configured, 1, 6);
    }

    private static async Task RespondErrorAsync(SocketInteractionContext context, string message)
    {
        if (context.Interaction.HasResponded)
            await context.Interaction.FollowupAsync(message, ephemeral: true).ConfigureAwait(false);
        else
            await context.Interaction.RespondAsync(message, ephemeral: true).ConfigureAwait(false);
    }

    private static int GenerateUniqueTradeID()
    {
        long timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        int randomValue = Random.Shared.Next(1000);
        return (int)((timestamp % int.MaxValue) * 1000 + randomValue);
    }
}
