using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SysBot.Pokemon.Discord;

/// <summary>
/// Interaction-context equivalent of <see cref="RequireQueueRoleAttribute"/>.
/// <para>
/// The message-based attribute derives from <see cref="Discord.Commands.PreconditionAttribute"/> and
/// takes an <see cref="Discord.Commands.ICommandContext"/>, so it cannot be applied to slash commands.
/// This is a parallel implementation for <see cref="InteractionModuleBase{T}"/>; the original attribute
/// is untouched and continues to guard every prefix command exactly as before.
/// </para>
/// <para>
/// It additionally performs the ban / user-whitelist / channel-whitelist checks that
/// <c>SysCord.TryHandleCommandAsync</c> applies to prefix commands. <c>HandleInteractionAsync</c> has no
/// equivalent gate, so without this attribute a slash command would honour neither the banned-ID list
/// nor the channel whitelist.
/// </para>
/// </summary>
public sealed class RequireQueueRoleInteractionAttribute(string RoleName) : PreconditionAttribute
{
    public override Task<PreconditionResult> CheckRequirementsAsync(IInteractionContext context, ICommandInfo commandInfo, IServiceProvider services)
    {
        var mgr = SysCordSettings.Manager;

        // Banned users are refused outright, mirroring the prefix path in SysCord.TryHandleCommandAsync.
        var abuse = SysCordSettings.HubConfig.TradeAbuse;
        if (abuse.BannedIDs.List.Any(z => z.ID == context.User.Id))
            return Task.FromResult(PreconditionResult.FromError("You are banned from using this bot."));

        if (!mgr.CanUseCommandUser(context.User.Id))
            return Task.FromResult(PreconditionResult.FromError("You are not permitted to use this command."));

        if (!mgr.CanUseCommandChannel(context.Channel.Id) && context.User.Id != mgr.Owner)
            return Task.FromResult(PreconditionResult.FromError("You can't use that command here."));

        // Global sudo bypasses the role gate entirely, as it does for prefix commands.
        if (mgr.Config.AllowGlobalSudo && mgr.CanUseSudo(context.User.Id))
            return Task.FromResult(PreconditionResult.FromSuccess());

        if (context.User is not SocketGuildUser gUser)
            return Task.FromResult(PreconditionResult.FromError("You must be using this command from a server to run it."));

        var roles = gUser.Roles;
        if (mgr.CanUseSudo(roles.Select(z => z.Name)))
            return Task.FromResult(PreconditionResult.FromSuccess());

        if (!SysCordSettings.HubConfig.Queues.CanQueue)
            return Task.FromResult(PreconditionResult.FromError("Sorry, I am not currently accepting queue requests!"));

        if (!mgr.GetHasRoleAccess(RoleName, roles.Select(z => z.Name)))
            return Task.FromResult(PreconditionResult.FromError("You do not have the required role to run this command."));

        return Task.FromResult(PreconditionResult.FromSuccess());
    }
}
