using Discord;
using Discord.Interactions;
using PKHeX.Core;
using SysBot.Pokemon.Discord.Helpers;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SysBot.Pokemon.Discord.Commands.Bots.Autocomplete;

/// <summary>
/// Item autocomplete that resolves the item pool from whichever game this bot process is running,
/// rather than being fixed to one game like the existing per-game handlers.
/// The per-game handlers (ItemAutocompleteSVHandler and friends) exist because the guided
/// /create builders are declared per game. Commands like /itemtrade are declared once and
/// registered against whatever 'T' the process started with, so they need a handler that reads the
/// mode at runtime. Those handlers are untouched and still used by the builders.
/// </summary>
public class ItemAutocompleteCurrentGameHandler : AutocompleteHandler
{
    /// <summary>
    /// Maps the running game mode to the entity context used for held-item legality.
    /// Mirrors the constants hard-coded in the per-game handlers. PLZA intentionally uses Gen9 BTW.
    /// </summary>
    private static EntityContext GetContext() => BatchCommandNormalizer.CurrentGameMode switch
    {
        ProgramMode.LGPE => EntityContext.Gen7b,
        ProgramMode.SWSH => EntityContext.Gen8,
        ProgramMode.BDSP => EntityContext.Gen8,
        ProgramMode.LA => EntityContext.Gen8a,
        ProgramMode.SV => EntityContext.Gen9,
        ProgramMode.PLZA => EntityContext.Gen9,
        _ => EntityContext.Gen9,
    };

    public override Task<AutocompletionResult> GenerateSuggestionsAsync(
        IInteractionContext context,
        IAutocompleteInteraction autocompleteInteraction,
        IParameterInfo parameter,
        IServiceProvider services)
    {
        try
        {
            var userInput = autocompleteInteraction.Data.Current.Value?.ToString() ?? string.Empty;
            var entityContext = GetContext();
            var strings = GameInfo.GetStrings("en");

            var itemNames = strings.Item
                .Select((name, index) => new { Name = name, Index = index })
                .Where(item =>
                    !string.IsNullOrEmpty(item.Name) &&
                    item.Index > 0 &&
                    !item.Name.StartsWith("(") &&
                    !item.Name.Contains("???") &&
                    ItemRestrictions.IsHeldItemAllowed((ushort)item.Index, entityContext))
                .ToList();

            var filtered = string.IsNullOrWhiteSpace(userInput)
                ? itemNames.OrderBy(i => i.Name).Take(25)
                : itemNames
                    .Where(i => i.Name.Contains(userInput, StringComparison.OrdinalIgnoreCase))
                    .OrderBy(i => i.Name.StartsWith(userInput, StringComparison.OrdinalIgnoreCase) ? 0 : 1)
                    .ThenBy(i => i.Name)
                    .Take(25);

            var results = filtered.Select(i => new AutocompleteResult(i.Name, i.Name)).ToList();
            return Task.FromResult(AutocompletionResult.FromSuccess(results));
        }
        catch (Exception ex)
        {
            return Task.FromResult(AutocompletionResult.FromError(ex));
        }
    }
}
