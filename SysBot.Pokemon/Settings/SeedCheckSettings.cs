using System.ComponentModel;
using SysBot.Pokemon.Localization;

namespace SysBot.Pokemon;

public class SeedCheckSettings
{
    private const string FeatureToggle = nameof(FeatureToggle);

    [HubCategory(FeatureToggle), HubDescription("SeedCheckSettings_ResultDisplayMode_Description")]
    public SeedCheckResults ResultDisplayMode { get; set; }

    [HubCategory(FeatureToggle), HubDescription("SeedCheckSettings_ShowAllZ3Results_Description")]
    public bool ShowAllZ3Results { get; set; }

    public override string ToString() => "Seed Check Settings";
}

public enum SeedCheckResults
{
    ClosestOnly,            // Only gets the first shiny

    FirstStarAndSquare,     // Gets the first star shiny and first square shiny

    FirstThree,             // Gets the first three frames
}
