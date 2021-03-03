using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Collections.Generic;
using System.Linq;


namespace TreeTrunk.Modules{
    public class AdminCommands : ModuleBase<SocketCommandContext>{

        private readonly IConfigurationRoot _config;
        private readonly DiscordSocketClient _client;

        public AdminCommands(IConfigurationRoot config, DiscordSocketClient client){
            _config = config;
            _client = client;
        }

        [Command("userinfo")]
        [Summary("Get member discord tag.")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public Task UserInfoAsync(IUser user = null){
            var m = Context.Message;
            m.DeleteAsync();

            user = user ?? Context.User;
            ReplyAsync(user.ToString());
            return Task.CompletedTask;
        }

        //useful attributes to remember
        //[RequireBotPermission(GuildPermission.Administrator)]
        //[RequireContext(ContextType.Guild, ErrorMessage = "Sorry, this command must be ran from within a server, not a DM!")]

        [Command("cprefix")]
        [Alias("cp")]
        [Summary("Change command prefix.")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public Task ChangePrefix([Remainder] string text){
            var m = Context.Message;
            var guild = Context.Guild.Id;
            m.DeleteAsync();
            
            StaticFunctions.data[guild].prefix = text;

            //await StaticFunctions.WriteConfig("prefix",text);
            //_config["prefix"] = text;

            ReplyAsync("Changed prefix to: " + text);
            return Task.CompletedTask;
        }


        [Command("info")]
        [Summary("Get info.")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public Task Getinfo(){
            var m = Context.Guild.CreatedAt.ToOffset(TimeSpan.FromHours(-5));
            Console.WriteLine("Stickman's Archipelago Created at:");
            Console.WriteLine(m.ToString());
            Console.WriteLine("--------------------------------------------------------------------------------------------------");
            var members = Context.Guild.Users;
            foreach(var user in members){
                if(user.PremiumSince.ToString() != ""){
                    Console.WriteLine(user.Username.ToString() + ":");
                    Console.WriteLine("Premium since: " + (user.PremiumSince).ToString());
                    Console.WriteLine("----------------------------------------------------------------------------------");
                }
            }
            return Task.CompletedTask;
        }

        [Command("setstreamrole")]
        [Alias("sst")]
        [Summary("Reads all previous messages in server and collects them for statistics")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public Task SetStreamerRole([Remainder] string text){
            
            var roles = Context.Guild.Roles;
            var id = Context.Guild.Id;
            bool found = false;
            foreach(var role in roles){
                if(String.Equals(role.Name, text, StringComparison.CurrentCultureIgnoreCase)){
                    //StaticFunctions.AddGuildData(Context.Guild.Id, "StreamerRole", role.Id);
                    StaticFunctions.data[Context.Guild.Id].streamrole = role.Id;
                    found = true;
                }
            }
            if(!found) ReplyAsync(text + " does not exist as a role.");  
            return Task.CompletedTask;
                
            
        }


        [Command("saveguilddata")]
        [Alias("sgd")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public Task savedata(){                       
            var m = Context.Message;
            m.DeleteAsync();
            StaticFunctions.UpdateAR();
            return Task.CompletedTask;
        }



        // [Command("retrofit")]
        // [Summary("Reads all previous messages in server and collects them for statistics")]
        // [RequireUserPermission(GuildPermission.Administrator)]
        // public Task RetroFitData(){
            
        //     retro(Context);
        //     return Task.CompletedTask;
        // }
        
        // private async void retro(SocketCommandContext context){
        //     var guild_id = context.Guild.Id;
        //     var textchannels = context.Client.GetGuild(guild_id).TextChannels;
        //     IMessage message_index = null;
        //     foreach(var textchannel in textchannels){
                
        //         if(message_index == null){
        //             foreach(var message in await textchannel.GetMessagesAsync(1).FlattenAsync()){
        //                 message_index = message;
        //             }
        //         }
        //         IMessage last_message = message_index;
        //         //Console.WriteLine(textchannel.Name.ToString());

        //         int leng = 1;
        //         int count = 1;
        //         while(leng > 0){
        //             var messages = await textchannel.GetMessagesAsync(last_message, Direction.Before, 100).FlattenAsync();
        //             //Console.WriteLine(messages.Count());
        //             foreach( var message in messages){
        //                 //Console.WriteLine(message.Author.ToString());
                        
        //                 //Console.WriteLine(message);
        //                 last_message = message;
        //             }
        //             //Console.WriteLine(last_message);
                    
        //             leng = messages.Count();
        //             count += leng;
                    
        //         }
        //         Console.WriteLine(textchannel.Name.ToString() + " " + count.ToString());
        //         message_index = null;
                
                    
        //     }
        // }

    }
}