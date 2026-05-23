using System.ComponentModel;
using System.IO;
using SysBot.Pokemon.Localization;

namespace SysBot.Pokemon;

public class FolderSettings : IDumper
{
    private const string FeatureToggle = nameof(FeatureToggle);

    private const string Files = nameof(Files);

    [HubCategory(Files), HubDescription("FolderSettings_DistributeFolder_Description"), HubDisplayName("FolderSettings_DistributeFolder_DisplayName")]
    public string DistributeFolder { get; set; } = string.Empty;

    [HubCategory(FeatureToggle), HubDescription("FolderSettings_Dump_Description"), HubDisplayName("FolderSettings_Dump_DisplayName")]
    public bool Dump { get; set; }

    [HubCategory(Files), HubDescription("FolderSettings_DumpFolder_Description"), HubDisplayName("FolderSettings_DumpFolder_DisplayName")]
    public string DumpFolder { get; set; } = string.Empty;

    [HubCategory(Files), HubDescription("FolderSettings_HOMEReadyPKMFolder_Description"), HubDisplayName("FolderSettings_HOMEReadyPKMFolder_DisplayName")]
    public string HOMEReadyPKMFolder { get; set; } = string.Empty;

    [HubCategory(Files), HubDescription("FolderSettings_EventsFolder_Description"), HubDisplayName("FolderSettings_EventsFolder_DisplayName")]
    public string EventsFolder { get; set; } = string.Empty;

    [HubCategory(Files), HubDescription("FolderSettings_BattleReadyPKMFolder_Description"), HubDisplayName("FolderSettings_BattleReadyPKMFolder_DisplayName")]
    public string BattleReadyPKMFolder { get; set; } = string.Empty;

    [HubCategory(Files), HubDescription("FolderSettings_PKHeXDirectory_Description"), HubDisplayName("FolderSettings_PKHeXDirectory_DisplayName")]
    public string PKHeXDirectory { get; set; } = string.Empty;

    [HubCategory(Files), HubDescription("FolderSettings_SwitchRemoteForPC_Description"), HubDisplayName("FolderSettings_SwitchRemoteForPC_DisplayName")]
    public string SwitchRemoteForPC { get; set; } = string.Empty;

    public void CreateDefaults(string path)
    {
        var dump = Path.Combine(path, "dump");
        Directory.CreateDirectory(dump);
        DumpFolder = dump;
        Dump = true;

        var distribute = Path.Combine(path, "distribute");
        Directory.CreateDirectory(distribute);
        DistributeFolder = distribute;
    }

    public override string ToString() => "Folder / Dumping Settings";
}
