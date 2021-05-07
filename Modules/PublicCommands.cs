using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Discord;
using Discord.Commands;
using TreeTrunk.DataObjects;


namespace TreeTrunk.Modules{
    public class PublicCommands : ModuleBase<SocketCommandContext>{

        //list of emojis A-J
        private readonly string[] emojisA_J = {
            "\U0001F1E6","\U0001F1E7","\U0001F1E8","\U0001F1E9","\U0001F1EA",
            "\U0001F1EB","\U0001F1EC","\U0001F1ED","\U0001F1EE","\U0001F1EF",
            "\U0001F1F0","\U0001F1F1","\U0001F1F2","\U0001F1F3","\U0001F1F4",
            "\U0001F1F5","\U0001F1F6","\U0001F1F7","\U0001F1F8","\U0001F1F9"};

        [Command("ping")]
        [Summary("pong!")]
        [Alias("pong", "hello")]
        public Task PingAsync()
            => ReplyAsync("pong!");

        // [Remainder] takes the rest of the command's arguments as one argument, rather than splitting every space
        [Command("echo")]
        [Summary("Echo's your message.")]
        public Task EchoAsync([Remainder] string text)
            // Insert a ZWSP before the text to prevent triggering other bots!
            => ReplyAsync('\u200B' + text);
        
        [Command("poll")]
        [Summary("Can have 2-20 options. How to use: \"poll <title> <option1> <option2> <option3>...\"")]
        public Task PollAsync(params string[] objects){
            var m = Context.Message;
            var reactions = new List<IEmote>();
            m.DeleteAsync();

            if(objects.Length > 21 || objects.Length < 3){
                ReplyAsync(":bar_chart: Polls can only have 2-20 options",false);
                return Task.CompletedTask;
            }          
            
            string desc = "";
            for(int i = 1; i < objects.Length; i++){
                desc +=  emojisA_J[i-1] + " " + objects[i] + "\n";
                reactions.Add(new Emoji(emojisA_J[i-1]));
            }
            

            var builder = new EmbedBuilder(){
                Title = objects[0],
                Color = Color.DarkGreen,
                Description = desc
            };
            //bar_chart emoji
            var message = ReplyAsync("\U0001F4CA Poll!",false,builder.Build()).Result;
            message.AddReactionsAsync(reactions.ToArray());
            return Task.CompletedTask;
        }


        [Command("nextrank")]
        [Alias("nr")]
        [Summary("replys with percentage till next rank")]
        public Task NextRank(){
            var m = Context.Message;
            
            var ar = StaticFunctions.data[Context.Guild.Id].usermanager[m.Author.Id].activityrating;
            var ranks = new List<int>{
                StaticFunctions.data[Context.Guild.Id].armin,
                StaticFunctions.data[Context.Guild.Id].bronze_ar,
                StaticFunctions.data[Context.Guild.Id].silver_ar,
                StaticFunctions.data[Context.Guild.Id].gold_ar,
                StaticFunctions.data[Context.Guild.Id].plat_ar,
                StaticFunctions.data[Context.Guild.Id].diamond_ar,
                StaticFunctions.data[Context.Guild.Id].master_ar,
                StaticFunctions.data[Context.Guild.Id].gm_ar};

            for(int i = 1; i < ranks.Count; i++){
                if(ar <= ranks[i]){
                    var percent = 100 - (((ranks[i]-ar)*100) / (ranks[i] - ranks[i-1]));
                    ReplyAsync("**" + m.Author.Username.ToString() + "** you are " + percent.ToString() + "% in your current rank.",false);
                    break;
                }
            }
            return Task.CompletedTask;
        }

        // [Command("top10")]
        // [Summary("Displays the top10 ranked users.")]
        // public Task TopTen(){
        //     var m = Context.Message;
            
        //     var memberlist = StaticFunctions.data[Context.Guild.Id].usermanager;

        //     string desc = "";
        //     var members = new List<Profile>();
        //     foreach(var member in memberlist){
        //         var currentAR = member.Value.activityrating;
        //         for(var i = 0; i < members.Count ; i++ ){
        //             if(currentAR >= members[i].activityrating){
        //                 members.Insert(i,member.Value);
        //                 break;
        //             }
        //             else if(currentAR){

        //             }
        //         }
        //         if(members.Count >= 10){
        //             break;
        //         }
        //     }
            
            
            
        //     for(int i = 1; i < members.Length; i++){
        //         desc +=  emojisA_J[i-1] + " " + objects[i] + "\n";
        //         reactions.Add(new Emoji(emojisA_J[i-1]));
        //     }
            

        //     var builder = new EmbedBuilder(){
        //         Title = objects[0],
        //         Color = Color.DarkGreen,
        //         Description = desc
        //     };
        //     //bar_chart emoji
        //     var message = ReplyAsync("\U0001F4CA Poll!",false,builder.Build()).Result;


        //     return Task.CompletedTask;
        // }
    }
}