using Discord;
using Discord.Interactions;
using PKHeX.Core;
using SysBot.Base;
using SysBot.Pokemon.Discord.Commands.Bots.Autocomplete;
using SysBot.Pokemon.Discord.Helpers;
using SysBot.Pokemon.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace SysBot.Pokemon.Discord.Commands.Bots.SlashCommands;

/// <summary>
/// Slash equivalents of the various batch commands: $batchTrade ($bt), $batchtradezip ($btz),
/// $batchTradeMysteryMon ($btmm), $batchTradeMysteryEgg ($btme) and $itemBatchTrade ($ibt).
/// Set parsing and per-set validation reuses the existing public
/// "BatchHelpers{T}.ParseBatchTradeContent" and "ProcessSingleTradeForBatch," and archive
/// extraction reuses "ArchiveService," so results match the prefix commands.
/// </summary>
[Group("batch", "Trade several Pokémon at once.")]
public class SlashBatchModule<T> : InteractionModuleBase<SocketInteractionContext> where T : PKM, new()
{
    private static readonly string[] PkmExtensions = [".pb7", ".pb8", ".pk8", ".pk9", ".pa8", ".pa9"];
    private static readonly string[] ArchiveExtensions = [".zip", ".rar", ".7z"];

    private static TradeQueueInfo<T> Info => SysCord<T>.Runner.Hub.Queues.Info;

    // ===========================
    // BATCH TRADE  ($bt)
    // ===========================
    [SlashCommand("trade", "Trade several Pokémon from Showdown sets separated by '---'.")]
    [RequireQueueRoleInteraction(nameof(DiscordManager.RolesTrade))]
    public async Task BatchTradeAsync(
        [Summary("sets", "Showdown sets separated by '---'. Leave blank to open a larger input box.")]
        string? sets = null,

        [Summary("code", "Link Trade code. Leave blank for a random or your stored code.")]
        [MinValue(0)][MaxValue(99999999)]
        int? code = null)
    {
        // A modal must be the initial response, so this has to be first.
        if (string.IsNullOrWhiteSpace(sets))
        {
            await RespondWithModalAsync<BatchTradeModal>("fusion_batch").ConfigureAwait(false);
            return;
        }

        await DeferAsync(ephemeral: false).ConfigureAwait(false);
        await RunBatchFromTextAsync(sets, code).ConfigureAwait(false);
    }

    [ModalInteraction("fusion_batch")]
    [RequireQueueRoleInteraction(nameof(DiscordManager.RolesTrade))]
    public async Task BatchTradeModalAsync(BatchTradeModal modal)
    {
        await DeferAsync(ephemeral: false).ConfigureAwait(false);

        int? code = null;
        if (!string.IsNullOrWhiteSpace(modal.Code))
        {
            if (!int.TryParse(modal.Code.Trim(), out var parsed) || parsed < 0 || parsed > 99999999)
            {
                await FollowupAsync("❌ Trade code must be a number between 00000000 and 99999999.", ephemeral: true).ConfigureAwait(false);
                return;
            }
            code = parsed;
        }

        await RunBatchFromTextAsync(modal.Sets, code).ConfigureAwait(false);
    }

    private async Task RunBatchFromTextAsync(string sets, int? code)
    {
        if (!await SlashBatchHelper<T>.EnsureBatchAllowedAsync(Context).ConfigureAwait(false))
            return;

        // The '---' delimiter survives the single-line option, so split before reflowing each block:
        // reflow is per set and would otherwise have no idea where one set ends and the next begins.
        var blocks = BatchHelpers<T>.ParseBatchTradeContent(sets)
            .Select(b => SlashShowdownText.ToMultiline(b))
            .Where(b => !string.IsNullOrWhiteSpace(b))
            .ToList();

        await ProcessSetsAsync(blocks, code).ConfigureAwait(false);
    }

    private async Task ProcessSetsAsync(List<string> blocks, int? code)
    {
        int max = SlashBatchHelper<T>.MaxBatchSize();

        if (blocks.Count == 0)
        {
            await FollowupAsync("❌ No Showdown sets found. Separate each one with `---`.", ephemeral: true).ConfigureAwait(false);
            return;
        }

        if (blocks.Count > max)
        {
            await FollowupAsync($"❌ You can only trade up to {max} Pokémon at a time. You provided {blocks.Count}.", ephemeral: true).ConfigureAwait(false);
            return;
        }

        var pokemon = new List<T>();
        var errors = new List<BatchTradeError>();

        for (int i = 0; i < blocks.Count; i++)
        {
            var (pk, error, set, hint) = await BatchHelpers<T>.ProcessSingleTradeForBatch(blocks[i]).ConfigureAwait(false);
            if (pk != null)
            {
                TradeModule<T>.TryApplyEarlyAutoOT(pk, Context.User.Id);
                pokemon.Add(pk);
            }
            else
            {
                errors.Add(new BatchTradeError
                {
                    TradeNumber = i + 1,
                    SpeciesName = set?.Species > 0 ? GameInfo.Strings.Species[set.Species] : "Unknown",
                    ErrorMessage = error ?? "Unknown error.",
                    LegalizationHint = hint,
                    ShowdownSet = blocks[i],
                });
            }
        }

        // Matches the prefix behavior: any invalid set aborts the whole batch rather than
        // silently trading a partial team.
        if (errors.Count > 0)
        {
            await SlashBatchHelper<T>.SendBatchErrorsAsync(Context, errors, blocks.Count).ConfigureAwait(false);
            return;
        }

        await SlashBatchHelper<T>.QueueBatchAsync(
            Context,
            code ?? Info.GetRandomTradeCode(Context.User.Id),
            pokemon).ConfigureAwait(false);
    }

    // ===========================
    // BATCH TRADE ZIP  ($btz)
    // ===========================
    [SlashCommand("zip", "Trade several Pokémon from a .zip/.rar/.7z of PKM files.")]
    [RequireQueueRoleInteraction(nameof(DiscordManager.RolesTrade))]
    public async Task BatchTradeZipAsync(
        [Summary("archive", "A .zip, .rar or .7z containing PKM files.")] IAttachment archive,

        [Summary("code", "Link Trade code. Leave blank for a random or your stored code.")]
        [MinValue(0)][MaxValue(99999999)]
        int? code = null)
    {
        await DeferAsync(ephemeral: false).ConfigureAwait(false);

        if (!await SlashBatchHelper<T>.EnsureBatchAllowedAsync(Context).ConfigureAwait(false))
            return;

        var ext = Path.GetExtension(archive.Filename).ToLowerInvariant();
        if (!ArchiveExtensions.Contains(ext))
        {
            await FollowupAsync("❌ Only **.zip**, **.rar** and **.7z** archives are accepted.", ephemeral: true).ConfigureAwait(false);
            return;
        }

        string? tempDir = null;
        try
        {
            using var http = new HttpClient();
            var archiveBytes = await http.GetByteArrayAsync(archive.Url).ConfigureAwait(false);

            tempDir = Path.Combine(Path.GetTempPath(), $"FusionBot_BTZ_{Guid.NewGuid()}");
            Directory.CreateDirectory(tempDir);

            var localArchivePath = Path.Combine(tempDir, Path.GetFileName(archive.Filename));
            await File.WriteAllBytesAsync(localArchivePath, archiveBytes).ConfigureAwait(false);
            ArchiveService.ExtractToDirectory(localArchivePath, tempDir);

            var files = Directory.GetFiles(tempDir, "*.*", SearchOption.AllDirectories)
                .Where(f => PkmExtensions.Contains(Path.GetExtension(f).ToLowerInvariant()))
                .OrderBy(f => f, StringComparer.Ordinal)
                .ToList();

            if (files.Count == 0)
            {
                await FollowupAsync("❌ No PKM files found in that archive.", ephemeral: true).ConfigureAwait(false);
                return;
            }

            int max = SlashBatchHelper<T>.MaxBatchSize();
            if (files.Count > max)
            {
                await FollowupAsync($"❌ That archive holds {files.Count} PKM files, but only {max} can be traded at a time.", ephemeral: true).ConfigureAwait(false);
                return;
            }

            var pokemon = new List<T>();
            var errors = new List<BatchTradeError>();

            for (int i = 0; i < files.Count; i++)
            {
                var file = files[i];
                var name = Path.GetFileName(file);
                try
                {
                    var raw = EntityFormat.GetFromBytes(await File.ReadAllBytesAsync(file).ConfigureAwait(false));
                    if (raw == null || raw.Species <= 0)
                    {
                        errors.Add(new BatchTradeError { TradeNumber = i + 1, SpeciesName = "Unknown", ErrorMessage = "Invalid or unreadable PKM file.", ShowdownSet = name });
                        continue;
                    }

                    var la = new LegalityAnalysis(raw);
                    if (!la.Valid)
                    {
                        errors.Add(new BatchTradeError { TradeNumber = i + 1, SpeciesName = GameInfo.Strings.Species[raw.Species], ErrorMessage = la.Report(), LegalizationHint = la.Info?.EncounterMatch?.ToString(), ShowdownSet = name });
                        continue;
                    }

                    var converted = raw as T ?? EntityConverter.ConvertToType(raw, typeof(T), out _) as T;
                    if (converted == null)
                    {
                        errors.Add(new BatchTradeError { TradeNumber = i + 1, SpeciesName = GameInfo.Strings.Species[raw.Species], ErrorMessage = "Failed to convert PKM to this bot's game type.", ShowdownSet = name });
                        continue;
                    }

                    TradeModule<T>.TryApplyEarlyAutoOT(converted, Context.User.Id);
                    pokemon.Add(converted);
                }
                catch (Exception ex)
                {
                    errors.Add(new BatchTradeError { TradeNumber = i + 1, SpeciesName = "Unknown", ErrorMessage = ex.Message, ShowdownSet = name });
                }
            }

            if (errors.Count > 0)
            {
                await SlashBatchHelper<T>.SendBatchErrorsAsync(Context, errors, files.Count).ConfigureAwait(false);
                return;
            }

            await SlashBatchHelper<T>.QueueBatchAsync(
                Context,
                code ?? Info.GetRandomTradeCode(Context.User.Id),
                pokemon).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            LogUtil.LogSafe(ex, nameof(SlashBatchModule<T>));
            await FollowupAsync($"❌ Could not process that archive: {ex.Message}", ephemeral: true).ConfigureAwait(false);
        }
        finally
        {
            if (tempDir != null)
                try { Directory.Delete(tempDir, true); } catch { }
        }
    }

    // ===========================
    // BATCH MYSTERY EGG  ($btme)
    // ===========================
    [SlashCommand("mysteryegg", "Trade several random shiny 6IV eggs at once.")]
    [RequireQueueRoleInteraction(nameof(DiscordManager.RolesTrade))]
    public async Task BatchMysteryEggAsync(
        [Summary("count", "How many eggs to generate.")][MinValue(1)][MaxValue(6)] int count,

        [Summary("code", "Link Trade code. Leave blank for a random or your stored code.")]
        [MinValue(0)][MaxValue(99999999)]
        int? code = null)
    {
        await DeferAsync(ephemeral: false).ConfigureAwait(false);

        if (typeof(T) == typeof(PB7))
        {
            await FollowupAsync("❌ Mystery Eggs are not available for Let's Go Pikachu/Eevee, as the game does not support breeding.", ephemeral: true).ConfigureAwait(false);
            return;
        }

        if (!await SlashBatchHelper<T>.EnsureBatchAllowedAsync(Context).ConfigureAwait(false))
            return;

        int max = SlashBatchHelper<T>.MaxBatchSize();
        if (count > max)
        {
            await FollowupAsync($"❌ You can only request between 1 and {max} Mystery Eggs per batch.", ephemeral: true).ConfigureAwait(false);
            return;
        }

        var eggs = new List<T>(count);
        for (int i = 0; i < count; i++)
        {
            var egg = MysteryEggModule<T>.GenerateLegalMysteryEgg();
            if (egg != null)
                eggs.Add(egg);
        }

        if (eggs.Count == 0)
        {
            await FollowupAsync("❌ I couldn't generate any mystery eggs right now. Please try again.", ephemeral: true).ConfigureAwait(false);
            return;
        }

        string plural = eggs.Count == 1 ? string.Empty : "s";
        await SlashBatchHelper<T>.QueueBatchAsync(
            Context,
            code ?? Info.GetRandomTradeCode(Context.User.Id),
            eggs,
            isMysteryEgg: true,
            summary: new SlashBatchHelper<T>.BatchSummary(
                "Mystery Egg Batch Trade",
                $"You are currently receiving **{eggs.Count}** Mystery Egg{plural}!\nWhat could they be?",
                "https://raw.githubusercontent.com/Secludedly/ZE-FusionBot-Sprite-Images/main/mysteryegg3.png")).ConfigureAwait(false);
    }

    // ===========================
    // BATCH MYSTERY MON  ($btmm)
    // ===========================
    [SlashCommand("mysterymon", "Trade several completely random Pokémon at once.")]
    [RequireQueueRoleInteraction(nameof(DiscordManager.RolesTrade))]
    public async Task BatchMysteryMonAsync(
        [Summary("count", "How many Pokémon to generate.")][MinValue(1)][MaxValue(6)] int count,

        [Summary("code", "Link Trade code. Leave blank for a random or your stored code.")]
        [MinValue(0)][MaxValue(99999999)]
        int? code = null)
    {
        await DeferAsync(ephemeral: false).ConfigureAwait(false);

        if (!await SlashBatchHelper<T>.EnsureBatchAllowedAsync(Context).ConfigureAwait(false))
            return;

        int max = SlashBatchHelper<T>.MaxBatchSize();
        if (count > max)
        {
            await FollowupAsync($"❌ You can only request between 1 and {max} Mystery Pokémon per batch.", ephemeral: true).ConfigureAwait(false);
            return;
        }

        var mons = new List<T>(count);
        for (int i = 0; i < count; i++)
        {
            using var cancel = new System.Threading.CancellationTokenSource(TimeSpan.FromSeconds(6));
            var pk = MysteryMonModule<T>.GenerateMysteryMon(cancel.Token);
            if (pk != null)
                mons.Add(pk);
        }

        if (mons.Count == 0)
        {
            await FollowupAsync("Please try to find your Mystery Pokémon again! Whatever they are, they're still waiting for you!", ephemeral: true).ConfigureAwait(false);
            return;
        }

        await SlashBatchHelper<T>.QueueBatchAsync(
            Context,
            code ?? Info.GetRandomTradeCode(Context.User.Id),
            mons).ConfigureAwait(false);
    }

    // ===========================
    // ITEM BATCH TRADE  ($ibt)
    // ===========================
    [SlashCommand("item", "Trade the same held item several times over.")]
    [RequireQueueRoleInteraction(nameof(DiscordManager.RolesTrade))]
    public async Task ItemBatchTradeAsync(
        [Summary("item", "The held item you want.")]
        [Autocomplete(typeof(ItemAutocompleteCurrentGameHandler))]
        string item,

        [Summary("count", "How many copies to receive.")][MinValue(1)][MaxValue(6)] int count,

        [Summary("code", "Link Trade code. Leave blank for a random or your stored code.")]
        [MinValue(0)][MaxValue(99999999)]
        int? code = null)
    {
        await DeferAsync(ephemeral: false).ConfigureAwait(false);

        if (!await SlashBatchHelper<T>.EnsureBatchAllowedAsync(Context).ConfigureAwait(false))
            return;

        int max = SlashBatchHelper<T>.MaxBatchSize();
        if (count > max)
        {
            await FollowupAsync($"❌ You can only request up to {max} items at a time.", ephemeral: true).ConfigureAwait(false);
            return;
        }

        try
        {
            Species species = Info.Hub.Config.Trade.TradeConfiguration.ItemTradeSpecies == Species.None
                ? Species.Diglett
                : Info.Hub.Config.Trade.TradeConfiguration.ItemTradeSpecies;

            var showdown = new ShowdownSet($"{SpeciesName.GetSpeciesNameGeneration((ushort)species, 2, 8)} @ {item.Trim()}");
            var template = AutoLegalityWrapper.GetTemplate(showdown);
            var sav = AutoLegalityWrapper.GetTrainerInfo<T>();

            var generated = sav.GetLegal(template, out _);
            if (generated == null)
            {
                await FollowupAsync("❌ That set took too long to legalize.", ephemeral: true).ConfigureAwait(false);
                return;
            }

            generated = EntityConverter.ConvertToType(generated, typeof(T), out _) ?? generated;
            if (generated.HeldItem == 0)
            {
                await FollowupAsync($"❌ {Context.User.Username}, the item you entered wasn't recognized.", ephemeral: true).ConfigureAwait(false);
                return;
            }

            if (generated is not T first)
            {
                await FollowupAsync("❌ I wasn't able to create something from that.", ephemeral: true).ConfigureAwait(false);
                return;
            }

            TradeModule<T>.TryApplyEarlyAutoOT(first, Context.User.Id);
            first.ResetPartyStats();

            // Each queued Pokémon must be its own instance, or edits to one alias across the batch.
            var all = new List<T> { first };
            for (int i = 1; i < count; i++)
                all.Add((T)first.Clone());

            var strings = GameInfo.GetStrings("en");
            string itemDisplayName = strings.itemlist[first.HeldItem];
            string heldItemKey = itemDisplayName.ToLower().Replace(" ", "");
            string speciesName = strings.Species[first.Species];
            bool canGmax = first is PK8 pk8 && pk8.CanGigantamax;
            string plural = count == 1 ? string.Empty : "s";

            await SlashBatchHelper<T>.QueueBatchAsync(
                Context,
                code ?? Info.GetRandomTradeCode(Context.User.Id),
                all,
                summary: new SlashBatchHelper<T>.BatchSummary(
                    "Item Batch Trade",
                    $"**{speciesName}** will deliver your **{count}** {itemDisplayName}{plural}!",
                    $"https://serebii.net/itemdex/sprites/{heldItemKey}.png",
                    TradeExtensions<T>.PokeImg(first, canGmax, false, SysCord<T>.Runner.Config.Trade.TradeEmbedSettings.PreferredImageSize))).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            LogUtil.LogSafe(ex, nameof(SlashBatchModule<T>));
            await FollowupAsync("❌ An error occurred while processing that item batch trade.", ephemeral: true).ConfigureAwait(false);
        }
    }
}

/// <summary>
/// Multi-line input for a batch of Showdown sets, for the same reason
/// "TradeSetModal" exists: slash options are single-line.
/// </summary>
public class BatchTradeModal : IModal
{
    public string Title => "Batch Trade";

    [InputLabel("Showdown Sets (separate each with ---)")]
    [ModalTextInput("sets", TextInputStyle.Paragraph,
        placeholder: "Garchomp @ Life Orb\nJolly Nature\n---\nDragonite @ Leftovers\nAdamant Nature",
        maxLength: 4000)]
    public string Sets { get; set; } = string.Empty;

    [InputLabel("Trade Code (optional)")]
    [RequiredInput(false)]
    [ModalTextInput("code", TextInputStyle.Short, placeholder: "8 digits, or leave blank", maxLength: 8)]
    public string? Code { get; set; }
}
