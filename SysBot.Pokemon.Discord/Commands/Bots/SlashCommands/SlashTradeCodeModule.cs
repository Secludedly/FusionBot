using Discord.Interactions;
using PKHeX.Core;
using SysBot.Base;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SysBot.Pokemon.Discord.Commands.Bots.SlashCommands;

/// <summary>
/// Slash command equivalents of $deleteTradeCode ($dtc) and $changeTradeCode ($ctc)
/// "TradeCodeStorage" is fully public so the storage calls are shared. Only the two small
/// validators are mirrored here, because "ValidateTradeCode" and "IsEasilyGuessableCode" are
/// private to "QueueModule."
/// Both commands reply ephemerally. The prefix versions have to delete the user's message and their own
/// reply on a timer to keep the code private; an ephemeral response is only ever visible to the caller,
/// so the code is never exposed to the channel in the first place.
/// </summary>
public class SlashTradeCodeModule<T> : InteractionModuleBase<SocketInteractionContext> where T : PKM, new()
{
    [SlashCommand("deletetradecode", "Delete your stored Link Trade Code.")]
    [RequireCommandAccessInteraction]
    public async Task DeleteTradeCodeAsync()
    {
        await DeferAsync(ephemeral: true).ConfigureAwait(false);

        try
        {
            var storage = new TradeCodeStorage();
            bool success = storage.DeleteTradeCode(Context.User.Id);
            await FollowupAsync(success
                ? "Your trade code has been successfully deleted."
                : "You don't have a stored trade code to delete.", ephemeral: true).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            LogUtil.LogError($"Error deleting trade code for user {Context.User.Id}: {ex.Message}", nameof(SlashTradeCodeModule<T>));
            await FollowupAsync("An error occurred while deleting your trade code. Please try again later.", ephemeral: true).ConfigureAwait(false);
        }
    }

    [SlashCommand("changetradecode", "Change your stored Link Trade Code.")]
    [RequireCommandAccessInteraction]
    public async Task ChangeTradeCodeAsync(
        [Summary("code", "Your new 8-digit trade code.")] string code)
    {
        await DeferAsync(ephemeral: true).ConfigureAwait(false);

        if (!ValidateTradeCode(code, out string errorMessage))
        {
            await FollowupAsync(errorMessage, ephemeral: true).ConfigureAwait(false);
            return;
        }

        try
        {
            var storage = new TradeCodeStorage();
            int parsed = int.Parse(code);
            await FollowupAsync(storage.UpdateTradeCode(Context.User.Id, parsed)
                ? "Your trade code has been successfully updated."
                : "You don't have a trade code set. Use a trade command to generate one first.", ephemeral: true).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            LogUtil.LogError($"Error changing trade code for user {Context.User.Id}: {ex.Message}", nameof(SlashTradeCodeModule<T>));
            await FollowupAsync("An error occurred while changing your trade code. Please try again later.", ephemeral: true).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Mirrors "QueueModule.ValidateTradeCode."
    /// </summary>
    private static bool ValidateTradeCode(string code, out string errorMessage)
    {
        errorMessage = string.Empty;

        if (code.Length != 8)
        {
            errorMessage = "Trade code must be exactly 8 digits long.";
            return false;
        }

        if (!Regex.IsMatch(code, @"^\d{8}$"))
        {
            errorMessage = "Trade code must contain only digits.";
            return false;
        }

        if (IsEasilyGuessableCode(code))
        {
            errorMessage = "Trade code is too easy to guess. Please choose a more complex code.";
            return false;
        }

        return true;
    }

    /// <summary>
    /// Mirrors "QueueModule.IsEasilyGuessableCode".
    /// </summary>
    private static bool IsEasilyGuessableCode(string code)
    {
        string[] easyPatterns = [
            @"^(\d)\1{7}$",           // All same digits (e.g., 11111111)
            @"^12345678$",            // Ascending sequence
            @"^87654321$",            // Descending sequence
            @"^(?:01234567|12345678|23456789)$" // Other common sequences
        ];

        foreach (var pattern in easyPatterns)
        {
            if (Regex.IsMatch(code, pattern))
                return true;
        }

        return false;
    }
}
