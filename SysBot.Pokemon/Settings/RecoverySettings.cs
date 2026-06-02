using System.ComponentModel;
using SysBot.Pokemon.Localization;

namespace SysBot.Pokemon;

/// <summary>
/// Configuration settings for automatic bot recovery after crashes or cancellation token stops.
/// </summary>
public class RecoverySettings
{
    private const string Recovery = nameof(Recovery);

    [HubDisplayName("RecoverySettings_EnableRecovery_DisplayName")]
    [HubCategory(Recovery), HubDescription("RecoverySettings_EnableRecovery_Description")]
    public bool EnableRecovery { get; set; } = true;

    [HubDisplayName("RecoverySettings_MaxRecoveryAttempts_DisplayName")]
    [HubCategory(Recovery), HubDescription("RecoverySettings_MaxRecoveryAttempts_Description")]
    public int MaxRecoveryAttempts { get; set; } = 3;

    [HubDisplayName("RecoverySettings_InitialRecoveryDelaySeconds_DisplayName")]
    [HubCategory(Recovery), HubDescription("RecoverySettings_InitialRecoveryDelaySeconds_Description")]
    public int InitialRecoveryDelaySeconds { get; set; } = 5;

    [HubDisplayName("RecoverySettings_MaxRecoveryDelaySeconds_DisplayName")]
    [HubCategory(Recovery), HubDescription("RecoverySettings_MaxRecoveryDelaySeconds_Description")]
    public int MaxRecoveryDelaySeconds { get; set; } = 300; // 5 minutes

    [HubDisplayName("RecoverySettings_BackoffMultiplier_DisplayName")]
    [HubCategory(Recovery), HubDescription("RecoverySettings_BackoffMultiplier_Description")]
    public double BackoffMultiplier { get; set; } = 2.0;

    [HubDisplayName("RecoverySettings_CrashHistoryWindowMinutes_DisplayName")]
    [HubCategory(Recovery), HubDescription("RecoverySettings_CrashHistoryWindowMinutes_Description")]
    public int CrashHistoryWindowMinutes { get; set; } = 60; // 1 hour

    [HubDisplayName("RecoverySettings_MaxCrashesInWindow_DisplayName")]
    [HubCategory(Recovery), HubDescription("RecoverySettings_MaxCrashesInWindow_Description")]
    public int MaxCrashesInWindow { get; set; } = 5;

    [HubDisplayName("RecoverySettings_RecoverIntentionalStops_DisplayName")]
    [HubCategory(Recovery), HubDescription("RecoverySettings_RecoverIntentionalStops_Description")]
    public bool RecoverIntentionalStops { get; set; } = false;

    [HubDisplayName("RecoverySettings_SuccessfulRecoveryResetDelaySeconds_DisplayName")]
    [HubCategory(Recovery), HubDescription("RecoverySettings_SuccessfulRecoveryResetDelaySeconds_Description")]
    public int SuccessfulRecoveryResetDelaySeconds { get; set; } = 300; // 5 minutes

    [HubDisplayName("RecoverySettings_NotifyOnRecoveryAttempt_DisplayName")]
    [HubCategory(Recovery), HubDescription("RecoverySettings_NotifyOnRecoveryAttempt_Description")]
    public bool NotifyOnRecoveryAttempt { get; set; } = true;

    [HubDisplayName("RecoverySettings_NotifyOnRecoveryFailure_DisplayName")]
    [HubCategory(Recovery), HubDescription("RecoverySettings_NotifyOnRecoveryFailure_Description")]
    public bool NotifyOnRecoveryFailure { get; set; } = true;

    [HubDisplayName("RecoverySettings_MinimumStableUptimeSeconds_DisplayName")]
    [HubCategory(Recovery), HubDescription("RecoverySettings_MinimumStableUptimeSeconds_Description")]
    public int MinimumStableUptimeSeconds { get; set; } = 600; // 10 minutes

    public override string ToString() => "Bot Recovery Settings";
}