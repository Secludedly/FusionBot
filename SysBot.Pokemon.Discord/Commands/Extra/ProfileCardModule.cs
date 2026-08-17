using Discord;
using Discord.Commands;
using Discord.WebSocket;
using SysBot.Pokemon;
using SysBot.Pokemon.Discord;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace FusionBot.Modules
{
    public class ProfileCardModule : ModuleBase<SocketCommandContext>
    {
        // ===========================
        // CONSTANTS
        // ===========================
        internal const int MAX_TRADES = 700;
        private const int MAX_LEVEL = 100;
        private const int MAX_QUOTE_LENGTH = 50;
        private const int PROGRESS_BAR_SEGMENTS = 20;

        // ===========================
        // DEPENDENCIES
        // ===========================
        private readonly TradeCodeStorage _tradeStorage = new();

        // Thread-safe Random for quote generation
        [ThreadStatic]
        private static Random? _random;
        private static Random Random => _random ??= new Random(Environment.TickCount + System.Threading.Thread.CurrentThread.ManagedThreadId);

        // ===========================
        // MEDAL MILESTONE DATA
        // ===========================
        private static readonly (int Milestone, string Title, Color Color)[] MedalData =
        [
            (1,   "Newbie Trainer",      new Color(169, 169, 169)),   // Grey
            (50,  "Novice Trainer",      new Color(205, 127, 50)),    // Bronze
            (100, "Pokémon Professor",   new Color(192, 192, 192)),   // Silver
            (150, "Pokémon Specialist",  new Color(255, 215, 0)),     // Gold
            (200, "Pokémon Champion",    new Color(0, 255, 255)),     // Cyan
            (250, "Pokémon Hero",        new Color(0, 128, 0)),       // Green
            (300, "Pokémon Elite",       new Color(255, 105, 180)),   // Hot Pink
            (350, "Pokémon Trader",      new Color(138, 43, 226)),    // BlueViolet
            (400, "Pokémon Sage",        new Color(75, 0, 130)),      // Indigo
            (450, "Pokémon Legend",      new Color(255, 69, 0)),      // OrangeRed
            (500, "Region Master",       new Color(255, 140, 0)),     // DarkOrange
            (550, "Trade Master",        new Color(0, 191, 255)),     // DeepSkyBlue
            (600, "World Famous",        new Color(124, 252, 0)),     // LawnGreen
            (650, "Pokémon Master",      new Color(218, 112, 214)),   // Orchid
            (700, "Pokémon God",         new Color(255, 0, 0))        // Red
        ];

        // ===========================
        // RANDOM QUOTES
        // ===========================
        private static readonly string[] DefaultQuotes =
        [
            "I only trade shiny Pokémon… sometimes.",
            "Here for the fun, not the fame!",
            "Rare candy addict.",
            "I speak fluent Pikachu.",
            "I bribed this bot with cookies.",
            "If you see me trading, run.",
            "Trading since the beginning of time.",
            "Gotta catch 'em all… eventually."
        ];

        // ===========================
        // MYINFO COMMAND
        // ===========================
        [Command("myinfo")]
        [Alias("mi")]
        [Summary("Displays your FusionBot profile card.")]
        public async Task MyInfoAsync(SocketGuildUser? targetUser = null)
        {
            // If no user is mentioned, fallback to the command invoker
            var user = targetUser ?? (Context.User as SocketGuildUser);

            if (user == null)
            {
                await ReplyAsync("❌ Can't show the profile of a user not in this server!");
                return;
            }

            // Get trade data
            var tradeDetails = _tradeStorage.GetTradeDetails(user.Id);
            if (tradeDetails == null)
            {
                await ReplyAsync(user.Id == Context.User.Id
                    ? "📊 You haven't traded yet, so no profile data exists!"
                    : $"📊 {user.Username} hasn't traded yet, so no profile data exists!");
                return;
            }

            int totalTrades = _tradeStorage.GetTradeCount(user.Id);

            // Get medal info
            var (milestone, medalTitle, embedColor) = GetMedalInfo(totalTrades);
            string medalImageUrl = $"https://raw.githubusercontent.com/Secludedly/ZE-FusionBot-Sprite-Images/main/{milestone:D3}.png";

            // Calculate level and progress
            int level = CalculateLevel(totalTrades);
            int tradesToNextLevel = CalculateTradesToNextLevel(level, totalTrades);
            double progressPct = (double)totalTrades / MAX_TRADES;
            string progressBar = BuildProgressBar(progressPct);

            // Get user info
            string quote = tradeDetails.Quote ?? GenerateRandomQuote();
            var topRole = GetTopRole(user);
            string topRoleDisplay = topRole?.Mention ?? "No Roles";

            // Discord metadata
            string serverName = Context.Guild?.Name ?? "this server";
            string accountCreated = user.CreatedAt.ToString("MMM dd, yyyy");
            string serverJoin = user.JoinedAt?.ToString("MMM dd, yyyy") ?? "Unknown";
            int roleCount = user.Roles.Count(r => !r.IsEveryone);

            // Build and send embed
            var embed = BuildProfileEmbed(
                user,
                tradeDetails.OT ?? "Unknown",
                tradeDetails.TID,
                tradeDetails.SID,
                totalTrades,
                level,
                tradesToNextLevel,
                progressPct,
                progressBar,
                quote,
                medalTitle,
                milestone,
                medalImageUrl,
                embedColor,
                serverName,
                accountCreated,
                serverJoin,
                roleCount,
                topRoleDisplay
            );

            await ReplyAsync(embed: embed.Build());
        }

        // ===========================
        // MYINFOQUOTE COMMAND
        // ===========================
        [Command("myinfoquote")]
        [Alias("miq")]
        [Summary("Set your personal FusionBot profile quote.")]
        public async Task SetQuoteAsync([Remainder] string quote)
        {
            // Validate quote length
            if (quote.Length > MAX_QUOTE_LENGTH)
            {
                await ReplyAsync($"❌ Quote is too long! Maximum {MAX_QUOTE_LENGTH} characters.");
                return;
            }

            // Get existing profile
            var details = _tradeStorage.GetTradeDetails(Context.User.Id);
            if (details == null)
            {
                await ReplyAsync("📊 You haven't traded yet, so no profile exists to set a quote.");
                return;
            }

            // Update quote
            _tradeStorage.UpdateTradeDetails(
                Context.User.Id,
                details.OT ?? "",
                details.TID,
                details.SID,
                quote
            );

            await ReplyAsync("✅ Your profile quote has been updated!");
        }

        // ===========================
        // HELPER METHODS - CALCULATIONS
        // ===========================
        internal static int CalculateLevel(int totalTrades)
        {
            int level = (int)Math.Round((double)totalTrades / MAX_TRADES * MAX_LEVEL);
            return Math.Clamp(level, 0, MAX_LEVEL);
        }

        internal static int CalculateTradesToNextLevel(int currentLevel, int totalTrades)
        {
            if (currentLevel >= MAX_LEVEL)
                return 0;

            int tradesPerLevel = MAX_TRADES / MAX_LEVEL;
            int tradesForNextLevel = (currentLevel + 1) * tradesPerLevel;
            return Math.Max(0, tradesForNextLevel - totalTrades);
        }

        // ===========================
        // HELPER METHODS - MEDAL INFO
        // ===========================
        internal static (int Milestone, string Title, Color Color) GetMedalInfo(int totalTrades)
        {
            int milestone = MedalHelpers.GetCurrentMilestone(totalTrades);

            foreach (var data in MedalData)
            {
                if (data.Milestone == milestone)
                    return data;
            }

            // Fallback for unrecognized milestones
            return (1, "New Trainer", new Color(128, 128, 128));
        }

        // ===========================
        // HELPER METHODS - USER INFO
        // ===========================
        internal static SocketRole? GetTopRole(SocketGuildUser user)
        {
            return user.Roles
                .Where(r => !r.IsEveryone)
                .OrderByDescending(r => r.Position)
                .FirstOrDefault();
        }

        internal static string GenerateRandomQuote()
        {
            return DefaultQuotes[Random.Next(DefaultQuotes.Length)];
        }

        // ===========================
        // HELPER METHODS - PROGRESS BAR
        // ===========================
        internal static string BuildProgressBar(double progressPct)
        {
            int filled = (int)Math.Round(progressPct * PROGRESS_BAR_SEGMENTS);
            filled = Math.Clamp(filled, 0, PROGRESS_BAR_SEGMENTS);
            int empty = PROGRESS_BAR_SEGMENTS - filled;

            // Use gradient: green -> lime green
            const string greenBlock = "🟩";
            const string grayBlock = "⬜";

            string bar = string.Concat(Enumerable.Repeat(greenBlock, filled));
            bar += string.Concat(Enumerable.Repeat(grayBlock, empty));

            return bar;
        }

        // ===========================
        // HELPER METHODS - EMBED BUILDER
        // ===========================
        internal static EmbedBuilder BuildProfileEmbed(
            SocketGuildUser user,
            string trainerOT,
            int trainerTID,
            int trainerSID,
            int totalTrades,
            int level,
            int tradesToNextLevel,
            double progressPct,
            string progressBar,
            string quote,
            string medalTitle,
            int milestone,
            string medalImageUrl,
            Color embedColor,
            string serverName,
            string accountCreated,
            string serverJoin,
            int roleCount,
            string topRoleDisplay)
        {
            return new EmbedBuilder()
                .WithTitle($"🎴 {user.Username}'s Bot Profile Card")
                .WithThumbnailUrl(user.GetAvatarUrl(size: 512) ?? user.GetDefaultAvatarUrl())
                .WithColor(embedColor)

                // Personal Quote
                .AddField(
                    "─ **PERSONAL QUOTE**",
                    $"\"*{quote}*\"",
                    inline: false)

                // Trainer Info
                .AddField(
                    "─ **TRAINER INFO**",
                    $"**OT:** {trainerOT}\n" +
                    $"**TID:** {trainerTID}\n" +
                    $"**SID:** {trainerSID}\n" +
                    $"**Total Trades:** {totalTrades:N0}",
                    inline: false)

                // Milestone Info
                .AddField(
                    "─ **MILESTONE INFO**",
                    $"**Current Title:** {medalTitle}\n" +
                    $"**Current Milestone:** {milestone}",
                    inline: false)

                // Level Progress
                .AddField(
                    "─ **LEVEL PROGRESS**",
                    $"🏆 **Current Level:** {level}   ⚡ **To Next Level:** {tradesToNextLevel:N0}\n" +
                    $"*Progress...*\n{progressBar} {(int)Math.Round(progressPct * 100)}%",
                    inline: false)

                // Discord Info
                .AddField(
                    "─ **DISCORD INFO**",
                    $"**Account Created:** {accountCreated}\n" +
                    $"**Joined {serverName}:** {serverJoin}\n" +
                    $"**Roles Count:** {roleCount}\n" +
                    $"**Top Role:** {topRoleDisplay}",
                    inline: false)

                // Medal Display
                .AddField(
                    "✨───────**CURRENT MEDAL**───────✨",
                    "\u200B",
                    inline: false)
                .WithImageUrl(medalImageUrl)

                // Footer
                .WithFooter(f => f.Text = "To set a personal quote, use the miq command");
        }
    }
}
