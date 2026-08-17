using FluentAssertions;
using PKHeX.Core;
using PKHeX.Core.AutoMod;
using SysBot.Pokemon;
using Xunit;

namespace SysBot.Tests;

/// <summary>
/// Guards the SWSH shiny regression where ALM generated with its own fallback trainer
/// (OT "ALM") and the wrapper patched OT/TID/SID afterwards. Rewriting the trainer IDs
/// forces a PID rebuild, which breaks the EC->PID correlation PKHeX enforces on SWSH
/// overworld wild slots: Star shinies (ShinyXor 1-15) failed with "PID+ correlation does
/// not match what was expected for the Encounter's type", while Square (ShinyXor 0) —
/// the state the forced-shiny formula happens to produce — kept working.
/// </summary>
public class ShinySwshTests
{
    static ShinySwshTests() => AutoLegalityWrapper.EnsureInitialized(new Pokemon.LegalitySettings());

    private static string ShinySet(string species) =>
        $"{species}\nShiny: Yes\nEVs: 252 Atk / 4 SpD / 252 Spe\nJolly Nature\n- Protect";

    [Theory]
    [InlineData("Dugtrio")]
    [InlineData("Gengar")]
    [InlineData("Corviknight")]
    public void ShinyWildSwshStaysLegal(string species)
    {
        var sav = AutoLegalityWrapper.GetTrainerInfo<PK8>();
        for (int i = 0; i < 10; i++)
        {
            var set = new ShowdownSet(ShinySet(species));
            var pk = sav.GetLegal(AutoLegalityWrapper.GetTemplate(set), out _);
            pk.Should().NotBeNull();
            pk.IsShiny.Should().BeTrue();
            new LegalityAnalysis(pk).Valid.Should().BeTrue($"{species} attempt {i} (ShinyXor {pk.ShinyXor}) should be legal");
        }
    }

    [Fact]
    public void GeneratesWithConfiguredTrainer()
    {
        var cfg = new Pokemon.LegalitySettings();
        var sav = AutoLegalityWrapper.GetTrainerInfo<PK8>();
        var set = new ShowdownSet(ShinySet("Dugtrio"));
        var pk = sav.GetLegal(AutoLegalityWrapper.GetTemplate(set), out _);

        // ALM must not fall back to its own trainer data — that is what forced the
        // PID-breaking fixup in the first place.
        pk.OriginalTrainerName.Should().Be(cfg.GenerateOT);
        pk.TID16.Should().Be(cfg.GenerateTID16);
        pk.SID16.Should().Be(cfg.GenerateSID16);
    }
}
