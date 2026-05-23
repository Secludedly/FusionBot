using SysBot.Base;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using SysBot.Pokemon.Localization;

namespace SysBot.Pokemon;

public class EncounterSettings : IBotStateSettings, ICountSettings
{
    private const string Counts = nameof(Counts);

    private const string Encounter = nameof(Encounter);

    private const string Settings = nameof(Settings);

    private int _completedEggs;

    private int _completedFossils;

    private int _completedLegend;

    private int _completedWild;

    [HubCategory(Counts), HubDescription("EncounterSettings_CompletedEggs_Description")]
    public int CompletedEggs
    {
        get => _completedEggs;
        set => _completedEggs = value;
    }

    [HubCategory(Counts), HubDescription("EncounterSettings_CompletedEncounters_Description")]
    public int CompletedEncounters
    {
        get => _completedWild;
        set => _completedWild = value;
    }

    [HubCategory(Counts), HubDescription("EncounterSettings_CompletedFossils_Description")]
    public int CompletedFossils
    {
        get => _completedFossils;
        set => _completedFossils = value;
    }

    [HubCategory(Counts), HubDescription("EncounterSettings_CompletedLegends_Description")]
    public int CompletedLegends
    {
        get => _completedLegend;
        set => _completedLegend = value;
    }

    [HubCategory(Encounter), HubDescription("EncounterSettings_ContinueAfterMatch_Description")]
    public ContinueAfterMatch ContinueAfterMatch { get; set; } = ContinueAfterMatch.StopExit;

    [HubCategory(Counts), HubDescription("EncounterSettings_EmitCountsOnStatusCheck_Description")]
    public bool EmitCountsOnStatusCheck { get; set; }

    [HubCategory(Encounter), HubDescription("EncounterSettings_EncounteringType_Description")]
    public EncounterMode EncounteringType { get; set; } = EncounterMode.VerticalLine;

    [HubCategory(Settings)]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public FossilSettings Fossil { get; set; } = new();

    [HubCategory(Encounter), HubDescription("EncounterSettings_ScreenOff_Description")]
    public bool ScreenOff { get; set; }

    public int AddCompletedEggs() => Interlocked.Increment(ref _completedEggs);

    public int AddCompletedEncounters() => Interlocked.Increment(ref _completedWild);

    public int AddCompletedFossils() => Interlocked.Increment(ref _completedFossils);

    public int AddCompletedLegends() => Interlocked.Increment(ref _completedLegend);

    public IEnumerable<string> GetNonZeroCounts()
    {
        if (!EmitCountsOnStatusCheck)
            yield break;
        if (CompletedEncounters != 0)
            yield return $"Wild Encounters: {CompletedEncounters}";
        if (CompletedLegends != 0)
            yield return $"Legendary Encounters: {CompletedLegends}";
        if (CompletedEggs != 0)
            yield return $"Eggs Received: {CompletedEggs}";
        if (CompletedFossils != 0)
            yield return $"Completed Fossils: {CompletedFossils}";
    }

    public override string ToString() => "Encounter Bot SWSH Settings";
}
