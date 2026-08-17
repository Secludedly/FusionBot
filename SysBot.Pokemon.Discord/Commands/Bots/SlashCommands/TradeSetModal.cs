using Discord;
using Discord.Interactions;

namespace SysBot.Pokemon.Discord.Commands.Bots.SlashCommands;

/// <summary>
/// Multi-line input for a trade request.
/// Discord's slash command string options are single-line.
/// The client will not accept a newline in an
/// option box -- which is a poor fit for Showdown sets and makes batch commands awkward.
/// A modal text input in "Paragraph" style does accept real newlines, so this is the proper surface for pasting
/// a full set.
/// </summary>
public class TradeSetModal : IModal
{
    public string Title => "Trade a Pokémon";

    // Batch commands go straight into this box alongside the rest of the set. It accepts real
    // newlines, so there is no reason for a separate field.
    [InputLabel("Showdown Set")]
    [ModalTextInput("showdown", TextInputStyle.Paragraph,
        placeholder: "Garchomp @ Life Orb\nJolly Nature\nShiny: Yes\n.Scale=255\n- Earthquake",
        maxLength: 3000)]
    public string Showdown { get; set; } = string.Empty;

    [InputLabel("Trade Code (optional)")]
    [RequiredInput(false)]
    [ModalTextInput("code", TextInputStyle.Short, placeholder: "8 digits, or leave blank", maxLength: 8)]
    public string? Code { get; set; }
}
