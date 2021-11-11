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
            _discord.ReactionAdded += ReactionAddAsync;
            _discord.ReactionRemoved += ReactionRemoveAsync;
            _discord.InviteCreated += InviteCreatedAsync;
            _discord.LatencyUpdated += LatencyStatus;
            _discord.MessageDeleted += MessageDeletedAsync;
        }

        public async Task InitializeAsync(){
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
        }
        
        private Task MessageReceivedAsync(SocketMessage rawMessage){
            
            if(rawMessage.Source == MessageSource.System || rawMessage.Source == MessageSource.Webhook){
                return Task.CompletedTask;
            }

            var message = rawMessage as SocketUserMessage;
            var context = new SocketCommandContext(_discord, message);
            var argPos = 0;
            if(rawMessage.Author.Id == context.Client.CurrentUser.Id){
                SelfMessageHandler(rawMessage, context);
            }
            else if(message.HasStringPrefix(StaticFunctions.data[context.Guild.Id].prefix, ref argPos)){
                context.Message.DeleteAsync();
                _commands.ExecuteAsync(context, argPos, _services);
            }
            else{
                MessageHandler(rawMessage, context);
            }
            return Task.CompletedTask;
        }

        public Task CommandExecutedAsync(Optional<CommandInfo> command, ICommandContext context, IResult result){
            if (!command.IsSpecified) return Task.CompletedTask;
            if (result.IsSuccess) return Task.CompletedTask;
            context.Channel.SendMessageAsync($"{result}");
            return Task.CompletedTask;
        }

    }
    
}