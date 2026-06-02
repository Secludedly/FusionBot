using System;
using System.ComponentModel;
using System.Linq;
using SysBot.Pokemon.Localization;

namespace SysBot.Pokemon
{
    public class BilibiliSettings
    {
        private const string Startup = nameof(Startup);

        public override string ToString() => "Bilibili Integration Settings";

        // Startup

        [HubDisplayName("BilibiliSettings_LogUrl_DisplayName")]
        [HubCategory(Startup), HubDescription("BilibiliSettings_LogUrl_Description")]
        public string LogUrl { get; set; } = string.Empty;

        [HubDisplayName("BilibiliSettings_RoomId_DisplayName")]
        [HubCategory(Startup), HubDescription("BilibiliSettings_RoomId_Description")]
        public int RoomId { get; set; } = 0;
    }
}
