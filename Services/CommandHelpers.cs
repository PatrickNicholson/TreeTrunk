using System;
using System.Threading.Tasks;
using Discord;
using Discord.Net;
using Discord.WebSocket;
using Discord.Commands;
using System.Linq;

namespace TreeTrunk.Services{
    public partial class CommandHandler{

        private Task messagehandler(SocketMessage rawMessage, SocketCommandContext context){
            //Console.WriteLine(DateTime.Now.ToString() + ": A message was collected.");
            int points = 0;
            var len = rawMessage.Content.ToString().Length;
            var embed = rawMessage.Embeds.Count;
            var attach = rawMessage.Attachments.Count;
            TimeSpan threshold = DateTime.Now - StaticFunctions.data[context.Guild.Id].usermanager[rawMessage.Author.Id].last_message;
            
            if(TimeSpan.Compare(threshold,StaticFunctions.data[context.Guild.Id].textspamlimit) >= 0){
                
                if(embed > 0) points += StaticFunctions.data[context.Guild.Id].embeds;
                else if(attach > 0) points += StaticFunctions.data[context.Guild.Id].attach;
                else if(len >= 3 && len <= 150) points += StaticFunctions.data[context.Guild.Id].mshort;
                else if(len >= 151) points += StaticFunctions.data[context.Guild.Id].mlong;
                
                StaticFunctions.data[context.Guild.Id].usermanager[rawMessage.Author.Id].points_earned += points;
            }

            if(StaticFunctions.data[context.Guild.Id].channelmessages.ContainsKey(context.Channel.Id)){
                StaticFunctions.data[context.Guild.Id].channelmessages[context.Channel.Id]++;
                StaticFunctions.data[context.Guild.Id].usermanager[context.User.Id].messagetotal ++;
            }
            else{
                StaticFunctions.data[context.Guild.Id].channelmessages.Add(context.Channel.Id, 1);
                StaticFunctions.data[context.Guild.Id].usermanager[context.User.Id].messagetotal ++;
            }
            StaticFunctions.data[context.Guild.Id].usermanager[rawMessage.Author.Id].last_message = DateTime.Now;
            return Task.CompletedTask;
        }
       
        private Task SelfMessageHandler(SocketMessage rawMessage, SocketCommandContext context){ 
            
            if(rawMessage.Channel.Id != StaticFunctions.data[context.Guild.Id].starboard) return Task.CompletedTask;
            ulong referenceId = Convert.ToUInt64(rawMessage.Embeds.First().Footer.GetValueOrDefault().Text);
            
            if(!StaticFunctions.data[context.Guild.Id].starboardmessages.ContainsKey(referenceId)){
                StaticFunctions.data[context.Guild.Id].starboardmessages.Add(referenceId, rawMessage.Id);
            }
            return Task.CompletedTask;
        }
    }
}