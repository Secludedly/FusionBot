using System.ComponentModel;
using SysBot.Pokemon.Localization;

namespace SysBot.Pokemon;

public class TradeAbuseSettings
{
    private const string Monitoring = nameof(Monitoring);
    public override string ToString() => "Trade Abuse Monitoring Settings";

    [HubCategory(Monitoring), HubDescription("TradeAbuseSettings_TradeCooldown_Description"), HubDisplayName("TradeAbuseSettings_TradeCooldown_DisplayName")]
    public double TradeCooldown { get; set; }

    [HubCategory(Monitoring), HubDescription("TradeAbuseSettings_EchoNintendoOnlineIDCooldown_Description"), HubDisplayName("TradeAbuseSettings_EchoNintendoOnlineIDCooldown_DisplayName")]
    public bool EchoNintendoOnlineIDCooldown { get; set; } = true;

    [HubCategory(Monitoring), HubDescription("TradeAbuseSettings_CooldownAbuseEchoMention_Description"), HubDisplayName("TradeAbuseSettings_CooldownAbuseEchoMention_DisplayName")]
    public string CooldownAbuseEchoMention { get; set; } = string.Empty;

    [HubCategory(Monitoring), HubDescription("TradeAbuseSettings_TradeAbuseExpiration_Description"), HubDisplayName("TradeAbuseSettings_TradeAbuseExpiration_DisplayName")]
    public double TradeAbuseExpiration { get; set; } = 120;

    [HubCategory(Monitoring), HubDescription("TradeAbuseSettings_EchoNintendoOnlineIDMulti_Description"), HubDisplayName("TradeAbuseSettings_EchoNintendoOnlineIDMulti_DisplayName")]
    public bool EchoNintendoOnlineIDMulti { get; set; } = true;

    [HubCategory(Monitoring), HubDescription("TradeAbuseSettings_EchoNintendoOnlineIDMultiRecipients_Description"), HubDisplayName("TradeAbuseSettings_EchoNintendoOnlineIDMultiRecipients_DisplayName")]
    public bool EchoNintendoOnlineIDMultiRecipients { get; set; } = true;

    [HubCategory(Monitoring), HubDescription("TradeAbuseSettings_TradeAbuseAction_Description"), HubDisplayName("TradeAbuseSettings_TradeAbuseAction_DisplayName")]
    public TradeAbuseAction TradeAbuseAction { get; set; } = TradeAbuseAction.Quit;

    [HubCategory(Monitoring), HubDescription("TradeAbuseSettings_BanIDWhenBlockingUser_Description"), HubDisplayName("TradeAbuseSettings_BanIDWhenBlockingUser_DisplayName")]
    public bool BanIDWhenBlockingUser { get; set; } = true;

    [HubCategory(Monitoring), HubDescription("TradeAbuseSettings_MultiAbuseEchoMention_Description"), HubDisplayName("TradeAbuseSettings_MultiAbuseEchoMention_DisplayName")]
    public string MultiAbuseEchoMention { get; set; } = string.Empty;

    [HubCategory(Monitoring), HubDescription("TradeAbuseSettings_MultiRecipientEchoMention_Description"), HubDisplayName("TradeAbuseSettings_MultiRecipientEchoMention_DisplayName")]
    public string MultiRecipientEchoMention { get; set; } = string.Empty;

    [HubCategory(Monitoring), HubDescription("TradeAbuseSettings_BannedIDs_Description"), HubDisplayName("TradeAbuseSettings_BannedIDs_DisplayName")]
    public RemoteControlAccessList BannedIDs { get; set; } = new();

    [HubCategory(Monitoring), HubDescription("TradeAbuseSettings_BlockDetectedBannedUser_Description"), HubDisplayName("TradeAbuseSettings_BlockDetectedBannedUser_DisplayName")]
    public bool BlockDetectedBannedUser { get; set; } = true;

    [HubCategory(Monitoring), HubDescription("TradeAbuseSettings_BannedIDMatchEchoMention_Description"), HubDisplayName("TradeAbuseSettings_BannedIDMatchEchoMention_DisplayName")]
    public string BannedIDMatchEchoMention { get; set; } = string.Empty;

    [HubCategory(Monitoring), HubDescription("TradeAbuseSettings_EchoNintendoOnlineIDLedy_Description"), HubDisplayName("TradeAbuseSettings_EchoNintendoOnlineIDLedy_DisplayName")]
    public bool EchoNintendoOnlineIDLedy { get; set; } = true;

    [HubCategory(Monitoring), HubDescription("TradeAbuseSettings_LedyAbuseEchoMention_Description"), HubDisplayName("TradeAbuseSettings_LedyAbuseEchoMention_DisplayName")]
    public string LedyAbuseEchoMention { get; set; } = string.Empty;
}
