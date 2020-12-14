using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Discord;
using Discord.Commands;


namespace TreeTrunk.Modules{
    public class PublicCommands : ModuleBase<SocketCommandContext>{

        private List<IEmote> reactions = new List<IEmote>();
        private readonly string[] emojisA_J = {
            "\U0001F1E6","\U0001F1E7","\U0001F1E8","\U0001F1E9","\U0001F1EA",
            "\U0001F1EB","\U0001F1EC","\U0001F1ED","\U0001F1EE","\U0001F1EF",
            "\U0001F1F0","\U0001F1F1","\U0001F1F2","\U0001F1F3","\U0001F1F4",
            "\U0001F1F5","\U0001F1F6","\U0001F1F7","\U0001F1F8","\U0001F1F9"};
        

        [Command("ping")]
        [Alias("pong", "hello")]
        public Task PingAsync()
            => ReplyAsync("pong!");

        // [Remainder] takes the rest of the command's arguments as one argument, rather than splitting every space
        [Command("echo")]
        public Task EchoAsync([Remainder] string text)
            // Insert a ZWSP before the text to prevent triggering other bots!
            => ReplyAsync('\u200B' + text);
        
        [Command("poll")]
        public async Task PollAsync(params string[] objects){
            var m = Context.Message;
            await m.DeleteAsync();
            if(objects.Length > 21 || objects.Length < 3){
                await ReplyAsync(":bar_chart: Polls can only have 2-20 options",false);
                return;
            }
            var builder = new EmbedBuilder();
            builder.WithTitle(objects[0]);
            builder.WithColor(Color.DarkGreen);
            string desc = "";
            for(int i = 1; i < objects.Length; i++){
                desc +=  emojisA_J[i-1] + " " + objects[i] + "\n";
                reactions.Add(new Emoji(emojisA_J[i-1]));
            }
            
            builder.WithDescription(desc);
            var message = await ReplyAsync(":bar_chart: Poll!",false,builder.Build());
            await Console.Out.WriteLineAsync(reactions.ToString());
            await message.AddReactionsAsync(reactions.ToArray());
            
            reactions.Clear();
            
        }
    }
}