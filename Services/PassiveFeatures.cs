using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using TreeTrunk.DataObjects;
using System.Linq;

namespace TreeTrunk.Services{
    public partial class CommandHandler{
        
        private Task ReactionAddAsync(Cacheable<IUserMessage, ulong> cachedMessage, ISocketMessageChannel messageChannel, SocketReaction reaction){
            var message = cachedMessage.GetOrDownloadAsync().Result;
            if(message.Author.IsBot) return Task.CompletedTask;
            int reacts = 0;
            Emote emoji = null;
            var guild = (message.Author as SocketGuildUser).Guild;
            ulong guildid = (message.Author as SocketGuildUser).Guild.Id;
            var starboardid = StaticFunctions.data[guildid].starboard;
            string emotename = StaticFunctions.data[guildid].starboarddefault;

            foreach(var emote in message.Reactions){
                if(emote.Value.ReactionCount >= reacts && emote.Key.Name == reaction.Emote.Name){
                    reacts = emote.Value.ReactionCount;
                    emoji = emote.Key as Emote;
                }
            }
            
            if(guild.Emotes.Contains(emoji)){
                emotename = "<:" + emoji.Name.ToString() + ":" + emoji.Id.ToString() + ">";
            }

            
            if(reacts >= StaticFunctions.data[guildid].reactbuff_limit){
                StaticFunctions.data[guildid].usermanager[message.Author.Id].points_earned += StaticFunctions.data[guildid].reactbuff;
            }
            
            
            if(reacts >= StaticFunctions.data[guildid].starboardlimit && !StaticFunctions.data[guildid].starboardmessages.ContainsKey(message.Id)){
                string imurl = null;
                if(message.Attachments.Count != 0){
                    imurl = message.Attachments.First().ProxyUrl;
                }
                var builder = new EmbedBuilder(){
                    Author = new EmbedAuthorBuilder(){
                                Name = message.Author.Username,
                                IconUrl = message.Author.GetAvatarUrl()
                            },
                    Color = Color.DarkGreen,
                    Description = message.Content,
                    ImageUrl = imurl,
                    Timestamp = message.Timestamp,
                    Footer = new EmbedFooterBuilder(){
                                Text = message.Id.ToString()
                            }
                }.AddField("**Source**","[Jump!]("+message.GetJumpUrl()+")").Build();
                guild.GetTextChannel(starboardid).SendMessageAsync(emotename + " **"+ reacts.ToString() +"** <#"+message.Channel.Id.ToString()+">",false,builder);
                
            }
            else if(reacts >= StaticFunctions.data[guildid].starboardlimit && StaticFunctions.data[guildid].starboardmessages.ContainsKey(message.Id)){
                var starmess = guild.GetTextChannel(starboardid).GetMessageAsync(StaticFunctions.data[guildid].starboardmessages[message.Id]).Result as IUserMessage;
                starmess.ModifyAsync(x => x.Content = emotename + " **"+ reacts.ToString() +"** <#"+message.Channel.Id.ToString()+">");
            }

            return Task.CompletedTask;
        }

        private Task ReactionRemoveAsync(Cacheable<IUserMessage, ulong> cachedMessage, ISocketMessageChannel messageChannel, SocketReaction reaction){
            var message = cachedMessage.GetOrDownloadAsync().Result;
            if(message.Author.IsBot) return Task.CompletedTask;
            var reacts = 0;
            Emote emoji = null;
            var guild = (message.Author as SocketGuildUser).Guild;
            ulong guildid = (message.Author as SocketGuildUser).Guild.Id;
            var starboardid = StaticFunctions.data[guildid].starboard;
            string emotename = StaticFunctions.data[guildid].starboarddefault;

            foreach(var emote in message.Reactions){
                if(emote.Value.ReactionCount >= reacts && emote.Key.Name != reaction.Emote.Name){
                    reacts = emote.Value.ReactionCount;
                    emoji = emote.Key as Emote;
                }
            }

            if(guild.Emotes.Contains(emoji)){
                emotename = "<:" + emoji.Name.ToString() + ":" + emoji.Id.ToString() + ">";
            }
            
            if(reacts < StaticFunctions.data[guildid].reactbuff_limit){
                StaticFunctions.data[guildid].usermanager[message.Author.Id].points_earned -= StaticFunctions.data[guildid].reactbuff;
            }
            
            
            if(reacts < StaticFunctions.data[guildid].starboardlimit && StaticFunctions.data[guildid].starboardmessages.ContainsKey(message.Id)){            
                guild.GetTextChannel(starboardid).DeleteMessageAsync(StaticFunctions.data[guildid].starboardmessages[message.Id]);
                StaticFunctions.data[guildid].starboardmessages.Remove(message.Id);
            }
            else if(reacts >= StaticFunctions.data[guildid].starboardlimit && StaticFunctions.data[guildid].starboardmessages.ContainsKey(message.Id)){
                var starmess = guild.GetTextChannel(starboardid).GetMessageAsync(StaticFunctions.data[guildid].starboardmessages[message.Id]).Result as IUserMessage;
                starmess.ModifyAsync(x => x.Content = emotename + " **"+ reacts.ToString() +"** <#"+message.Channel.Id.ToString()+">");
            }
            
            return Task.CompletedTask;
        }

        private Task ActivityAsync(SocketGuildUser initial, SocketGuildUser final){           
            if(initial.IsBot || final.IsBot) return Task.CompletedTask;

            var roles = final.Roles.ToList();

            ActivityType initialAct = ActivityType.CustomStatus;
            ActivityType initialGame = ActivityType.CustomStatus;
            foreach(var activity in initial.Activities){
                if(activity.Type == ActivityType.Streaming){
                    initialAct = ActivityType.Streaming;
                }
                else if(activity.Type == ActivityType.Playing){
                    initialGame = ActivityType.Playing;
                }
            }
            ActivityType finalAct = ActivityType.CustomStatus;
            ActivityType finalGame = ActivityType.CustomStatus;
            string gamename = "";
            string service = "";
            string url = "";
            foreach(var activity in final.Activities){
                if(activity.Type == ActivityType.Streaming){
                    finalAct = ActivityType.Streaming;
                    StreamingGame activitytemp = activity as StreamingGame;
                    service = activitytemp.Name;
                    url = activitytemp.Url;                    
                }
                else if(activity.Type == ActivityType.Playing){
                    finalGame = ActivityType.Playing;
                    gamename = activity.Name;
                }
            }
            
            if(initialGame == ActivityType.CustomStatus && finalGame == ActivityType.Playing){
                if(StaticFunctions.data[final.Guild.Id].usermanager[final.Id].activityrating > StaticFunctions.data[final.Guild.Id].gameactivitylimit){
                    if(StaticFunctions.data[final.Guild.Id].gameactivity.ContainsKey(gamename)){
                        StaticFunctions.data[final.Guild.Id].gameactivity[gamename]++;
                    }
                    else{
                        StaticFunctions.data[final.Guild.Id].gameactivity.Add(gamename, 1);
                    }
                }
            }



            ulong id = initial.Guild.Id;
            var streamerrole = initial.Guild.GetRole(StaticFunctions.data[id].streamrole);
            
            if(streamerrole != null && (initialAct == ActivityType.Streaming || finalAct == ActivityType.Streaming)){
                if(initialAct != ActivityType.Streaming && finalAct == ActivityType.Streaming && !roles.Contains(streamerrole)){
                    //Console.WriteLine(DateTime.Now.ToString() + ": " + final.Username.ToString() + " started streaming.");
                    final.AddRoleAsync(streamerrole);
                    
                    ulong adchannel = StaticFunctions.data[final.Guild.Id].adchat;    
                    var chnl =  final.Guild.GetChannel(adchannel) as IMessageChannel;
                    chnl.SendMessageAsync("**" + final.Username.ToString() + "** is currently streaming on " + service + "! Check it out: " + url);
                }
                else if(initialAct == ActivityType.Streaming && finalAct != ActivityType.Streaming && roles.Contains(streamerrole)){
                    //Console.WriteLine(DateTime.Now.ToString() + ": " + initial.Username.ToString() + " stopped streaming.");
                    final.RemoveRoleAsync(streamerrole);
                }
            }
            
            return Task.CompletedTask;
        }

        private Task UserJoinAsync(SocketGuildUser user){
            if(user.IsBot) return Task.CompletedTask;
            //Console.WriteLine(DateTime.Now.ToString() + ": " + user.Username.ToString() + " joined the guild.");
            ulong defaultrole = StaticFunctions.data[user.Guild.Id].unranked;
            StaticFunctions.data[user.Guild.Id].usermanager.TryAdd(user.Id,new Profile(user.Id,user.Username));
            user.AddRoleAsync(user.Guild.GetRole(defaultrole));
            return Task.CompletedTask;
        }

        private Task UserLeftAsync(SocketGuildUser user){
            if(user.IsBot) return Task.CompletedTask;
            //Console.WriteLine(DateTime.Now.ToString() + ": " + user.Username.ToString() + " left the guild.");
            ulong modchannel = StaticFunctions.data[user.Guild.Id].modchat;    
            var chnl =  user.Guild.GetChannel(modchannel) as IMessageChannel;
            chnl.SendMessageAsync(user.Username.ToString() + " has left the discord.");
            return Task.CompletedTask;
        }

        private Task UserVoiceStateUpdatedAsync(SocketUser user, SocketVoiceState initial, SocketVoiceState final){
            if(user.IsBot) return Task.CompletedTask;
            var before = initial.VoiceChannel;
            var after = final.VoiceChannel;
            
            
            if(before == null && after != null){
                VoiceStart(after.Guild.Id, after.Guild.AFKChannel.Id,final,user,final.IsStreaming);
                StaticFunctions.data[after.Guild.Id].usermanager[user.Id].total_voiceminute_marker = DateTime.Now;

                if(StaticFunctions.data[after.Guild.Id].vcroles.Contains(after.Id)){
                    var roles = after.Guild.Roles;
                    bool found = false;
                    foreach(var role in roles){
                        if(role.Name.Contains(after.Name)){
                            after.Guild.GetUser(user.Id).AddRoleAsync(role);
                            found = true;
                            break;
                        }
                    }
                    if(!found){
                        after.Guild.CreateRoleAsync(after.Name, null, null,false,null);   
                        for(int i = 0; i < 5; i++){
                            roles = after.Guild.Roles;
                            bool gotit = false;
                            foreach(var role in roles){
                                if(role.Name.Contains(after.Name)){
                                    after.Guild.GetUser(user.Id).AddRoleAsync(role);
                                    gotit = true;
                                    break;
                                }
                            }
                            if(gotit){
                                break;
                            }
                        }
                    }
                }
            }
            else if(before != null && after != null && before != after){
                //Console.WriteLine(DateTime.Now.ToString() + ": " + user.Username.ToString() + " moved voicechats.");
                VoiceEnd(before.Guild.Id,before,user,initial.IsStreaming);
                VoiceStart(after.Guild.Id,after.Guild.AFKChannel.Id,final,user,final.IsStreaming);

                DateTime currenttime = DateTime.Now;
                DateTime userstart = StaticFunctions.data[after.Guild.Id].usermanager[user.Id].total_voiceminute_marker;
                if(userstart != DateTime.MinValue){
                    if(StaticFunctions.data[after.Guild.Id].voicechatminutes.ContainsKey(initial.VoiceChannel.Id)){
                        StaticFunctions.data[after.Guild.Id].voicechatminutes[initial.VoiceChannel.Id] += Convert.ToUInt64((currenttime - userstart).TotalMinutes);
                    }
                    else{
                        StaticFunctions.data[after.Guild.Id].voicechatminutes.Add(initial.VoiceChannel.Id, Convert.ToUInt64((currenttime - userstart).TotalMinutes));
                    }
                }
                StaticFunctions.data[after.Guild.Id].usermanager[user.Id].total_voiceminute_marker = DateTime.Now;

                
                if(StaticFunctions.data[before.Guild.Id].vcroles.Contains(before.Id)){
                    var roles = before.Guild.Roles;
                    SocketRole roll = null;
                    foreach(var role in roles){
                        if(role.Name.Contains(before.Name)){
                            roll = role;
                            break;
                        }
                    }
                    if(roll != null){
                        before.Guild.GetUser(user.Id).RemoveRoleAsync(roll);
                    }
                }
                if(StaticFunctions.data[after.Guild.Id].vcroles.Contains(after.Id)){
                    var roles = after.Guild.Roles;
                    bool found = false;
                    foreach(var role in roles){
                        if(role.Name.Contains(after.Name)){
                            after.Guild.GetUser(user.Id).AddRoleAsync(role);
                            found = true;
                            break;
                        }
                    }
                    if(!found){
                        after.Guild.CreateRoleAsync(after.Name, null, null,false,null);   
                        for(int i = 0; i < 5; i++){
                            roles = after.Guild.Roles;
                            bool gotit = false;
                            foreach(var role in roles){
                                if(role.Name.Contains(after.Name)){
                                    
                                    after.Guild.GetUser(user.Id).AddRoleAsync(role);
                                    gotit = true;
                                    break;
                                }
                            }
                            if(gotit){
                                break;
                            }
                        }
                    }
                }

            }
            else if(before != null && after != null && before == after){
                if(!initial.IsStreaming && final.IsStreaming){
                    //Console.WriteLine(DateTime.Now.ToString() + ": " + user.Username.ToString() + " started sharing screens.");
                    if((!final.IsSelfDeafened && !final.IsSelfMuted)){
                        DateTime currenttime = DateTime.Now;
                        DateTime voice_start = StaticFunctions.data[before.Guild.Id].usermanager[user.Id].voice_start;
                        StaticFunctions.data[after.Guild.Id].usermanager[user.Id].share_start = currenttime;
                        if(voice_start != DateTime.MinValue){
                            double pointsregular = (StaticFunctions.data[before.Guild.Id].vactive) / 60.0d;
                            StaticFunctions.data[before.Guild.Id].usermanager[user.Id].points_earned += Convert.ToInt32((currenttime - voice_start).TotalMinutes *pointsregular);
                            //Console.WriteLine("Total Points earned regular: " + StaticFunctions.data[before.Guild.Id].usermanager[user.Id].points_earned.ToString());
                            StaticFunctions.data[before.Guild.Id].usermanager[user.Id].voice_start = DateTime.MinValue;
                        }
                    }
                }
                else if(initial.IsStreaming && !final.IsStreaming){
                    //Console.WriteLine(DateTime.Now.ToString() + ": " + user.Username.ToString() + " stopped sharing screens.");
                    if((!final.IsSelfDeafened && !final.IsSelfMuted)){
                        DateTime currenttime = DateTime.Now;
                        DateTime share_start = StaticFunctions.data[before.Guild.Id].usermanager[user.Id].share_start; 
                        StaticFunctions.data[after.Guild.Id].usermanager[user.Id].voice_start = currenttime;
                        if(share_start != DateTime.MinValue){
                            double pointsshare = (StaticFunctions.data[before.Guild.Id].vstream) / 60.0d;
                            StaticFunctions.data[before.Guild.Id].usermanager[user.Id].points_earned += Convert.ToInt32((currenttime - share_start).TotalMinutes *pointsshare);
                            //Console.WriteLine("Total Points earned sharing: " + StaticFunctions.data[before.Guild.Id].usermanager[user.Id].points_earned.ToString());
                            StaticFunctions.data[before.Guild.Id].usermanager[user.Id].share_start = DateTime.MinValue;
                        }
                    }
                }
                else if((!initial.IsSelfDeafened && final.IsSelfDeafened) || (!initial.IsSelfMuted && final.IsSelfMuted)){
                    //Console.WriteLine(DateTime.Now.ToString() + ": " + user.Username.ToString() + " muted or deafened.");
                    VoiceEnd(before.Guild.Id, before,user,initial.IsStreaming);
                }
                else if((initial.IsSelfDeafened && !final.IsSelfDeafened) || (initial.IsSelfMuted && !final.IsSelfMuted)){
                    //Console.WriteLine(DateTime.Now.ToString() + ": " + user.Username.ToString() + " unmuted or undeafened.");
                    VoiceStart(before.Guild.Id,after.Guild.AFKChannel.Id, final,user,final.IsStreaming);
                }
            }
            else if(before != null && after == null){
                //Console.WriteLine(DateTime.Now.ToString() + ": " + user.Username.ToString() + " left voice.");
                VoiceEnd(before.Guild.Id,before,user,initial.IsStreaming);

                DateTime currenttime = DateTime.Now;
                DateTime userstart = StaticFunctions.data[before.Guild.Id].usermanager[user.Id].total_voiceminute_marker;
                if(userstart != DateTime.MinValue){
                    if(StaticFunctions.data[before.Guild.Id].voicechatminutes.ContainsKey(initial.VoiceChannel.Id)){
                        StaticFunctions.data[before.Guild.Id].voicechatminutes[initial.VoiceChannel.Id] += Convert.ToUInt64((currenttime - userstart).TotalMinutes);
                        StaticFunctions.data[before.Guild.Id].usermanager[user.Id].voiceminutetotal += Convert.ToUInt64((currenttime - userstart).TotalMinutes);
                    }
                    else{
                        StaticFunctions.data[before.Guild.Id].voicechatminutes.Add(initial.VoiceChannel.Id, Convert.ToUInt64((currenttime - userstart).TotalMinutes));
                        StaticFunctions.data[before.Guild.Id].usermanager[user.Id].voiceminutetotal += Convert.ToUInt64((currenttime - userstart).TotalMinutes);
                    }
                }
                StaticFunctions.data[before.Guild.Id].usermanager[user.Id].total_voiceminute_marker = DateTime.MinValue;
                
                if(StaticFunctions.data[before.Guild.Id].vcroles.Contains(before.Id)){
                    var roles = before.Guild.Roles;
                    SocketRole roll = null;
                    foreach(var role in roles){
                        if(role.Name.Contains(before.Name)){
                            roll = role;
                            break;
                        }
                    }
                    if(roll != null){
                        before.Guild.GetUser(user.Id).RemoveRoleAsync(roll);
                    }
                }
            }
            
            
            return Task.CompletedTask;
            
        }

        private void VoiceEnd(ulong guild, SocketVoiceChannel channel, SocketUser user, bool isSharing){
            
            DateTime currenttime = DateTime.Now;
            double pointsregular = (StaticFunctions.data[guild].vactive) / 60.0d;
            double pointsshare = (StaticFunctions.data[guild].vstream) / 60.0d;


            DateTime voicetype = StaticFunctions.data[guild].usermanager[user.Id].voice_start;
            if(isSharing){
                voicetype = StaticFunctions.data[guild].usermanager[user.Id].share_start;
            }
            
            var bots = 0;
            foreach(var users in channel.Users){
                if(users.IsBot || users.IsSelfDeafened || users.IsSelfMuted){
                    bots++;
                }
            }
            if(channel.Users.Count - bots == 2){
                foreach(var users in channel.Users){               
                    if(user.Id != users.Id && users.IsStreaming && !users.IsSelfDeafened && !users.IsSelfMuted && !users.IsBot){
                        DateTime other_share_start = StaticFunctions.data[guild].usermanager[users.Id].share_start;
                        if(other_share_start != DateTime.MinValue){
                            StaticFunctions.data[guild].usermanager[users.Id].points_earned += Convert.ToInt32((currenttime - other_share_start).TotalMinutes *pointsshare);
                            StaticFunctions.data[guild].usermanager[users.Id].share_start = DateTime.MinValue;
                        }
                    }
                    else if(user.Id != users.Id && !users.IsStreaming && !users.IsSelfDeafened && !users.IsSelfMuted && !users.IsBot){
                        DateTime other_voice_start = StaticFunctions.data[guild].usermanager[users.Id].voice_start;
                        if(other_voice_start != DateTime.MinValue){
                            StaticFunctions.data[guild].usermanager[users.Id].points_earned += Convert.ToInt32((currenttime - other_voice_start).TotalMinutes *pointsregular);
                            StaticFunctions.data[guild].usermanager[users.Id].voice_start = DateTime.MinValue;
                        }
                    }
                }
            }
            
            if(voicetype != DateTime.MinValue){
                if(isSharing){
                    StaticFunctions.data[guild].usermanager[user.Id].points_earned += Convert.ToInt32((currenttime - voicetype).TotalMinutes *pointsshare);
                    //Console.WriteLine("Total Points earned sharing: " + StaticFunctions.data[guild].usermanager[user.Id].points_earned.ToString());
                }
                else{
                    StaticFunctions.data[guild].usermanager[user.Id].points_earned += Convert.ToInt32((currenttime - voicetype).TotalMinutes *pointsregular);
                    //Console.WriteLine("Total Points earned regular: " + StaticFunctions.data[guild].usermanager[user.Id].points_earned.ToString());
                }

            }
            
            StaticFunctions.data[guild].usermanager[user.Id].voice_start = DateTime.MinValue;
            StaticFunctions.data[guild].usermanager[user.Id].share_start = DateTime.MinValue;
        }

        private void VoiceStart(ulong guild, ulong afk, SocketVoiceState state, SocketUser user, bool isSharing){
            DateTime voicetype = StaticFunctions.data[guild].usermanager[user.Id].voice_start;
            DateTime currenttime = DateTime.Now;
            var channel = state.VoiceChannel;
            if(isSharing){
                voicetype = StaticFunctions.data[guild].usermanager[user.Id].share_start;
            }
            var bots = 0;
            foreach(var users in channel.Users){
                if(users.IsBot || users.IsSelfDeafened || users.IsSelfMuted){
                    bots++;
                }
            }
            if(channel.Id != afk && channel.Users.Count - bots > 1 && !state.IsSelfDeafened && !state.IsSelfMuted){
                if(channel.Users.Count - bots == 2){
                    foreach(var users in channel.Users){
                        if(users.IsStreaming && !users.IsSelfDeafened && !users.IsSelfMuted && !users.IsBot){
                            StaticFunctions.data[guild].usermanager[users.Id].share_start = currenttime;
                        }
                        else if(!users.IsStreaming && !users.IsSelfDeafened && !users.IsSelfMuted && !users.IsBot){
                            StaticFunctions.data[guild].usermanager[users.Id].voice_start = currenttime;
                        }
                    }
                }
                else if(channel.Users.Count - bots > 2){
                    StaticFunctions.data[guild].usermanager[user.Id].voice_start = currenttime;
                }
            }
            else{
                StaticFunctions.data[guild].usermanager[user.Id].share_start = DateTime.MinValue;
                StaticFunctions.data[guild].usermanager[user.Id].voice_start = DateTime.MinValue;
            }
        }

    }
}