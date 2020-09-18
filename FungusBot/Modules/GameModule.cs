using Discord.Commands;
using Discord.WebSocket;
using FungusBot.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static FungusBot.GameState;

namespace FungusBot.Modules {
    public class GameModule : ModuleBase<SocketCommandContext> {
        public MemoryService MemoryService { get; set; }
        public StateService StateService { get; set; }

        [Command("start")]
        [Summary("Start the game.")]
        public async Task StartAsync() {
            await StateService.SetAll(x => {
                x.Mute = true;
                x.Deaf = true;
            });
            await ReplyAsync("Game started, muted relevant people.");
        }

        [Command("end")]
        [Summary("End the game.")]
        public async Task EndAsync() {
            await StateService.SetAll(x => {
                x.Mute = false;
                x.Deaf = false;
            });
            await ReplyAsync("Game ended, unmuted relevant people.");
        }
    }
}
