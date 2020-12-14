using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;
using Discord;
using Discord.Commands;

namespace TreeTrunk.Modules{
    public class AdminCommands : ModuleBase<SocketCommandContext>{

        [Command("userinfo")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task UserInfoAsync(IUser user = null){
            user = user ?? Context.User;

            await ReplyAsync(user.ToString());
        }

        //useful attributes to remember
        //[RequireBotPermission(GuildPermission.Administrator)]
        //[RequireContext(ContextType.Guild, ErrorMessage = "Sorry, this command must be ran from within a server, not a DM!")]

        [Command("cprefix")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task ChangePrefix([Remainder] string text){
            var m = Context.Message;
            await m.DeleteAsync();

            string json = File.ReadAllText("config.json"); //find a better system for configs than json
            dynamic jsonObj = Newtonsoft.Json.JsonConvert.DeserializeObject(json);
            jsonObj["Token"] = text;
            string output = Newtonsoft.Json.JsonConvert.SerializeObject(jsonObj, Newtonsoft.Json.Formatting.Indented);
            File.WriteAllText("settings.json", output);

            await ReplyAsync("Changed prefix to: " + text);
        }
    }
}