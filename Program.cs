using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
//using System.Collections.Generic;
using Discord;
using Discord.WebSocket;
using Discord.Commands;
//using Newtonsoft.Json;
using TreeTrunk.Services;

namespace TreeTrunk{
    class Program{

        static void Main(string[] args) 
            => new Program().MainAsync().GetAwaiter().GetResult();

        public async Task MainAsync(){

            var service = new ServiceCollection();
            ConfigureServices(service);
            var services = service.BuildServiceProvider();

            var client = services.GetRequiredService<DiscordSocketClient>();
            client.Log += LogAsync;

            services.GetRequiredService<CommandService>().Log += LogAsync;
            
            var config = services.GetRequiredService<IConfigurationRoot>();

            string discordToken = config["Token"];
            if (string.IsNullOrWhiteSpace(discordToken))
                throw new Exception("Please enter your bot's token into the `default_config.json` file.");

            await client.LoginAsync(TokenType.Bot, discordToken);
            await client.StartAsync();
            await services.GetRequiredService<CommandHandler>().InitializeAsync();

            await Task.Delay(Timeout.Infinite);
            
        }


        // public static Task AppendDictData(Dictionary<string,string> data){
            

        //     var old_data = ReadDictData();



        //     string output = Newtonsoft.Json.JsonConvert.SerializeObject(jsonObj, Newtonsoft.Json.Formatting.Indented);
        //     File.WriteAllText("config.json", output);



        //     string json = JsonConvert.SerializeObject(points, Newtonsoft.Json.Formatting.Indented);


        //     return Task.CompletedTask;
        // }

        // public static Dictionary<string,string> ReadDictData(){
        //     var text = File.ReadAllText("config.json");
        //     var data = JsonConvert.DeserializeObject<Dictionary<string, string>>(text);
        //     return data;
        // }


        public static Task WriteConfig(string key, string value){
            string json = File.ReadAllText("config.json");
            dynamic jsonObj = Newtonsoft.Json.JsonConvert.DeserializeObject(json);
            jsonObj[key] = value;
            string output = Newtonsoft.Json.JsonConvert.SerializeObject(jsonObj, Newtonsoft.Json.Formatting.Indented);
            File.WriteAllText("config.json", output);
            return Task.CompletedTask;
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
