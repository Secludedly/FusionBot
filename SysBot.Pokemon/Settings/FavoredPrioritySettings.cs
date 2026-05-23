using System;
using System.ComponentModel;
using SysBot.Pokemon.Localization;

namespace SysBot.Pokemon;

/// <summary>
/// Settings for priority user favoritism in the trade queue.
/// Priority users can skip ahead of regular users while ensuring regular users still get processed.
/// </summary>
public class FavoredPrioritySettings : IFavoredCPQSetting
{
    private const int MinSkipPercentage = 0;
    private const int MaxSkipPercentage = 100;
    private const int MinRegularUsers = 0;

    private const string Configure = nameof(Configure);
    private const string Operation = nameof(Operation);

    private int _skipPercentage = 50;
    private int _minimumRegularUsersFirst = 3;

    [HubCategory(Operation), HubDescription("FavoredPrioritySettings_EnableFavoritism_Description"), HubDisplayName("FavoredPrioritySettings_EnableFavoritism_DisplayName")]
    public bool EnableFavoritism { get; set; } = true;

    [HubCategory(Configure), HubDescription("FavoredPrioritySettings_SkipPercentage_Description"), HubDisplayName("FavoredPrioritySettings_SkipPercentage_DisplayName")]
    public int SkipPercentage
    {
        get => _skipPercentage;
        set => _skipPercentage = Math.Clamp(value, MinSkipPercentage, MaxSkipPercentage);
    }

    [HubCategory(Configure), HubDescription("FavoredPrioritySettings_MinimumRegularUsersFirst_Description"), HubDisplayName("FavoredPrioritySettings_MinimumRegularUsersFirst_DisplayName")]
    public int MinimumRegularUsersFirst
    {
        get => _minimumRegularUsersFirst;
        set => _minimumRegularUsersFirst = Math.Max(MinRegularUsers, value);
    }

    public override string ToString() => "Favoritism Settings";
}
