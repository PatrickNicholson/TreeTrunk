using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Discord.WebSocket;

namespace TreeTrunk.Services{
    public class PassiveFeatures{
        private readonly DiscordSocketClient _discord;
        private readonly IServiceProvider _services;
        private readonly IConfigurationRoot _config;
        

        public PassiveFeatures(IServiceProvider services){
            _discord = services.GetRequiredService<DiscordSocketClient>();
            _config = services.GetRequiredService<IConfigurationRoot>();
            _services = services;
            _discord.GuildMemberUpdated += ActivityAsync;
            _discord.UserVoiceStateUpdated += test;
            
        }

        private async Task test(SocketUser u,SocketVoiceState a, SocketVoiceState b){
            
            await Console.Out.WriteLineAsync(u.Username.ToString());
            await Console.Out.WriteLineAsync(a.VoiceChannel.Id.ToString());
            await Console.Out.WriteLineAsync(b.VoiceChannel.Id.ToString());
            await Console.Out.WriteLineAsync("-------------------------------------------------");

        }
    
        private async Task ActivityAsync(SocketGuildUser initial, SocketGuildUser final){
            if(initial.VoiceChannel != null){
                await Console.Out.WriteLineAsync((initial.Username).ToString());
                await Console.Out.WriteLineAsync("inital: " + (initial.VoiceChannel).ToString());
                await Console.Out.WriteLineAsync("final: " + (final.VoiceChannel).ToString());
            }
            await Console.Out.WriteLineAsync(initial.Username.ToString());
            await Console.Out.WriteLineAsync("Activity-----");
            if(initial.Activity != null || initial.Activity != null){
                await Console.Out.WriteLineAsync(initial.Activity.ToString());
                await Console.Out.WriteLineAsync(final.Activity.ToString());
            }
            await Console.Out.WriteLineAsync("Status-----");
            await Console.Out.WriteLineAsync(initial.Status.ToString());
            await Console.Out.WriteLineAsync(final.Status.ToString());
            
            await Console.Out.WriteLineAsync("----------------------------------------");
            
            
          
            return;
        }

    }
}