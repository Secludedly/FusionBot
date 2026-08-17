using Discord.Interactions;
using Discord.WebSocket;
using FusionBot.Modules;
using PKHeX.Core;
using System.Linq;
using System.Threading.Tasks;

namespace SysBot.Pokemon.Discord.Commands.Bots.SlashCommands;

/// <summary>
/// Slash command equivalent of $myinfo ($mi) from "ProfileCardModule."
/// The prefix command is unchanged.
/// </summary>
public class SlashProfileModule<T> : InteractionModuleBase<SocketInteractionContext> where T : PKM, new()
{
    [SlashCommand("myinfo", "Display your FusionBot profile card.")]
    [RequireCommandAccessInteraction]
    public async Task MyInfoAsync(
        [Summary("user", "Whose profile to show. Defaults to yourself.")] SocketGuildUser? user = null)
    {
        await DeferAsync(ephemeral: false).ConfigureAwait(false);

        var target = user ?? Context.User as SocketGuildUser;
        if (target == null)
        {
            await FollowupAsync("❌ Can't show the profile of a user not in this server!", ephemeral: true).ConfigureAwait(false);
            return;
        }

        var storage = new TradeCodeStorage();
        var tradeDetails = storage.GetTradeDetails(target.Id);
        if (tradeDetails == null)
        {
            await FollowupAsync(target.Id == Context.User.Id
                ? "📊 You haven't traded yet, so no profile data exists!"
                : $"📊 {target.Username} hasn't traded yet, so no profile data exists!", ephemeral: true).ConfigureAwait(false);
            return;
        }

        int totalTrades = storage.GetTradeCount(target.Id);

        var (milestone, medalTitle, embedColor) = ProfileCardModule.GetMedalInfo(totalTrades);
        string medalImageUrl = $"https://raw.githubusercontent.com/Secludedly/ZE-FusionBot-Sprite-Images/main/{milestone:D3}.png";

        int level = ProfileCardModule.CalculateLevel(totalTrades);
        int tradesToNextLevel = ProfileCardModule.CalculateTradesToNextLevel(level, totalTrades);
        double progressPct = (double)totalTrades / ProfileCardModule.MAX_TRADES;
        string progressBar = ProfileCardModule.BuildProgressBar(progressPct);

        string quote = tradeDetails.Quote ?? ProfileCardModule.GenerateRandomQuote();
        var topRole = ProfileCardModule.GetTopRole(target);
        string topRoleDisplay = topRole?.Mention ?? "No Roles";

        string serverName = Context.Guild?.Name ?? "this server";
        string accountCreated = target.CreatedAt.ToString("MMM dd, yyyy");
        string serverJoin = target.JoinedAt?.ToString("MMM dd, yyyy") ?? "Unknown";
        int roleCount = target.Roles.Count(r => !r.IsEveryone);

        var embed = ProfileCardModule.BuildProfileEmbed(
            target,
            tradeDetails.OT ?? "Unknown",
            tradeDetails.TID,
            tradeDetails.SID,
            totalTrades,
            level,
            tradesToNextLevel,
            progressPct,
            progressBar,
            quote,
            medalTitle,
            milestone,
            medalImageUrl,
            embedColor,
            serverName,
            accountCreated,
            serverJoin,
            roleCount,
            topRoleDisplay);

        await FollowupAsync(embed: embed.Build()).ConfigureAwait(false);
    }
}
