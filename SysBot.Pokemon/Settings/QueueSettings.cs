using System;
using System.ComponentModel;
using SysBot.Pokemon.Localization;

// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global

namespace SysBot.Pokemon;

public enum FlexBiasMode
{
    Add,

    Multiply,
}

public enum FlexYieldMode
{
    LessCheatyFirst,

    Weighted,
}

public class QueueSettings
{
    private const string FeatureToggle = nameof(FeatureToggle);

    private const string QueueToggle = nameof(QueueToggle);

    private const string TimeBias = nameof(TimeBias);

    private const string UserBias = nameof(UserBias);

    [HubCategory(FeatureToggle), HubDescription("QueueSettings_CanDequeueIfProcessing_Description")]
    public bool CanDequeueIfProcessing { get; set; } = true;

    [HubCategory(FeatureToggle), HubDescription("QueueSettings_CanQueue_Description")]
    public bool CanQueue { get; set; } = true;

    [HubCategory(TimeBias), HubDescription("QueueSettings_EstimatedDelayFactor_Description")]
    public float EstimatedDelayFactor { get; set; } = 1.2f;

    [HubCategory(FeatureToggle), HubDescription("QueueSettings_FlexMode_Description")]
    public FlexYieldMode FlexMode { get; set; } = FlexYieldMode.Weighted;

    [HubCategory(QueueToggle), HubDescription("QueueSettings_IntervalCloseFor_Description")]
    public int IntervalCloseFor { get; set; } = 15 * 60;

    [HubCategory(QueueToggle), HubDescription("QueueSettings_IntervalOpenFor_Description")]
    public int IntervalOpenFor { get; set; } = 5 * 60;

    // General
    [HubCategory(FeatureToggle), HubDescription("QueueSettings_MaxQueueCount_Description")]
    public int MaxQueueCount { get; set; } = 200;

    [HubCategory(FeatureToggle), HubDescription("QueueSettings_QueueToggleMode_Description")]
    public QueueOpening QueueToggleMode { get; set; } = QueueOpening.Threshold;

    [HubCategory(FeatureToggle), HubDescription("QueueSettings_NotifyOnQueueClose_Description")]
    public bool NotifyOnQueueClose { get; set; } = true;

    [HubCategory(QueueToggle), HubDescription("QueueSettings_ThresholdLock_Description")]
    public int ThresholdLock { get; set; } = 200;

    [HubCategory(QueueToggle), HubDescription("QueueSettings_ThresholdUnlock_Description")]
    public int ThresholdUnlock { get; set; } = 0;

    [HubCategory(UserBias), HubDescription("QueueSettings_YieldMultCountClone_Description")]
    public int YieldMultCountClone { get; set; } = 100;

    [HubCategory(UserBias), HubDescription("QueueSettings_YieldMultCountDump_Description")]
    public int YieldMultCountDump { get; set; } = 100;

    [HubCategory(UserBias), HubDescription("QueueSettings_YieldMultCountFixOT_Description")]
    public int YieldMultCountFixOT { get; set; } = 100;

    [HubCategory(UserBias), HubDescription("QueueSettings_YieldMultCountSeedCheck_Description")]
    public int YieldMultCountSeedCheck { get; set; } = 100;

    [HubCategory(UserBias), HubDescription("QueueSettings_YieldMultCountTrade_Description")]
    public int YieldMultCountTrade { get; set; } = 100;

    [HubCategory(TimeBias), HubDescription("QueueSettings_YieldMultWait_Description")]
    public FlexBiasMode YieldMultWait { get; set; } = FlexBiasMode.Multiply;

    [HubCategory(TimeBias), HubDescription("QueueSettings_YieldMultWaitClone_Description")]
    public int YieldMultWaitClone { get; set; } = 1;

    [HubCategory(TimeBias), HubDescription("QueueSettings_YieldMultWaitDump_Description")]
    public int YieldMultWaitDump { get; set; } = 1;

    [HubCategory(TimeBias), HubDescription("QueueSettings_YieldMultWaitFixOT_Description")]
    public int YieldMultWaitFixOT { get; set; } = 1;

    [HubCategory(TimeBias), HubDescription("QueueSettings_YieldMultWaitSeedCheck_Description")]
    public int YieldMultWaitSeedCheck { get; set; } = 1;

    // Queue Toggle
    // Flex Users
    // Flex Time
    [HubCategory(TimeBias), HubDescription("QueueSettings_YieldMultWaitTrade_Description")]
    public int YieldMultWaitTrade { get; set; } = 1;

    /// <summary>
    /// Estimates the amount of time (minutes) until the user will be processed.
    /// </summary>
    /// <param name="position">Position in the queue</param>
    /// <param name="botct">Amount of bots processing requests</param>
    /// <returns>Estimated time in Minutes</returns>
    public float EstimateDelay(int position, int botct) => (EstimatedDelayFactor * position) / botct;

    /// <summary>
    /// Gets the weight of a <see cref="PokeTradeType"/> based on the count of users in the queue and time users have waited.
    /// </summary>
    /// <param name="count">Count of users for <see cref="type"/></param>
    /// <param name="time">Next-to-be-processed user's time joining the queue</param>
    /// <param name="type">Queue type</param>
    /// <returns>Effective weight for the trade type.</returns>
    public long GetWeight(int count, DateTime time, PokeTradeType type)
    {
        var now = DateTime.Now;
        var seconds = (now - time).Seconds;

        var cb = GetCountBias(type) * count;
        var tb = GetTimeBias(type) * seconds;

        return YieldMultWait switch
        {
            FlexBiasMode.Multiply => cb * tb,
            _ => cb + tb,
        };
    }

    public override string ToString() => "Queue Joining Settings";

    private int GetCountBias(PokeTradeType type) => type switch
    {
        PokeTradeType.Seed => YieldMultCountSeedCheck,
        PokeTradeType.Clone => YieldMultCountClone,
        PokeTradeType.Dump => YieldMultCountDump,
        PokeTradeType.FixOT => YieldMultCountFixOT,
        _ => YieldMultCountTrade,
    };

    private int GetTimeBias(PokeTradeType type) => type switch
    {
        PokeTradeType.Seed => YieldMultWaitSeedCheck,
        PokeTradeType.Clone => YieldMultWaitClone,
        PokeTradeType.Dump => YieldMultWaitDump,
        PokeTradeType.FixOT => YieldMultWaitFixOT,
        _ => YieldMultWaitTrade,
    };
}

public enum QueueOpening
{
    Manual,

    Threshold,

    Interval,
}
