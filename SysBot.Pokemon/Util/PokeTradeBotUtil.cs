using PKHeX.Core;

namespace SysBot.Pokemon;

public static class PokeTradeBotUtil
{
    public static readonly byte[] EMPTY_EC = new byte[4];

    public static readonly byte[] EMPTY_SLOT = new byte[344];

    /// <summary>
    /// Formats the final OT/TID/SID/OTGender that will be visible to the trade
    /// partner. Call this immediately before <c>ConfirmAndStartTrading</c> in
    /// each game's trade bot so the log reflects every AutoOT and user-override
    /// mutation applied to the PKM.
    /// </summary>
    public static string FormatFinalTrainerInfo(PKM pk)
    {
        var gender = pk.OriginalTrainerGender == 0 ? "Male" : "Female";
        return $"Final Applied Trainer Information = OT: {pk.OriginalTrainerName} | TID: {pk.TrainerTID7} | SID: {pk.TrainerSID7} | OTGender: {gender}";
    }
}
