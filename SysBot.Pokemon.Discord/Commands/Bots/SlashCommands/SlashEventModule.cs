using Discord;
using Discord.Interactions;
using PKHeX.Core;
using SysBot.Pokemon.Discord.Helpers;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SysBot.Pokemon.Discord.Commands.Bots.SlashCommands;

/// <summary>
/// Slash equivalents of $specialrequestpokemon ($srp / $sr) and $geteventpokemon ($gep).
/// The wondercard database lookup, filtering and PKM conversion all come from
/// "SpecialRequestModule{T}", so both surfaces read the same event data. The prefix
/// module is untouched.
/// The prefix version overloads one command to both list and request depending on whether the trailing
/// argument parses as a number. Slash commands make that split explicit instead.
/// </summary>
[Group("wondercard", "Browse and request Pokémon wondercard events from the mystery gift database.")]
public class SlashEventModule<T> : InteractionModuleBase<SocketInteractionContext> where T : PKM, new()
{
    private const int ItemsPerPage = 20;

    private static TradeQueueInfo<T> Info => SysCord<T>.Runner.Hub.Queues.Info;

    [SlashCommand("list", "List wondercard events for a generation or game.")]
    [RequireCommandAccessInteraction]
    public async Task ListAsync(
        [Summary("game", "Which generation or game's event database to read.")]
        [Choice("Gen 4", "gen4")][Choice("Gen 5", "gen5")][Choice("Gen 6", "gen6")][Choice("Gen 7", "gen7")]
        [Choice("Let's Go", "lgpe")][Choice("Sword/Shield", "swsh")][Choice("Legends: Arceus", "pla")]
        [Choice("BD/SP", "bdsp")][Choice("Scarlet/Violet", "gen9")][Choice("Legends: Z-A", "plza")]
        string game,

        [Summary("species", "Only show events for this species.")] string? species = null,
        [Summary("page", "Page number.")][MinValue(1)] int page = 1)
    {
        await DeferAsync(ephemeral: true).ConfigureAwait(false);

        var eventData = SpecialRequestModule<T>.GetEventData(game);
        if (eventData == null)
        {
            await FollowupAsync($"❌ Invalid generation or game: {game}", ephemeral: true).ConfigureAwait(false);
            return;
        }

        var all = SpecialRequestModule<T>.GetFilteredEvents(eventData, species ?? string.Empty).ToArray();
        if (all.Length == 0)
        {
            await FollowupAsync($"No events found for {game}{(string.IsNullOrWhiteSpace(species) ? string.Empty : $" matching '{species}'")}.", ephemeral: true).ConfigureAwait(false);
            return;
        }

        int pageCount = (int)Math.Ceiling(all.Length / (double)ItemsPerPage);
        page = Math.Clamp(page, 1, pageCount);

        var embed = new EmbedBuilder()
            .WithTitle($"Available Events — {game.ToUpperInvariant()}")
            .WithDescription($"Page {page} of {pageCount} · {all.Length} event(s)\nUse `/wondercard request game:{game} index:<number>` to queue one.")
            .WithColor(Color.Blue);

        foreach (var (index, info) in all.Skip((page - 1) * ItemsPerPage).Take(ItemsPerPage))
        {
            // Discord caps a field name at 256 characters.
            var name = $"{index}. {info}";
            embed.AddField(name.Length > 256 ? name[..253] + "…" : name, $"`/wondercard request game:{game} index:{index}`");
        }

        await FollowupAsync(embed: embed.Build(), ephemeral: true).ConfigureAwait(false);
    }

    [SlashCommand("request", "Queue a wondercard event by its number from the list.")]
    [RequireQueueRoleInteraction(nameof(DiscordManager.RolesTrade))]
    public async Task RequestAsync(
        [Summary("game", "Which generation or game's event database to read.")]
        [Choice("Gen 4", "gen4")][Choice("Gen 5", "gen5")][Choice("Gen 6", "gen6")][Choice("Gen 7", "gen7")]
        [Choice("Let's Go", "lgpe")][Choice("Sword/Shield", "swsh")][Choice("Legends: Arceus", "pla")]
        [Choice("BD/SP", "bdsp")][Choice("Scarlet/Violet", "gen9")][Choice("Legends: Z-A", "plza")]
        string game,

        [Summary("index", "The number shown next to the event in /wondercard list.")][MinValue(1)] int index)
    {
        await DeferAsync(ephemeral: false).ConfigureAwait(false);

        if (!TryGetEvent(game, index, out var selected, out var error))
        {
            await FollowupAsync($"❌ {error}", ephemeral: true).ConfigureAwait(false);
            return;
        }

        var pk = SpecialRequestModule<T>.ConvertEventToPKM(selected!);
        if (pk == null)
        {
            await FollowupAsync("❌ That wondercard is not compatible with this bot's game.", ephemeral: true).ConfigureAwait(false);
            return;
        }

        await SlashTradeHelper<T>.QueueTradeAsync(
            Context,
            Info.GetRandomTradeCode(Context.User.Id),
            pk,
            lgcode: Info.GetRandomLGTradeCode()).ConfigureAwait(false);
    }

    [SlashCommand("download", "Get a wondercard event sent to you as a PKM file.")]
    [RequireCommandAccessInteraction]
    public async Task DownloadAsync(
        [Summary("game", "Which generation or game's event database to read.")]
        [Choice("Gen 4", "gen4")][Choice("Gen 5", "gen5")][Choice("Gen 6", "gen6")][Choice("Gen 7", "gen7")]
        [Choice("Let's Go", "lgpe")][Choice("Sword/Shield", "swsh")][Choice("Legends: Arceus", "pla")]
        [Choice("BD/SP", "bdsp")][Choice("Scarlet/Violet", "gen9")][Choice("Legends: Z-A", "plza")]
        string game,

        [Summary("index", "The number shown next to the event in /wondercard list.")][MinValue(1)] int index,

        [Summary("language", "Override the language of the generated file.")]
        [Choice("Japanese", 1)][Choice("English", 2)][Choice("French", 3)][Choice("Italian", 4)]
        [Choice("German", 5)][Choice("Spanish", 7)][Choice("Korean", 8)]
        [Choice("Chinese (Simplified)", 9)][Choice("Chinese (Traditional)", 10)]
        int? language = null)
    {
        await DeferAsync(ephemeral: true).ConfigureAwait(false);

        if (!TryGetEvent(game, index, out var selected, out var error))
        {
            await FollowupAsync($"❌ {error}", ephemeral: true).ConfigureAwait(false);
            return;
        }

        var pk = SpecialRequestModule<T>.ConvertEventToPKM(selected!, (byte?)language);
        if (pk == null)
        {
            await FollowupAsync("❌ That wondercard is not compatible with this bot's game.", ephemeral: true).ConfigureAwait(false);
            return;
        }

        try
        {
            await Context.User.SendPKMAsync(pk).ConfigureAwait(false);
            await FollowupAsync("✅ Sent to you by DM.", ephemeral: true).ConfigureAwait(false);
        }
        catch (global::Discord.Net.HttpException)
        {
            await FollowupAsync("❌ I couldn't DM you. Please check your **Server Privacy Settings** and try again.", ephemeral: true).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Resolves a 1-based index against the entity-only event list, which is the same ordering the
    /// list command numbers against.
    /// </summary>
    private static bool TryGetEvent(string game, int index, out MysteryGift? selected, out string error)
    {
        selected = null;
        error = string.Empty;

        var eventData = SpecialRequestModule<T>.GetEventData(game);
        if (eventData == null)
        {
            error = $"Invalid generation or game: {game}";
            return false;
        }

        var entityEvents = eventData.Where(g => g.IsEntity && !g.IsItem).ToArray();
        if (index < 1 || index > entityEvents.Length)
        {
            error = $"Invalid event index. Use `/wondercard list game:{game}` to see valid numbers (1–{entityEvents.Length}).";
            return false;
        }

        selected = entityEvents[index - 1];
        return true;
    }
}
