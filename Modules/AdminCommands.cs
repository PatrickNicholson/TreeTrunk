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
        public async Task UserInfoAsync(IUser user = null){
            var m = Context.Message;
            await m.DeleteAsync();

            user = user ?? Context.User;
            await ReplyAsync(user.ToString());
        }

        //useful attributes to remember
        //[RequireBotPermission(GuildPermission.Administrator)]
        //[RequireContext(ContextType.Guild, ErrorMessage = "Sorry, this command must be ran from within a server, not a DM!")]

        [Command("cprefix")]
        [Alias("cp")]
        [Summary("Change command prefix.")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task ChangePrefix([Remainder] string text){
            var m = Context.Message;
            var guild = Context.Guild.Id;
            await m.DeleteAsync();
            
            StaticFunctions.data[guild].prefix = text;

            //await StaticFunctions.WriteConfig("prefix",text);
            //_config["prefix"] = text;

            await ReplyAsync("Changed prefix to: " + text);
        }


        [Command("info")]
        [Summary("Get info.")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task Getinfo(){
            var m = Context.Guild.CreatedAt.ToOffset(TimeSpan.FromHours(-5));
            Console.WriteLine("Stickman's Archipelago Created at:");
            await Console.Out.WriteLineAsync(m.ToString());
            Console.WriteLine("--------------------------------------------------------------------------------------------------");
            var members = Context.Guild.Users;
            foreach(var user in members){
                if(user.PremiumSince.ToString() != ""){
                    Console.WriteLine(user.Username.ToString() + ":");
                    Console.WriteLine("Premium since: " + (user.PremiumSince).ToString());
                    Console.WriteLine("----------------------------------------------------------------------------------");
                }
            }
        }


        [Command("retrofit")]
        [Summary("Reads all previous messages in server and collects them for statistics")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public Task RetroFitData(){
            
            retro(Context);
            return Task.CompletedTask;
        }

        [Command("retrofitusers")]
        [Summary("Reads all previous messages in server and collects them for statistics")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public Task RetroFitProfiles(){
            
            UpdateProfileDatabase(Context);
            return Task.CompletedTask;
        }
/*
        [Command("test")]
        [Summary("Reads all previous messages in server and collects them for statistics")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public Task Test(){
           

            foreach (KeyValuePair<string, object> kvp in StaticFunctions.data[422449575127678976].storeddata)
            {
                string streamrole;
                StaticFunctions.CastIt<string>(kvp.Value, out streamrole);
                Console.WriteLine(kvp.Key, streamrole);
            }

            return Task.CompletedTask;
                
            
        }
*/
        [Command("setstreamrole")]
        [Alias("sst")]
        [Summary("Reads all previous messages in server and collects them for statistics")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task SetStreamerRole([Remainder] string text){
            
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
            if(!found) await ReplyAsync(text + " does not exist as a role.");  
            return;
                
            
        }


        [Command("saveguilddata")]
        [Alias("sgd")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public Task savedata(){
            
            StaticFunctions.WriteGuildData();
            
            return Task.CompletedTask;
                
            
        }



        private async void retro(SocketCommandContext context){
            var guild_id = context.Guild.Id;
            var textchannels = context.Client.GetGuild(guild_id).TextChannels;
            IMessage message_index = null;
            foreach(var textchannel in textchannels){
                
                if(message_index == null){
                    foreach(var message in await textchannel.GetMessagesAsync(1).FlattenAsync()){
                        message_index = message;
                    }
                }
                IMessage last_message = message_index;
                //Console.WriteLine(textchannel.Name.ToString());

                int leng = 1;
                int count = 1;
                while(leng > 0){
                    var messages = await textchannel.GetMessagesAsync(last_message, Direction.Before, 100).FlattenAsync();
                    //Console.WriteLine(messages.Count());
                    foreach( var message in messages){
                        //Console.WriteLine(message.Author.ToString());
                        
                        //Console.WriteLine(message);
                        last_message = message;
                    }
                    //Console.WriteLine(last_message);
                    
                    leng = messages.Count();
                    count += leng;
                    
                }
                Console.WriteLine(textchannel.Name.ToString() + " " + count.ToString());
                message_index = null;
                
                    
            }
        }

        private async void UpdateProfileDatabase(SocketCommandContext context){
            var guild_id = context.Guild.Id;
            await context.Client.GetGuild(guild_id).DownloadUsersAsync();
            var users = context.Client.GetGuild(guild_id).Users.ToList();

            foreach( var user in users){

            }

        }


    }
}