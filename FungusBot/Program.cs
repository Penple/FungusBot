using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Threading.Tasks;
using System.Threading;
using System.Text;
using FungusBot.Services;
using Microsoft.Extensions.DependencyInjection;

namespace FungusBot {
    class Program {
        static void Main(string[] args)
            => new Program().MainAsync().GetAwaiter().GetResult();

        private DiscordSocketClient _client;
        private IConfiguration _config;

        private MemoryService memory;
        private StateService state;

        private GameState gameState;
        private GameState lastGameState;

        public async Task MainAsync() {
            var services = ConfigureServices();

            memory = services.GetRequiredService<MemoryService>();
            state = services.GetRequiredService<StateService>();

            _client = services.GetRequiredService<DiscordSocketClient>();
            _config = BuildConfig();

            _client.Log += LogAsync;
            services.GetRequiredService<CommandService>().Log += LogAsync;
            _client.Ready += ReadyAsync;

            await _client.LoginAsync(TokenType.Bot, _config["token"]);
            await _client.StartAsync();

            await services.GetRequiredService<CommandHandlingService>().InitializeAsync();

            await Task.Delay(Timeout.Infinite);
        }

        private Task ReadyAsync() {
            _ = Task.Run(async () => {
                while (true) {
                    gameState = memory.ReadGameState();

                    if (lastGameState != null && gameState.voteState != lastGameState.voteState) {
                        if (gameState.voteState == GameState.VoteState.Voting) {
                            await state.SetDead(x => {
                                x.Mute = true;
                                x.Deaf = false;
                            });
                            await state.SetAlive(x => {
                                x.Mute = false;
                                x.Deaf = false;
                            });
                        } else if (gameState.voteState == GameState.VoteState.InGame) {
                            await Task.Delay(5000);
                            await state.SetAlive(x => {
                                x.Mute = true;
                                x.Deaf = true;
                            });
                            await state.SetDead(x => {
                                x.Mute = false;
                            });
                        }
                    }

                    lastGameState = gameState;

                    await Task.Delay(100);
                }
            });
            return Task.CompletedTask;
        }

        private Task LogAsync(LogMessage log) {
            Console.WriteLine(log.ToString());

            return Task.CompletedTask;
        }

        private IConfiguration BuildConfig() {
            return new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("config.json")
                .Build();
        }

        private ServiceProvider ConfigureServices() {
            return new ServiceCollection()
                .AddSingleton<MemoryService>()
                .AddSingleton<DiscordSocketClient>()
                .AddSingleton<CommandService>()
                .AddSingleton<CommandHandlingService>()
                .AddSingleton<StateService>()
                .AddSingleton(_config)
                .BuildServiceProvider();
        }
    }
}
