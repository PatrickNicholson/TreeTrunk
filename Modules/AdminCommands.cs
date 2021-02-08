using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Collections.Generic;


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
            await m.DeleteAsync();
            
            await StaticFunctions.WriteConfig("prefix",text);
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
            
            StaticFunctions.retro(Context);
            return Task.CompletedTask;
        }

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
                    StaticFunctions.AddGuildData(Context.Guild.Id, "StreamerRole", role.Id);
                    found = true;
                }
            }
            if(!found) await ReplyAsync(text + " does not exist as a role.");  
            return;
                
            
        }


    }
}