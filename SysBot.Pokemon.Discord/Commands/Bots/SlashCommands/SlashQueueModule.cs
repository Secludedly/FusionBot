using Discord.Interactions;
using PKHeX.Core;
using System.Threading.Tasks;

namespace SysBot.Pokemon.Discord.Commands.Bots.SlashCommands;

/// <summary>
/// Slash equivalents of the queue commands $queueClear ($qc) and $queueStatus ($qs).
/// The prefix module is untouched.
/// </summary>
[Group("queue", "Check or clear your own place in the trade queue.")]
public class SlashQueueModule<T> : InteractionModuleBase<SocketInteractionContext> where T : PKM, new()
{
    private static TradeQueueInfo<T> Info => SysCord<T>.Runner.Hub.Queues.Info;

    [SlashCommand("clear", "Remove yourself from the trade queue.")]
    [RequireCommandAccessInteraction]
    public async Task ClearAsync()
    {
        await DeferAsync(ephemeral: true).ConfigureAwait(false);

        var result = Info.ClearTrade(Context.User.Id);
        await FollowupAsync(DescribeClearResult(result), ephemeral: true).ConfigureAwait(false);
    }

    [SlashCommand("status", "Check your current position in the trade queue.")]
    [RequireCommandAccessInteraction]
    public async Task StatusAsync()
    {
        await DeferAsync(ephemeral: true).ConfigureAwait(false);

        var userID = Context.User.Id;
        var entry = Info.GetDetail(userID);

        var message = entry == null
            ? "You are not currently in the queue."
            : Info.GetPositionString(userID, entry.UniqueTradeID, entry.Type);

        await FollowupAsync(message, ephemeral: true).ConfigureAwait(false);
    }

    /// <summary>
    /// Mirrors "QueueModule.GetClearTradeMessage," which is private to that module.
    /// </summary>
    private static string DescribeClearResult(QueueResultRemove result) => result switch
    {
        QueueResultRemove.Removed => "Removed your pending trades from the queue.",
        QueueResultRemove.CurrentlyProcessing => "Looks like you have trades currently being processed! Did not remove those from the queue.",
        QueueResultRemove.CurrentlyProcessingRemoved => "Looks like you have trades currently being processed! Removed other pending trades from the queue.",
        QueueResultRemove.NotInQueue => "Sorry, you are not currently in the queue.",
        // The prefix version throws here. A slash command should not surface an unhandled exception
        // to the user for an enum value that is unrecognized.
        _ => "Your queue status could not be determined.",
    };
}
