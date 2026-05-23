using System.ComponentModel;
using SysBot.Pokemon.Localization;

namespace SysBot.Pokemon;

/// <summary>
/// Settings for the Web Control Panel server
/// </summary>
public sealed class WebServerSettings
{
    private const string WebServer = nameof(WebServer);
    
    [HubCategory(WebServer)]
    [HubDescription("WebServerSettings_ControlPanelPort_Description")]
    public int ControlPanelPort { get; set; } = 8080;
    
    [HubCategory(WebServer)]
    [HubDescription("WebServerSettings_EnableWebServer_Description")]
    public bool EnableWebServer { get; set; } = true;
    
    [HubCategory(WebServer)]
    [HubDescription("WebServerSettings_AllowExternalConnections_Description")]
    public bool AllowExternalConnections { get; set; } = false;
}
