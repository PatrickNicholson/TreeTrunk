using System.Threading.Tasks;
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

        [Command("guild_only")]
        [RequireUserPermission(GuildPermission.Administrator)]
        [RequireBotPermission(GuildPermission.Administrator)]
        [RequireContext(ContextType.Guild, ErrorMessage = "Sorry, this command must be ran from within a server, not a DM!")]
        public Task GuildOnlyCommand()
            => ReplyAsync("Nothing to see here!");
    }
}