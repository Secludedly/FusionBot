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
    [HubCategory(Fossil), HubDescription("FossilSettings_InjectWhenEmpty_Description")]
    public bool InjectWhenEmpty { get; set; }

    [HubCategory(Fossil), HubDescription("FossilSettings_Species_Description")]
    public FossilSpecies Species { get; set; } = FossilSpecies.Dracozolt;

    public override string ToString() => "Fossil Bot Settings";
}
