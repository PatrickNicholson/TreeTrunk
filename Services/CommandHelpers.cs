using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Discord.Commands;

namespace TreeTrunk.Services{
    public partial class CommandHandler{

        private Task messagehandler(SocketMessage rawMessage, SocketCommandContext context){
            Console.WriteLine(DateTime.Now.ToString() + ": A message was collected.");
            int points = 0;
            var len = rawMessage.Content.ToString().Length;
            var embed = rawMessage.Embeds.Count;
            var attach = rawMessage.Attachments.Count;

            if(len >= 3 && len <= 150) points += StaticFunctions.data[context.Guild.Id].mshort; 
            else if(len >= 151) points += StaticFunctions.data[context.Guild.Id].mlong;
            if(embed > 0) points += StaticFunctions.data[context.Guild.Id].embeds;
            if(attach > 0) points += StaticFunctions.data[context.Guild.Id].attach;
            
            StaticFunctions.data[context.Guild.Id].usermanager[rawMessage.Author.Id].points_earned += points;
            if(StaticFunctions.data[context.Guild.Id].channelmessages.ContainsKey(context.Channel.Id)){
                StaticFunctions.data[context.Guild.Id].channelmessages[context.Channel.Id]++;
                StaticFunctions.data[context.Guild.Id].usermanager[context.User.Id].messagetotal ++;
            }
            else{
                StaticFunctions.data[context.Guild.Id].channelmessages.Add(context.Channel.Id, 1);
                StaticFunctions.data[context.Guild.Id].usermanager[context.User.Id].messagetotal ++;
            }

            return Task.CompletedTask;
        }
       
    }
}