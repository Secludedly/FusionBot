using System;
using System.ComponentModel;
using static SysBot.Pokemon.TradeSettings;
using SysBot.Pokemon.Localization;

namespace SysBot.Pokemon;

public class DiscordSettings
{
    private const string Channels = nameof(Channels);

    private const string Operation = nameof(Operation);

    private const string Roles = nameof(Roles);

    private const string Servers = nameof(Servers);

    private const string Startup = nameof(Startup);

    private const string Users = nameof(Users);

    public enum EmbedColorOption
    {
        Blue,

        Green,

        Red,

        Gold,

        Purple,

        Teal,

        Orange,

        Magenta,

        LightGrey,

        DarkGrey
    }

    public enum ThumbnailOption
    {
        Gengar,

        Pikachu,

        Umbreon,

        Sylveon,

        Charmander,

        Jigglypuff,

        Flareon,

        Custom
    }

    [HubCategory(Startup), HubDescription("DiscordSettings_Token_Description"), HubDisplayName("DiscordSettings_Token_DisplayName")]
    public string Token { get; set; } = string.Empty;

    [HubCategory(Operation), HubDescription("DiscordSettings_AdditionalEmbedText_Description"), HubDisplayName("DiscordSettings_AdditionalEmbedText_DisplayName")]
    public string[] AdditionalEmbedText { get; set; } = [];

    [HubCategory(Users), HubDescription("DiscordSettings_AllowGlobalSudo_Description"), HubDisplayName("DiscordSettings_AllowGlobalSudo_DisplayName")]
    public bool AllowGlobalSudo { get; set; } = true;

    [HubCategory(Channels), HubDescription("DiscordSettings_AnnouncementChannels_Description"), HubDisplayName("DiscordSettings_AnnouncementChannels_DisplayName")]
    public RemoteControlAccessList AnnouncementChannels { get; set; } = new();

    [HubCategory(Channels), HubDescription("DiscordSettings_AbuseLogChannels_Description"), HubDisplayName("DiscordSettings_AbuseLogChannels_DisplayName")]
    public RemoteControlAccessList AbuseLogChannels { get; set; } = new();

    public AnnouncementSettingsCategory AnnouncementSettings { get; set; } = new();

    [HubCategory(Startup), HubDescription("DiscordSettings_BotColorStatusTradeOnly_Description"), HubDisplayName("DiscordSettings_BotColorStatusTradeOnly_DisplayName")]
    public bool BotColorStatusTradeOnly { get; set; } = true;

    [HubCategory(Startup), HubDescription("DiscordSettings_BotEmbedStatus_Description"), HubDisplayName("DiscordSettings_BotEmbedStatus_DisplayName")]
    public bool BotEmbedStatus { get; set; } = true;

    [HubCategory(Startup), HubDescription("DiscordSettings_BotGameStatus_Description"), HubDisplayName("DiscordSettings_BotGameStatus_DisplayName")]
    public string BotGameStatus { get; set; } = "Trading Pokémon";

    [HubCategory(Startup), HubDescription("DiscordSettings_ChannelStatus_Description"), HubDisplayName("DiscordSettings_ChannelStatus_DisplayName")]
    public bool ChannelStatus { get; set; } = true;

    [HubCategory(Channels), HubDescription("DiscordSettings_ChannelWhitelist_Description"), HubDisplayName("DiscordSettings_ChannelWhitelist_DisplayName")]
    public RemoteControlAccessList ChannelWhitelist { get; set; } = new();

    [HubCategory(Startup), HubDescription("DiscordSettings_CommandPrefix_Description"), HubDisplayName("DiscordSettings_CommandPrefix_DisplayName")]
    public string CommandPrefix { get; set; } = "$";

    [HubCategory(Startup), HubDescription("DiscordSettings_AllowAnyPrefix_Description"), HubDisplayName("DiscordSettings_AllowAnyPrefix_DisplayName")]
    public bool AllowAnyPrefix { get; set; } = false;

    [HubCategory(Operation), HubDescription("DiscordSettings_ConvertPKMReplyAnyChannel_Description"), HubDisplayName("DiscordSettings_ConvertPKMReplyAnyChannel_DisplayName")]
    public bool ConvertPKMReplyAnyChannel { get; set; } = false;

    [HubCategory(Operation), HubDescription("DiscordSettings_ConvertPKMToShowdownSet_Description"), HubDisplayName("DiscordSettings_ConvertPKMToShowdownSet_DisplayName")]
    public bool ConvertPKMToShowdownSet { get; set; } = true;

    [HubCategory(Channels), HubDescription("DiscordSettings_UserDMsToBotForwarder_Description"), HubDisplayName("DiscordSettings_UserDMsToBotForwarder_DisplayName")]
    public string UserDMsToBotForwarder { get; set; } = string.Empty;

    [HubCategory(Users), HubDescription("DiscordSettings_GlobalSudoList_Description"), HubDisplayName("DiscordSettings_GlobalSudoList_DisplayName")]
    public RemoteControlAccessList GlobalSudoList { get; set; } = new();

    [HubCategory(Operation), HubDescription("DiscordSettings_HelloResponse_Description"), HubDisplayName("DiscordSettings_HelloResponse_DisplayName")]
    public string HelloResponse { get; set; } = "Hi {0}!";

    [HubCategory(Channels), HubDescription("DiscordSettings_LoggingChannels_Description"), HubDisplayName("DiscordSettings_LoggingChannels_DisplayName")]
    public RemoteControlAccessList LoggingChannels { get; set; } = new();

    [HubCategory(Startup), HubDescription("DiscordSettings_ModuleBlacklist_Description"), HubDisplayName("DiscordSettings_ModuleBlacklist_DisplayName")]
    public string ModuleBlacklist { get; set; } = string.Empty;

    [HubCategory(Startup), HubDescription("DiscordSettings_OfflineEmoji_Description"), HubDisplayName("DiscordSettings_OfflineEmoji_DisplayName")]
    public string OfflineEmoji { get; set; } = "❌";

    [HubCategory(Startup), HubDescription("DiscordSettings_OnlineEmoji_Description"), HubDisplayName("DiscordSettings_OnlineEmoji_DisplayName")]
    public string OnlineEmoji { get; set; } = "✅";

    [HubCategory(Operation), HubDescription("DiscordSettings_ReplyCannotUseCommandInChannel_Description"), HubDisplayName("DiscordSettings_ReplyCannotUseCommandInChannel_DisplayName")]
    public bool ReplyCannotUseCommandInChannel { get; set; } = true;

    [HubCategory(Operation), HubDescription("DiscordSettings_ReplyToThanks_Description"), HubDisplayName("DiscordSettings_ReplyToThanks_DisplayName")]
    public bool ReplyToThanks { get; set; } = false;

    [HubCategory(Operation), HubDescription("DiscordSettings_ReturnPKMs_Description"), HubDisplayName("DiscordSettings_ReturnPKMs_DisplayName")]
    public bool ReturnPKMs { get; set; } = true;

    [HubCategory(Operation), HubDescription("DiscordSettings_MessageDeletionEnabled_Description"), HubDisplayName("DiscordSettings_MessageDeletionEnabled_DisplayName")]
    public bool MessageDeletionEnabled { get; set; } = true;

    [HubCategory(Operation), HubDescription("DiscordSettings_ErrorMessageDeleteDelaySeconds_Description"), HubDisplayName("DiscordSettings_ErrorMessageDeleteDelaySeconds_DisplayName")]
    public int ErrorMessageDeleteDelaySeconds { get; set; } = 10;

    [HubCategory(Operation), HubDescription("DiscordSettings_DeleteUserCommandMessages_Description"), HubDisplayName("DiscordSettings_DeleteUserCommandMessages_DisplayName")]
    public bool DeleteUserCommandMessages { get; set; } = true;

    [HubCategory(Roles), HubDescription("DiscordSettings_RoleCanClone_Description"), HubDisplayName("DiscordSettings_RoleCanClone_DisplayName")]
    public RemoteControlAccessList RoleCanClone { get; set; } = new() { AllowIfEmpty = true };

    [HubCategory(Roles), HubDescription("DiscordSettings_RoleCanDump_Description"), HubDisplayName("DiscordSettings_RoleCanDump_DisplayName")]
    public RemoteControlAccessList RoleCanDump { get; set; } = new() { AllowIfEmpty = true };

    [HubCategory(Roles), HubDescription("DiscordSettings_RoleCanFixOT_Description"), HubDisplayName("DiscordSettings_RoleCanFixOT_DisplayName")]
    public RemoteControlAccessList RoleCanFixOT { get; set; } = new() { AllowIfEmpty = true };

    [HubCategory(Roles), HubDescription("DiscordSettings_RoleCanSeedCheckorSpecialRequest_Description"), HubDisplayName("DiscordSettings_RoleCanSeedCheckorSpecialRequest_DisplayName")]
    public RemoteControlAccessList RoleCanSeedCheckorSpecialRequest { get; set; } = new() { AllowIfEmpty = true };

    [HubCategory(Roles), HubDescription("DiscordSettings_RoleCanTrade_Description"), HubDisplayName("DiscordSettings_RoleCanTrade_DisplayName")]
    public RemoteControlAccessList RoleCanTrade { get; set; } = new() { AllowIfEmpty = true };

    [HubCategory(Roles), HubDescription("DiscordSettings_RoleFavored_Description"), HubDisplayName("DiscordSettings_RoleFavored_DisplayName")]
    public RemoteControlAccessList RoleFavored { get; set; } = new() { AllowIfEmpty = false };

    // Whitelists
    [HubCategory(Roles), HubDescription("DiscordSettings_RoleRemoteControl_Description"), HubDisplayName("DiscordSettings_RoleRemoteControl_DisplayName")]
    public RemoteControlAccessList RoleRemoteControl { get; set; } = new() { AllowIfEmpty = false };

    [HubCategory(Roles), HubDescription("DiscordSettings_RoleSudo_Description"), HubDisplayName("DiscordSettings_RoleSudo_DisplayName")]
    public RemoteControlAccessList RoleSudo { get; set; } = new() { AllowIfEmpty = false };

    // Operation
    [HubCategory(Servers), HubDescription("DiscordSettings_ServerBlacklist_Description"), HubDisplayName("DiscordSettings_ServerBlacklist_DisplayName")]
    public RemoteControlAccessList ServerBlacklist { get; set; } = new() { AllowIfEmpty = false };

    [HubCategory(Channels), HubDescription("DiscordSettings_TradeStartingChannels_Description"), HubDisplayName("DiscordSettings_TradeStartingChannels_DisplayName")]
    public RemoteControlAccessList TradeStartingChannels { get; set; } = new();

    [HubCategory(Channels), HubDescription("DiscordSettings_FullTradeErrorLogChannels_Description"), HubDisplayName("DiscordSettings_FullTradeErrorLogChannels_DisplayName")]
    public RemoteControlAccessList FullTradeErrorLogChannels { get; set; } = new();

    // Startup
    [HubCategory(Users), HubDescription("DiscordSettings_UserBlacklist_Description"), HubDisplayName("DiscordSettings_UserBlacklist_DisplayName")]
    public RemoteControlAccessList UserBlacklist { get; set; } = new();

    public override string ToString() => "Discord Integration Settings";

    [HubCategory(Operation), TypeConverter(typeof(CategoryConverter<AnnouncementSettingsCategory>))]
    public class AnnouncementSettingsCategory
    {
        public EmbedColorOption AnnouncementEmbedColor { get; set; } = EmbedColorOption.Purple;

        [HubCategory("Embed Settings"), HubDescription("AnnouncementSettingsCategory_AnnouncementThumbnailOption_Description")]
        public ThumbnailOption AnnouncementThumbnailOption { get; set; } = ThumbnailOption.Gengar;

        [HubCategory("Embed Settings"), HubDescription("AnnouncementSettingsCategory_CustomAnnouncementThumbnailUrl_Description")]
        public string CustomAnnouncementThumbnailUrl { get; set; } = string.Empty;

        [HubCategory("Embed Settings"), HubDescription("AnnouncementSettingsCategory_RandomAnnouncementColor_Description")]
        public bool RandomAnnouncementColor { get; set; } = false;

        [HubCategory("Embed Settings"), HubDescription("AnnouncementSettingsCategory_RandomAnnouncementThumbnail_Description")]
        public bool RandomAnnouncementThumbnail { get; set; } = false;

        public override string ToString() => "Announcement Settings";
    }
}
