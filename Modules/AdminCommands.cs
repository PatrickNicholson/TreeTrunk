using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Text.Unicode;


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
            var guild = Context.Guild.Id;
            
            StaticFunctions.data[guild].prefix = text;
            
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
        [Summary("sets the role to use when a user starts streaming")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public Task SetStreamerRole([Remainder] string text){
            
            var roles = Context.Guild.Roles;
            var id = Context.Guild.Id;
            bool found = false;
            foreach(var role in roles){
                if(String.Equals(role.Name, text, StringComparison.CurrentCultureIgnoreCase)){
                    StaticFunctions.data[Context.Guild.Id].streamrole = role.Id;
                    found = true;
                    break;
                }
            }
            if(!found) ReplyAsync(text + " does not exist as a role.");  
            return Task.CompletedTask;
                
            
        }


        [Command("forcerankupdate")]
        [Alias("fru")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public Task savedata(){                       
            StaticFunctions.UpdateAR();
            return Task.CompletedTask;
        }

        [Command("save")]
        [Alias("s")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public Task save(){
            StaticFunctions.WriteGuildData();
            return Task.CompletedTask;
        }

        [Command("lookup")]
        [Alias("lu")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public Task LookUp([Remainder] string text){
            var members = Context.Guild.Users;
            ulong userid = 0;
            foreach(var member in members){
                if(String.Compare(member.Username, text, true) == 0){
                    userid = member.Id;
                }
            }
            if(userid == 0) ReplyAsync(text + " does not exist in this server.");
            else{
                ReplyAsync(StaticFunctions.data[Context.Guild.Id].usermanager[userid].activityrating.ToString());
            }
            return Task.CompletedTask;
        }

        [Command("addrolevc")]
        [Alias("avc")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public Task AddVoiceChat([Remainder] string text){
            var chats = Context.Guild.VoiceChannels;
            ulong chatid = 0;
            string chatname = "";
            foreach(var chat in chats){
                if(chat.Name.Contains(text)){
                    chatid = chat.Id;
                    chatname = chat.Name;
                }
            }
            if(chatid == 0) ReplyAsync(text + " does not exist in the Voicechat List.");
            else{
                if(StaticFunctions.data[Context.Guild.Id].vcroles.Contains(chatid)){
                    ReplyAsync(chatname + " already exists in the Voicechat List." );
                }
                else{
                    StaticFunctions.data[Context.Guild.Id].vcroles.Add(chatid);
                    ReplyAsync(chatname + " was added to the Voicechat List.");
                }
                
            }
            return Task.CompletedTask;
        }

        [Command("removerolevc")]
        [Alias("rvc")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public Task RemoveVoiceChat([Remainder] string text){
            var chats = Context.Guild.VoiceChannels;
            ulong chatid = 0;
            string chatname = "";
            foreach(var chat in chats){
                if(chat.Name.Contains(text)){
                    chatid = chat.Id;
                    chatname = chat.Name;
                }
            }
            if(chatid == 0) ReplyAsync(text + " does not exist in the Voicechat List.");
            else{
                if(StaticFunctions.data[Context.Guild.Id].vcroles.Contains(chatid)){
                    StaticFunctions.data[Context.Guild.Id].vcroles.Remove(chatid);
                    ReplyAsync(chatname + " was removed from the ignore list");
                }
                else{
                    ReplyAsync(chatname + " does not exist in the ignore list.");
                }
                
            }
            return Task.CompletedTask;
        }

        [Command("adjustdecay")]
        [Alias("ad")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public Task ChangeDecay([Remainder] string text){
            var value = StaticFunctions.data[Context.Guild.Id].decay;
            try{
                value = Convert.ToInt32(text);
            }
            catch{
                ReplyAsync("\"" + text + "\" cant be converted to a number");
                return Task.CompletedTask;
            }

            StaticFunctions.data[Context.Guild.Id].decay = value;

            ReplyAsync("successfully changed to: " + value.ToString());

            return Task.CompletedTask;
        }

        [Command("adjuststarlimit")]
        [Alias("asl")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public Task ChangeStarLimit([Remainder] string text){                       
            var value = StaticFunctions.data[Context.Guild.Id].starboardlimit;
            try{
                value = Convert.ToInt32(text);
            }
            catch{
                ReplyAsync("\"" + text + "\" cant be converted to a number.");
                return Task.CompletedTask;
            }

            StaticFunctions.data[Context.Guild.Id].starboardlimit = value;
            ReplyAsync("successfully changed to: " + value.ToString());

            return Task.CompletedTask;
        }

        [Command("setstardefault")]
        [Alias("ssdefault")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public Task ChangeStarDefault([Remainder] string text){
            text.Trim();
            Console.WriteLine(text[0].ToString());
            Console.WriteLine(text.Length);
            Console.WriteLine(System.Text.ASCIIEncoding.Unicode.GetByteCount(text));
            
            if(text.EndsWith(">") && text.StartsWith("<")){
                ulong id = 0;
                try{
                    id = Convert.ToUInt64(text.Substring(text.LastIndexOf(":") + 1, text.Length - 7));
                }
                catch{
                    ReplyAsync("\"" + text + "\" is not an emote.");
                    return Task.CompletedTask;
                }
                
                foreach(var emote in Context.Guild.Emotes){
                    if(emote.Id == id){
                        StaticFunctions.data[Context.Guild.Id].starboarddefault = text;
                        ReplyAsync("Successfully changed to: " + text.ToString());
                        break;
                    }
                }
            }
            else if(text.Length < 50){
                StaticFunctions.data[Context.Guild.Id].starboarddefault = text;
                ReplyAsync("Successfully changed to: " + text.ToString());
            }
            else{
                ReplyAsync("\"" + text + "\" must less than 50 characters.");
            }
            return Task.CompletedTask;
        }

    }
}