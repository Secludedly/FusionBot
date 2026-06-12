using FluentAssertions;
using PKHeX.Core;
using PKHeX.Core.AutoMod;
using SysBot.Pokemon;
using Xunit;

namespace SysBot.Tests;

// Regression coverage for egg trades honoring custom Ball and batch commands (e.g. .Scale).
// Two distinct bugs were fixed:
//   1. ALM's egg generator never runs the batch pipeline (and the egg's checksum is invalid,
//      which makes PKHeX's batch editor skip it), so .Scale=/marks/etc. were silently dropped.
//   2. ALM's SetSuggestedBall replaces a user-specified ball with a color-matched one whenever
//      its egg ball verifier rejects the request, so custom balls were lost on eggs.
// AutoLegalityWrapper.GenerateEgg now refreshes the checksum, re-asserts the requested ball,
// and runs the batch pipeline — each reverted only if it makes the egg illegal.
public class EggBatchEditTests
{
    static EggBatchEditTests() => AutoLegalityWrapper.EnsureInitialized(new Pokemon.LegalitySettings());

    [Fact]
    public void Egg_HonorsBallAndScale_Simple()
    {
        var sav = AutoLegalityWrapper.GetTrainerInfo<PK9>();
        var regen = new RegenTemplate(new ShowdownSet("Larvesta (F)\nBall: Dusk Ball\n.Scale=255"));

        var pk = AutoLegalityWrapper.GenerateEgg(sav, regen, out var result);

        result.Should().Be(LegalizationResult.Regenerated);
        pk.IsEgg.Should().BeTrue();
        ((Ball)pk.Ball).Should().Be(Ball.Dusk);
        ((PK9)pk).Scale.Should().Be(255);
        new LegalityAnalysis(pk).Valid.Should().BeTrue();
    }

    // Mirrors the real call sites (.egg / .t Egg() / Twitch): GetTemplate(set) is called first,
    // which parses Ball:/.Scale= into the template AND removes them from set.InvalidLines. The egg
    // path must reuse THAT template — a second RegenTemplate(set) is empty and loses ball/scale.
    [Fact]
    public void Egg_ReusingTemplateFromGetTemplate_KeepsBallAndScale()
    {
        var sav = AutoLegalityWrapper.GetTrainerInfo<PK9>();
        var set = new ShowdownSet("Charmander\nBall: Great\n.Scale=255");

        var template = AutoLegalityWrapper.GetTemplate(set);   // consumes the Ball:/.Scale= lines

        // A second template from the same set is now empty — the original bug.
        var second = new RegenTemplate(set);
        second.Regen.Extra.Ball.Should().Be(Ball.None, "the first GetTemplate consumed the lines");
        second.Regen.HasBatchSettings.Should().BeFalse();

        // Reusing the first template preserves the user's request.
        template.Regen.Extra.Ball.Should().Be(Ball.Great);
        template.Regen.HasBatchSettings.Should().BeTrue();

        var pk = AutoLegalityWrapper.GenerateEgg(sav, template, out var result);
        result.Should().Be(LegalizationResult.Regenerated);
        ((Ball)pk.Ball).Should().Be(Ball.Great);
        ((PK9)pk).Scale.Should().Be(255);
        new LegalityAnalysis(pk).Valid.Should().BeTrue();
    }

    [Fact]
    public void Egg_HonorsBallAndScale_Charmander()
    {
        var sav = AutoLegalityWrapper.GetTrainerInfo<PK9>();
        var regen = new RegenTemplate(new ShowdownSet("Charmander\nBall: Great\n.Scale=255"));

        var pk = AutoLegalityWrapper.GenerateEgg(sav, regen, out var result);

        result.Should().Be(LegalizationResult.Regenerated);
        ((Ball)pk.Ball).Should().Be(Ball.Great);
        ((PK9)pk).Scale.Should().Be(255);
        new LegalityAnalysis(pk).Valid.Should().BeTrue();
    }

    // Simulates ALM's color-matcher having replaced the requested ball: the wrapper must
    // recover the user's ball as long as it is legal for the egg.
    [Fact]
    public void Egg_RequestedBallSurvivesColorMatching()
    {
        var sav = AutoLegalityWrapper.GetTrainerInfo<PK9>();
        var pk = AutoLegalityWrapper.GenerateEgg(sav, new RegenTemplate(new ShowdownSet("Charmander\nBall: Great")), out _);
        ((Ball)pk.Ball).Should().Be(Ball.Great);

        // Force a color-matched ball, then prove Great is independently legal on the egg
        // (this is the legality check the wrapper relies on to keep the requested ball).
        pk.Ball = (byte)Ball.Repeat;
        pk.RefreshChecksum();
        new LegalityAnalysis(pk).Valid.Should().BeTrue("a color-matched ball is legal");

        pk.Ball = (byte)Ball.Great;
        pk.RefreshChecksum();
        new LegalityAnalysis(pk).Valid.Should().BeTrue("the requested Great Ball is also legal, so the wrapper keeps it");
    }

    // Full repro of the user's ".t Egg(Larvesta)" set (post-normalization), Scale: 0.
    [Fact]
    public void Egg_HonorsBallAndScale_FullSet()
    {
        var content =
            "Larvesta (F)\nBall: Dusk\nLevel: 1\nShiny: No\nAbility: Flame Body\n" +
            "IVs: 6 HP / 0 Atk / 20 Def / 15 SpD\nTera Type: Fire\nBold Nature\n" +
            "~=Location=6\n.MetLevel=1\n.Version=50\n.Scale=0\n" +
            ".HT_HP=false\n.HT_ATK=false\n.HT_DEF=false\n.HT_SPA=false\n.HT_SPD=false\n.HT_SPE=false\n" +
            "- Ember\n- Morning Sun\n- String Shot\n- Thrash";
        var sav = AutoLegalityWrapper.GetTrainerInfo<PK9>();

        var pk = AutoLegalityWrapper.GenerateEgg(sav, new RegenTemplate(new ShowdownSet(content)), out var result);

        result.Should().Be(LegalizationResult.Regenerated);
        ((Ball)pk.Ball).Should().Be(Ball.Dusk);
        ((PK9)pk).Scale.Should().Be(0);
        new LegalityAnalysis(pk).Valid.Should().BeTrue();
    }
}
