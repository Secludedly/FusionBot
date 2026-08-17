using Discord.Interactions;
using PKHeX.Core;
using System.Threading.Tasks;

namespace SysBot.Pokemon.Discord.Commands.Bots.SlashCommands;

/// <summary>
/// Slash command equivalents of $dump ($d) and $fixOT ($fix / $f). Like $clone, these are "show the bot
/// a Pokémon" routines, so they queue a blank "T" through the same special-trade.
/// The prefix modules are untouched.
/// </summary>
public class SlashSpecialTradeModule<T> : InteractionModuleBase<SocketInteractionContext> where T : PKM, new()
{
    private static TradeQueueInfo<T> Info => SysCord<T>.Runner.Hub.Queues.Info;

    [SlashCommand("dump", "Dump the Pokémon you show the bot via Link Trade.")]
    [RequireQueueRoleInteraction(nameof(DiscordManager.RolesDump))]
    public async Task DumpAsync(
        [Summary("code", "Link Trade code. Leave blank for a random or your stored code.")]
        [MinValue(0)][MaxValue(99999999)]
        int? code = null)
    {
        await DeferAsync(ephemeral: false).ConfigureAwait(false);

        await SlashTradeHelper<T>.QueueSpecialTradeAsync(
            Context,
            code ?? Info.GetRandomTradeCode(Context.User.Id),
            PokeRoutineType.Dump,
            PokeTradeType.Dump).ConfigureAwait(false);
    }

    [SlashCommand("fixot", "Fix the OT and nickname of a Pokémon you show the bot, if an advert is detected.")]
    [RequireQueueRoleInteraction(nameof(DiscordManager.RolesFixOT))]
    public async Task FixOTAsync(
        [Summary("code", "Link Trade code. Leave blank for a random or your stored code.")]
        [MinValue(0)][MaxValue(99999999)]
        int? code = null)
    {
        await DeferAsync(ephemeral: false).ConfigureAwait(false);

        await SlashTradeHelper<T>.QueueSpecialTradeAsync(
            Context,
            code ?? Info.GetRandomTradeCode(Context.User.Id),
            PokeRoutineType.FixOT,
            PokeTradeType.FixOT).ConfigureAwait(false);
    }
}
