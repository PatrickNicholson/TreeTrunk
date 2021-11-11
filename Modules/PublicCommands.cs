using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Discord;
using Discord.WebSocket;
using Discord.Commands;
using TreeTrunk.DataObjects;


namespace TreeTrunk.Modules{
    public class PublicCommands : ModuleBase<SocketCommandContext>{

        //list of emojis A-J
        private readonly string[] emojisA_J = {
            "\U0001F1E6","\U0001F1E7","\U0001F1E8","\U0001F1E9","\U0001F1EA",
            "\U0001F1EB","\U0001F1EC","\U0001F1ED","\U0001F1EE","\U0001F1EF",
            "\U0001F1F0","\U0001F1F1","\U0001F1F2","\U0001F1F3","\U0001F1F4",
            "\U0001F1F5","\U0001F1F6","\U0001F1F7","\U0001F1F8","\U0001F1F9"
        };

        [Command("ping")]
        [Summary("pong!")]
        [Alias("pong", "hello")]
        public Task PingAsync() => ReplyAsync("pong!");

        // [Remainder] takes the rest of the command's arguments as one argument, rather than splitting every space
        [Command("echo")]
        [Summary("Echo's your message.")]
        public Task EchoAsync([Remainder] string text) => ReplyAsync('\u200B' + text);
        // Insert a ZWSP before the text to prevent triggering other bots!
        
        [Command("poll")]
        [Summary("Can have 2-20 options. How to use: \"poll <title> <option1> <option2> <option3>...\"")]
        public Task PollAsync(params string[] objects){
            var reactions = new List<IEmote>();

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
        public Task NextRank([Remainder] string text){
            var ar = -1;
            var name = "";

            foreach(var profiles in StaticFunctions.data[Context.Guild.Id].usermanager.Values){
                if(profiles.name.Contains(text,StringComparison.InvariantCultureIgnoreCase)){
                    ar = profiles.activityrating;
                    name = profiles.name;
                }
            }

            if(ar == -1){
                ReplyAsync("\"" + text + "\"" + " does not exist.");
                return Task.CompletedTask;
            }



            var ranks = new List<int>{
                StaticFunctions.data[Context.Guild.Id].armin,
                StaticFunctions.data[Context.Guild.Id].bronze_ar,
                StaticFunctions.data[Context.Guild.Id].silver_ar,
                StaticFunctions.data[Context.Guild.Id].gold_ar,
                StaticFunctions.data[Context.Guild.Id].plat_ar,
                StaticFunctions.data[Context.Guild.Id].diamond_ar,
                StaticFunctions.data[Context.Guild.Id].master_ar,
                StaticFunctions.data[Context.Guild.Id].gm_ar};
            var rank_roles = new List<ulong>{
                StaticFunctions.data[Context.Guild.Id].quickplay,
                StaticFunctions.data[Context.Guild.Id].bronze,
                StaticFunctions.data[Context.Guild.Id].silver,
                StaticFunctions.data[Context.Guild.Id].gold,
                StaticFunctions.data[Context.Guild.Id].plat,
                StaticFunctions.data[Context.Guild.Id].diamond,
                StaticFunctions.data[Context.Guild.Id].master,
                StaticFunctions.data[Context.Guild.Id].gm};

            for(int i = 1; i < ranks.Count; i++){
                if(ar <= ranks[i]){
                    var percent = 100 - (((ranks[i]-ar)*100) / (ranks[i] - ranks[i-1]));
                    var rolename = Context.Guild.GetRole(rank_roles[i]).Name;
                    ReplyAsync("**" + name + "** you are " + percent.ToString() + "% in `@"+ rolename + "`.",false);
                    break;
                }
            }
            return Task.CompletedTask;
        }

        [Command("nextrank")]
        [Alias("nr")]
        [Summary("replys with percentage till next rank")]
        public Task NextRank(){
            var ar = StaticFunctions.data[Context.Guild.Id].usermanager[Context.Message.Author.Id].activityrating;
            var name = Context.Message.Author.Username.ToString();

            var ranks = new List<int>{
                StaticFunctions.data[Context.Guild.Id].armin,
                StaticFunctions.data[Context.Guild.Id].bronze_ar,
                StaticFunctions.data[Context.Guild.Id].silver_ar,
                StaticFunctions.data[Context.Guild.Id].gold_ar,
                StaticFunctions.data[Context.Guild.Id].plat_ar,
                StaticFunctions.data[Context.Guild.Id].diamond_ar,
                StaticFunctions.data[Context.Guild.Id].master_ar,
                StaticFunctions.data[Context.Guild.Id].gm_ar};
            var rank_roles = new List<ulong>{
                StaticFunctions.data[Context.Guild.Id].quickplay,
                StaticFunctions.data[Context.Guild.Id].bronze,
                StaticFunctions.data[Context.Guild.Id].silver,
                StaticFunctions.data[Context.Guild.Id].gold,
                StaticFunctions.data[Context.Guild.Id].plat,
                StaticFunctions.data[Context.Guild.Id].diamond,
                StaticFunctions.data[Context.Guild.Id].master,
                StaticFunctions.data[Context.Guild.Id].gm};

            for(int i = 1; i < ranks.Count; i++){
                if(ar <= ranks[i]){
                    var percent = 100 - (((ranks[i]-ar)*100) / (ranks[i] - ranks[i-1]));
                    var rolename = Context.Guild.GetRole(rank_roles[i]).Name;
                    ReplyAsync("**" + name + "** you are " + percent.ToString() + "% in `@"+ rolename + "`.",false);
                    break;
                }
            }
            return Task.CompletedTask;
        }

        [Command("profilepic")]
        [Alias("pfpic", "av")]
        [Summary("Gets user profile pic")]
        public Task ProfilePic([Remainder] string text){
            
            var members = Context.Guild.Users;
            SocketGuildUser user = null;
            foreach(var member in members){
                if(String.Compare(member.Username, text, true) == 0){
                    user = member;
                }
            }
            if(user == null) ReplyAsync(text + " does not exist in this server.");
            else{
                ReplyAsync("Profile picture of **" + user.Username + "**");
                ReplyAsync(user.GetAvatarUrl());
            }
            


            return Task.CompletedTask;
        }      

        [Command("top10")]
        [Alias("t10")]
        [Summary("Displays the top10 ranked users.")]
        public Task TopTen(){
            
            var memberlist = StaticFunctions.data[Context.Guild.Id].usermanager;
            var ranks = new List<int>{
                StaticFunctions.data[Context.Guild.Id].armin,
                StaticFunctions.data[Context.Guild.Id].bronze_ar,
                StaticFunctions.data[Context.Guild.Id].silver_ar,
                StaticFunctions.data[Context.Guild.Id].gold_ar,
                StaticFunctions.data[Context.Guild.Id].plat_ar,
                StaticFunctions.data[Context.Guild.Id].diamond_ar,
                StaticFunctions.data[Context.Guild.Id].master_ar,
                StaticFunctions.data[Context.Guild.Id].gm_ar};
            var rank_roles = new List<ulong>{
                StaticFunctions.data[Context.Guild.Id].quickplay,
                StaticFunctions.data[Context.Guild.Id].bronze,
                StaticFunctions.data[Context.Guild.Id].silver,
                StaticFunctions.data[Context.Guild.Id].gold,
                StaticFunctions.data[Context.Guild.Id].plat,
                StaticFunctions.data[Context.Guild.Id].diamond,
                StaticFunctions.data[Context.Guild.Id].master,
                StaticFunctions.data[Context.Guild.Id].gm};
            

            string desc = "";
            var members = new List<Profile>();
            
            foreach(var member in memberlist){
                var currentAR = member.Value.activityrating;
                if(members.Count == 0){
                    members.Add(member.Value);
                }
                else{
                    for(int i = 0; i < members.Count ; i++ ){
                        if(currentAR >= members[i].activityrating){
                            members.Insert(i,member.Value);
                            break;
                        }
                        else if(i >= members.Count){
                            members.Add(member.Value);
                        }
                    }
                }
            }

            int length = 10;
            if(members.Count < 10) length = members.Count;
            

            for(int i = 0; i < length; i++){
                String rolename = "";
                String percent = "";
                for(int j = 1; j < ranks.Count; j++){
                    if(members[i].activityrating <= ranks[j]){
                        percent = (100 - (((ranks[j]-members[i].activityrating)*100) / (ranks[j] - ranks[j -1]))).ToString() + "%";
                        rolename = Context.Guild.GetRole(rank_roles[j]).Name;
                        break;
                    }
                }
                desc += "**" + (i+1).ToString() + "):** " + rolename + " **-** " +  percent + " **-** " + members[i].name + "\n";
            }

            var builder = new EmbedBuilder(){
                Title = "Leaderboard",
                Color = Color.DarkGreen,
                Description = desc
            };
            
            var message = ReplyAsync("",false,builder.Build()).Result;


            return Task.CompletedTask;
        }
    }
}