using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System;
using Discord;
using Discord.Commands;


namespace TreeTrunk.Modules{
    public class AdminCommands : ModuleBase<SocketCommandContext>{

        private readonly IConfigurationRoot _config;

        public AdminCommands(IConfigurationRoot config)
        {
            _config = config;
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
    }
}