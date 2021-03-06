using Discord;
using Discord.Commands;
using Microsoft.Extensions.Configuration;
using System.Linq;
using System.Threading.Tasks;



namespace TreeTrunk.Modules{
    public class HelpCommands : ModuleBase<SocketCommandContext>{
        private readonly CommandService _commands;
        private readonly IConfigurationRoot _config;

        public HelpCommands(CommandService commands, IConfigurationRoot config)
        {
            _commands = commands;
            _config = config;
        }

        [Command("help")]
        [Alias("h")]
        public async Task HelpAsync(){
            var builder = new EmbedBuilder(){
                Color = Color.DarkGreen,
                Description = "These are the commands you can use"
            };
            
            foreach (var module in _commands.Modules){
                if(module.Name == "AdminCommands" || module.Name == "HelpCommands") continue;
                string description = null;
                foreach (var cmd in module.Commands){
                    var result = await cmd.CheckPreconditionsAsync(Context);
                    if (result.IsSuccess)
                        description += $"{_config["prefix"]}{cmd.Aliases.First()}\n";
                }
                
                if (!string.IsNullOrWhiteSpace(description)){
                    builder.AddField(x =>{
                        x.Name = module.Name;
                        x.Value = description;
                        x.IsInline = false;
                    });
                }
            }

            await ReplyAsync("", false, builder.Build());
        }

        [Command("ahelp")]
        [Alias("ah")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task AdminHelpAsync(){
            var builder = new EmbedBuilder(){
                Color = Color.DarkGreen,
                Description = "These are the commands you can use"
            };
            
            foreach (var module in _commands.Modules){
                string description = null;
                foreach (var cmd in module.Commands){
                    var result = await cmd.CheckPreconditionsAsync(Context);
                    if (result.IsSuccess)
                        description += $"{_config["prefix"]}{cmd.Aliases.First()}\n";
                }
                
                if (!string.IsNullOrWhiteSpace(description)){
                    builder.AddField(x =>{
                        x.Name = module.Name;
                        x.Value = description;
                        x.IsInline = false;
                    });
                }
            }

            await ReplyAsync("", false, builder.Build());
        }

    }
}
