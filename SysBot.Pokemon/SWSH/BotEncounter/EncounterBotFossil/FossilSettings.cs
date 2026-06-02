using System.ComponentModel;
using SysBot.Pokemon.Localization;

namespace SysBot.Pokemon;

public class FossilSettings
{
    private const string Counts = nameof(Counts);

    private const string Fossil = nameof(Fossil);

    /// <summary>
    /// Toggle for injecting fossil pieces.
    /// </summary>
    [HubDisplayName("FossilSettings_InjectWhenEmpty_DisplayName")]
    [HubCategory(Fossil), HubDescription("FossilSettings_InjectWhenEmpty_Description")]
    public bool InjectWhenEmpty { get; set; }

    [HubDisplayName("FossilSettings_Species_DisplayName")]
    [HubCategory(Fossil), HubDescription("FossilSettings_Species_Description")]
    public FossilSpecies Species { get; set; } = FossilSpecies.Dracozolt;

    public override string ToString() => "Fossil Bot Settings";
}
