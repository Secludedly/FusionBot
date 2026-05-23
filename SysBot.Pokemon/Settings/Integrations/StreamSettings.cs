using PKHeX.Core;
using SysBot.Base;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using SysBot.Pokemon.Localization;

// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global

namespace SysBot.Pokemon;

public class StreamSettings
{
    private const string Operation = nameof(Operation);

    private static readonly byte[] BlackPixel = // 1x1 black pixel
    [
        0x42, 0x4D, 0x3A, 0x00, 0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x36, 0x00, 0x00, 0x00, 0x28, 0x00,
        0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01, 0x00,
        0x00, 0x00, 0x01, 0x00, 0x18, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x04, 0x00, 0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        0x00, 0x00,
    ];

    public static Action<PKM, string>? CreateSpriteFile { get; set; }

    [HubCategory(Operation), HubDescription("StreamSettings_CompletedTradesFormat_Description"), HubDisplayName("StreamSettings_CompletedTradesFormat_DisplayName")]
    public string CompletedTradesFormat { get; set; } = "Completed Trades: {0}";

    [HubCategory(Operation), HubDescription("StreamSettings_CopyImageFile_Description"), HubDisplayName("StreamSettings_CopyImageFile_DisplayName")]
    public bool CopyImageFile { get; set; } = true;

    [HubCategory(Operation), HubDescription("StreamSettings_CreateAssets_Description"), HubDisplayName("StreamSettings_CreateAssets_DisplayName")]
    public bool CreateAssets { get; set; }

    [HubCategory(Operation), HubDescription("StreamSettings_CreateCompletedTrades_Description"), HubDisplayName("StreamSettings_CreateCompletedTrades_DisplayName")]
    public bool CreateCompletedTrades { get; set; } = true;

    [HubCategory(Operation), HubDescription("StreamSettings_CreateEstimatedTime_Description"), HubDisplayName("StreamSettings_CreateEstimatedTime_DisplayName")]
    public bool CreateEstimatedTime { get; set; } = true;

    [HubCategory(Operation), HubDescription("StreamSettings_CreateOnDeck_Description"), HubDisplayName("StreamSettings_CreateOnDeck_DisplayName")]
    public bool CreateOnDeck { get; set; } = true;

    [HubCategory(Operation), HubDescription("StreamSettings_CreateOnDeck2_Description"), HubDisplayName("StreamSettings_CreateOnDeck2_DisplayName")]
    public bool CreateOnDeck2 { get; set; } = true;

    [HubCategory(Operation), HubDescription("StreamSettings_CreateTradeStart_Description"), HubDisplayName("StreamSettings_CreateTradeStart_DisplayName")]
    public bool CreateTradeStart { get; set; } = true;

    [HubCategory(Operation), HubDescription("StreamSettings_CreateTradeStartSprite_Description"), HubDisplayName("StreamSettings_CreateTradeStartSprite_DisplayName")]
    public bool CreateTradeStartSprite { get; set; } = true;

    [HubCategory(Operation), HubDescription("StreamSettings_CreateUserList_Description"), HubDisplayName("StreamSettings_CreateUserList_DisplayName")]
    public bool CreateUserList { get; set; } = true;

    [HubCategory(Operation), HubDescription("StreamSettings_CreateUsersInQueue_Description"), HubDisplayName("StreamSettings_CreateUsersInQueue_DisplayName")]
    public bool CreateUsersInQueue { get; set; } = true;

    [HubCategory(Operation), HubDescription("StreamSettings_CreateWaitedTime_Description"), HubDisplayName("StreamSettings_CreateWaitedTime_DisplayName")]
    public bool CreateWaitedTime { get; set; } = true;

    [HubCategory(Operation), HubDescription("StreamSettings_EstimatedFulfillmentFormat_Description"), HubDisplayName("StreamSettings_EstimatedFulfillmentFormat_DisplayName")]
    public string EstimatedFulfillmentFormat { get; set; } = @"hh\:mm\:ss";

    // Estimated Time
    [HubCategory(Operation), HubDescription("StreamSettings_EstimatedTimeFormat_Description"), HubDisplayName("StreamSettings_EstimatedTimeFormat_DisplayName")]
    public string EstimatedTimeFormat { get; set; } = "Estimated time: {0:F1} minutes";

    [HubCategory(Operation), HubDescription("StreamSettings_OnDeckFormat_Description"), HubDisplayName("StreamSettings_OnDeckFormat_DisplayName")]
    public string OnDeckFormat { get; set; } = "(ID {0}) - {3}";

    [HubCategory(Operation), HubDescription("StreamSettings_OnDeckFormat2_Description"), HubDisplayName("StreamSettings_OnDeckFormat2_DisplayName")]
    public string OnDeckFormat2 { get; set; } = "(ID {0}) - {3}";

    [HubCategory(Operation), HubDescription("StreamSettings_OnDeckSeparator_Description"), HubDisplayName("StreamSettings_OnDeckSeparator_DisplayName")]
    public string OnDeckSeparator { get; set; } = "\n";

    [HubCategory(Operation), HubDescription("StreamSettings_OnDeckSeparator2_Description"), HubDisplayName("StreamSettings_OnDeckSeparator2_DisplayName")]
    public string OnDeckSeparator2 { get; set; } = "\n";

    [HubCategory(Operation), HubDescription("StreamSettings_OnDeckSkip_Description"), HubDisplayName("StreamSettings_OnDeckSkip_DisplayName")]
    public int OnDeckSkip { get; set; }

    [HubCategory(Operation), HubDescription("StreamSettings_OnDeckSkip2_Description"), HubDisplayName("StreamSettings_OnDeckSkip2_DisplayName")]
    public int OnDeckSkip2 { get; set; }

    // On Deck
    [HubCategory(Operation), HubDescription("StreamSettings_OnDeckTake_Description"), HubDisplayName("StreamSettings_OnDeckTake_DisplayName")]
    public int OnDeckTake { get; set; } = 5;

    // On Deck 2
    [HubCategory(Operation), HubDescription("StreamSettings_OnDeckTake2_Description"), HubDisplayName("StreamSettings_OnDeckTake2_DisplayName")]
    public int OnDeckTake2 { get; set; } = 5;

    // TradeCodeBlock
    [HubCategory(Operation), HubDescription("StreamSettings_TradeBlockFile_Description"), HubDisplayName("StreamSettings_TradeBlockFile_DisplayName")]
    public string TradeBlockFile { get; set; } = string.Empty;

    [HubCategory(Operation), HubDescription("StreamSettings_TradeBlockFormat_Description"), HubDisplayName("StreamSettings_TradeBlockFormat_DisplayName")]
    public string TradeBlockFormat { get; set; } = "block_{0}.png";

    [HubCategory(Operation), HubDescription("StreamSettings_TrainerTradeStart_Description"), HubDisplayName("StreamSettings_TrainerTradeStart_DisplayName")]
    public string TrainerTradeStart { get; set; } = "(ID {0}) {1}";

    [HubCategory(Operation), HubDescription("StreamSettings_UserListFormat_Description"), HubDisplayName("StreamSettings_UserListFormat_DisplayName")]
    public string UserListFormat { get; set; } = "(ID {0}) - {3}";

    [HubCategory(Operation), HubDescription("StreamSettings_UserListSeparator_Description"), HubDisplayName("StreamSettings_UserListSeparator_DisplayName")]
    public string UserListSeparator { get; set; } = ", ";

    [HubCategory(Operation), HubDescription("StreamSettings_UserListSkip_Description"), HubDisplayName("StreamSettings_UserListSkip_DisplayName")]
    public int UserListSkip { get; set; }

    // User List
    [HubCategory(Operation), HubDescription("StreamSettings_UserListTake_Description"), HubDisplayName("StreamSettings_UserListTake_DisplayName")]
    public int UserListTake { get; set; } = -1;

    // Users in Queue
    [HubCategory(Operation), HubDescription("StreamSettings_UsersInQueueFormat_Description"), HubDisplayName("StreamSettings_UsersInQueueFormat_DisplayName")]
    public string UsersInQueueFormat { get; set; } = "Users in Queue: {0}";

    // Waited Time
    [HubCategory(Operation), HubDescription("StreamSettings_WaitedTimeFormat_Description"), HubDisplayName("StreamSettings_WaitedTimeFormat_DisplayName")]
    public string WaitedTimeFormat { get; set; } = @"hh\:mm\:ss";

    public void EndEnterCode(PokeRoutineExecutorBase b)
    {
        try
        {
            var file = GetBlockFileName(b);
            if (File.Exists(file))
                File.Delete(file);
        }
        catch (Exception e)
        {
            LogUtil.LogError(e.Message, nameof(StreamSettings));
        }
    }

    public void IdleAssets(PokeRoutineExecutorBase b)
    {
        if (!CreateAssets)
            return;

        try
        {
            foreach (var file in Directory.EnumerateFiles(Directory.GetCurrentDirectory(), "*", SearchOption.TopDirectoryOnly))
            {
                if (file.Contains(b.Connection.Name))
                    File.Delete(file);
            }

            if (CreateWaitedTime)
                File.WriteAllText("waited.txt", "00:00:00");
            if (CreateEstimatedTime)
            {
                File.WriteAllText("estimatedTime.txt", "Estimated time: 0 minutes");
                File.WriteAllText("estimatedTimestamp.txt", "");
            }
            if (CreateOnDeck)
                File.WriteAllText("ondeck.txt", "Waiting...");
            if (CreateOnDeck2)
                File.WriteAllText("ondeck2.txt", "Queue is empty!");
            if (CreateUserList)
                File.WriteAllText("users.txt", "None");
            if (CreateUsersInQueue)
                File.WriteAllText("queuecount.txt", "Users in Queue: 0");
        }
        catch (Exception e)
        {
            LogUtil.LogError(e.Message, nameof(StreamSettings));
        }
    }

    public void StartEnterCode(PokeRoutineExecutorBase b)
    {
        if (!CreateAssets)
            return;

        try
        {
            var file = GetBlockFileName(b);
            if (CopyImageFile && File.Exists(TradeBlockFile))
                File.Copy(TradeBlockFile, file);
            else
                File.WriteAllBytes(file, BlackPixel);
        }
        catch (Exception e)
        {
            LogUtil.LogError(e.Message, nameof(StreamSettings));
        }
    }

    // Completed Trades
    public void StartTrade<T>(PokeRoutineExecutorBase b, PokeTradeDetail<T> detail, PokeTradeHub<T> hub) where T : PKM, new()
    {
        if (!CreateAssets)
            return;

        try
        {
            if (CreateTradeStart)
                GenerateBotConnection(b, detail);
            if (CreateWaitedTime)
                GenerateWaitedTime(detail.Time);
            if (CreateEstimatedTime)
                GenerateEstimatedTime(hub);
            if (CreateUsersInQueue)
                GenerateUsersInQueue(hub.Queues.Info.Count);
            if (CreateOnDeck)
                GenerateOnDeck(hub);
            if (CreateOnDeck2)
                GenerateOnDeck2(hub);
            if (CreateUserList)
                GenerateUserList(hub);
            if (CreateCompletedTrades)
                GenerateCompletedTrades(hub);
            if (CreateTradeStartSprite)
                GenerateBotSprite(b, detail);
        }
        catch (Exception e)
        {
            LogUtil.LogError(e.Message, nameof(StreamSettings));
        }
    }

    public override string ToString() => "Stream Settings";

    private static void GenerateBotSprite<T>(PokeRoutineExecutorBase b, PokeTradeDetail<T> detail) where T : PKM, new()
    {
        var func = CreateSpriteFile;
        if (func == null)
            return;
        var file = b.Connection.Name;
        var pk = detail.TradeData;
        func.Invoke(pk, $"sprite_{file}.png");
    }

    private void GenerateBotConnection<T>(PokeRoutineExecutorBase b, PokeTradeDetail<T> detail) where T : PKM, new()
    {
        var file = b.Connection.Name;
        var name = string.Format(TrainerTradeStart, detail.ID, detail.Trainer.TrainerName, (Species)detail.TradeData.Species);
        File.WriteAllText($"{file}.txt", name);
    }

    private void GenerateCompletedTrades<T>(PokeTradeHub<T> hub) where T : PKM, new()
    {
        var msg = string.Format(CompletedTradesFormat, hub.Config.Trade.CountStatsSettings.CompletedTrades);
        File.WriteAllText("completed.txt", msg);
    }

    private void GenerateEstimatedTime<T>(PokeTradeHub<T> hub) where T : PKM, new()
    {
        var count = hub.Queues.Info.Count;
        var estimate = hub.Config.Queues.EstimateDelay(count, hub.Bots.Count);

        // Minutes
        var wait = string.Format(EstimatedTimeFormat, estimate);
        File.WriteAllText("estimatedTime.txt", wait);

        // Expected to be fulfilled at this time
        var now = DateTime.Now;
        var difference = now.AddMinutes(estimate);
        var date = difference.ToString(EstimatedFulfillmentFormat);
        File.WriteAllText("estimatedTimestamp.txt", date);
    }

    private void GenerateOnDeck<T>(PokeTradeHub<T> hub) where T : PKM, new()
    {
        var ondeck = hub.Queues.Info.GetUserList(OnDeckFormat);
        ondeck = ondeck.Skip(OnDeckSkip).Take(OnDeckTake); // filter down
        File.WriteAllText("ondeck.txt", string.Join(OnDeckSeparator, ondeck));
    }

    private void GenerateOnDeck2<T>(PokeTradeHub<T> hub) where T : PKM, new()
    {
        var ondeck = hub.Queues.Info.GetUserList(OnDeckFormat2);
        ondeck = ondeck.Skip(OnDeckSkip2).Take(OnDeckTake2); // filter down
        File.WriteAllText("ondeck2.txt", string.Join(OnDeckSeparator2, ondeck));
    }

    private void GenerateUserList<T>(PokeTradeHub<T> hub) where T : PKM, new()
    {
        var users = hub.Queues.Info.GetUserList(UserListFormat);
        users = users.Skip(UserListSkip);
        if (UserListTake > 0)
            users = users.Take(UserListTake); // filter down
        File.WriteAllText("users.txt", string.Join(UserListSeparator, users));
    }

    private void GenerateUsersInQueue(int count)
    {
        var value = string.Format(UsersInQueueFormat, count);
        File.WriteAllText("queuecount.txt", value);
    }

    private void GenerateWaitedTime(DateTime time)
    {
        var now = DateTime.Now;
        var difference = now - time;
        var value = difference.ToString(WaitedTimeFormat);
        File.WriteAllText("waited.txt", value);
    }

    private string GetBlockFileName(PokeRoutineExecutorBase b) => string.Format(TradeBlockFormat, b.Connection.Name);
}
