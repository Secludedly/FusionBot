using System;
using System.ComponentModel;
using System.Linq;
using SysBot.Pokemon.Localization;

namespace SysBot.Pokemon;

public class TwitchSettings
{
    private const string Messages = nameof(Messages);

    private const string Operation = nameof(Operation);

    private const string Startup = nameof(Startup);

    [HubDisplayName("TwitchSettings_AllowCommandsViaChannel_DisplayName")]
    [HubCategory(Operation), HubDescription("TwitchSettings_AllowCommandsViaChannel_Description")]
    public bool AllowCommandsViaChannel { get; set; } = true;

    [HubDisplayName("TwitchSettings_AllowCommandsViaWhisper_DisplayName")]
    [HubCategory(Operation), HubDescription("TwitchSettings_AllowCommandsViaWhisper_Description")]
    public bool AllowCommandsViaWhisper { get; set; }

    [HubDisplayName("TwitchSettings_Channel_DisplayName")]
    [HubCategory(Startup), HubDescription("TwitchSettings_Channel_Description")]
    public string Channel { get; set; } = string.Empty;

    [HubDisplayName("TwitchSettings_CommandPrefix_DisplayName")]
    [HubCategory(Startup), HubDescription("TwitchSettings_CommandPrefix_Description")]
    public char CommandPrefix { get; set; } = '$';

    [HubDisplayName("TwitchSettings_DiscordLink_DisplayName")]
    [HubCategory(Operation), HubDescription("TwitchSettings_DiscordLink_Description")]
    public string DiscordLink { get; set; } = string.Empty;

    [HubDisplayName("TwitchSettings_DistributionCountDown_DisplayName")]
    [HubCategory(Messages), HubDescription("TwitchSettings_DistributionCountDown_Description")]
    public bool DistributionCountDown { get; set; } = true;

    [HubDisplayName("TwitchSettings_DonationLink_DisplayName")]
    [HubCategory(Operation), HubDescription("TwitchSettings_DonationLink_Description")]
    public string DonationLink { get; set; } = string.Empty;

    [HubDisplayName("TwitchSettings_MessageStart_DisplayName")]
    [HubCategory(Operation), HubDescription("TwitchSettings_MessageStart_Description")]
    public string MessageStart { get; set; } = string.Empty;

    [HubDisplayName("TwitchSettings_NotifyDestination_DisplayName")]
    [HubCategory(Messages), HubDescription("TwitchSettings_NotifyDestination_Description")]
    public TwitchMessageDestination NotifyDestination { get; set; }

    [HubDisplayName("TwitchSettings_SudoList_DisplayName")]
    [HubCategory(Operation), HubDescription("TwitchSettings_SudoList_Description")]
    public string SudoList { get; set; } = string.Empty;

    [HubDisplayName("TwitchSettings_ThrottleMessages_DisplayName")]
    [HubCategory(Operation), HubDescription("TwitchSettings_ThrottleMessages_Description")]
    public int ThrottleMessages { get; set; } = 100;

    // Messaging
    [HubDisplayName("TwitchSettings_ThrottleSeconds_DisplayName")]
    [HubCategory(Operation), HubDescription("TwitchSettings_ThrottleSeconds_Description")]
    public double ThrottleSeconds { get; set; } = 30;

    [HubDisplayName("TwitchSettings_ThrottleWhispers_DisplayName")]
    [HubCategory(Operation), HubDescription("TwitchSettings_ThrottleWhispers_Description")]
    public int ThrottleWhispers { get; set; } = 100;

    [HubDisplayName("TwitchSettings_ThrottleWhispersSeconds_DisplayName")]
    [HubCategory(Operation), HubDescription("TwitchSettings_ThrottleWhispersSeconds_Description")]
    public double ThrottleWhispersSeconds { get; set; } = 60;

    [HubDisplayName("TwitchSettings_Token_DisplayName")]
    [HubCategory(Startup), HubDescription("TwitchSettings_Token_Description")]
    public string Token { get; set; } = string.Empty;

    [HubDisplayName("TwitchSettings_TradeCanceledDestination_DisplayName")]
    [HubCategory(Messages), HubDescription("TwitchSettings_TradeCanceledDestination_Description")]
    public TwitchMessageDestination TradeCanceledDestination { get; set; } = TwitchMessageDestination.Channel;

    [HubDisplayName("TwitchSettings_TradeFinishDestination_DisplayName")]
    [HubCategory(Messages), HubDescription("TwitchSettings_TradeFinishDestination_Description")]
    public TwitchMessageDestination TradeFinishDestination { get; set; }

    [HubDisplayName("TwitchSettings_TradeSearchDestination_DisplayName")]
    [HubCategory(Messages), HubDescription("TwitchSettings_TradeSearchDestination_Description")]
    public TwitchMessageDestination TradeSearchDestination { get; set; }

    // Message Destinations
    [HubDisplayName("TwitchSettings_TradeStartDestination_DisplayName")]
    [HubCategory(Messages), HubDescription("TwitchSettings_TradeStartDestination_Description")]
    public TwitchMessageDestination TradeStartDestination { get; set; } = TwitchMessageDestination.Channel;

    [HubDisplayName("TwitchSettings_TutorialLink_DisplayName")]
    [HubCategory(Operation), HubDescription("TwitchSettings_TutorialLink_Description")]
    public string TutorialLink { get; set; } = string.Empty;

    [HubDisplayName("TwitchSettings_TutorialText_DisplayName")]
    [HubCategory(Operation), HubDescription("TwitchSettings_TutorialText_Description")]
    public string TutorialText { get; set; } = string.Empty;

    // Operation
    [HubDisplayName("TwitchSettings_UserBlacklist_DisplayName")]
    [HubCategory(Operation), HubDescription("TwitchSettings_UserBlacklist_Description")]
    public string UserBlacklist { get; set; } = string.Empty;

    // Startup
    [HubDisplayName("TwitchSettings_Username_DisplayName")]
    [HubCategory(Startup), HubDescription("TwitchSettings_Username_Description")]
    public string Username { get; set; } = string.Empty;

    public bool IsSudo(string username)
    {
        var sudos = SudoList.Split([",", ", ", " "], StringSplitOptions.RemoveEmptyEntries);
        return sudos.Contains(username);
    }

    public override string ToString() => "Twitch Integration Settings";
}

public enum TwitchMessageDestination
{
    Disabled,

    Channel,

    Whisper,
}
