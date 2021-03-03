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
/*
        [Command("help")]
        [Alias("h")]
        public Task HelpAsync(string command)
        {
            var result = _commands.Search(Context, command);

            if (!result.IsSuccess){
                ReplyAsync($"Sorry, I couldn't find a command like **{command}**.");
                return Task.CompletedTask;
            }
            var builder = new EmbedBuilder(){
                Color = Color.DarkGreen,
                Description = $"Here are some commands like **{command}**"
            };

            foreach (var match in result.Commands){
                var cmd = match.Command;

                builder.AddField(x =>
                {
                    x.Name = string.Join(", ", cmd.Aliases);
                    x.Value = $"Parameters: {string.Join(", ", cmd.Parameters.Select(p => p.Name))}\n" + 
                              $"Summary: {cmd.Summary}";
                    x.IsInline = false;
                });
            }

            ReplyAsync("", false, builder.Build());
            return Task.CompletedTask;
        }
*/
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
