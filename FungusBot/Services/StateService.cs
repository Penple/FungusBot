using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static FungusBot.GameState;

namespace FungusBot.Services {
    public class StateService {
        private readonly DiscordSocketClient _discord;
        private readonly MemoryService _memory;
        private readonly IConfiguration _config;
        private readonly IServiceProvider _services;

        private SocketGuild guild;
        private SocketVoiceChannel channel;

        public StateService(IServiceProvider services) {
            _discord = services.GetRequiredService<DiscordSocketClient>();
            _memory = services.GetRequiredService<MemoryService>();
            _services = services;

            _discord.Ready += InitAsync;
        }

        public Task InitAsync() {
            guild = _discord.GetGuild(ulong.Parse(_config["guild"]));
            channel = guild.GetVoiceChannel(ulong.Parse(_config["channel"]));

            return Task.CompletedTask;
        }

        public IEnumerable<SocketGuildUser> GetAll() {
            GameState state = _memory.ReadGameState();
            foreach (Player player in state.players) {
                Console.WriteLine(player.name);
            }
            return channel.Users.Where(x => state.players.Find(y => ((x.Nickname ?? x.Username) == y.name)) != null);
        }

        public async Task SetAll(Action<GuildUserProperties> func) {
            Console.WriteLine("Setting All");
            foreach (SocketGuildUser user in GetAll()) {
                Console.WriteLine(user.Nickname ?? user.Username);
                await user.ModifyAsync(func);
            }
        }

        public IEnumerable<SocketGuildUser> GetAlive() {
            GameState state = _memory.ReadGameState();
            return channel.Users.Where(x => state.players.Find(y => (!y.dead && (x.Nickname ?? x.Username) == y.name)) != null);
        }

        public async Task SetAlive(Action<GuildUserProperties> func) {
            Console.WriteLine("Setting Alive");
            foreach (SocketGuildUser user in GetAlive()) {
                Console.WriteLine(user.Nickname ?? user.Username);
                await user.ModifyAsync(func);
            }
        }

        public IEnumerable<SocketGuildUser> GetDead() {
            GameState state = _memory.ReadGameState();
            return channel.Users.Where(x => state.players.Find(y => (y.dead && (x.Nickname ?? x.Username) == y.name)) != null);
        }

        public async Task SetDead(Action<GuildUserProperties> func) {
            Console.WriteLine("Setting Dead");
            foreach (SocketGuildUser user in GetDead()) {
                Console.WriteLine(user.Nickname ?? user.Username);
                await user.ModifyAsync(func);
            }
        }
    }
}
