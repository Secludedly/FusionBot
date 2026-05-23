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

    [HubCategory(Startup), HubDescription("YouTubeSettings_ChannelID_Description")]
    public string ChannelID { get; set; } = string.Empty;

    [HubCategory(Startup), HubDescription("YouTubeSettings_ClientID_Description")]
    public string ClientID { get; set; } = string.Empty;

    // Startup
    [HubCategory(Startup), HubDescription("YouTubeSettings_ClientSecret_Description")]
    public string ClientSecret { get; set; } = string.Empty;

    [HubCategory(Startup), HubDescription("YouTubeSettings_CommandPrefix_Description")]
    public char CommandPrefix { get; set; } = '$';

    [HubCategory(Operation), HubDescription("YouTubeSettings_MessageStart_Description")]
    public string MessageStart { get; set; } = string.Empty;

    [HubCategory(Operation), HubDescription("YouTubeSettings_SudoList_Description")]
    public string SudoList { get; set; } = string.Empty;

    // Operation
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
