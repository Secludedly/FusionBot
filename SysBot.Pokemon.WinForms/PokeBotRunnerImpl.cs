using PKHeX.Core;
using SysBot.Base;
using SysBot.Pokemon.Discord;
using SysBot.Pokemon.WinForms;
using System;
using System.Threading;
using System.Threading.Tasks;


namespace SysBot.Pokemon;

/// <summary>
/// Bot Environment implementation with Integrations added.
/// </summary>
public class PokeBotRunnerImpl<T> : PokeBotRunner<T> where T : PKM, new()
{
#pragma warning disable CS0649 // Field is never assigned
    private readonly ProgramConfig? _config;
#pragma warning restore CS0649
    public PokeBotRunnerImpl(PokeTradeHub<T> hub, BotFactory<T> fac) : base(hub, fac) { }
    public PokeBotRunnerImpl(PokeTradeHubConfig config, BotFactory<T> fac) : base(config, fac) { }



    protected override void AddIntegrations()
    {
        AddDiscordBot(Hub.Config.Discord.Token);
    }

    private void AddDiscordBot(string apiToken)
    {
        if (string.IsNullOrWhiteSpace(apiToken))
            return;

        // Supervise the Discord client: if MainAsync ever returns or faults (gateway died,
        // client disposed, health watchdog tripped, etc.) build a brand-new SysCord and
        // reconnect, with backoff. Previously this was a single fire-and-forget call, so any
        // terminal failure left the bot permanently offline until the whole process restarted.
        _ = Task.Run(() => RunDiscordBotSupervisedAsync(apiToken));
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
                // Returned cleanly (e.g. health watchdog tripped): reset backoff.
                consecutiveFailures = 0;
            }
            catch (Exception ex)
            {
                consecutiveFailures++;
                LogUtil.LogError($"Discord client terminated unexpectedly: {ex.Message}", "SysCord");
            }

            // Backoff: 5s, 10s, 20s, 40s, capped at 60s. Avoids hammering Discord on a bad token.
            int delaySeconds = System.Math.Min(60, 5 * (int)System.Math.Pow(2, System.Math.Min(consecutiveFailures, 4)));
            LogUtil.LogText($"Recreating Discord client in {delaySeconds}s...");
            await Task.Delay(TimeSpan.FromSeconds(delaySeconds)).ConfigureAwait(false);
        }
    }

}
