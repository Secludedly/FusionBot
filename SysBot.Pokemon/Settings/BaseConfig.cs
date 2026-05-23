using System.ComponentModel;
using SysBot.Pokemon.Localization;

namespace SysBot.Pokemon;

/// <summary>
/// Console agnostic settings
/// </summary>
public abstract class BaseConfig
{
    protected const string FeatureToggle = nameof(FeatureToggle);

    protected const string Operation = nameof(Operation);

    [Browsable(false)]
    private const string Debug = nameof(Debug);

    [HubCategory(FeatureToggle), HubDescription("BaseConfig_AntiIdle_Description")]
    public bool AntiIdle { get; set; } = true;

    [HubCategory(Operation)]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public FolderSettings Folder { get; set; } = new();

    [HubCategory(Operation)]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public LegalitySettings Legality { get; set; } = new();

    [HubCategory(FeatureToggle), HubDescription("BaseConfig_LoggingEnabled_Description")]
    public bool LoggingEnabled { get; set; } = true;

    [HubCategory(FeatureToggle), HubDescription("BaseConfig_MaxArchiveFiles_Description")]
    public int MaxArchiveFiles { get; set; } = 10;

    public abstract bool Shuffled { get; }

    [Browsable(false)]
    [HubCategory(Debug), HubDescription("BaseConfig_SkipConsoleBotCreation_Description")]
    public bool SkipConsoleBotCreation { get; set; }

    [HubCategory(FeatureToggle), HubDescription("BaseConfig_UseKeyboard_Description")]
    public bool UseKeyboard { get; set; } = true;
}
