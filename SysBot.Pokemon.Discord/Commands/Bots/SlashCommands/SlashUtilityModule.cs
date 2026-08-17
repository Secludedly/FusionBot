using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using PKHeX.Core;
using SysBot.Pokemon.Discord.Helpers;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SysBot.Pokemon.Discord.Commands.Bots.SlashCommands;

/// <summary>
/// Slash command equivalents of utility commands: $convert, $legalize, $lc, $lcv, $hello and $joke.
/// Every corresponding prefix module is left untouched and keeps working.
/// The legality and legalization work is delegated to the existing public helpers.
/// Results match the prefix commands exactly rather than being reimplemented.
/// </summary>
public class SlashUtilityModule<T> : InteractionModuleBase<SocketInteractionContext> where T : PKM, new()
{
    private static readonly Random Rng = new();

    // ===========================
    // CONVERT  ($convert / $showdown)
    // ===========================
    [SlashCommand("convert", "Convert a Showdown set into RegenTemplate format.")]
    [RequireCommandAccessInteraction]
    public async Task ConvertAsync(
        [Summary("set", "Showdown set. Separate lines with ';' e.g. Pikachu @ Light Ball; Timid Nature; Shiny: Yes")]
        string set,

        [Summary("generation", "Optional target generation/format. Leave blank to use this bot's game.")]
        [MinValue(1)]
        [MaxValue(9)]
        int? generation = null)
    {
        await DeferAsync(ephemeral: true).ConfigureAwait(false);

        var content = SlashShowdownText.ToMultiline(set);
        if (string.IsNullOrWhiteSpace(content))
        {
            await FollowupAsync("❌ No Showdown set provided.", ephemeral: true).ConfigureAwait(false);
            return;
        }

        try
        {
            // Both overloads post their result straight to the channel, same as the prefix command.
            if (generation is { } gen)
                await Context.Channel.ReplyWithLegalizedSetAsync(content, (byte)gen).ConfigureAwait(false);
            else
                await Context.Channel.ReplyWithLegalizedSetAsync<T>(content).ConfigureAwait(false);

            await FollowupAsync("✅ Conversion posted below.", ephemeral: true).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            await FollowupAsync($"❌ Could not convert that set: {ex.Message}", ephemeral: true).ConfigureAwait(false);
        }
    }

    // ===========================
    // LEGALIZE  ($legalize / $alm)
    // ===========================
    [SlashCommand("legalize", "Attempt to legalize an attached PKM file.")]
    [RequireCommandAccessInteraction]
    public async Task LegalizeAsync(
        [Summary("file", "The PKM file to legalize.")] IAttachment file)
    {
        await DeferAsync(ephemeral: true).ConfigureAwait(false);

        try
        {
            await Context.Channel.ReplyWithLegalizedSetAsync(file).ConfigureAwait(false);
            await FollowupAsync("✅ Legalization result posted below.", ephemeral: true).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            await FollowupAsync($"❌ Could not legalize that file: {ex.Message}", ephemeral: true).ConfigureAwait(false);
        }
    }

    // ===========================
    // VALIDATE  ($lc / $check / $validate / $verify)
    // ===========================
    [SlashCommand("validate", "Verify an attached PKM file for legality.")]
    [RequireCommandAccessInteraction]
    public Task ValidateAsync(
        [Summary("file", "The PKM file to check.")] IAttachment file)
        => LegalityCheckAsync(file, verbose: false);

    // ===========================
    // VERBOSE  ($lcv / $verbose)
    // ===========================
    [SlashCommand("verbose", "Verify an attached PKM file for legality, with verbose output.")]
    [RequireCommandAccessInteraction]
    public Task VerboseAsync(
        [Summary("file", "The PKM file to check.")] IAttachment file)
        => LegalityCheckAsync(file, verbose: true);

    /// <summary>
    /// Mirrors "LegalityCheckModule.LegalityCheck," which is private to that module.
    /// </summary>
    private async Task LegalityCheckAsync(IAttachment att, bool verbose)
    {
        await DeferAsync(ephemeral: false).ConfigureAwait(false);

        var download = await NetUtil.DownloadPKMAsync(att).ConfigureAwait(false);
        if (!download.Success)
        {
            await FollowupAsync(download.ErrorMessage, ephemeral: true).ConfigureAwait(false);
            return;
        }

        var pkm = download.Data!;
        var la = new LegalityAnalysis(pkm);
        var builder = new EmbedBuilder
        {
            Color = la.Valid ? Color.Green : Color.Red,
            Description = $"Legality Report for {download.SanitizedFileName}:",
        };

        builder.AddField(x =>
        {
            x.Name = la.Valid ? "Valid" : "Invalid";
            // The report can exceed Discord's 1024 character field limit on a badly illegal file,
            // which would throw. The prefix path has the same exposure; truncate rather than fail.
            var report = la.Report(verbose);
            x.Value = report.Length > 1024 ? report[..1010] + "\n… (truncated)" : report;
            x.IsInline = false;
        });

        await FollowupAsync("Here's the legality report!", embed: builder.Build()).ConfigureAwait(false);
    }

    // ===========================
    // HELLO  ($hello / $hi / $hey / $yo)
    // ===========================
    [SlashCommand("hello", "Say hello to the bot and get a response.")]
    [RequireCommandAccessInteraction]
    public async Task HelloAsync()
    {
        var str = SysCordSettings.Settings.HelloResponse;
        var msg = string.Format(str, Context.User.Mention);

        string? imageUrl = null;
        var urlMatch = Regex.Match(msg, @"(http[s]?:\/\/.*\.(?:png|jpg|gif|jpeg))", RegexOptions.IgnoreCase);
        if (urlMatch.Success)
        {
            imageUrl = urlMatch.Value;
            msg = msg.Replace(imageUrl, "").Trim();
        }

        var embedBuilder = new EmbedBuilder()
            .WithTitle("Hello!")
            .WithDescription(msg)
            .WithColor(Color.Green);

        if (!string.IsNullOrEmpty(imageUrl))
            embedBuilder.WithImageUrl(imageUrl);

        await RespondAsync(embed: embedBuilder.Build()).ConfigureAwait(false);
    }

    // ===========================
    // JOKE  ($joke / $lol / $insult)
    // ===========================
    [SlashCommand("joke", "Tells a random joke.")]
    [RequireCommandAccessInteraction]
    public async Task JokeAsync()
    {
        // Reads the same list the prefix command uses, so both stay in sync automatically.
        var jokes = JokeModule.Jokes;
        await RespondAsync(jokes[Rng.Next(jokes.Count)]).ConfigureAwait(false);
    }
}
