using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
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
                await client.LoginAsync(TokenType.Bot, "");//Environment.GetEnvironmentVariable("token"));
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
