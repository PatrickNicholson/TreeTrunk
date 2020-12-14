using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace TreeTrunk.Services{
    public class CommandHandler{
        private readonly CommandService _commands;
        private readonly DiscordSocketClient _discord;
        private readonly IServiceProvider _services;
        

        public CommandHandler(IServiceProvider services){
            _commands = services.GetRequiredService<CommandService>();
            _discord = services.GetRequiredService<DiscordSocketClient>();
            _services = services;
            _commands.CommandExecuted += CommandExecutedAsync;
            _discord.MessageReceived += MessageReceivedAsync;
            _discord.GuildMemberUpdated += ActivityAsync;
            
        }

        public async Task InitializeAsync(){
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
        }
        
        public async Task MessageReceivedAsync(SocketMessage rawMessage){
            if(!(rawMessage is SocketUserMessage message)) return;
            if(message.Source != MessageSource.User) return;
            var argPos = 0;
            if(!message.HasCharPrefix('!', ref argPos)) return;
            
            var context = new SocketCommandContext(_discord, message);
            await _commands.ExecuteAsync(context, argPos, _services);
        }
    

        public async Task ActivityAsync(SocketGuildUser initial, SocketGuildUser final){
            
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

        public async Task CommandExecutedAsync(Optional<CommandInfo> command, ICommandContext context, IResult result){
            // command is unspecified when there was a search failure (command not found); we don't care about these errors
            if (!command.IsSpecified) return;
            // the command was successful, we don't care about this result, unless we want to log that a command succeeded.
            if (result.IsSuccess) return;
            // the command failed, let's notify the user that something happened.
            //await context.Channel.SendMessageAsync($"error: {result}");
            await context.Channel.SendMessageAsync($"{result}");
        }

    }
}