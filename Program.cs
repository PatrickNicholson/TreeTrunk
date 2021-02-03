using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Discord;
using Discord.WebSocket;
using Discord.Commands;
using TreeTrunk.Services;

namespace TreeTrunk{
    class Program{

        static void Main(string[] args) 
            => new Program().MainAsync().GetAwaiter().GetResult();

        public async Task MainAsync(){

            var service = new ServiceCollection();
            ConfigureServices(service);
            var services = service.BuildServiceProvider();

            //hook into logging service
            var client = services.GetRequiredService<DiscordSocketClient>();
            client.Log += LogAsync;
            services.GetRequiredService<CommandService>().Log += LogAsync;
            
            var config = services.GetRequiredService<IConfigurationRoot>();

            string discordToken = config["Token"];
            if (string.IsNullOrWhiteSpace(discordToken))
                throw new Exception("Please enter your bot's token into the `config.json` file.");
            
            await StaticFunctions.LoadGuildData();
            TaskSchedule.Instance.ScheduleTask(23, 59, 24, () 
                => StaticFunctions.WriteGuildData());
    
            await client.LoginAsync(TokenType.Bot, discordToken);
            await client.StartAsync();
            await services.GetRequiredService<CommandHandler>().InitializeAsync();
            await Task.Delay(Timeout.Infinite);
            
        }

        private Task LogAsync(LogMessage log){
            Console.WriteLine(log.ToString());
            return Task.CompletedTask;
        }

        private void ConfigureServices(IServiceCollection service){
            service.Add(new ServiceDescriptor(typeof(IConfigurationRoot),
                        configuration => new ConfigurationBuilder()
                                .SetBasePath(Directory.GetCurrentDirectory())
                                .AddJsonFile("config.json",optional:false,reloadOnChange: true)
                                .Build(),ServiceLifetime.Singleton));

            service.AddSingleton<DiscordSocketClient>()
                .AddSingleton<CommandService>()
                .AddSingleton<CommandHandler>()
                .AddSingleton<HttpClient>();
        }
    }
}
