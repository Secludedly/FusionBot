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

    [HubCategory(Operation), HubDescription("TwitchSettings_AllowCommandsViaChannel_Description")]
    public bool AllowCommandsViaChannel { get; set; } = true;

    [HubCategory(Operation), HubDescription("TwitchSettings_AllowCommandsViaWhisper_Description")]
    public bool AllowCommandsViaWhisper { get; set; }

    [HubCategory(Startup), HubDescription("TwitchSettings_Channel_Description")]
    public string Channel { get; set; } = string.Empty;

    [HubCategory(Startup), HubDescription("TwitchSettings_CommandPrefix_Description")]
    public char CommandPrefix { get; set; } = '$';

    [HubCategory(Operation), HubDescription("TwitchSettings_DiscordLink_Description")]
    public string DiscordLink { get; set; } = string.Empty;

    [HubCategory(Messages), HubDescription("TwitchSettings_DistributionCountDown_Description")]
    public bool DistributionCountDown { get; set; } = true;

    [HubCategory(Operation), HubDescription("TwitchSettings_DonationLink_Description")]
    public string DonationLink { get; set; } = string.Empty;

    [HubCategory(Operation), HubDescription("TwitchSettings_MessageStart_Description")]
    public string MessageStart { get; set; } = string.Empty;

    [HubCategory(Messages), HubDescription("TwitchSettings_NotifyDestination_Description")]
    public TwitchMessageDestination NotifyDestination { get; set; }

    [HubCategory(Operation), HubDescription("TwitchSettings_SudoList_Description")]
    public string SudoList { get; set; } = string.Empty;

    [HubCategory(Operation), HubDescription("TwitchSettings_ThrottleMessages_Description")]
    public int ThrottleMessages { get; set; } = 100;

    // Messaging
    [HubCategory(Operation), HubDescription("TwitchSettings_ThrottleSeconds_Description")]
    public double ThrottleSeconds { get; set; } = 30;

    [HubCategory(Operation), HubDescription("TwitchSettings_ThrottleWhispers_Description")]
    public int ThrottleWhispers { get; set; } = 100;

    [HubCategory(Operation), HubDescription("TwitchSettings_ThrottleWhispersSeconds_Description")]
    public double ThrottleWhispersSeconds { get; set; } = 60;

    [HubCategory(Startup), HubDescription("TwitchSettings_Token_Description")]
    public string Token { get; set; } = string.Empty;

    [HubCategory(Messages), HubDescription("TwitchSettings_TradeCanceledDestination_Description")]
    public TwitchMessageDestination TradeCanceledDestination { get; set; } = TwitchMessageDestination.Channel;

    [HubCategory(Messages), HubDescription("TwitchSettings_TradeFinishDestination_Description")]
    public TwitchMessageDestination TradeFinishDestination { get; set; }

    [HubCategory(Messages), HubDescription("TwitchSettings_TradeSearchDestination_Description")]
    public TwitchMessageDestination TradeSearchDestination { get; set; }

    // Message Destinations
    [HubCategory(Messages), HubDescription("TwitchSettings_TradeStartDestination_Description")]
    public TwitchMessageDestination TradeStartDestination { get; set; } = TwitchMessageDestination.Channel;

    [HubCategory(Operation), HubDescription("TwitchSettings_TutorialLink_Description")]
    public string TutorialLink { get; set; } = string.Empty;

    [HubCategory(Operation), HubDescription("TwitchSettings_TutorialText_Description")]
    public string TutorialText { get; set; } = string.Empty;

    // Operation
    [HubCategory(Operation), HubDescription("TwitchSettings_UserBlacklist_Description")]
    public string UserBlacklist { get; set; } = string.Empty;

    // Startup
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
