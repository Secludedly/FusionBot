using Discord;
using Discord.Interactions;
using PKHeX.Core;
using SysBot.Base;
using SysBot.Pokemon.Discord.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SysBot.Pokemon.Discord.Commands.Bots.SlashCommands;

/// <summary>
/// Slash equivalents of $textTrade ($tt / $text) and $textView ($tv).
/// Shared with the prefix command. Both read and write
/// "TradeModule{T}._pendingTextTrades" so a file uploaded with
/// "/texttrade upload" can be viewed with "$tv" and vice versa. The prefix module is untouched.
/// </summary>
[Group("texttrade", "Upload a file of Showdown sets, then browse and trade from it.")]
public class SlashTextTradeModule<T> : InteractionModuleBase<SocketInteractionContext> where T : PKM, new()
{
    private static readonly string[] AllowedExtensions = [".txt", ".csv", ".rtf", ".docx", ".pdf"];

    private static TradeQueueInfo<T> Info => SysCord<T>.Runner.Hub.Queues.Info;

    [SlashCommand("upload", "Upload a txt/csv/rtf/docx/pdf file of Showdown sets.")]
    [RequireQueueRoleInteraction(nameof(DiscordManager.RolesTrade))]
    public async Task UploadAsync(
        [Summary("file", "A text-based file containing Showdown sets.")] IAttachment file)
    {
        await DeferAsync(ephemeral: true).ConfigureAwait(false);

        if (!AllowedExtensions.Any(ext => file.Filename.EndsWith(ext, StringComparison.OrdinalIgnoreCase)))
        {
            await FollowupAsync("❌ Only `.txt`, `.csv`, `.rtf`, `.docx` and `.pdf` files are supported.", ephemeral: true).ConfigureAwait(false);
            return;
        }

        try
        {
            using var http = new HttpClient();
            var data = await http.GetStringAsync(file.Url).ConfigureAwait(false);

            // Sets are separated by "---" or by a blank line, same as the prefix command.
            var blocks = Regex.Split(data, @"(?:---|\r?\n\s*\r?\n)+")
                .Select(b => b.Trim())
                .Where(b => !string.IsNullOrWhiteSpace(b))
                .ToList();

            if (blocks.Count == 0)
            {
                await FollowupAsync("❌ No Showdown sets found in that file.", ephemeral: true).ConfigureAwait(false);
                return;
            }

            TradeModule<T>._pendingTextTrades[Context.User.Id] = blocks;

            var embed = new EmbedBuilder()
                .WithTitle($"📄 Loaded {blocks.Count} set(s) from {file.Filename}")
                .WithColor(Color.DarkPurple)
                .WithFooter("Use /texttrade view index:<n> to preview, or /texttrade trade index:<n> to queue one.");

            foreach (var (block, i) in blocks.Take(20).Select((b, i) => (b, i + 1)))
            {
                var firstLine = block.Split('\n').FirstOrDefault()?.Trim() ?? "(empty)";
                if (firstLine.Length > 100)
                    firstLine = firstLine[..97] + "…";
                embed.AddField($"{i}.", firstLine);
            }

            if (blocks.Count > 20)
                embed.AddField("…", $"and {blocks.Count - 20} more.");

            await FollowupAsync(embed: embed.Build(), ephemeral: true).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            LogUtil.LogSafe(ex, nameof(SlashTextTradeModule<T>));
            await FollowupAsync($"❌ Could not read that file: {ex.Message}", ephemeral: true).ConfigureAwait(false);
        }
    }

    [SlashCommand("view", "Preview one set from your uploaded file.")]
    [RequireCommandAccessInteraction]
    public async Task ViewAsync(
        [Summary("index", "Which set to preview.")][MinValue(1)] int index)
    {
        await DeferAsync(ephemeral: true).ConfigureAwait(false);
        await ShowSetAsync(Context, index).ConfigureAwait(false);
    }

    [SlashCommand("trade", "Queue one or more sets from your uploaded file.")]
    [RequireQueueRoleInteraction(nameof(DiscordManager.RolesTrade))]
    public async Task TradeAsync(
        [Summary("index", "Which set(s) to trade. One number, or several like '1 3 5'.")] string index,

        [Summary("code", "Link Trade code. Leave blank for a random or your stored code.")]
        [MinValue(0)][MaxValue(99999999)]
        int? code = null)
    {
        await DeferAsync(ephemeral: false).ConfigureAwait(false);

        // Accepts "1", "1 3 5" and "1,3,5" -- the prefix command takes space-separated numbers.
        var picks = index
            .Split([' ', ',', ';'], StringSplitOptions.RemoveEmptyEntries)
            .Select(t => int.TryParse(t.Trim(), out var n) ? n : 0)
            .Where(n => n > 0)
            .Distinct()
            .ToList();

        if (picks.Count == 0)
        {
            await FollowupAsync("❌ Provide a set number, e.g. `1`, or several like `1 3 5`.", ephemeral: true).ConfigureAwait(false);
            return;
        }

        int max = SlashBatchHelper<T>.MaxBatchSize();
        if (picks.Count > max)
        {
            await FollowupAsync($"❌ You can only trade up to {max} Pokémon at a time. You selected {picks.Count}.", ephemeral: true).ConfigureAwait(false);
            return;
        }

        try
        {
            var pokemon = new List<T>();
            var errors = new List<BatchTradeError>();

            foreach (var pick in picks)
            {
                if (!TryGetSet(Context.User.Id, pick, out var block, out var error))
                {
                    await FollowupAsync($"❌ {error}", ephemeral: true).ConfigureAwait(false);
                    return;
                }

                var (pk, err, set, hint) = await BatchHelpers<T>.ProcessSingleTradeForBatch(block!).ConfigureAwait(false);
                if (pk != null)
                {
                    TradeModule<T>.TryApplyEarlyAutoOT(pk, Context.User.Id);
                    pokemon.Add(pk);
                }
                else
                {
                    errors.Add(new BatchTradeError
                    {
                        TradeNumber = pick,
                        SpeciesName = set?.Species > 0 ? GameInfo.Strings.Species[set.Species] : "Unknown",
                        ErrorMessage = err ?? "Unknown error.",
                        LegalizationHint = hint,
                        ShowdownSet = block,
                    });
                }
            }

            if (errors.Count > 0)
            {
                await SlashBatchHelper<T>.SendBatchErrorsAsync(Context, errors, picks.Count).ConfigureAwait(false);
                return;
            }

            var tradeCode = code ?? Info.GetRandomTradeCode(Context.User.Id);

            // A single pick goes through the normal trade path so it gets the full single-trade embed;
            // several go through the batch container, exactly as the prefix command does.
            if (pokemon.Count == 1)
                await SlashTradeHelper<T>.QueueTradeAsync(Context, tradeCode, pokemon[0]).ConfigureAwait(false);
            else
                await SlashBatchHelper<T>.QueueBatchAsync(Context, tradeCode, pokemon).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            LogUtil.LogSafe(ex, nameof(SlashTextTradeModule<T>));
            await FollowupAsync("❌ An error occurred while processing those sets.", ephemeral: true).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Shared by "/texttrade view" and the "/textview".
    /// </summary>
    internal static async Task ShowSetAsync(SocketInteractionContext context, int index)
    {
        if (!TryGetSet(context.User.Id, index, out var block, out var error))
        {
            await context.Interaction.FollowupAsync($"❌ {error}", ephemeral: true).ConfigureAwait(false);
            return;
        }

        var text = ReusableActions.StripCodeBlock(block!.Trim());
        if (text.Length > 3900)
            text = text[..3900] + "\n… (truncated)";

        var embed = new EmbedBuilder()
            .WithTitle($"👀 Viewing Set #{index}")
            .WithDescription($"```text\n{text}\n```")
            .WithFooter($"Use /texttrade trade index:{index} to queue this Pokémon.")
            .WithColor(Color.DarkPurple)
            .Build();

        await context.Interaction.FollowupAsync(embed: embed, ephemeral: true).ConfigureAwait(false);
    }

    private static bool TryGetSet(ulong userId, int index, out string? block, out string error)
    {
        block = null;
        error = string.Empty;

        if (!TradeModule<T>._pendingTextTrades.TryGetValue(userId, out var sets) || sets.Count == 0)
        {
            error = "You don't have a TextTrade file loaded. Upload one first with `/texttrade upload`.";
            return false;
        }

        if (index < 1 || index > sets.Count)
        {
            error = $"Invalid set number. Choose between 1 and {sets.Count}.";
            return false;
        }

        block = sets[index - 1];
        return true;
    }
}

/// <summary>
/// Standalone "/textview," matching the $textView ($tv) prefix command name. Delegates to the
/// same implementation as "/texttrade view".
/// </summary>
public class SlashTextViewModule<T> : InteractionModuleBase<SocketInteractionContext> where T : PKM, new()
{
    [SlashCommand("textview", "Preview one set from your uploaded TextTrade file.")]
    [RequireCommandAccessInteraction]
    public async Task TextViewAsync(
        [Summary("index", "Which set to preview.")][MinValue(1)] int index)
    {
        await DeferAsync(ephemeral: true).ConfigureAwait(false);
        await SlashTextTradeModule<T>.ShowSetAsync(Context, index).ConfigureAwait(false);
    }
}
