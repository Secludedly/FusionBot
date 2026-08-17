using Discord;
using Discord.Interactions;
using PKHeX.Core;
using SysBot.Base;
using SysBot.Pokemon.Discord.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SysBot.Pokemon.Discord.Commands.Bots.SlashCommands;

/// <summary>
/// Slash equivalent of $pokepaste ($pp).
/// Fetching and parsing reuse "Pokepaste.GetPokePasteHtml" and "Pokepaste.ParseShowdownSets"
/// (widened from private), so the same URLs parse the same way. The prefix module is untouched.
/// One difference: the prefix version also renders a combined sprite image of the team with
/// System.Drawing. That is Windows-only (the existing code carries CA1416 suppressions for it) and is
/// the bulk of that command's complexity, so the slash version sends the ZIP of PKM files plus a text
/// summary and skips the image.
/// </summary>
public class SlashPokepasteModule<T> : InteractionModuleBase<SocketInteractionContext> where T : PKM, new()
{
    [SlashCommand("pokepaste", "Generate a team from a PokePaste URL and receive it by DM.")]
    [RequireCommandAccessInteraction]
    public async Task PokepasteAsync(
        [Summary("url", "The PokePaste URL, e.g. https://pokepast.es/xxxxxxxxxxxx")] string url)
    {
        await DeferAsync(ephemeral: true).ConfigureAwait(false);

        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            await FollowupAsync("❌ That doesn't look like a valid URL.", ephemeral: true).ConfigureAwait(false);
            return;
        }

        try
        {
            var html = await Pokepaste.GetPokePasteHtml(url).ConfigureAwait(false);
            var sets = Pokepaste.ParseShowdownSets(html);

            if (sets.Count == 0)
            {
                await FollowupAsync($"❌ No valid Showdown sets found at {url}", ephemeral: true).ConfigureAwait(false);
                return;
            }

            var namer = new DefaultPKMFileNamer();
            var generated = new List<(string Name, byte[] Data)>();
            var skipped = new List<string>();

            foreach (var set in sets)
            {
                var speciesName = GameInfo.Strings.Species[set.Species];
                try
                {
                    var template = AutoLegalityWrapper.GetTemplate(set);
                    var sav = AutoLegalityWrapper.GetTrainerInfo<T>();

                    PKM? pk;
                    using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10)))
                    {
                        try
                        {
                            pk = await Task.Run(() => sav.GetLegal(template, out _), cts.Token).ConfigureAwait(false);
                        }
                        catch (OperationCanceledException)
                        {
                            skipped.Add($"{speciesName} (timed out)");
                            continue;
                        }
                    }

                    if (pk is not T typed || !new LegalityAnalysis(typed).Valid)
                    {
                        skipped.Add($"{speciesName} (could not be made legal)");
                        continue;
                    }

                    generated.Add(($"{namer.GetName(typed)}.{typed.Extension}", typed.Data.ToArray()));
                }
                catch (Exception ex)
                {
                    LogUtil.LogSafe(ex, nameof(SlashPokepasteModule<T>));
                    skipped.Add($"{speciesName} (error)");
                }
            }

            if (generated.Count == 0)
            {
                await FollowupAsync("❌ None of the sets in that paste could be generated.", ephemeral: true).ConfigureAwait(false);
                return;
            }

            using var zipStream = new MemoryStream();
            using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Create, true))
            {
                foreach (var (name, data) in generated)
                {
                    var entry = archive.CreateEntry(name);
                    await using var entryStream = entry.Open();
                    await entryStream.WriteAsync(data).ConfigureAwait(false);
                }
            }
            zipStream.Position = 0;

            var summary = new EmbedBuilder()
                .WithTitle("PokePaste Team")
                .WithDescription($"{generated.Count} of {sets.Count} set(s) generated.")
                .WithColor(Color.Blue);

            if (skipped.Count > 0)
                summary.AddField("Skipped", string.Join("\n", skipped.Take(10)));

            try
            {
                var dm = await Context.User.CreateDMChannelAsync().ConfigureAwait(false);
                await dm.SendFileAsync(zipStream, "pokepasteteam.zip", embed: summary.Build()).ConfigureAwait(false);
                await FollowupAsync($"✅ Sent you {generated.Count} Pokémon by DM.", ephemeral: true).ConfigureAwait(false);
            }
            catch (global::Discord.Net.HttpException)
            {
                await FollowupAsync("❌ I couldn't DM you the team. Please check your **Server Privacy Settings** and try again.", ephemeral: true).ConfigureAwait(false);
            }
        }
        catch (Exception ex)
        {
            LogUtil.LogSafe(ex, nameof(SlashPokepasteModule<T>));
            await FollowupAsync($"❌ Could not read that PokePaste: {ex.Message}", ephemeral: true).ConfigureAwait(false);
        }
    }
}
