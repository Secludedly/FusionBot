using Discord.Interactions;
using PKHeX.Core;
using System.Threading.Tasks;

namespace SysBot.Pokemon.Discord.Commands.Bots.SlashCommands;

/// <summary>
/// Slash command equivalent of "CloneModule{T}" ($clone / $c).
/// The prefix module is untouched and keeps working exactly as before; this is an additional entry
/// point so cloning stays available once Discord revokes the Message Content intent bullshit.
/// </summary>
public class SlashCloneModule<T> : InteractionModuleBase<SocketInteractionContext> where T : PKM, new()
{
    private static TradeQueueInfo<T> Info => SysCord<T>.Runner.Hub.Queues.Info;

    [SlashCommand("clone", "Clone the Pokémon you show the bot via Link Trade.")]
    [RequireQueueRoleInteraction(nameof(DiscordManager.RolesClone))]
    public async Task CloneAsync(
        [Summary("code", "Link Trade code (00000000-99999999). Leave blank for a random or your stored code.")]
        [MinValue(0)]
        [MaxValue(99999999)]
        int? code = null)
    {
        // Non-ephemeral defer, matching the pattern already in use by CreatePokemonHelper: the queue
        // embed is a public followup, while every rejection path replies ephemerally.
        await DeferAsync(ephemeral: false).ConfigureAwait(false);

        // GetRandomTradeCode respects the user's stored trade code when StoreTradeCodes is enabled,
        // which is the same behavior the prefix command relies on.
        var tradeCode = code ?? Info.GetRandomTradeCode(Context.User.Id);

        await SlashTradeHelper<T>.QueueSpecialTradeAsync(
            Context,
            tradeCode,
            PokeRoutineType.Clone,
            PokeTradeType.Clone).ConfigureAwait(false);
    }
}
