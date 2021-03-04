using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using TreeTrunk.DataObjects;

namespace TreeTrunk.Services{
    public partial class CommandHandler{

        private Task ActivityAsync(SocketGuildUser initial, SocketGuildUser final){           
            if(initial.IsBot || final.IsBot) return Task.CompletedTask;

            var initialAct = initial.Activity == null? ActivityType.CustomStatus : initial.Activity.Type;
            var finalAct = final.Activity == null? ActivityType.CustomStatus : final.Activity.Type;

            ulong id = initial.Guild.Id;
            var streamerrole = StaticFunctions.data[id].streamrole;
            if(initialAct != finalAct){
                if(initialAct != ActivityType.Streaming && finalAct == ActivityType.Streaming){
                    final.AddRoleAsync(initial.Guild.GetRole(streamerrole));
                }
                else if(initialAct == ActivityType.Streaming && finalAct != ActivityType.Streaming){
                    final.RemoveRoleAsync(initial.Guild.GetRole(streamerrole));
                }
                else if(initialAct != ActivityType.Playing && finalAct == ActivityType.Playing){
                    
                }
                else if(initialAct != ActivityType.Playing && finalAct == ActivityType.Playing){
                    
                }
            }
            else{
                if(initialAct == ActivityType.Streaming || finalAct == ActivityType.Streaming){
                    final.AddRoleAsync(initial.Guild.GetRole(streamerrole));
                }
                else if(initialAct != ActivityType.Streaming || finalAct != ActivityType.Streaming){
                    final.RemoveRoleAsync(initial.Guild.GetRole(streamerrole));
                }
            }
            
            return Task.CompletedTask;
        }


        private async Task UserJoinAsync(SocketGuildUser user){
            if(user.IsBot) return;
            //quick fix start
            ulong defaultrole = StaticFunctions.data[user.Guild.Id].unranked;
            StaticFunctions.data[user.Guild.Id].usermanager.TryAdd(user.Id,new Profile(user.Id,user.Username));
            //quick fix end
            await user.AddRoleAsync(user.Guild.GetRole(defaultrole));
            return;
        }

        private async Task UserLeftAsync(SocketGuildUser user){
            if(user.IsBot) return;
            //quick fix start
            ulong modchannel = StaticFunctions.data[user.Guild.Id].modchat;    
            var chnl =  user.Guild.GetChannel(modchannel) as IMessageChannel;
            //quick fix end
            await chnl.SendMessageAsync(user.Username.ToString() + " has left the discord.");
            return;
        }

        private Task UserVoiceStateUpdatedAsync(SocketUser user, SocketVoiceState initial, SocketVoiceState final){
            //points only count if the voice chat has more than 1 person and if they are not self deafened
            //if a user gets sent into the afk channel, subtract the afk timeout from their time. 
            if(user.IsBot) return Task.CompletedTask;
            var before = initial.VoiceChannel;
            var after = final.VoiceChannel;
          
            
            if(before == null && after != null){
                DateTime voice_start = StaticFunctions.data[after.Guild.Id].usermanager[user.Id].voice_start;
                DateTime share_start = StaticFunctions.data[after.Guild.Id].usermanager[user.Id].share_start; 
                var after_bots = 0;
                foreach(var users in after.Users){
                    if(users.IsBot){
                        after_bots++;
                    }
                }
                
                if(after.Id != after.Guild.AFKChannel.Id && after.Users.Count - after_bots > 1 && !final.IsSelfDeafened && !final.IsSelfMuted){
                    if(after.Users.Count - after_bots == 2){
                        foreach(var users in after.Users){
                            if(users.IsStreaming && !users.IsSelfDeafened && !users.IsSelfMuted && !users.IsBot){
                                StaticFunctions.data[after.Guild.Id].usermanager[users.Id].share_start = DateTime.Now;
                            }
                            else if(!users.IsStreaming && !users.IsSelfDeafened && !users.IsSelfMuted && !users.IsBot){
                                StaticFunctions.data[after.Guild.Id].usermanager[users.Id].voice_start = DateTime.Now;
                            }
                        }
                    }
                    else if(after.Users.Count - after_bots > 2){
                        StaticFunctions.data[after.Guild.Id].usermanager[user.Id].voice_start = DateTime.Now;
                    }
                }
            }
            else if(before != null && after != null && before != after){
                DateTime voice_start = StaticFunctions.data[before.Guild.Id].usermanager[user.Id].voice_start;
                DateTime share_start = StaticFunctions.data[before.Guild.Id].usermanager[user.Id].share_start; 
                var after_bots = 0;
                foreach(var users in after.Users){
                    if(users.IsBot){
                        after_bots++;
                    }
                }
                
                if(after.Id != after.Guild.AFKChannel.Id && after.Users.Count - after_bots > 1){
                    if(after.Users.Count - after_bots == 2){
                        foreach(var users in after.Users){
                            if(user.Id != users.Id && users.IsStreaming && !users.IsSelfDeafened && !users.IsSelfMuted && !users.IsBot){
                                StaticFunctions.data[after.Guild.Id].usermanager[users.Id].share_start = DateTime.Now;
                            }
                            else if(user.Id != users.Id && !users.IsStreaming && !users.IsSelfDeafened && !users.IsSelfMuted && !users.IsBot){
                                StaticFunctions.data[after.Guild.Id].usermanager[users.Id].voice_start = DateTime.Now;
                            }
                        }
                    }
                }
                else{
                    DateTime curr = DateTime.Now;
                    if(voice_start != DateTime.MinValue){
                        StaticFunctions.data[before.Guild.Id].usermanager[user.Id].voice_timespan += (curr - voice_start).TotalMinutes;
                        StaticFunctions.data[before.Guild.Id].usermanager[user.Id].voice_start = DateTime.MinValue;
                    }
                    if(share_start != DateTime.MinValue){
                        StaticFunctions.data[before.Guild.Id].usermanager[user.Id].share_timespan += (curr - share_start).TotalMinutes;
                        StaticFunctions.data[before.Guild.Id].usermanager[user.Id].share_start = DateTime.MinValue;
                    }
                }
            }
            else if(before != null && after != null && before == after){
                DateTime voice_start = StaticFunctions.data[before.Guild.Id].usermanager[user.Id].voice_start;
                DateTime share_start = StaticFunctions.data[before.Guild.Id].usermanager[user.Id].share_start; 
                if(!initial.IsStreaming && final.IsStreaming){
                    if((!final.IsSelfDeafened && !final.IsSelfMuted)){
                        StaticFunctions.data[after.Guild.Id].usermanager[user.Id].share_start = DateTime.Now;
                        if(voice_start != DateTime.MinValue){
                            DateTime curr = DateTime.Now;
                            StaticFunctions.data[before.Guild.Id].usermanager[user.Id].voice_timespan += (curr - voice_start).TotalMinutes;
                            StaticFunctions.data[before.Guild.Id].usermanager[user.Id].voice_start = DateTime.MinValue;
                        }
                    }
                }
                else if(initial.IsStreaming && !final.IsStreaming){
                    if((!final.IsSelfDeafened && !final.IsSelfMuted)){
                        StaticFunctions.data[after.Guild.Id].usermanager[user.Id].voice_start = DateTime.Now;
                        if(share_start != DateTime.MinValue){
                            DateTime curr = DateTime.Now;
                            StaticFunctions.data[before.Guild.Id].usermanager[user.Id].share_timespan += (curr - share_start).TotalMinutes;
                            StaticFunctions.data[before.Guild.Id].usermanager[user.Id].share_start = DateTime.MinValue;
                        }
                    }
                }
                else if((!initial.IsSelfDeafened && final.IsSelfDeafened) || (!initial.IsSelfMuted && final.IsSelfMuted)){
                    if(voice_start != DateTime.MinValue){
                        DateTime curr = DateTime.Now;
                        StaticFunctions.data[before.Guild.Id].usermanager[user.Id].voice_timespan += (curr - voice_start).TotalMinutes;
                        StaticFunctions.data[before.Guild.Id].usermanager[user.Id].voice_start = DateTime.MinValue;
                    }
                    if(share_start != DateTime.MinValue){
                        DateTime curr = DateTime.Now;
                        StaticFunctions.data[before.Guild.Id].usermanager[user.Id].share_timespan += (curr - share_start).TotalMinutes;
                        StaticFunctions.data[before.Guild.Id].usermanager[user.Id].share_start = DateTime.MinValue;
                    }
                }
                else if((initial.IsSelfDeafened && !final.IsSelfDeafened) || (initial.IsSelfMuted && !final.IsSelfMuted)){
                    if(voice_start == DateTime.MinValue && !final.IsStreaming){
                        StaticFunctions.data[before.Guild.Id].usermanager[user.Id].voice_start = DateTime.Now;
                    }
                    if(share_start == DateTime.MinValue && final.IsStreaming){
                        StaticFunctions.data[before.Guild.Id].usermanager[user.Id].share_start = DateTime.Now;
                    }
                }
            }
            else if(before != null && after == null){
                DateTime voice_start = StaticFunctions.data[before.Guild.Id].usermanager[user.Id].voice_start;
                DateTime share_start = StaticFunctions.data[before.Guild.Id].usermanager[user.Id].share_start; 
                var before_bots = 0;
                foreach(var users in before.Users){
                    if(users.IsBot){
                        before_bots++;
                    }
                }
                if(before.Users.Count - before_bots == 2){
                    foreach(var users in before.Users){               
                        if(user.Id != users.Id && users.IsStreaming && !users.IsSelfDeafened && !users.IsSelfMuted && !users.IsBot){
                            DateTime other_share_start = StaticFunctions.data[before.Guild.Id].usermanager[users.Id].share_start;
                            if(other_share_start != DateTime.MinValue){
                                DateTime curr = DateTime.Now;
                                StaticFunctions.data[before.Guild.Id].usermanager[users.Id].share_timespan += (curr - other_share_start).TotalMinutes;
                                StaticFunctions.data[before.Guild.Id].usermanager[users.Id].share_start = DateTime.MinValue;
                            }
                        }
                        else if(user.Id != users.Id && !users.IsStreaming && !users.IsSelfDeafened && !users.IsSelfMuted && !users.IsBot){
                            DateTime other_voice_start = StaticFunctions.data[before.Guild.Id].usermanager[users.Id].voice_start;
                            if(other_voice_start != DateTime.MinValue){
                                DateTime curr = DateTime.Now;
                                StaticFunctions.data[before.Guild.Id].usermanager[users.Id].voice_timespan += (curr - other_voice_start).TotalMinutes;
                                StaticFunctions.data[before.Guild.Id].usermanager[users.Id].voice_start = DateTime.MinValue;
                            }
                        }
                    }
                }
                
                DateTime currenttime = DateTime.Now;
                double pointsregular = (StaticFunctions.data[before.Guild.Id].vactive) / 60.0d;
                double pointsshare = (StaticFunctions.data[before.Guild.Id].vstream) / 60.0d;
                double regular = StaticFunctions.data[before.Guild.Id].usermanager[user.Id].voice_timespan;
                double share = StaticFunctions.data[before.Guild.Id].usermanager[user.Id].share_timespan;

                
                if(voice_start != DateTime.MinValue){
                    regular += (currenttime - voice_start).TotalMinutes + share;
                }
                if(share_start != DateTime.MinValue){
                    share += (currenttime - share_start).TotalMinutes + regular;
                }
                

                StaticFunctions.data[before.Guild.Id].usermanager[user.Id].points_earned += Convert.ToInt32((regular*pointsregular) + (share *pointsshare));

                StaticFunctions.data[before.Guild.Id].usermanager[user.Id].voice_start = DateTime.MinValue;
                StaticFunctions.data[before.Guild.Id].usermanager[user.Id].share_start = DateTime.MinValue;
                StaticFunctions.data[before.Guild.Id].usermanager[user.Id].voice_timespan = 0;
                StaticFunctions.data[before.Guild.Id].usermanager[user.Id].share_timespan = 0;



                
            }
            
            return Task.CompletedTask;
            
        }
    }
}