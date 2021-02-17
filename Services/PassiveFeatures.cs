using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using TreeTrunk.DataObjects;

namespace TreeTrunk.Services{
    public partial class CommandHandler{

        private async Task ActivityAsync(SocketGuildUser initial, SocketGuildUser final){           
            if(initial.IsBot || final.IsBot) return;

            var initialAct = initial.Activity == null? ActivityType.CustomStatus : initial.Activity.Type;
            var finalAct = final.Activity == null? ActivityType.CustomStatus : final.Activity.Type;

            

            if(initialAct != finalAct){
                ulong id = initial.Guild.Id;

                var streamerrole = StaticFunctions.data[id].streamrole;

                /*
                object roleid = StaticFunctions.GetGuildData(id, "StreamerRole");
                if(roleid == null) return;
                ulong streamrole;
                StaticFunctions.CastIt<ulong>(roleid, out streamrole);
                */
                //Console.WriteLine(initial.Username.ToString());
                //Console.WriteLine(initialAct.ToString());
                //Console.WriteLine(finalAct.ToString());

                if(initialAct != ActivityType.Streaming && finalAct == ActivityType.Streaming){
                    await initial.AddRoleAsync(initial.Guild.GetRole(streamerrole));
                }
                else if(initialAct == ActivityType.Streaming && finalAct != ActivityType.Streaming){
                    await initial.RemoveRoleAsync(initial.Guild.GetRole(streamerrole));
                }
                else if(initialAct != ActivityType.Playing && finalAct == ActivityType.Playing){
                    
                }
                else if(initialAct != ActivityType.Playing && finalAct == ActivityType.Playing){
                    
                }
            }
            
            return;
        }


        private async Task UserJoinAsync(SocketGuildUser user){
            if(user.IsBot) return;
            //quick fix start
            ulong defaultrole = StaticFunctions.data[user.Guild.Id].unranked;
            StaticFunctions.data[user.Guild.Id].usermanager.TryAdd(user.Id,new Profile(user.Id,user.Username));
            //quick fix end
            await user.AddRoleAsync(user.Guild.GetRole(defaultrole));
        }

        private async Task UserLeftAsync(SocketGuildUser user){
            if(user.IsBot) return;
            //quick fix start
            ulong modchannel = StaticFunctions.data[user.Guild.Id].modchat;    
            var chnl =  user.Guild.GetChannel(modchannel) as IMessageChannel;
            //quick fix end
            await chnl.SendMessageAsync(user.Username.ToString() + " has left the discord.");
        }

        private Task UserVoiceStateUpdatedAsync(SocketUser user, SocketVoiceState initial, SocketVoiceState final){
            //points only count if the voice chat has more than 1 person and if they are not self deafened
            //if a user gets sent into the afk channel, subtract the afk timeout from their time. 
            if(user.IsBot) return Task.CompletedTask;
            var before = initial.VoiceChannel;
            var after = final.VoiceChannel;
            
                        
            //user joined voice and isnt self deafened
            if(before == null && after != null && after.Id != after.Guild.AFKChannel.Id && after.Users.Count > 1){
                if(after.Users.Count == 2 && !final.IsSelfDeafened){
                    foreach(var users in after.Users){
                        if(users.IsStreaming && !users.IsSelfDeafened){
                            StaticFunctions.data[after.Guild.Id].usermanager[users.Id].share_start = DateTime.Now;
                        }
                        else if(!users.IsStreaming && !users.IsSelfDeafened){
                            StaticFunctions.data[after.Guild.Id].usermanager[users.Id].voice_start = DateTime.Now;
                        }
                    }
                }
                else if(after.Users.Count == 2 && final.IsSelfDeafened){
                    foreach(var users in after.Users){
                        if(users.Id != user.Id){
                            if(users.IsStreaming && !users.IsSelfDeafened){
                                StaticFunctions.data[after.Guild.Id].usermanager[users.Id].share_start = DateTime.Now;
                            }
                            else if(!users.IsStreaming && !users.IsSelfDeafened){
                                StaticFunctions.data[after.Guild.Id].usermanager[users.Id].voice_start = DateTime.Now;
                            }
                        }
                    }
                }
                else if(after.Users.Count > 2 && !final.IsSelfDeafened){
                    StaticFunctions.data[after.Guild.Id].usermanager[user.Id].voice_start = DateTime.Now;
                }
            }
            //user left disconnects
            else if(before != null && after == null && before.Id != before.Guild.AFKChannel.Id){
                if(before.Users.Count == 2){
                    foreach(var users in after.Users){
                        if(users.Id != user.Id){
                            if(initial.IsStreaming && !users.IsSelfDeafened){
                                DateTime currenttime2 = DateTime.Now;
                                DateTime share_start2 = StaticFunctions.data[before.Guild.Id].usermanager[users.Id].share_start;
                                StaticFunctions.data[before.Guild.Id].usermanager[users.Id].share_timespan += (float)(currenttime2 - share_start2).TotalMinutes;
                                StaticFunctions.data[before.Guild.Id].usermanager[users.Id].share_start = DateTime.MinValue;
                            }
                            else if(!initial.IsStreaming && !users.IsSelfDeafened){
                                DateTime currenttime2 = DateTime.Now;
                                DateTime voice_start2 = StaticFunctions.data[before.Guild.Id].usermanager[users.Id].voice_start;
                                StaticFunctions.data[before.Guild.Id].usermanager[users.Id].voice_timespan += (float)(currenttime2 - voice_start2).TotalMinutes;
                                StaticFunctions.data[before.Guild.Id].usermanager[users.Id].voice_start = DateTime.MinValue;
                            }
                        }
                    }
                }
                
                float regular = 0;
                float share = 0;
                var pointsregular = (StaticFunctions.data[before.Guild.Id].vactive) / 60;
                var pointsshare = (StaticFunctions.data[before.Guild.Id].vstream) / 60;
                DateTime currenttime = DateTime.Now;
                DateTime voice_start = StaticFunctions.data[before.Guild.Id].usermanager[user.Id].voice_start;
                DateTime share_start = StaticFunctions.data[before.Guild.Id].usermanager[user.Id].share_start;
                float voice_span = StaticFunctions.data[before.Guild.Id].usermanager[user.Id].voice_timespan;
                float share_span = StaticFunctions.data[before.Guild.Id].usermanager[user.Id].share_timespan;


                
                if(voice_start != DateTime.MinValue){
                    regular = (float)(currenttime - voice_start).TotalMinutes + voice_span + share_span;
                    share = share_span;
                }
                else if(share_start != DateTime.MinValue){
                    regular = voice_span;
                    share = (float)(currenttime - share_start).TotalMinutes + voice_span + share_span;
                }
                else{
                    regular = voice_span;
                    share = share_span;
                }

                StaticFunctions.data[before.Guild.Id].usermanager[user.Id].points_earned += (int) ((regular*pointsregular) + (share *pointsshare));

                StaticFunctions.data[before.Guild.Id].usermanager[user.Id].voice_start = DateTime.MinValue;
                StaticFunctions.data[before.Guild.Id].usermanager[user.Id].share_start = DateTime.MinValue;
                StaticFunctions.data[before.Guild.Id].usermanager[user.Id].voice_timespan = 0;
                StaticFunctions.data[before.Guild.Id].usermanager[user.Id].share_timespan = 0;

            }
            //user moves voice channels
            else if(before != null && after != null && before != after){
                if(after.Id == after.Guild.AFKChannel.Id){
                    float timeout = after.Guild.AFKTimeout / 60;
                    float regular = 0;
                    float share = 0;
                    var pointsregular = (StaticFunctions.data[before.Guild.Id].vactive) / 60;
                    var pointsshare = (StaticFunctions.data[before.Guild.Id].vstream) / 60;
                    DateTime currenttime = DateTime.Now;
                    DateTime voice_start = StaticFunctions.data[before.Guild.Id].usermanager[user.Id].voice_start;
                    DateTime share_start = StaticFunctions.data[before.Guild.Id].usermanager[user.Id].share_start;
                    float voice_span = StaticFunctions.data[before.Guild.Id].usermanager[user.Id].voice_timespan;
                    float share_span = StaticFunctions.data[before.Guild.Id].usermanager[user.Id].share_timespan;


                    
                    if(voice_start != DateTime.MinValue){
                        regular = (float)(currenttime - voice_start).TotalMinutes + voice_span + share_span - timeout;
                        share = share_span;
                    }
                    else if(share_start != DateTime.MinValue){
                        regular = voice_span;
                        share = (float)(currenttime - share_start).TotalMinutes + voice_span + share_span - timeout;
                    }
                    else{
                        regular = voice_span;
                        share = share_span;
                    }

                    StaticFunctions.data[before.Guild.Id].usermanager[user.Id].points_earned += (int) ((regular*pointsregular) + (share *pointsshare));

                    StaticFunctions.data[before.Guild.Id].usermanager[user.Id].voice_start = DateTime.MinValue;
                    StaticFunctions.data[before.Guild.Id].usermanager[user.Id].share_start = DateTime.MinValue;
                    StaticFunctions.data[before.Guild.Id].usermanager[user.Id].voice_timespan = 0;
                    StaticFunctions.data[before.Guild.Id].usermanager[user.Id].share_timespan = 0;

                }
                else if(before.Id == after.Guild.AFKChannel.Id){
                    if(after.Users.Count == 2 && !final.IsSelfDeafened){
                        foreach(var users in after.Users){
                            StaticFunctions.data[after.Guild.Id].usermanager[users.Id].voice_start = DateTime.Now;
                        }
                    }
                    else if(after.Users.Count == 2 && final.IsSelfDeafened){
                        foreach(var users in after.Users){
                            if(users.Id != user.Id){
                                StaticFunctions.data[after.Guild.Id].usermanager[users.Id].voice_start = DateTime.Now;
                            }
                        }
                    }
                    else if(after.Users.Count > 2 && !final.IsSelfDeafened){
                        StaticFunctions.data[after.Guild.Id].usermanager[user.Id].voice_start = DateTime.Now;
                    }
                }
                else if(after.Users.Count < 2 && (!initial.IsSelfDeafened || !final.IsSelfDeafened)){
                    if(initial.IsStreaming){
                        DateTime currenttime = DateTime.Now;
                        DateTime share_start = StaticFunctions.data[before.Guild.Id].usermanager[user.Id].share_start;
                        StaticFunctions.data[before.Guild.Id].usermanager[user.Id].share_timespan += (float)(currenttime - share_start).TotalMinutes;
                        StaticFunctions.data[before.Guild.Id].usermanager[user.Id].share_start = DateTime.MinValue;
                    }
                    else{
                        DateTime currenttime = DateTime.Now;
                        DateTime voice_start = StaticFunctions.data[before.Guild.Id].usermanager[user.Id].voice_start;
                        StaticFunctions.data[before.Guild.Id].usermanager[user.Id].voice_timespan += (float)(currenttime - voice_start).TotalMinutes;
                        StaticFunctions.data[before.Guild.Id].usermanager[user.Id].voice_start = DateTime.MinValue;
                    }
                }
                else if(after.Users.Count == 2){
                    if(initial.IsStreaming && (!initial.IsSelfDeafened || !final.IsSelfDeafened)){
                        DateTime currenttime = DateTime.Now;
                        DateTime share_start = StaticFunctions.data[before.Guild.Id].usermanager[user.Id].share_start;
                        StaticFunctions.data[before.Guild.Id].usermanager[user.Id].share_timespan += (float)(currenttime - share_start).TotalMinutes;
                        StaticFunctions.data[before.Guild.Id].usermanager[user.Id].share_start = DateTime.MinValue;
                    }
                    
                    foreach(var users in after.Users){
                        if(users.Id != user.Id){
                            if(users.IsStreaming && !users.IsSelfDeafened){
                                StaticFunctions.data[after.Guild.Id].usermanager[users.Id].share_start = DateTime.Now;
                            }
                            else if(!users.IsStreaming && !users.IsSelfDeafened){
                                StaticFunctions.data[after.Guild.Id].usermanager[users.Id].voice_start = DateTime.Now;
                            }
                        }
                    }

                }
                else if(after.Users.Count > 2){
                    if(initial.IsStreaming && (!initial.IsSelfDeafened || !final.IsSelfDeafened)){
                        DateTime currenttime = DateTime.Now;
                        DateTime share_start = StaticFunctions.data[before.Guild.Id].usermanager[user.Id].share_start;
                        StaticFunctions.data[before.Guild.Id].usermanager[user.Id].share_timespan += (float)(currenttime - share_start).TotalMinutes;
                        StaticFunctions.data[before.Guild.Id].usermanager[user.Id].share_start = DateTime.MinValue;
                    }
                }
            }
            //user state updates in same channel
            else if(before != null && after != null && before == after){
                if(!initial.IsStreaming && final.IsStreaming && (!initial.IsSelfDeafened || !final.IsSelfDeafened)){
                    StaticFunctions.data[after.Guild.Id].usermanager[user.Id].share_start = DateTime.Now;
                    if(StaticFunctions.data[after.Guild.Id].usermanager[user.Id].voice_start != DateTime.MinValue){
                        DateTime currenttime = DateTime.Now;
                        DateTime voice_start = StaticFunctions.data[before.Guild.Id].usermanager[user.Id].voice_start;
                        StaticFunctions.data[before.Guild.Id].usermanager[user.Id].voice_timespan += (float)(currenttime - voice_start).TotalMinutes;
                        StaticFunctions.data[before.Guild.Id].usermanager[user.Id].voice_start = DateTime.MinValue;
                    }
                }
                else if(initial.IsStreaming && !final.IsStreaming && (!initial.IsSelfDeafened || !final.IsSelfDeafened)){
                    StaticFunctions.data[after.Guild.Id].usermanager[user.Id].voice_start = DateTime.Now;
                    if(StaticFunctions.data[after.Guild.Id].usermanager[user.Id].share_start != DateTime.MinValue){
                        DateTime currenttime = DateTime.Now;
                        DateTime share_start = StaticFunctions.data[before.Guild.Id].usermanager[user.Id].share_start;
                        StaticFunctions.data[before.Guild.Id].usermanager[user.Id].share_timespan += (float)(currenttime - share_start).TotalMinutes;
                        StaticFunctions.data[before.Guild.Id].usermanager[user.Id].share_start = DateTime.MinValue;
                    }
                }
                else if(!initial.IsSelfDeafened && final.IsSelfDeafened){
                    if(initial.IsStreaming){
                        DateTime currenttime = DateTime.Now;
                        DateTime share_start = StaticFunctions.data[before.Guild.Id].usermanager[user.Id].share_start;
                        StaticFunctions.data[before.Guild.Id].usermanager[user.Id].share_timespan += (float)(currenttime - share_start).TotalMinutes;
                        StaticFunctions.data[before.Guild.Id].usermanager[user.Id].share_start = DateTime.MinValue;
                    }
                    else{
                        DateTime currenttime = DateTime.Now;
                        DateTime voice_start = StaticFunctions.data[before.Guild.Id].usermanager[user.Id].voice_start;
                        StaticFunctions.data[before.Guild.Id].usermanager[user.Id].voice_timespan += (float)(currenttime - voice_start).TotalMinutes;
                        StaticFunctions.data[before.Guild.Id].usermanager[user.Id].voice_start = DateTime.MinValue;
                    }
                }
                else if(initial.IsSelfDeafened && !final.IsSelfDeafened){
                    if(initial.IsStreaming){
                        StaticFunctions.data[before.Guild.Id].usermanager[user.Id].share_start = DateTime.Now;
                    }
                    else{
                        StaticFunctions.data[before.Guild.Id].usermanager[user.Id].voice_start = DateTime.Now;
                    }
                }
            }

            return Task.CompletedTask;
            
        }
    }
}