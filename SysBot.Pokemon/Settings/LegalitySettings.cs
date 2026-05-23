using PKHeX.Core;
using PKHeX.Core.AutoMod;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using SysBot.Pokemon.Localization;

namespace SysBot.Pokemon;

public class LegalitySettings
{
    private const string Generate = nameof(Generate);

    private const string Misc = nameof(Misc);

    private string DefaultTrainerName = "FreeMons.Org";

    [HubCategory(Generate), HubDescription("LegalitySettings_AllowBatchCommands_Description")]
    public bool AllowBatchCommands { get; set; } = true;

    [HubCategory(Generate), HubDescription("LegalitySettings_AllowTrainerDataOverride_Description")]
    public bool AllowTrainerDataOverride { get; set; } = true;

    [HubCategory(Generate), HubDescription("LegalitySettings_DisallowNonNatives_Description"), HubDisplayName("LegalitySettings_DisallowNonNatives_DisplayName")]
    public bool DisallowNonNatives { get; set; } = false;

    [HubCategory(Generate), HubDescription("LegalitySettings_DisallowTracked_Description"), HubDisplayName("LegalitySettings_DisallowTracked_DisplayName")]
    public bool DisallowTracked { get; set; } = false;

    [HubCategory(Generate), HubDescription("LegalitySettings_EnableEasterEggs_Description")]
    public bool EnableEasterEggs { get; set; } = false;

    [HubCategory(Generate), HubDescription("LegalitySettings_EnableHOMETrackerCheck_Description")]
    public bool EnableHOMETrackerCheck { get; set; } = false;

    [HubCategory(Generate), HubDescription("LegalitySettings_ForceLevel100for50_Description")]
    public bool ForceLevel100for50 { get; set; } = false;

    [HubCategory(Generate), HubDescription("LegalitySettings_ForceSpecifiedBall_Description")]
    public bool ForceSpecifiedBall { get; set; } = true;

    [HubCategory(Generate), HubDescription("LegalitySettings_GenerateLanguage_Description")]
    public LanguageID GenerateLanguage { get; set; } = LanguageID.English;

    [HubCategory(Generate), HubDescription("LegalitySettings_GenerateOT_Description")]
    public string GenerateOT
    {
        get => DefaultTrainerName;
        set
        {
            if (!StringsUtil.IsSpammyString(value))
                DefaultTrainerName = value;
        }
    }

    [HubCategory(Generate), HubDescription("LegalitySettings_GeneratePathTrainerInfo_Description")]
    public string GeneratePathTrainerInfo { get; set; } = string.Empty;

    [HubCategory(Generate), HubDescription("LegalitySettings_GenerateSID16_Description")]
    public ushort GenerateSID16 { get; set; } = 54321;

    [HubCategory(Generate), HubDescription("LegalitySettings_GenerateTID16_Description")]
    public ushort GenerateTID16 { get; set; } = 12345;

    // Generate
    [HubCategory(Generate), HubDescription("LegalitySettings_MGDBPath_Description")]
    public string MGDBPath { get; set; } = string.Empty;

    [HubCategory(Generate), HubDescription("LegalitySettings_PrioritizeEncounters_Description")]
    public List<EncounterTypeGroup> PrioritizeEncounters { get; set; } =
[
    EncounterTypeGroup.Trade, EncounterTypeGroup.Slot,
        EncounterTypeGroup.Mystery, EncounterTypeGroup.Egg,
        EncounterTypeGroup.Static,
    ];

    [HubCategory(Generate), HubDescription("LegalitySettings_GameVersionPriority_Description")]
    public GameVersionPriorityType GameVersionPriority { get; set; } = GameVersionPriorityType.PriorityOrder;

    [HubCategory(Generate), HubDescription("LegalitySettings_PriorityOrder_Description")]
    public List<GameVersion> PriorityOrder { get; set; } = Enum.GetValues<GameVersion>().Where(GameUtil.IsValidSavedVersion).Reverse().ToList();

    // Misc
    [Browsable(false)]
    [HubCategory(Misc), HubDescription("LegalitySettings_ResetHOMETracker_Description")]
    public bool ResetHOMETracker { get; set; } = false;

    [HubCategory(Generate), HubDescription("LegalitySettings_SetAllLegalRibbons_Description")]
    public bool SetAllLegalRibbons { get; set; } = false;

    [Browsable(false)]
    [HubCategory(Generate), HubDescription("LegalitySettings_SetBattleVersion_Description")]
    public bool SetBattleVersion { get; set; } = false;

    [HubCategory(Generate), HubDescription("LegalitySettings_SetMatchingBalls_Description")]
    public bool SetMatchingBalls { get; set; } = true;

    [HubCategory(Generate), HubDescription("LegalitySettings_Timeout_Description")]
    public int Timeout { get; set; } = 20;

    [HubCategory(Misc), HubDescription("LegalitySettings_UseTradePartnerInfo_Description")]
    public bool UseTradePartnerInfo { get; set; } = true;

    public override string ToString() => "Legality Generating Settings";
}
