using Discord;
using PKHeX.Core;
using SysBot.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using SysBot.Pokemon.Localization;

namespace SysBot.Pokemon;

public class TradeSettings : IBotStateSettings, ICountSettings
{
    private const string CountStats = nameof(CountStats);

    private const string HOMELegality = nameof(HOMELegality);

    private const string TradeConfig = nameof(TradeConfig);

    private const string VGCPastesConfig = nameof(VGCPastesConfig);

    private const string Miscellaneous = nameof(Miscellaneous);

    private const string EmbedSettings = nameof(EmbedSettings);

    public override string ToString() => "Trade Configuration Settings";

    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class EmojiInfo
    {
        [HubDescription("EmojiInfo_EmojiString_Description")]
        public string EmojiString { get; set; } = string.Empty;

        public override string ToString()
        {
            return string.IsNullOrEmpty(EmojiString) ? "Not Set" : EmojiString;
        }
    }

    [HubCategory(TradeConfig), HubDescription("EmojiInfo_TradeConfiguration_Description"), HubDisplayName("EmojiInfo_TradeConfiguration_DisplayName"), Browsable(true)]
    public TradeSettingsCategory TradeConfiguration { get; set; } = new();

    [HubCategory(EmbedSettings), HubDescription("EmojiInfo_TradeEmbedSettings_Description"), HubDisplayName("EmojiInfo_TradeEmbedSettings_DisplayName"), Browsable(true)]
    public TradeEmbedSettingsCategory TradeEmbedSettings { get; set; } = new();

    [HubCategory(CountStats), HubDescription("EmojiInfo_CountStatsSettings_Description"), HubDisplayName("EmojiInfo_CountStatsSettings_DisplayName"), Browsable(true)]
    public CountStatsSettingsCategory CountStatsSettings { get; set; } = new();

    [HubCategory(TradeConfig), TypeConverter(typeof(CategoryConverter<TradeSettingsCategory>))]
    public class TradeSettingsCategory
    {
        public override string ToString() => "Trade Configuration Settings";

        [HubCategory(TradeConfig), HubDescription("TradeSettingsCategory_MinTradeCode_Description"), HubDisplayName("TradeSettingsCategory_MinTradeCode_DisplayName")]
        public int MinTradeCode { get; set; } = 0;

        [HubCategory(TradeConfig), HubDescription("TradeSettingsCategory_MaxTradeCode_Description"), HubDisplayName("TradeSettingsCategory_MaxTradeCode_DisplayName")]
        public int MaxTradeCode { get; set; } = 9999_9999;

        [HubCategory(TradeConfig), HubDescription("TradeSettingsCategory_StoreTradeCodes_Description"), HubDisplayName("TradeSettingsCategory_StoreTradeCodes_DisplayName")]
        public bool StoreTradeCodes { get; set; } = true;

        [HubCategory(TradeConfig), HubDescription("TradeSettingsCategory_TradeWaitTime_Description"), HubDisplayName("TradeSettingsCategory_TradeWaitTime_DisplayName")]
        public int TradeWaitTime { get; set; } = 55;

        [HubCategory(TradeConfig), HubDescription("TradeSettingsCategory_MaxTradeConfirmTime_Description"), HubDisplayName("TradeSettingsCategory_MaxTradeConfirmTime_DisplayName")]
        public int MaxTradeConfirmTime { get; set; } = 45;

        [HubCategory(TradeConfig), HubDescription("TradeSettingsCategory_ItemTradeSpecies_Description"), HubDisplayName("TradeSettingsCategory_ItemTradeSpecies_DisplayName")]
        public Species ItemTradeSpecies { get; set; } = Species.None;

        [HubCategory(TradeConfig), HubDescription("TradeSettingsCategory_DefaultHeldItem_Description"), HubDisplayName("TradeSettingsCategory_DefaultHeldItem_DisplayName")]
        public HeldItem DefaultHeldItem { get; set; } = HeldItem.None;

        [HubCategory(TradeConfig), HubDescription("TradeSettingsCategory_SuggestRelearnMoves_Description"), HubDisplayName("TradeSettingsCategory_SuggestRelearnMoves_DisplayName")]
        public bool SuggestRelearnMoves { get; set; } = true;

        [HubCategory(TradeConfig), HubDescription("TradeSettingsCategory_AllowBatchTrades_Description"), HubDisplayName("TradeSettingsCategory_AllowBatchTrades_DisplayName")]
        public bool AllowBatchTrades { get; set; } = true;

        [HubCategory(TradeConfig), HubDescription("TradeSettingsCategory_EnableSpamCheck_Description"), HubDisplayName("TradeSettingsCategory_EnableSpamCheck_DisplayName")]
        public bool EnableSpamCheck { get; set; } = true;

        [HubCategory(TradeConfig), HubDescription("TradeSettingsCategory_MaxPkmsPerTrade_Description"), HubDisplayName("TradeSettingsCategory_MaxPkmsPerTrade_DisplayName")]
        public int MaxPkmsPerTrade { get; set; } = 6;

        [HubCategory(TradeConfig), HubDescription("TradeSettingsCategory_MaxDumpsPerTrade_Description"), HubDisplayName("TradeSettingsCategory_MaxDumpsPerTrade_DisplayName")]
        public int MaxDumpsPerTrade { get; set; } = 25;

        [HubCategory(TradeConfig), HubDescription("TradeSettingsCategory_MaxDumpTradeTime_Description"), HubDisplayName("TradeSettingsCategory_MaxDumpTradeTime_DisplayName")]
        public int MaxDumpTradeTime { get; set; } = 60;

        [HubCategory(TradeConfig), HubDescription("TradeSettingsCategory_DumpTradeLegalityCheck_Description"), HubDisplayName("TradeSettingsCategory_DumpTradeLegalityCheck_DisplayName")]
        public bool DumpTradeLegalityCheck { get; set; } = true;

        [HubCategory(TradeConfig), HubDescription("TradeSettingsCategory_DisallowTradeEvolve_Description"), HubDisplayName("TradeSettingsCategory_DisallowTradeEvolve_DisplayName")]
        public bool DisallowTradeEvolve { get; set; } = true;

        [HubCategory(TradeConfig), Description("LGPE Setting.")]
        public int TradeAnimationMaxDelaySeconds = 25;

        public enum HeldItem
        {
            None = 0,

            MasterBall = 1,

            RareCandy = 50,

            ppUp = 51,

            ppMax = 53,

            BigPearl = 89,

            Nugget = 92,

            AbilityCapsule = 645,

            BottleCap = 795,

            GoldBottleCap = 796,

            expCandyL = 1127,

            expCandyXL = 1128,

            AbilityPatch = 1606,

            FreshStartMochi = 2479,
        }
    }

    [HubCategory(EmbedSettings), TypeConverter(typeof(CategoryConverter<TradeEmbedSettingsCategory>))]
    public class TradeEmbedSettingsCategory
    {
        public override string ToString() => "Trade Embed Configuration Settings";

        private bool _useEmbeds = true;

        [HubCategory(EmbedSettings), HubDescription("TradeEmbedSettingsCategory_UseEmbeds_Description"), HubDisplayName("TradeEmbedSettingsCategory_UseEmbeds_DisplayName")]
        public bool UseEmbeds
        {
            get => _useEmbeds;
            set
            {
                _useEmbeds = value;
                OnUseEmbedsChanged();
            }
        }

        private void OnUseEmbedsChanged()
        {
            if (!_useEmbeds)
            {
                PreferredImageSize = ImageSize.Size128x128;
                MoveTypeEmojis = false;
                ShowScale = false;
                ShowTeraType = false;
                ShowLevel = false;
                ShowBall = false;
                ShowMetLevel = false;
                ShowMetDate = false;
                ShowMetLocation = false;
                ShowAbility = false;
                ShowNature = false;
                ShowIVs = false;
            }
        }

        [HubCategory(EmbedSettings), HubDescription("TradeEmbedSettingsCategory_PreferredImageSize_Description"), HubDisplayName("TradeEmbedSettingsCategory_PreferredImageSize_DisplayName")]
        public ImageSize PreferredImageSize { get; set; } = ImageSize.Size128x128;

        [HubCategory(EmbedSettings), HubDescription("TradeEmbedSettingsCategory_MoveTypeEmojis_Description"), HubDisplayName("TradeEmbedSettingsCategory_MoveTypeEmojis_DisplayName")]
        public bool MoveTypeEmojis { get; set; } = true;

        [HubCategory(EmbedSettings), HubDescription("TradeEmbedSettingsCategory_CustomTypeEmojis_Description"), HubDisplayName("TradeEmbedSettingsCategory_CustomTypeEmojis_DisplayName")]
        public List<MoveTypeEmojiInfo> CustomTypeEmojis { get; set; } =
        [
            new(MoveType.Bug),
            new(MoveType.Fire),
            new(MoveType.Flying),
            new(MoveType.Ground),
            new(MoveType.Water),
            new(MoveType.Grass),
            new(MoveType.Ice),
            new(MoveType.Rock),
            new(MoveType.Ghost),
            new(MoveType.Steel),
            new(MoveType.Fighting),
            new(MoveType.Electric),
            new(MoveType.Dragon),
            new(MoveType.Psychic),
            new(MoveType.Dark),
            new(MoveType.Normal),
            new(MoveType.Poison),
            new(MoveType.Fairy),
            new(MoveType.Stellar)
        ];

        [HubCategory(EmbedSettings), HubDescription("TradeEmbedSettingsCategory_MaleEmoji_Description"), HubDisplayName("TradeEmbedSettingsCategory_MaleEmoji_DisplayName")]
        public EmojiInfo MaleEmoji { get; set; } = new EmojiInfo();

        [HubCategory(EmbedSettings), HubDescription("TradeEmbedSettingsCategory_FemaleEmoji_Description"), HubDisplayName("TradeEmbedSettingsCategory_FemaleEmoji_DisplayName")]
        public EmojiInfo FemaleEmoji { get; set; } = new EmojiInfo();

        [HubCategory(EmbedSettings), HubDescription("TradeEmbedSettingsCategory_MysteryGiftEmoji_Description"), HubDisplayName("TradeEmbedSettingsCategory_MysteryGiftEmoji_DisplayName")]
        public EmojiInfo MysteryGiftEmoji { get; set; } = new EmojiInfo();

        [HubCategory(EmbedSettings), HubDescription("TradeEmbedSettingsCategory_AlphaMarkEmoji_Description"), HubDisplayName("TradeEmbedSettingsCategory_AlphaMarkEmoji_DisplayName")]
        public EmojiInfo AlphaMarkEmoji { get; set; } = new EmojiInfo();

        [HubCategory(EmbedSettings), HubDescription("TradeEmbedSettingsCategory_UsePlusMoveEmoji_Description"), HubDisplayName("TradeEmbedSettingsCategory_UsePlusMoveEmoji_DisplayName")]
        public EmojiInfo UsePlusMoveEmoji { get; set; } = new EmojiInfo();

        [HubCategory(EmbedSettings), HubDescription("TradeEmbedSettingsCategory_MightiestMarkEmoji_Description"), HubDisplayName("TradeEmbedSettingsCategory_MightiestMarkEmoji_DisplayName")]
        public EmojiInfo MightiestMarkEmoji { get; set; } = new EmojiInfo();

        [HubCategory(EmbedSettings), HubDescription("TradeEmbedSettingsCategory_AlphaPLAEmoji_Description"), HubDisplayName("TradeEmbedSettingsCategory_AlphaPLAEmoji_DisplayName")]
        public EmojiInfo AlphaPLAEmoji { get; set; } = new EmojiInfo();

        [HubCategory(EmbedSettings), HubDescription("TradeEmbedSettingsCategory_UseTeraEmojis_Description"), HubDisplayName("TradeEmbedSettingsCategory_UseTeraEmojis_DisplayName")]
        public bool UseTeraEmojis { get; set; } = true;

        [HubCategory(EmbedSettings), HubDescription("TradeEmbedSettingsCategory_TeraTypeEmojis_Description"), HubDisplayName("TradeEmbedSettingsCategory_TeraTypeEmojis_DisplayName")]
        public List<TeraTypeEmojiInfo> TeraTypeEmojis { get; set; } =
        [
            new(MoveType.Bug),
            new(MoveType.Fire),
            new(MoveType.Flying),
            new(MoveType.Ground),
            new(MoveType.Water),
            new(MoveType.Grass),
            new(MoveType.Ice),
            new(MoveType.Rock),
            new(MoveType.Ghost),
            new(MoveType.Steel),
            new(MoveType.Fighting),
            new(MoveType.Electric),
            new(MoveType.Dragon),
            new(MoveType.Psychic),
            new(MoveType.Dark),
            new(MoveType.Normal),
            new(MoveType.Poison),
            new(MoveType.Fairy),
            new(MoveType.Stellar)
        ];

        [HubCategory(EmbedSettings), HubDescription("TradeEmbedSettingsCategory_ShowScale_Description"), HubDisplayName("TradeEmbedSettingsCategory_ShowScale_DisplayName")]
        public bool ShowScale { get; set; } = true;

        [HubCategory(EmbedSettings), HubDescription("TradeEmbedSettingsCategory_ShowTeraType_Description"), HubDisplayName("TradeEmbedSettingsCategory_ShowTeraType_DisplayName")]
        public bool ShowTeraType { get; set; } = true;

        [HubCategory(EmbedSettings), HubDescription("TradeEmbedSettingsCategory_ShowLevel_Description"), HubDisplayName("TradeEmbedSettingsCategory_ShowLevel_DisplayName")]
        public bool ShowLevel { get; set; } = true;

        [HubCategory(EmbedSettings), HubDescription("TradeEmbedSettingsCategory_ShowBall_Description"), HubDisplayName("TradeEmbedSettingsCategory_ShowBall_DisplayName")]
        public bool ShowBall { get; set; } = true;

        [HubCategory(EmbedSettings), HubDescription("TradeEmbedSettingsCategory_ShowMetLevel_Description"), HubDisplayName("TradeEmbedSettingsCategory_ShowMetLevel_DisplayName")]
        public bool ShowMetLevel { get; set; } = true;

        [HubCategory(EmbedSettings), HubDescription("TradeEmbedSettingsCategory_ShowMetDate_Description"), HubDisplayName("TradeEmbedSettingsCategory_ShowMetDate_DisplayName")]
        public bool ShowMetDate { get; set; } = true;

        [HubCategory(EmbedSettings), HubDescription("TradeEmbedSettingsCategory_ShowMetLocation_Description"), HubDisplayName("TradeEmbedSettingsCategory_ShowMetLocation_DisplayName")]
        public bool ShowMetLocation { get; set; } = true;

        [HubCategory(EmbedSettings), HubDescription("TradeEmbedSettingsCategory_ShowAbility_Description"), HubDisplayName("TradeEmbedSettingsCategory_ShowAbility_DisplayName")]
        public bool ShowAbility { get; set; } = true;

        [HubCategory(EmbedSettings), HubDescription("TradeEmbedSettingsCategory_ShowNature_Description"), HubDisplayName("TradeEmbedSettingsCategory_ShowNature_DisplayName")]
        public bool ShowNature { get; set; } = true;

        [HubCategory(EmbedSettings), HubDescription("TradeEmbedSettingsCategory_ShowLanguage_Description"), HubDisplayName("TradeEmbedSettingsCategory_ShowLanguage_DisplayName")]
        public bool ShowLanguage { get; set; } = true;

        [HubCategory(EmbedSettings), HubDescription("TradeEmbedSettingsCategory_ShowIVs_Description"), HubDisplayName("TradeEmbedSettingsCategory_ShowIVs_DisplayName")]
        public bool ShowIVs { get; set; } = true;

        [HubCategory(EmbedSettings), HubDescription("TradeEmbedSettingsCategory_ShowEVs_Description"), HubDisplayName("TradeEmbedSettingsCategory_ShowEVs_DisplayName")]
        public bool ShowEVs { get; set; } = true;
    }

    [HubCategory(Miscellaneous)]
    [HubDescription("TradeEmbedSettingsCategory_ScreenOff_Description")]
    [HubDisplayName("TradeEmbedSettingsCategory_ScreenOff_DisplayName")]
    public bool ScreenOff { get; set; } = false;

    /// <summary>
    /// Gets a random trade code based on the range settings.
    /// </summary>
    public int GetRandomTradeCode() => Util.Rand.Next(TradeConfiguration.MinTradeCode, TradeConfiguration.MaxTradeCode + 1);

    public static List<Pictocodes> GetRandomLGTradeCode(bool randomtrade = false)
    {
        var lgcode = new List<Pictocodes>();
        if (randomtrade)
        {
            for (int i = 0; i <= 2; i++)
            {
                // code.Add((pictocodes)Util.Rand.Next(10));
                lgcode.Add(Pictocodes.Pikachu);
            }
        }
        else
        {
            for (int i = 0; i <= 2; i++)
            {
                lgcode.Add((Pictocodes)Util.Rand.Next(10));

                // code.Add(pictocodes.Pikachu);
            }
        }
        return lgcode;
    }

    [HubCategory(CountStats), TypeConverter(typeof(CategoryConverter<CountStatsSettingsCategory>))]
    public class CountStatsSettingsCategory
    {
        public override string ToString() => "Trade Count Statistics";

        private int _completedSurprise;

        private int _completedDistribution;

        private int _completedTrades;

        private int _completedSeedChecks;

        private int _completedClones;

        private int _completedDumps;

        private int _completedFixOTs;

        [HubCategory(CountStats), HubDescription("CountStatsSettingsCategory_CompletedSurprise_Description")]
        public int CompletedSurprise
        {
            get => _completedSurprise;
            set => _completedSurprise = value;
        }

        [Category(), HubDescription("CountStatsSettingsCategory_CompletedDistribution_Description")]
        public int CompletedDistribution
        {
            get => _completedDistribution;
            set => _completedDistribution = value;
        }

        [HubCategory(CountStats), HubDescription("CountStatsSettingsCategory_CompletedTrades_Description")]
        public int CompletedTrades
        {
            get => _completedTrades;
            set => _completedTrades = value;
        }

        [HubCategory(CountStats), HubDescription("CountStatsSettingsCategory_CompletedFixOTs_Description")]
        public int CompletedFixOTs
        {
            get => _completedFixOTs;
            set => _completedFixOTs = value;
        }

        [Browsable(false)]
        [HubCategory(CountStats), HubDescription("CountStatsSettingsCategory_CompletedSeedChecks_Description")]
        public int CompletedSeedChecks
        {
            get => _completedSeedChecks;
            set => _completedSeedChecks = value;
        }

        [HubCategory(CountStats), HubDescription("CountStatsSettingsCategory_CompletedClones_Description")]
        public int CompletedClones
        {
            get => _completedClones;
            set => _completedClones = value;
        }

        [HubCategory(CountStats), HubDescription("CountStatsSettingsCategory_CompletedDumps_Description")]
        public int CompletedDumps
        {
            get => _completedDumps;
            set => _completedDumps = value;
        }

        [HubCategory(CountStats), HubDescription("CountStatsSettingsCategory_EmitCountsOnStatusCheck_Description")]
        public bool EmitCountsOnStatusCheck { get; set; }

        public void AddCompletedTrade() => Interlocked.Increment(ref _completedTrades);

        public void AddCompletedSeedCheck() => Interlocked.Increment(ref _completedSeedChecks);

        public void AddCompletedSurprise() => Interlocked.Increment(ref _completedSurprise);

        public void AddCompletedDistribution() => Interlocked.Increment(ref _completedDistribution);

        public void AddCompletedDumps() => Interlocked.Increment(ref _completedDumps);

        public void AddCompletedClones() => Interlocked.Increment(ref _completedClones);

        public void AddCompletedFixOTs() => Interlocked.Increment(ref _completedFixOTs);

        public IEnumerable<string> GetNonZeroCounts()
        {
            if (!EmitCountsOnStatusCheck)
                yield break;
            if (CompletedSeedChecks != 0)
                yield return $"Seed Check Trades: {CompletedSeedChecks}";
            if (CompletedClones != 0)
                yield return $"Clone Trades: {CompletedClones}";
            if (CompletedDumps != 0)
                yield return $"Dump Trades: {CompletedDumps}";
            if (CompletedTrades != 0)
                yield return $"Link Trades: {CompletedTrades}";
            if (CompletedDistribution != 0)
                yield return $"Distribution Trades: {CompletedDistribution}";
            if (CompletedFixOTs != 0)
                yield return $"FixOT Trades: {CompletedFixOTs}";
            if (CompletedSurprise != 0)
                yield return $"Surprise Trades: {CompletedSurprise}";
        }
    }

    public bool EmitCountsOnStatusCheck
    {
        get => CountStatsSettings.EmitCountsOnStatusCheck;
        set => CountStatsSettings.EmitCountsOnStatusCheck = value;
    }

    public IEnumerable<string> GetNonZeroCounts()
    {
        // Delegating the call to CountStatsSettingsCategory
        return CountStatsSettings.GetNonZeroCounts();
    }

    public class CategoryConverter<T> : TypeConverter
    {
        public override bool GetPropertiesSupported(ITypeDescriptorContext? context) => true;

        public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext? context, object value, Attribute[]? attributes) => TypeDescriptor.GetProperties(typeof(T));

        public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType) => destinationType != typeof(string) && base.CanConvertTo(context, destinationType);
    }

    public enum ImageSize
    {
        Size256x256,

        Size128x128
    }

    public enum MoveType
    {
        Normal,
        Fighting,
        Flying,
        Poison,
        Ground,
        Rock,
        Bug,
        Ghost,
        Steel,
        Fire,
        Water,
        Grass,
        Electric,
        Psychic,
        Ice,
        Dragon,
        Dark,
        Fairy,
        Stellar
    }

    public class MoveTypeEmojiInfo
    {
        [HubDescription("MoveTypeEmojiInfo_MoveType_Description")]
        public MoveType MoveType { get; set; }
        [HubDescription("MoveTypeEmojiInfo_EmojiCode_Description")]
        public string EmojiCode { get; set; } = string.Empty;
        public MoveTypeEmojiInfo()
        { }
        public MoveTypeEmojiInfo(MoveType moveType)
        {
            MoveType = moveType;
            EmojiCode = string.Empty;
        }
        public override string ToString()
        {
            if (string.IsNullOrEmpty(EmojiCode))
                return MoveType.ToString();
            return $"{EmojiCode}";
        }
    }

    public class TeraTypeEmojiInfo
    {
        [HubDescription("TeraTypeEmojiInfo_MoveType_Description")]
        public MoveType MoveType { get; set; }
        [HubDescription("TeraTypeEmojiInfo_EmojiCode_Description")]
        public string EmojiCode { get; set; }
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public TeraTypeEmojiInfo()
        { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public TeraTypeEmojiInfo(MoveType teraType)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        {
            MoveType = teraType;
        }
        public override string ToString()
        {
            if (string.IsNullOrEmpty(EmojiCode))
                return MoveType.ToString();
            return $"{EmojiCode}";
        }
    }
}
