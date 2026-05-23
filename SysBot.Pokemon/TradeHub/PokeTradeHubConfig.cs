using System.ComponentModel;
using SysBot.Pokemon.Localization;

// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global

namespace SysBot.Pokemon;

public sealed class PokeTradeHubConfig : BaseConfig
{
    [Browsable(false)]
    private const string BotEncounter = nameof(BotEncounter);

    private const string BotTrade = nameof(BotTrade);

    private const string Integration = nameof(Integration);

    [HubCategory(Operation), HubDescription("PokeTradeHubConfig_Language_Description")]
    public UILanguage Language { get; set; } = UILanguage.English;

    [HubCategory(BotTrade), HubDescription("PokeTradeHubConfig_BotName_Description")]
    public string BotName { get; set; } = string.Empty;

    [HubCategory(BotTrade)]
    [HubDescription("PokeTradeHubConfig_BotLogoImage_Description")]
    public string BotLogoImage { get; set; } = string.Empty;

    [HubCategory(BotTrade)]
    [HubDescription("PokeTradeHubConfig_BotLogoSparkleColor1_Description")]
    public string BotLogoSparkleColor1 { get; set; } = string.Empty;

    [HubCategory(BotTrade)]
    [HubDescription("PokeTradeHubConfig_BotLogoSparkleColor2_Description")]
    public string BotLogoSparkleColor2 { get; set; } = string.Empty;

    [HubCategory(Integration)]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public DiscordSettings Discord { get; set; } = new();

    [HubCategory(BotTrade), HubDescription("PokeTradeHubConfig_Distribution_Description")]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public DistributionSettings Distribution { get; set; } = new();

    // Encounter Bots - For finding or hosting Pokémon in-game.
    [Browsable(false)]
    [HubCategory(BotEncounter)]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public EncounterSettings EncounterSWSH { get; set; } = new();

    [HubCategory(Integration), HubDescription("PokeTradeHubConfig_Favoritism_Description")]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public FavoredPrioritySettings Favoritism { get; set; } = new();

    [HubCategory(Operation)]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public QueueSettings Queues { get; set; } = new();

    [Browsable(false)]
    [HubCategory(BotEncounter)]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public RaidSettings RaidSWSH { get; set; } = new();

    [Browsable(false)]
    [HubCategory(BotTrade)]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public SeedCheckSettings SeedCheckSWSH { get; set; } = new();

    [Browsable(false)]
    public override bool Shuffled => Distribution.Shuffled;

    [Browsable(false)]
    [HubCategory(BotEncounter), HubDescription("PokeTradeHubConfig_StopConditions_Description")]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public StopConditionSettings StopConditions { get; set; } = new();

    [HubCategory(Integration), HubDescription("PokeTradeHubConfig_Stream_Description")]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public StreamSettings Stream { get; set; } = new();

    [Browsable(false)]
    [HubCategory(Integration), HubDescription("PokeTradeHubConfig_ThemeOption_Description")]
    public string ThemeOption { get; set; } = string.Empty;

    [HubCategory(Operation), HubDescription("PokeTradeHubConfig_Timings_Description")]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public TimingSettings Timings { get; set; } = new();

    // Trade Bots

    [HubCategory(BotTrade)]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public TradeSettings Trade { get; set; } = new();

    [HubCategory(BotTrade)]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public TradeAbuseSettings TradeAbuse { get; set; } = new();

    // Integration
    [HubCategory(Integration)]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public TwitchSettings Twitch { get; set; } = new();

    [HubCategory(Integration)]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public YouTubeSettings YouTube { get; set; } = new();

    [HubCategory(Operation), HubDescription("PokeTradeHubConfig_Recovery_Description")]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public RecoverySettings Recovery { get; set; } = new();

    [HubCategory(Integration), HubDescription("PokeTradeHubConfig_WebServer_Description")]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public WebServerSettings WebServer { get; set; } = new();
}
