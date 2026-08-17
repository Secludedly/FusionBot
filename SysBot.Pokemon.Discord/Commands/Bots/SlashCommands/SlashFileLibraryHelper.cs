using Discord;
using Discord.Interactions;
using PKHeX.Core;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SysBot.Pokemon.Discord.Commands.Bots.SlashCommands;

/// <summary>
/// Slash-command counterpart "ListHelpers{T}" for browsing a folder of PKM files and
/// queueing one by index. Backs the events, battle-ready and HOME-ready command groups.
/// The prefix helpers take a "SocketCommandContext," so the browse/queue logic is mirrored rather
/// than shared, but the file loading and queueing still go through the existing public helpers
/// ("Helpers{T}.GetRequest", "SlashTradeHelper{T}"), so a queued file behaves
/// identically to the prefix path.
/// Two deliberate differences from the prefix versions: the list is posted ephemerally instead of by
/// DM (no "please enable your DMs" failure shit, and it doesn't need cleaning up afterwards), and the
/// index is validated against the same ordering the request command uses.
/// </summary>
public static class SlashFileLibraryHelper<T> where T : PKM, new()
{
    private const int ItemsPerPage = 20;

    private static TradeQueueInfo<T> Info => SysCord<T>.Runner.Hub.Queues.Info;

    /// <summary>
    /// Files in the stable order both list and request rely on. Empty if unconfigured.
    /// </summary>
    private static string[] GetFiles(string folderPath)
    {
        if (string.IsNullOrWhiteSpace(folderPath) || !Directory.Exists(folderPath))
            return [];

        return Directory.GetFiles(folderPath)
            .Select(Path.GetFileName)
            .Where(x => !string.IsNullOrEmpty(x))
            .OrderBy(x => x, StringComparer.Ordinal)
            .ToArray()!;
    }

    public static async Task ListAsync(SocketInteractionContext context, string folderPath, string itemType, string requestCommand, string? filter, int page)
    {
        var files = GetFiles(folderPath);
        if (files.Length == 0)
        {
            await context.Interaction.FollowupAsync(
                string.IsNullOrWhiteSpace(folderPath)
                    ? "This bot does not have this feature set up."
                    : $"No {itemType} are available.",
                ephemeral: true).ConfigureAwait(false);
            return;
        }

        // Index is 1-based against the FULL list, so a filtered view still shows usable numbers.
        var matches = files
            .Select((name, i) => (Name: Path.GetFileNameWithoutExtension(name), Index: i + 1))
            .Where(x => string.IsNullOrWhiteSpace(filter) || x.Name.Contains(filter, StringComparison.OrdinalIgnoreCase))
            .ToArray();

        if (matches.Length == 0)
        {
            await context.Interaction.FollowupAsync($"No {itemType} found matching '{filter}'.", ephemeral: true).ConfigureAwait(false);
            return;
        }

        int pageCount = (int)Math.Ceiling(matches.Length / (double)ItemsPerPage);
        page = Math.Clamp(page < 1 ? 1 : page, 1, pageCount);

        var embed = new EmbedBuilder()
            .WithTitle($"Available {char.ToUpper(itemType[0]) + itemType[1..]}"
                       + (string.IsNullOrWhiteSpace(filter) ? string.Empty : $" — Filter: '{filter}'"))
            .WithDescription($"Page {page} of {pageCount} · {matches.Length} result(s)\nUse `/{requestCommand} index:<number>` to request one.")
            .WithColor(Color.Blue);

        foreach (var (name, index) in matches.Skip((page - 1) * ItemsPerPage).Take(ItemsPerPage))
            embed.AddField($"{index}. {name}", $"`/{requestCommand} index:{index}`");

        await context.Interaction.FollowupAsync(embed: embed.Build(), ephemeral: true).ConfigureAwait(false);
    }

    public static async Task RequestAsync(SocketInteractionContext context, string folderPath, int index, string itemType, string listCommand)
    {
        var files = GetFiles(folderPath);
        if (files.Length == 0)
        {
            await context.Interaction.FollowupAsync(
                string.IsNullOrWhiteSpace(folderPath)
                    ? "This bot does not have this feature set up."
                    : $"No {itemType} are available.",
                ephemeral: true).ConfigureAwait(false);
            return;
        }

        if (index < 1 || index > files.Length)
        {
            await context.Interaction.FollowupAsync(
                $"Invalid {itemType} index. Use `/{listCommand}` to see valid numbers (1–{files.Length}).",
                ephemeral: true).ConfigureAwait(false);
            return;
        }

        try
        {
            var fileData = await File.ReadAllBytesAsync(Path.Combine(folderPath, files[index - 1])).ConfigureAwait(false);
            var download = new Download<PKM>
            {
                Data = EntityFormat.GetFromBytes(fileData),
                Success = true,
            };

            var pk = Helpers<T>.GetRequest(download);
            if (pk == null)
            {
                await context.Interaction.FollowupAsync($"Failed to convert that {itemType} file to this bot's PKM type.", ephemeral: true).ConfigureAwait(false);
                return;
            }

            await SlashTradeHelper<T>.QueueTradeAsync(
                context,
                Info.GetRandomTradeCode(context.User.Id),
                pk,
                lgcode: Info.GetRandomLGTradeCode()).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            await context.Interaction.FollowupAsync($"An error occurred: {ex.Message}", ephemeral: true).ConfigureAwait(false);
        }
    }
}
