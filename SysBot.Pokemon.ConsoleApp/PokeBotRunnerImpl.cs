using PKHeX.Core;
using SysBot.Base;
using SysBot.Pokemon.Discord;
using SysBot.Pokemon.Twitch;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SysBot.Pokemon.ConsoleApp;

/// <summary>
/// Bot Environment implementation with Integrations added.
/// </summary>
public class PokeBotRunnerImpl<T> : PokeBotRunner<T> where T : PKM, new()
{
    private static TwitchBot<T>? Twitch;
    private readonly ProgramConfig _config;

    public PokeBotRunnerImpl(PokeTradeHub<T> hub, BotFactory<T> fac, ProgramConfig config) : base(hub, fac)
    {
        _config = config;
    }

    protected override void AddIntegrations()
    {
        AddDiscordBot(Hub.Config.Discord);
        AddTwitchBot(Hub.Config.Twitch);
    }

    private void AddDiscordBot(DiscordSettings config)
    {
        var token = config.Token;
        if (string.IsNullOrWhiteSpace(token))
            return;

        // Supervise the Discord client so a terminal failure rebuilds it instead of leaving
        // the bot permanently offline until the process restarts. See WinForms PokeBotRunnerImpl.
        _ = Task.Run(() => RunDiscordBotSupervisedAsync(token));
    }

    private async Task RunDiscordBotSupervisedAsync(string apiToken)
    {
        int consecutiveFailures = 0;
        while (true)
        {
            try
            {
                var bot = new SysCord<T>(this, _config);
                await bot.MainAsync(apiToken, CancellationToken.None).ConfigureAwait(false);
                consecutiveFailures = 0;
            }
            catch (Exception ex)
            {
                consecutiveFailures++;
                LogUtil.LogError($"Discord client terminated unexpectedly: {ex.Message}", "SysCord");
            }

            int delaySeconds = System.Math.Min(60, 5 * (int)System.Math.Pow(2, System.Math.Min(consecutiveFailures, 4)));
            LogUtil.LogText($"Recreating Discord client in {delaySeconds}s...");
            await Task.Delay(TimeSpan.FromSeconds(delaySeconds)).ConfigureAwait(false);
        }
    }

    private void AddTwitchBot(TwitchSettings config)
    {
        if (string.IsNullOrWhiteSpace(config.Token))
            return;
        if (Twitch != null)
            return; // already created

        if (string.IsNullOrWhiteSpace(config.Channel))
            return;
        if (string.IsNullOrWhiteSpace(config.Username))
            return;
        if (string.IsNullOrWhiteSpace(config.Token))
            return;

        Twitch = new TwitchBot<T>(config, Hub.Config);
        if (config.DistributionCountDown)
            Hub.BotSync.BarrierReleasingActions.Add(() => Twitch.StartingDistribution(config.MessageStart));
    }
}
