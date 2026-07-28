using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace SysBot.Pokemon.Discord.Helpers
{
    public class DMRelayService : IDisposable
    {
        private readonly DiscordSocketClient _client;
        private readonly ulong _forwardTargetId;
        private static string Prefix => SysCordSettings.Settings.CommandPrefix;

        // Any message beginning with one of these is treated as a command attempt
        // (mirrors the allowed prefixes recognized in SysCord.HandleMessageAsync).
        private static readonly char[] CommandPrefixes =
        {
            '$', '!', '.', '=', '%', '~', '-', '+', ',', '/', '?', '*', '^',
            '<', '>', '"', '`', '4', ';', ':'
        };
        public DMRelayService(DiscordSocketClient client, ulong forwardTargetId)
        {
            _client = client;
            _forwardTargetId = forwardTargetId;

            if (_forwardTargetId != 0)
                _client.MessageReceived += HandleMessageAsync;
        }

        public void Dispose()
        {
            if (_forwardTargetId != 0)
                _client.MessageReceived -= HandleMessageAsync;
        }

        private async Task HandleMessageAsync(SocketMessage msg)
        {
            if (msg is not SocketUserMessage umsg) return;
            if (umsg.Author.IsBot) return;
            if (umsg.Channel is not SocketDMChannel dm) return;

            // Skip command attempts: anything starting with the configured prefix,
            // a bot mention, or any recognized command prefix character (e.g. "$trade", ".btz").
            int argPos = 0;
            if (umsg.HasStringPrefix(Prefix, ref argPos) || umsg.HasMentionPrefix(_client.CurrentUser, ref argPos))
                return;

            string trimmed = umsg.Content.TrimStart();
            if (trimmed.Length > 0 && Array.IndexOf(CommandPrefixes, trimmed[0]) >= 0)
                return;

            // Build the base forward message
            string forwardContent = $"📩 **DM from {umsg.Author} ({umsg.Author.Id})**:\n{umsg.Content}";

            // Include attachments in the log if they exist
            if (umsg.Attachments.Count > 0)
            {
                forwardContent += "\n\n**Attachments:**";
                foreach (var att in umsg.Attachments)
                {
                    forwardContent += $"\n- [{att.Filename}]({att.Url})"; // Discord Markdown link
                }
            }

            // Try sending to user
            var user = _client.GetUser(_forwardTargetId);
            if (user != null)
            {
                await user.SendMessageAsync(forwardContent);
                return;
            }

            // Try sending to channel
            if (_client.GetChannel(_forwardTargetId) is IMessageChannel channel)
                await channel.SendMessageAsync(forwardContent);
        }
    }
}
