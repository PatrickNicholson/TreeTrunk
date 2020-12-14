using System;
using System.IO;
using System.Net.Http;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Discord;
using Discord.WebSocket;
using Discord.Commands;
using TreeTrunk.Services;

namespace TreeTrunk{
    class Program{
        
        static void Main(string[] args) 
            => new Program().MainAsync().GetAwaiter().GetResult();

        public async Task MainAsync(){
            using (var services = ConfigureServices()){
                var client = services.GetRequiredService<DiscordSocketClient>();
                client.Log += LogAsync;

                services.GetRequiredService<CommandService>().Log += LogAsync;
                
                var text = File.ReadAllText("config.json"); //find a better system for configs than json
                string bot_token = (JsonConvert.DeserializeObject<Dictionary<string, string>>(text))["Token"];
                
                await client.LoginAsync(TokenType.Bot, bot_token);
                await client.StartAsync();
                await services.GetRequiredService<CommandHandler>().InitializeAsync();

                await Task.Delay(Timeout.Infinite);
            }
        }

        private Task LogAsync(LogMessage log){
            Console.WriteLine(log.ToString());
            return Task.CompletedTask;
        }

        private ServiceProvider ConfigureServices(){
            return new ServiceCollection()
                .AddSingleton<DiscordSocketClient>()
                .AddSingleton<CommandService>()
                .AddSingleton<CommandHandler>()
                .AddSingleton<HttpClient>()
                .BuildServiceProvider();
        }
    }
}
