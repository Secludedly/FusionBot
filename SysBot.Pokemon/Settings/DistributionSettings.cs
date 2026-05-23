using PKHeX.Core;
using SysBot.Base;
using System.ComponentModel;
using SysBot.Pokemon.Localization;

namespace SysBot.Pokemon;

public class DistributionSettings : ISynchronizationSetting
{
    private const string Distribute = nameof(Distribute);

    private const string Synchronize = nameof(Synchronize);

    [HubCategory(Distribute), HubDescription("DistributionSettings_DistributeWhileIdle_Description"), HubDisplayName("DistributionSettings_DistributeWhileIdle_DisplayName")]
    public bool DistributeWhileIdle { get; set; } = true;

    [HubCategory(Distribute), HubDescription("DistributionSettings_LedyQuitIfNoMatch_Description"), HubDisplayName("DistributionSettings_LedyQuitIfNoMatch_DisplayName")]
    public bool LedyQuitIfNoMatch { get; set; }

    [HubCategory(Distribute), HubDescription("DistributionSettings_LedySpecies_Description"), HubDisplayName("DistributionSettings_LedySpecies_DisplayName")]
    public Species LedySpecies { get; set; } = Species.None;

    [HubCategory(Distribute), HubDescription("DistributionSettings_RandomCode_Description"), HubDisplayName("DistributionSettings_RandomCode_DisplayName")]
    public bool RandomCode { get; set; }

    [HubCategory(Distribute), HubDescription("DistributionSettings_RemainInUnionRoomBDSP_Description"), HubDisplayName("DistributionSettings_RemainInUnionRoomBDSP_DisplayName")]
    public bool RemainInUnionRoomBDSP { get; set; } = true;

    // Distribute
    [HubCategory(Distribute), HubDescription("DistributionSettings_Shuffled_Description"), HubDisplayName("DistributionSettings_Shuffled_DisplayName")]
    public bool Shuffled { get; set; }

    [HubCategory(Synchronize), HubDescription("DistributionSettings_SynchronizeBots_Description"), HubDisplayName("DistributionSettings_SynchronizeBots_DisplayName")]
    public BotSyncOption SynchronizeBots { get; set; } = BotSyncOption.LocalSync;

    // Synchronize
    [HubCategory(Synchronize), HubDescription("DistributionSettings_SynchronizeDelayBarrier_Description"), HubDisplayName("DistributionSettings_SynchronizeDelayBarrier_DisplayName")]
    public int SynchronizeDelayBarrier { get; set; }

    [HubCategory(Synchronize), HubDescription("DistributionSettings_SynchronizeTimeout_Description"), HubDisplayName("DistributionSettings_SynchronizeTimeout_DisplayName")]
    public double SynchronizeTimeout { get; set; } = 90;

    [HubCategory(Distribute), HubDescription("DistributionSettings_TradeCode_Description"), HubDisplayName("DistributionSettings_TradeCode_DisplayName")]
    public int TradeCode { get; set; } = 7196;

    public override string ToString() => "Distribution Trade Settings";
}
