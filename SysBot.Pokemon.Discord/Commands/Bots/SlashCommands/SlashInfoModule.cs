using Discord;
using Discord.Interactions;
using PKHeX.Core;
using SysBot.Pokemon.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SysBot.Pokemon.Discord.Commands.Bots.SlashCommands;

/// <summary>
/// Slash equivalents of $info ($about / $whoami / $owner / $bot) and $help. Both are open to any user.
/// The prefix modules are untouched.
/// </summary>
public class SlashInfoModule<T> : InteractionModuleBase<SocketInteractionContext> where T : PKM, new()
{
    [SlashCommand("info", "Show information about this bot.")]
    [RequireCommandAccessInteraction]
    public async Task InfoAsync()
    {
        await DeferAsync(ephemeral: true).ConfigureAwait(false);

        var app = await Context.Client.GetApplicationInfoAsync().ConfigureAwait(false);

        var builder = new EmbedBuilder
        {
            Color = new Color(114, 137, 218),
            Description = InfoModule.detail,
        };

        builder.AddField("Info",
            $"- {Format.Bold("Owner")}: {app.Owner} ({app.Owner.Id})\n" +
            $"- {Format.Bold("Original Repo")}: [SysBot.Net](https://github.com/kwsch/SysBot.NET)\n" +
            $"- {Format.Bold("This Bot")}: [FusionBot](https://github.com/Secludedly/FusionBot)\n" +
            $"- {Format.Bold("Forked From")}: [PokeBot](https://github.com/hexbyt3/PokeBot)\n" +
            $"- {Format.Bold("Library")}: Discord.Net ({DiscordConfig.Version})\n" +
            $"- {Format.Bold("Uptime")}: {InfoModule.GetUptime()}\n" +
            $"- {Format.Bold("Runtime")}: {RuntimeInformation.FrameworkDescription} {RuntimeInformation.ProcessArchitecture}\n" +
            $"- {Format.Bold("Buildtime")}: {InfoModule.GetVersionInfo("SysBot.Base", false)}\n" +
            $"- {Format.Bold("FusionBot Version")}: {TradeBot.Version}\n" +
            $"- {Format.Bold("Core & ALM Version")}: {InfoModule.GetVersionInfo("PKHeX.Core")}\n" +
            $"- {Format.Bold("Contributers")}: Kaphotics, hexbyt3, Secludedly\n");

        builder.AddField("Stats",
            $"- {Format.Bold("Heap Size")}: {InfoModule.GetHeapSize()}MiB\n" +
            $"- {Format.Bold("Servers")}: {Context.Client.Guilds.Count}\n" +
            $"- {Format.Bold("Channels")}: {Context.Client.Guilds.Sum(g => g.Channels.Count)}\n" +
            $"- {Format.Bold("Users")}: {Context.Client.Guilds.Sum(g => g.MemberCount)}\n");

        await FollowupAsync("Here's some info about me!", embed: builder.Build(), ephemeral: true).ConfigureAwait(false);
    }

    [SlashCommand("help", "List every slash command this bot offers.")]
    [RequireCommandAccessInteraction]
    public async Task HelpAsync()
    {
        await DeferAsync(ephemeral: true).ConfigureAwait(false);

        var commands = DiscoverCommands();

        var embed = new EmbedBuilder()
            .WithTitle("FusionBot — Slash Commands")
            .WithDescription($"{commands.Sum(g => g.Value.Count)} commands available. Type `/` in the message box to browse them with descriptions and autocomplete.")
            .WithColor(new Color(114, 137, 218))
            .WithFooter($"FusionBot {TradeBot.Version} · some commands require a role or are staff-only");

        foreach (var group in commands.OrderBy(g => CategoryRank(g.Key)).ThenBy(g => g.Key, StringComparer.OrdinalIgnoreCase))
        {
            var body = new StringBuilder();
            foreach (var (name, description) in group.Value.OrderBy(c => c.Name, StringComparer.OrdinalIgnoreCase))
            {
                var line = $"`/{name}` — {description}\n";
                // Discord caps a field value at 1024 characters and would throw otherwise.
                if (body.Length + line.Length > 1000)
                {
                    body.Append('…');
                    break;
                }
                body.Append(line);
            }

            embed.AddField(group.Key, body.ToString());
        }

        await FollowupAsync(embed: embed.Build(), ephemeral: true).ConfigureAwait(false);
    }

    /// <summary>
    /// Builds the command list by reflecting over this assembly's "[SlashCommand]" methods rather
    /// than from a handwritten table, so it can never drift out of step with what is registered.
    /// </summary>
    internal static Dictionary<string, List<(string Name, string Description)>> DiscoverCommands()
    {
        var result = new Dictionary<string, List<(string, string)>>(StringComparer.OrdinalIgnoreCase);

        foreach (var type in Assembly.GetExecutingAssembly().GetTypes())
        {
            if (!type.IsClass || type.IsAbstract)
                continue;

            var groupAttr = type.GetCustomAttribute<GroupAttribute>();

            foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            {
                var cmd = method.GetCustomAttribute<SlashCommandAttribute>();
                if (cmd == null)
                    continue;

                var fullName = groupAttr == null ? cmd.Name : $"{groupAttr.Name} {cmd.Name}";
                var category = groupAttr == null
                    ? CategoriseTopLevel(cmd.Name)
                    : GroupDisplayNames.GetValueOrDefault(groupAttr.Name, groupAttr.Name);

                if (!result.TryGetValue(category, out var list))
                    result[category] = list = [];

                if (!list.Any(x => x.Item1 == fullName))
                    list.Add((fullName, cmd.Description));
            }
        }

        return result.ToDictionary(k => k.Key, v => v.Value, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Friendly headings for the "[Group]" based categories. A group with no entry here falls back
    /// to its raw group name, so adding a new group still shows up in help without a code change.
    /// </summary>
    private static readonly Dictionary<string, string> GroupDisplayNames = new(StringComparer.OrdinalIgnoreCase)
    {
        ["batch"] = "Batch Trading",
        ["battleready"] = "Battle-Ready Trading",
        ["events"] = "Event Trading",
        ["homeready"] = "HOME-Ready Trading",
        ["queue"] = "Queue Management",
        ["texttrade"] = "TXT Trading",
        ["wondercard"] = "Wondercard Trading",
    };

    /// <summary>
    /// Display order for the help sections. Anything not listed sorts to the end alphabetically, so a
    /// new category is never silently dropped, it just lands at the bottom until it's added here.
    /// </summary>
    private static readonly string[] CategoryOrder =
    [
        "Standard Trading",
        "Mystery Trading",
        "PokePaste Trading",
        "Ditto Trading",
        "TXT Trading",
        "Batch Trading",
        "Battle-Ready Trading",
        "Event Trading",
        "Wondercard Trading",
        "HOME-Ready Trading",
        "Legality Tools",
        "Queue Management",
        "Your Account",
        "Other",
    ];

    internal static int CategoryRank(string category)
    {
        int index = Array.FindIndex(CategoryOrder, c => c.Equals(category, StringComparison.OrdinalIgnoreCase));
        return index < 0 ? int.MaxValue : index;
    }

    private static string CategoriseTopLevel(string name) => name switch
    {
        "trade" or "hidetrade" or "egg" or "itemtrade" or "clone" or "dump" or "fixot" or "create" => "Standard Trading",
        "mysteryegg" or "mysterymon" => "Mystery Trading",
        "pokepaste" => "PokePaste Trading",
        "dittotrade" => "Ditto Trading",
        "convert" or "legalize" or "validate" or "verbose" => "Legality Tools",
        "deletetradecode" or "changetradecode" or "myinfo" => "Your Account",
        _ => "Other",
    };
}
