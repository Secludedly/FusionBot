using System;
using System.ComponentModel;
using System.Linq;
using SysBot.Pokemon.Localization;

namespace SysBot.Pokemon;

public class YouTubeSettings
{
    private const string Messages = nameof(Messages);

    private const string Operation = nameof(Operation);

    private const string Startup = nameof(Startup);

    [HubDisplayName("YouTubeSettings_ChannelID_DisplayName")]
    [HubCategory(Startup), HubDescription("YouTubeSettings_ChannelID_Description")]
    public string ChannelID { get; set; } = string.Empty;

    [HubDisplayName("YouTubeSettings_ClientID_DisplayName")]
    [HubCategory(Startup), HubDescription("YouTubeSettings_ClientID_Description")]
    public string ClientID { get; set; } = string.Empty;

    // Startup
    [HubDisplayName("YouTubeSettings_ClientSecret_DisplayName")]
    [HubCategory(Startup), HubDescription("YouTubeSettings_ClientSecret_Description")]
    public string ClientSecret { get; set; } = string.Empty;

    [HubDisplayName("YouTubeSettings_CommandPrefix_DisplayName")]
    [HubCategory(Startup), HubDescription("YouTubeSettings_CommandPrefix_Description")]
    public char CommandPrefix { get; set; } = '$';

    [HubDisplayName("YouTubeSettings_MessageStart_DisplayName")]
    [HubCategory(Operation), HubDescription("YouTubeSettings_MessageStart_Description")]
    public string MessageStart { get; set; } = string.Empty;

    [HubDisplayName("YouTubeSettings_SudoList_DisplayName")]
    [HubCategory(Operation), HubDescription("YouTubeSettings_SudoList_Description")]
    public string SudoList { get; set; } = string.Empty;

    // Operation
    [HubDisplayName("YouTubeSettings_UserBlacklist_DisplayName")]
    [HubCategory(Operation), HubDescription("YouTubeSettings_UserBlacklist_Description")]
    public string UserBlacklist { get; set; } = string.Empty;

    public bool IsSudo(string username)
    {
        var sudos = SudoList.Split([",", ", ", " "], StringSplitOptions.RemoveEmptyEntries);
        return sudos.Contains(username);
    }

    public override string ToString() => "YouTube Integration Settings";
}

public enum YouTubeMessageDestination
{
    Disabled,

    Channel,
}
