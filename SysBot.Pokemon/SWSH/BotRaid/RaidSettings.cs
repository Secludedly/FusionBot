using PKHeX.Core;
using SysBot.Base;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using SysBot.Pokemon.Localization;

namespace SysBot.Pokemon;

public class RaidSettings : IBotStateSettings, ICountSettings
{
    private const string Counts = nameof(Counts);

    private const string FeatureToggle = nameof(FeatureToggle);

    private const string Hosting = nameof(Hosting);

    private int _completedRaids;

    [HubCategory(Counts), HubDescription("RaidSettings_CompletedRaids_Description")]
    public int CompletedRaids
    {
        get => _completedRaids;
        set => _completedRaids = value;
    }

    [HubCategory(FeatureToggle), HubDescription("RaidSettings_EchoPartyReady_Description")]
    public bool EchoPartyReady { get; set; }

    [HubCategory(Counts), HubDescription("RaidSettings_EmitCountsOnStatusCheck_Description")]
    public bool EmitCountsOnStatusCheck { get; set; }

    [HubCategory(FeatureToggle), HubDescription("RaidSettings_FriendCode_Description")]
    public string FriendCode { get; set; } = string.Empty;

    [HubCategory(Hosting), HubDescription("RaidSettings_InitialRaidsToHost_Description")]
    public int InitialRaidsToHost { get; set; }

    [HubCategory(Hosting), HubDescription("RaidSettings_MaxRaidCode_Description")]
    public int MaxRaidCode { get; set; } = 8199;

    [HubCategory(Hosting), HubDescription("RaidSettings_MinRaidCode_Description")]
    public int MinRaidCode { get; set; } = 8180;

    [HubCategory(Hosting), HubDescription("RaidSettings_NumberFriendsToAdd_Description")]
    public int NumberFriendsToAdd { get; set; }

    [HubCategory(Hosting), HubDescription("RaidSettings_NumberFriendsToDelete_Description")]
    public int NumberFriendsToDelete { get; set; }

    [HubCategory(Hosting), HubDescription("RaidSettings_ProfileNumber_Description")]
    public int ProfileNumber { get; set; } = 1;

    [HubCategory(FeatureToggle), HubDescription("RaidSettings_RaidDescription_Description")]
    public string RaidDescription { get; set; } = string.Empty;

    [HubCategory(Hosting), HubDescription("RaidSettings_RaidsBetweenAddFriends_Description")]
    public int RaidsBetweenAddFriends { get; set; }

    [HubCategory(Hosting), HubDescription("RaidSettings_RaidsBetweenDeleteFriends_Description")]
    public int RaidsBetweenDeleteFriends { get; set; }

    [HubCategory(Hosting), HubDescription("RaidSettings_RowStartAddingFriends_Description")]
    public int RowStartAddingFriends { get; set; } = 1;

    [HubCategory(Hosting), HubDescription("RaidSettings_RowStartDeletingFriends_Description")]
    public int RowStartDeletingFriends { get; set; } = 1;

    [HubCategory(FeatureToggle), HubDescription("RaidSettings_ScreenOff_Description")]
    public bool ScreenOff { get; set; }

    [HubCategory(Hosting), HubDescription("RaidSettings_TimeToWait_Description")]
    public int TimeToWait { get; set; } = 90;

    public int AddCompletedRaids() => Interlocked.Increment(ref _completedRaids);

    public IEnumerable<string> GetNonZeroCounts()
    {
        if (!EmitCountsOnStatusCheck)
            yield break;
        if (CompletedRaids != 0)
            yield return $"Started Raids: {CompletedRaids}";
    }

    /// <summary>
    /// Gets a random trade code based on the range settings.
    /// </summary>
    public int GetRandomRaidCode() => Util.Rand.Next(MinRaidCode, MaxRaidCode + 1);

    public override string ToString() => "Raid Bot Settings";
}
