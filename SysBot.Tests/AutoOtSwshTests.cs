using System;
using System.Collections.Generic;
using System.Linq;
using PKHeX.Core;
using PKHeX.Core.AutoMod;
using SysBot.Pokemon;
using Xunit;

namespace SysBot.Tests;

/// <summary>
/// Guards the SWSH AutoOT regression where applying the trade partner's OT/TID/SID to a
/// shiny wild Pokemon made it illegal, so the bot logged "Pokemon not valid after using
/// Trade Partner Info" and shipped the mon with the host's configured OT instead.
///
/// Overworld wild slots (EncounterSlot8) derive the EC and the PID from one encounter seed
/// and PKHeX validates that correlation, accepting only the raw seed PID or the PID the
/// game's forced-shiny formula produces — which is always Square. Rebuilding the PID to
/// preserve a Star shiny (ShinyXor 1-15) against the partner's IDs therefore fails with
/// "PID+ correlation does not match what was expected for the Encounter's type".
///
/// <see cref="PokeTradeBotSWSH"/> answers that with a fallback ladder, since AutoOT sticking
/// matters more than the shiny type or the requested language. These tests mirror that ladder.
/// </summary>
public class AutoOtSwshTests
{
    static AutoOtSwshTests() => AutoLegalityWrapper.EnsureInitialized(new Pokemon.LegalitySettings());

    private const uint PartnerTidSid = 123456789u;
    private const string PartnerName = "Partner";
    private const int PartnerLanguage = (int)LanguageID.English;

    /// Mirrors the trainer swap and fallback ladder in PokeTradeBotSWSH.ApplyAutoOT.
    private static PK8? ApplyAutoOT(PK8 toSend)
    {
        var cfg = new Pokemon.LegalitySettings();
        var cln = toSend.Clone();
        cln.OriginalTrainerGender = 0;
        cln.TrainerTID7 = PartnerTidSid % 1_000_000;
        cln.TrainerSID7 = PartnerTidSid / 1_000_000;

        var configLanguage = (int)cfg.GenerateLanguage;
        cln.Language = toSend.Language != configLanguage && toSend.Language is >= 1 and <= 12
            ? toSend.Language
            : PartnerLanguage;

        cln.OriginalTrainerTrash.Clear();
        cln.OriginalTrainerName = PartnerName;

        if (!toSend.IsNicknamed)
            cln.ClearNickname();

        var languages = cln.Language == PartnerLanguage
            ? [cln.Language]
            : new[] { cln.Language, PartnerLanguage };
        var shinyTypes = toSend.IsShiny && toSend.ShinyXor != 0
            ? new uint[] { toSend.ShinyXor, 0 }
            : [toSend.ShinyXor];

        uint pidLow = toSend.PID & 0xFFFF;

        foreach (var language in languages)
        {
            foreach (var shinyXor in shinyTypes)
            {
                var candidate = cln.Clone();
                candidate.Language = language;

                if (!toSend.IsNicknamed)
                    candidate.ClearNickname();

                if (toSend.IsShiny)
                    candidate.PID = (uint)((candidate.TID16 ^ candidate.SID16 ^ pidLow ^ shinyXor) << 16) | pidLow;

                if (!toSend.ChecksumValid)
                    candidate.RefreshChecksum();

                if (new LegalityAnalysis(candidate).Valid)
                    return candidate;
            }
        }

        return null; // AutoOT dropped; bot ships toSend with the host's OT.
    }

    private static PK8 Generate(string text)
    {
        var sav = AutoLegalityWrapper.GetTrainerInfo<PK8>();
        var pk = sav.GetLegal(AutoLegalityWrapper.GetTemplate(new ShowdownSet(text)), out var res) as PK8;
        Assert.NotNull(pk);
        Assert.True(new LegalityAnalysis(pk).Valid, $"generation failed for '{text.Split('\n')[0]}' ({res})");
        return pk;
    }

    [Theory]
    [InlineData("Dugtrio")]
    [InlineData("Gengar")]
    [InlineData("Corviknight")]
    public void AutoOtKeepsShinyWildLegal(string species)
    {
        for (int i = 0; i < 8; i++)
        {
            var pk = Generate($"{species}\nShiny: Yes\nEVs: 252 Atk / 4 SpD / 252 Spe\nJolly Nature\n- Protect");
            Assert.True(pk.IsShiny, $"{species} attempt {i} generated non-shiny");

            var traded = ApplyAutoOT(pk);

            Assert.NotNull(traded);
            Assert.Equal(PartnerName, traded.OriginalTrainerName);
            Assert.True(traded.IsShiny, $"{species} attempt {i} lost shiny through AutoOT");
            Assert.True(new LegalityAnalysis(traded).Valid,
                $"{species} attempt {i} (ShinyXor {pk.ShinyXor} -> {traded.ShinyXor}) illegal after AutoOT: " +
                new LegalityAnalysis(traded).Report());

            // The spread the user asked for must survive the PID rebuild untouched.
            Assert.Equal(pk.EncryptionConstant, traded.EncryptionConstant);
            Assert.Equal(pk.Nature, traded.Nature);
            Assert.Equal(pk.IV32, traded.IV32);
        }
    }

    /// All three shiny tokens are honoured at generation; AutoOT must then stick for each.
    [Theory]
    [InlineData("Shiny: Yes")]
    [InlineData("Shiny: Star")]
    [InlineData("Shiny: Square")]
    public void AutoOtSticksForEveryShinyToken(string shinyLine)
    {
        for (int i = 0; i < 4; i++)
        {
            var pk = Generate($"Corviknight\n{shinyLine}\nJolly Nature\n- Protect");
            Assert.True(pk.IsShiny);
            if (shinyLine.EndsWith("Star")) Assert.NotEqual(0u, pk.ShinyXor);
            if (shinyLine.EndsWith("Square")) Assert.Equal(0u, pk.ShinyXor);

            var traded = ApplyAutoOT(pk);
            Assert.NotNull(traded);
            Assert.Equal(PartnerName, traded.OriginalTrainerName);
            Assert.True(traded.IsShiny);
            Assert.True(new LegalityAnalysis(traded).Valid, new LegalityAnalysis(traded).Report());
        }
    }

    /// <summary>
    /// The Square fallback is scoped to the encounters that need it. Only overworld wild slots tie
    /// the PID to the encounter seed, so a Star request must still arrive as Star everywhere else —
    /// eggs, statics and Max Lair legendaries keep it through AutoOT untouched.
    /// </summary>
    [Theory]
    [InlineData("Mewtwo")]   // EncounterStatic8U (Dynamax Adventure)
    [InlineData("Dracovish")] // EncounterStatic8
    public void AutoOtKeepsStarOutsideOverworld(string species)
    {
        for (int i = 0; i < 3; i++)
        {
            var pk = Generate($"{species}\nShiny: Star\nJolly Nature\n- Protect");
            Assert.True(pk.IsShiny);
            Assert.NotEqual(0u, pk.ShinyXor);
            if (new LegalityAnalysis(pk).EncounterMatch is EncounterSlot8)
                continue; // overworld: Square is expected, covered elsewhere.

            var traded = ApplyAutoOT(pk);

            Assert.NotNull(traded);
            Assert.Equal(PartnerName, traded.OriginalTrainerName);
            Assert.True(new LegalityAnalysis(traded).Valid, new LegalityAnalysis(traded).Report());
            Assert.Equal(pk.ShinyXor, traded.ShinyXor); // Star preserved, not downgraded.
        }
    }

    /// <summary>
    /// AutoOT has to stick across request shapes, not just plain wild mons. A Japanese request is
    /// the interesting one: JPN/KOR cap the OT at 6 characters, so a 7+ character partner name only
    /// fits once the ladder falls back to the partner's language (and re-stamps the species name).
    /// Mystery Gifts are excluded on purpose — a Wonder Card's OT is part of the encounter identity,
    /// so no partner info can be applied to one and the bot deliberately skips them.
    /// </summary>
    [Theory]
    [InlineData("Dugtrio\nShiny: Yes\nJolly Nature\n- Protect")]
    [InlineData("Dugtrio\nJolly Nature\n- Protect")]
    [InlineData("Dugtrio (Diggy)\nShiny: Yes\nJolly Nature\n- Protect")]
    [InlineData("Ditto\nShiny: Yes\nLanguage: Japanese\n- Transform")]
    [InlineData("Hydreigon\nShiny: Yes\nModest Nature\n- Dark Pulse")]
    [InlineData("Dracovish\nShiny: Yes\nAdamant Nature\n- Fishious Rend")]
    [InlineData("Charizard-Gmax\nShiny: Yes\nTimid Nature\n- Flamethrower")]
    [InlineData("Rotom\nShiny: Yes\nBall: Beast Ball\nTimid Nature\n- Thunder Shock")]
    public void AutoOtSticksAcrossRequestShapes(string text)
    {
        for (int i = 0; i < 3; i++)
        {
            var pk = Generate(text);
            if (pk.FatefulEncounter)
                continue; // Mystery Gift: AutoOT is not applicable.

            var traded = ApplyAutoOT(pk);

            Assert.NotNull(traded);
            Assert.Equal(PartnerName, traded.OriginalTrainerName);
            Assert.True(new LegalityAnalysis(traded).Valid, new LegalityAnalysis(traded).Report());
            Assert.Equal(pk.IsShiny, traded.IsShiny);
        }
    }
}
