using Discord.Interactions;
using PKHeX.Core;
using System.Threading.Tasks;

namespace SysBot.Pokemon.Discord.Commands.Bots.SlashCommands;

/// <summary>
/// Slash equivalents of $listevents ($le) and $eventrequest ($er).
/// Grouped as "/events list" and "/events request" so they read as one feature.
/// </summary>
[Group("events", "Browse and request event Pokémon files.")]
public class SlashEventsModule<T> : InteractionModuleBase<SocketInteractionContext> where T : PKM, new()
{
    private static string Folder => SysCord<T>.Runner.Config.Folder.EventsFolder;

    [SlashCommand("list", "List the available event files.")]
    [RequireCommandAccessInteraction]
    public async Task ListAsync(
        [Summary("filter", "Only show entries containing this text.")] string? filter = null,
        [Summary("page", "Page number.")][MinValue(1)] int page = 1)
    {
        await DeferAsync(ephemeral: true).ConfigureAwait(false);
        await SlashFileLibraryHelper<T>.ListAsync(Context, Folder, "events", "events request", filter, page).ConfigureAwait(false);
    }

    [SlashCommand("request", "Queue an event file by its number from the list.")]
    [RequireQueueRoleInteraction(nameof(DiscordManager.RolesTrade))]
    public async Task RequestAsync(
        [Summary("index", "The number shown next to the entry in /events list.")][MinValue(1)] int index)
    {
        await DeferAsync(ephemeral: false).ConfigureAwait(false);
        await SlashFileLibraryHelper<T>.RequestAsync(Context, Folder, index, "event", "events list").ConfigureAwait(false);
    }
}

/// <summary>
/// Slash equivalents of $battlereadylist ($brl) and $battlereadyrequest ($brr / $br).
/// </summary>
[Group("battleready", "Browse and request battle-ready Pokémon files.")]
public class SlashBattleReadyModule<T> : InteractionModuleBase<SocketInteractionContext> where T : PKM, new()
{
    private static string Folder => SysCord<T>.Runner.Config.Folder.BattleReadyPKMFolder;

    [SlashCommand("list", "List the available battle-ready files.")]
    [RequireCommandAccessInteraction]
    public async Task ListAsync(
        [Summary("filter", "Only show entries containing this text.")] string? filter = null,
        [Summary("page", "Page number.")][MinValue(1)] int page = 1)
    {
        await DeferAsync(ephemeral: true).ConfigureAwait(false);
        await SlashFileLibraryHelper<T>.ListAsync(Context, Folder, "battle-ready files", "battleready request", filter, page).ConfigureAwait(false);
    }

    [SlashCommand("request", "Queue a battle-ready file by its number from the list.")]
    [RequireQueueRoleInteraction(nameof(DiscordManager.RolesTrade))]
    public async Task RequestAsync(
        [Summary("index", "The number shown next to the entry in /battleready list.")][MinValue(1)] int index)
    {
        await DeferAsync(ephemeral: false).ConfigureAwait(false);
        await SlashFileLibraryHelper<T>.RequestAsync(Context, Folder, index, "battle-ready file", "battleready list").ConfigureAwait(false);
    }
}

/// <summary>
/// Slash equivalents of $homereadylist ($hrl) and $homereadyrequest ($hrr).
/// Note that every [Command] method in "HOMEReadyModule{T}" is declared
/// private, and Discord.Net only registers public command methods, so none of the prefix
/// HOME-Ready commands are actually reachable. These slash commands are therefore the only working
/// entry point to the HOME-Ready folder. The prefix module is left exactly as it is.
/// </summary>
[Group("homeready", "Browse and request HOME-ready Pokémon files.")]
public class SlashHomeReadyModule<T> : InteractionModuleBase<SocketInteractionContext> where T : PKM, new()
{
    private static string Folder => SysCord<T>.Runner.Config.Folder.HOMEReadyPKMFolder;

    [SlashCommand("list", "List the available HOME-ready files.")]
    [RequireCommandAccessInteraction]
    public async Task ListAsync(
        [Summary("filter", "Only show entries containing this text.")] string? filter = null,
        [Summary("page", "Page number.")][MinValue(1)] int page = 1)
    {
        await DeferAsync(ephemeral: true).ConfigureAwait(false);
        await SlashFileLibraryHelper<T>.ListAsync(Context, Folder, "HOME-ready files", "homeready request", filter, page).ConfigureAwait(false);
    }

    [SlashCommand("request", "Queue a HOME-ready file by its number from the list.")]
    [RequireQueueRoleInteraction(nameof(DiscordManager.RolesTrade))]
    public async Task RequestAsync(
        [Summary("index", "The number shown next to the entry in /homeready list.")][MinValue(1)] int index)
    {
        await DeferAsync(ephemeral: false).ConfigureAwait(false);
        await SlashFileLibraryHelper<T>.RequestAsync(Context, Folder, index, "HOME-ready file", "homeready list").ConfigureAwait(false);
    }
}
