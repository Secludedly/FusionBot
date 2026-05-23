using System.ComponentModel;
using SysBot.Pokemon.Localization;

namespace SysBot.Pokemon;

public class TimingSettings
{
    private const string CloseGame = nameof(CloseGame);

    private const string Misc = nameof(Misc);

    private const string OpenGame = nameof(OpenGame);

    private const string Raid = nameof(Raid);

    [HubCategory(Misc), HubDescription("TimingSettings_AvoidSystemUpdate_Description"), HubDisplayName("TimingSettings_AvoidSystemUpdate_DisplayName")]
    public bool AvoidSystemUpdate { get; set; } = true;

    [HubCategory(Misc), HubDescription("TimingSettings_ExtraReconnectDelay_Description"), HubDisplayName("TimingSettings_ExtraReconnectDelay_DisplayName")]
    public int ExtraReconnectDelay { get; set; }

    [HubCategory(Raid), HubDescription("TimingSettings_ExtraTimeAddFriend_Description"), HubDisplayName("TimingSettings_ExtraTimeAddFriend_DisplayName")]
    public int ExtraTimeAddFriend { get; set; }

    [HubCategory(CloseGame), HubDescription("TimingSettings_ExtraTimeCloseGame_Description"), HubDisplayName("TimingSettings_ExtraTimeCloseGame_DisplayName")]
    public int ExtraTimeCloseGame { get; set; }

    // Miscellaneous settings.
    [HubCategory(Misc), HubDescription("TimingSettings_ExtraTimeConnectOnline_Description"), HubDisplayName("TimingSettings_ExtraTimeConnectOnline_DisplayName")]
    public int ExtraTimeConnectOnline { get; set; }

    [HubCategory(Raid), HubDescription("TimingSettings_ExtraTimeDeleteFriend_Description"), HubDisplayName("TimingSettings_ExtraTimeDeleteFriend_DisplayName")]
    public int ExtraTimeDeleteFriend { get; set; }

    [HubCategory(Raid), HubDescription("TimingSettings_ExtraTimeEndRaid_Description"), HubDisplayName("TimingSettings_ExtraTimeEndRaid_DisplayName")]
    public int ExtraTimeEndRaid { get; set; }

    [HubCategory(Misc), HubDescription("TimingSettings_ExtraTimeJoinUnionRoom_Description"), HubDisplayName("TimingSettings_ExtraTimeJoinUnionRoom_DisplayName")]
    public int ExtraTimeJoinUnionRoom { get; set; } = 500;

    [HubCategory(Misc), HubDescription("TimingSettings_ExtraTimeLeaveUnionRoom_Description"), HubDisplayName("TimingSettings_ExtraTimeLeaveUnionRoom_DisplayName")]
    public int ExtraTimeLeaveUnionRoom { get; set; } = 1000;

    [HubCategory(OpenGame), HubDescription("TimingSettings_ExtraTimeLoadGame_Description"), HubDisplayName("TimingSettings_ExtraTimeLoadGame_DisplayName")]
    public int ExtraTimeLoadGame { get; set; } = 5000;

    [HubCategory(OpenGame), HubDescription("TimingSettings_ExtraTimeLoadOverworld_Description"), HubDisplayName("TimingSettings_ExtraTimeLoadOverworld_DisplayName")]
    public int ExtraTimeLoadOverworld { get; set; } = 3000;

    [HubCategory(Misc), HubDescription("TimingSettings_ExtraTimeLoadPortal_Description"), HubDisplayName("TimingSettings_ExtraTimeLoadPortal_DisplayName")]
    public int ExtraTimeLoadPortal { get; set; } = 1000;

    // Opening the game.
    [HubCategory(OpenGame), HubDescription("TimingSettings_ProfileSelectionRequired_Description"), HubDisplayName("TimingSettings_ProfileSelectionRequired_DisplayName")]
    public bool ProfileSelectionRequired { get; set; } = false;

    [HubCategory(OpenGame), HubDescription("TimingSettings_ExtraTimeLoadProfile_Description"), HubDisplayName("TimingSettings_ExtraTimeLoadProfile_DisplayName")]
    public int ExtraTimeLoadProfile { get; set; }

    [HubCategory(OpenGame), HubDescription("TimingSettings_CheckGameDelay_Description"), HubDisplayName("TimingSettings_CheckGameDelay_DisplayName")]
    public bool CheckGameDelay { get; set; } = false;

    [HubCategory(OpenGame), HubDescription("TimingSettings_ExtraTimeCheckGame_Description"), HubDisplayName("TimingSettings_ExtraTimeCheckGame_DisplayName")]
    public int ExtraTimeCheckGame { get; set; } = 200;

    // Raid-specific timings.
    [HubCategory(Raid), HubDescription("TimingSettings_ExtraTimeLoadRaid_Description"), HubDisplayName("TimingSettings_ExtraTimeLoadRaid_DisplayName")]
    public int ExtraTimeLoadRaid { get; set; }

    [HubCategory(Misc), HubDescription("TimingSettings_ExtraTimeOpenBox_Description"), HubDisplayName("TimingSettings_ExtraTimeOpenBox_DisplayName")]
    public int ExtraTimeOpenBox { get; set; } = 1000;

    [HubCategory(Misc), HubDescription("TimingSettings_ExtraTimeOpenCodeEntry_Description"), HubDisplayName("TimingSettings_ExtraTimeOpenCodeEntry_DisplayName")]
    public int ExtraTimeOpenCodeEntry { get; set; } = 1000;

    [HubCategory(Raid), HubDescription("TimingSettings_ExtraTimeOpenRaid_Description"), HubDisplayName("TimingSettings_ExtraTimeOpenRaid_DisplayName")]
    public int ExtraTimeOpenRaid { get; set; }

    [HubCategory(Misc), HubDescription("TimingSettings_ExtraTimeOpenYMenu_Description"), HubDisplayName("TimingSettings_ExtraTimeOpenYMenu_DisplayName")]
    public int ExtraTimeOpenYMenu { get; set; } = 500;

    // Closing the game.
    [HubCategory(CloseGame), HubDescription("TimingSettings_ExtraTimeReturnHome_Description"), HubDisplayName("TimingSettings_ExtraTimeReturnHome_DisplayName")]
    public int ExtraTimeReturnHome { get; set; }

    [HubCategory(Misc), HubDescription("TimingSettings_KeypressTime_Description"), HubDisplayName("TimingSettings_KeypressTime_DisplayName")]
    public int KeypressTime { get; set; } = 200;

    [HubCategory(Misc), HubDescription("TimingSettings_ReconnectAttempts_Description"), HubDisplayName("TimingSettings_ReconnectAttempts_DisplayName")]
    public int ReconnectAttempts { get; set; } = 30;
    public override string ToString() => "Extra Time Settings";
}
