using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using TreeTrunk.DataObjects;
using System.Threading;

namespace TreeTrunk.Services{
    public partial class CommandHandler{
        private readonly CommandService _commands;
        private readonly DiscordSocketClient _discord;
        private readonly IServiceProvider _services;
        private readonly IConfigurationRoot _config;
        

        public CommandHandler(IServiceProvider services){
            _commands = services.GetRequiredService<CommandService>();
            _discord = services.GetRequiredService<DiscordSocketClient>();
            _config = services.GetRequiredService<IConfigurationRoot>();
            _services = services;
            
            _commands.CommandExecuted += CommandExecutedAsync;
            _discord.MessageReceived += MessageReceivedAsync;
            _discord.GuildMemberUpdated += ActivityAsync; 
            _discord.UserJoined += UserJoinAsync;
            _discord.UserLeft += UserLeftAsync;
            _discord.UserVoiceStateUpdated += UserVoiceStateUpdatedAsync;
        }

        public async Task InitializeAsync(){
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
        }
        
        private async Task MessageReceivedAsync(SocketMessage rawMessage){
            if(!(rawMessage is SocketUserMessage message)) return;
            if(message.Source != MessageSource.User) return;
            var argPos = 0;
            var context = new SocketCommandContext(_discord, message);
            var guild = context.Guild.Id;
            if(!message.HasStringPrefix(StaticFunctions.data[guild].prefix, ref argPos)){
                await messagehandler(rawMessage, context);
                return;
            }
            Console.WriteLine(DateTime.Now.ToString() + ": A command was executed.");
            await _commands.ExecuteAsync(context, argPos, _services);
        }

        public async Task CommandExecutedAsync(Optional<CommandInfo> command, ICommandContext context, IResult result){
            if (!command.IsSpecified) return;
            if (result.IsSuccess) return;
            await context.Channel.SendMessageAsync($"{result}");
        }

    }
    
}