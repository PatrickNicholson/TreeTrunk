using System;
using Discord;
using Discord.Commands;
using System.Linq;
using System.Threading.Tasks;



namespace TreeTrunk.Modules{
    public class HelpCommands : ModuleBase<SocketCommandContext>{
        private readonly CommandService _commands;
        private readonly IServiceProvider _services;
        private string prefix = "!"; //string prefix = _config["prefix"];

        public HelpCommands(CommandService commands, IServiceProvider services)
        {
            _commands = commands;
            _services = services;
        }
        //private readonly CommandService _commands;
        [Command("help")]
        [Alias("h")]
        public async Task HelpAsync(){
            var builder = new EmbedBuilder(){
                Color = new Color(114, 137, 218),
                Description = "These are the commands you can use"
            };
            
            foreach (var module in _commands.Modules){
                if(module.Name == "AdminCommands" || module.Name == "HelpCommands") continue;
                string description = null;
                foreach (var cmd in module.Commands){
                    var result = await cmd.CheckPreconditionsAsync(Context);
                    if (result.IsSuccess)
                        description += $"{prefix}{cmd.Aliases.First()}\n";
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

        [Command("help")]
        [Alias("h")]
        public async Task HelpAsync(string command)
        {
            var result = _commands.Search(Context, command);

            if (!result.IsSuccess){
                await ReplyAsync($"Sorry, I couldn't find a command like **{command}**.");
                return;
            }
            var builder = new EmbedBuilder(){
                Color = new Color(114, 137, 218),
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

            await ReplyAsync("", false, builder.Build());
        }

        [Command("ahelp")]
        [Alias("ah")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task AdminHelpAsync(){
            var builder = new EmbedBuilder(){
                Color = new Color(114, 137, 218),
                Description = "These are the commands you can use"
            };
            
            foreach (var module in _commands.Modules){
                string description = null;
                foreach (var cmd in module.Commands){
                    var result = await cmd.CheckPreconditionsAsync(Context);
                    if (result.IsSuccess)
                        description += $"{prefix}{cmd.Aliases.First()}\n";
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
