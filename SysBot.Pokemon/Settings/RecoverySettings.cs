using System.ComponentModel;
using SysBot.Pokemon.Localization;

namespace SysBot.Pokemon;

/// <summary>
/// Configuration settings for automatic bot recovery after crashes or cancellation token stops.
/// </summary>
public class RecoverySettings
{
    private const string Recovery = nameof(Recovery);

    [HubCategory(Recovery), HubDescription("RecoverySettings_EnableRecovery_Description")]
    public bool EnableRecovery { get; set; } = true;

    [HubCategory(Recovery), HubDescription("RecoverySettings_MaxRecoveryAttempts_Description")]
    public int MaxRecoveryAttempts { get; set; } = 3;

    [HubCategory(Recovery), HubDescription("RecoverySettings_InitialRecoveryDelaySeconds_Description")]
    public int InitialRecoveryDelaySeconds { get; set; } = 5;

    [HubCategory(Recovery), HubDescription("RecoverySettings_MaxRecoveryDelaySeconds_Description")]
    public int MaxRecoveryDelaySeconds { get; set; } = 300; // 5 minutes

    [HubCategory(Recovery), HubDescription("RecoverySettings_BackoffMultiplier_Description")]
    public double BackoffMultiplier { get; set; } = 2.0;

    [HubCategory(Recovery), HubDescription("RecoverySettings_CrashHistoryWindowMinutes_Description")]
    public int CrashHistoryWindowMinutes { get; set; } = 60; // 1 hour

    [HubCategory(Recovery), HubDescription("RecoverySettings_MaxCrashesInWindow_Description")]
    public int MaxCrashesInWindow { get; set; } = 5;

    [HubCategory(Recovery), HubDescription("RecoverySettings_RecoverIntentionalStops_Description")]
    public bool RecoverIntentionalStops { get; set; } = false;

    [HubCategory(Recovery), HubDescription("RecoverySettings_SuccessfulRecoveryResetDelaySeconds_Description")]
    public int SuccessfulRecoveryResetDelaySeconds { get; set; } = 300; // 5 minutes

    [HubCategory(Recovery), HubDescription("RecoverySettings_NotifyOnRecoveryAttempt_Description")]
    public bool NotifyOnRecoveryAttempt { get; set; } = true;

    [HubCategory(Recovery), HubDescription("RecoverySettings_NotifyOnRecoveryFailure_Description")]
    public bool NotifyOnRecoveryFailure { get; set; } = true;

    [HubCategory(Recovery), HubDescription("RecoverySettings_MinimumStableUptimeSeconds_Description")]
    public int MinimumStableUptimeSeconds { get; set; } = 600; // 10 minutes

    public override string ToString() => "Bot Recovery Settings";
}