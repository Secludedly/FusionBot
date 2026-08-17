using Discord;
using Discord.Interactions;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SysBot.Pokemon.Discord;

/// <summary>
/// Applies the ban / user-whitelist / channel-whitelist checks that TryHandleCommandAsync
/// performs for prefix commands, for slash commands that do NOT touch the trade queue.
/// HandleInteractionAsync has no equivalent gate, so without this a banned user could
/// still run slash commands and the channel whitelist would be ignored.
/// </summary>
public sealed class RequireCommandAccessInteractionAttribute : PreconditionAttribute
{
    public override Task<PreconditionResult> CheckRequirementsAsync(IInteractionContext context, ICommandInfo commandInfo, IServiceProvider services)
    {
        var mgr = SysCordSettings.Manager;

        var abuse = SysCordSettings.HubConfig.TradeAbuse;
        if (abuse.BannedIDs.List.Any(z => z.ID == context.User.Id))
            return Task.FromResult(PreconditionResult.FromError("You are banned from using this bot."));

        if (!mgr.CanUseCommandUser(context.User.Id))
            return Task.FromResult(PreconditionResult.FromError("You are not permitted to use this command."));

        if (!mgr.CanUseCommandChannel(context.Channel.Id) && context.User.Id != mgr.Owner)
            return Task.FromResult(PreconditionResult.FromError("You can't use that command here."));

        return Task.FromResult(PreconditionResult.FromSuccess());
    }
}
