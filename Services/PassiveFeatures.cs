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
            //quick fix start
            ulong defaultrole = StaticFunctions.data[user.Guild.Id].unranked;
            StaticFunctions.data[user.Guild.Id].usermanager.TryAdd(user.Id,new Profile(user.Id,user.Username));
            //quick fix end
            await user.AddRoleAsync(user.Guild.GetRole(defaultrole));
        }

        private async Task UserLeftAsync(SocketGuildUser user){

            //quick fix start
            ulong modchannel = StaticFunctions.data[user.Guild.Id].modchat;    
            var chnl =  user.Guild.GetChannel(modchannel) as IMessageChannel;
            //quick fix end
            await chnl.SendMessageAsync(user.Username.ToString() + " has left the discord.");
        }

        private Task UserVoiceStateUpdatedAsync(SocketUser user, SocketVoiceState initial, SocketVoiceState final){
            //points only count if the voice chat has more than 1 person and if they are not self deafened
            //if a user gets sent into the afk channel, subtract the afk timeout from their time. 
            var before = initial.VoiceChannel;
            var after = final.VoiceChannel;
            
                        
            //user joined voice and isnt self deafened
            if(before == null && after != null){
                if(final.IsSelfDeafened){
                    StaticFunctions.data[after.Guild.Id].usermanager[user.Id].deafened_start = DateTime.Now;
                }
                if(after.Id == after.Guild.AFKChannel.Id){
                    StaticFunctions.data[after.Guild.Id].usermanager[user.Id].voice_start = DateTime.MinValue;
                    StaticFunctions.data[after.Guild.Id].usermanager[user.Id].voiceshare_start = DateTime.MinValue;
                    StaticFunctions.data[after.Guild.Id].usermanager[user.Id].voiceshare_end = DateTime.MinValue;
                    StaticFunctions.data[after.Guild.Id].usermanager[user.Id].voiceshare_timespan = 0;
                    StaticFunctions.data[after.Guild.Id].usermanager[user.Id].deafened_timespan = 0;
                }
                else if(after.Users.Count == 2){
                    foreach(var users in after.Users){
                        StaticFunctions.data[after.Guild.Id].usermanager[users.Id].voice_start = DateTime.Now;
                    }
                }
                else if(after.Users.Count > 2){
                    StaticFunctions.data[after.Guild.Id].usermanager[user.Id].voice_start = DateTime.Now;
                }
            }
            //user left disconnects
            else if(before != null && after == null && StaticFunctions.data[before.Guild.Id].usermanager[user.Id].voice_start != DateTime.MinValue){
                float regular = 0;
                float share = 0;
                var pointsregular = (StaticFunctions.data[before.Guild.Id].vactive) / 60;
                var pointsshare = (StaticFunctions.data[before.Guild.Id].vstream) / 60;
                DateTime currenttime = DateTime.Now;
                DateTime regular_start = StaticFunctions.data[before.Guild.Id].usermanager[user.Id].voice_start;
                DateTime share_start = StaticFunctions.data[before.Guild.Id].usermanager[user.Id].voiceshare_start;
                DateTime share_end = StaticFunctions.data[before.Guild.Id].usermanager[user.Id].voiceshare_end;
                float deafened_span = StaticFunctions.data[before.Guild.Id].usermanager[user.Id].deafened_timespan;
                float vcshare_span = StaticFunctions.data[before.Guild.Id].usermanager[user.Id].voiceshare_timespan;
                
                if(share_start != DateTime.MinValue && share_end != DateTime.MinValue){
                    regular = (float)(currenttime- regular_start).TotalMinutes - deafened_span;
                    share = (float)(share_end - share_start).TotalMinutes + vcshare_span;
                    regular -= share;
                }
                else if(share_start != DateTime.MinValue){
                    regular = (float)(share_start - regular_start).TotalMinutes - deafened_span;
                    share = (float)(currenttime - share_start).TotalMinutes + vcshare_span;
                    regular -= vcshare_span;
                }
                else{
                    regular = (float)(currenttime - regular_start).TotalMinutes - deafened_span;
                }

                StaticFunctions.data[before.Guild.Id].usermanager[user.Id].points_earned += (int) ((regular*pointsregular) + (share *pointsshare));

                StaticFunctions.data[before.Guild.Id].usermanager[user.Id].voice_start = DateTime.MinValue;
                StaticFunctions.data[before.Guild.Id].usermanager[user.Id].voiceshare_start = DateTime.MinValue;
                StaticFunctions.data[before.Guild.Id].usermanager[user.Id].voiceshare_end = DateTime.MinValue;
                StaticFunctions.data[before.Guild.Id].usermanager[user.Id].voiceshare_timespan = 0;
                StaticFunctions.data[before.Guild.Id].usermanager[user.Id].deafened_timespan = 0;

            }
            //user moves voice channels
            else if(before != null && after != null && before != after){
                if(after.Id == after.Guild.AFKChannel.Id){
                    float timeout = (after.Guild.AFKTimeout)/60;
                    float regular = 0;
                    float share = 0;
                    var pointsregular = (StaticFunctions.data[before.Guild.Id].vactive) / 60;
                    var pointsshare = (StaticFunctions.data[before.Guild.Id].vstream) / 60;
                    DateTime currenttime = DateTime.Now;
                    DateTime regular_start = StaticFunctions.data[before.Guild.Id].usermanager[user.Id].voice_start;
                    DateTime share_start = StaticFunctions.data[before.Guild.Id].usermanager[user.Id].voiceshare_start;
                    DateTime share_end = StaticFunctions.data[before.Guild.Id].usermanager[user.Id].voiceshare_end;
                    float deafened_span = StaticFunctions.data[before.Guild.Id].usermanager[user.Id].deafened_timespan;
                    float vcshare_span = StaticFunctions.data[before.Guild.Id].usermanager[user.Id].voiceshare_timespan;
                    
                    if(share_start != DateTime.MinValue && share_end != DateTime.MinValue){
                        regular = (float)(currenttime- regular_start).TotalMinutes - deafened_span;
                        share = (float)(share_end - share_start).TotalMinutes + vcshare_span;
                        regular -= share;
                    }
                    else if(share_start != DateTime.MinValue){
                        regular = (float)(share_start - regular_start).TotalMinutes - deafened_span;
                        share = (float)(currenttime - share_start).TotalMinutes + vcshare_span;
                        regular -= vcshare_span;
                    }
                    else{
                        regular = (float)(currenttime - regular_start).TotalMinutes - deafened_span;
                    }

                    StaticFunctions.data[before.Guild.Id].usermanager[user.Id].points_earned += (int) ((regular*pointsregular) + (share *pointsshare));

                    StaticFunctions.data[before.Guild.Id].usermanager[user.Id].voice_start = DateTime.MinValue;
                    StaticFunctions.data[before.Guild.Id].usermanager[user.Id].voiceshare_start = DateTime.MinValue;
                    StaticFunctions.data[before.Guild.Id].usermanager[user.Id].voiceshare_end = DateTime.MinValue;
                    StaticFunctions.data[before.Guild.Id].usermanager[user.Id].voiceshare_timespan = 0;
                    StaticFunctions.data[before.Guild.Id].usermanager[user.Id].deafened_timespan = 0;

                }
                else if(before.Id == before.Guild.AFKChannel.Id){
                    if(after.Users.Count == 2){
                        foreach(var users in after.Users){
                            StaticFunctions.data[after.Guild.Id].usermanager[users.Id].voice_start = DateTime.Now;
                        }
                    }
                    else if(after.Users.Count > 2){
                        StaticFunctions.data[after.Guild.Id].usermanager[user.Id].voice_start = DateTime.Now;
                    }
                }
                else if(after.Users.Count == 2){
                    foreach(var users in after.Users){
                        StaticFunctions.data[after.Guild.Id].usermanager[users.Id].voice_start = DateTime.Now;
                    }
                }
                else if(after.Users.Count > 2){
                    StaticFunctions.data[after.Guild.Id].usermanager[user.Id].voice_start = DateTime.Now;
                }
            }
            //user state updates in same channel
            else if(before != null && after != null && before == after){
                if(!initial.IsStreaming && final.IsStreaming){
                    StaticFunctions.data[after.Guild.Id].usermanager[user.Id].voiceshare_start = DateTime.Now;
                }
                else if(initial.IsStreaming && !final.IsStreaming){
                    DateTime currenttime = DateTime.Now;
                    DateTime share_start = StaticFunctions.data[after.Guild.Id].usermanager[user.Id].voiceshare_start;
                    StaticFunctions.data[after.Guild.Id].usermanager[user.Id].voiceshare_end = currenttime;
                    StaticFunctions.data[after.Guild.Id].usermanager[user.Id].voiceshare_timespan += (float)(currenttime - share_start).TotalMinutes;
                }
                else if(!initial.IsSelfDeafened && final.IsSelfDeafened){
                    StaticFunctions.data[after.Guild.Id].usermanager[user.Id].deafened_start = DateTime.Now;
                }
                else if(initial.IsSelfDeafened && !final.IsSelfDeafened){
                    DateTime currenttime = DateTime.Now;
                    DateTime deafened_start = StaticFunctions.data[after.Guild.Id].usermanager[user.Id].deafened_start;
                    StaticFunctions.data[after.Guild.Id].usermanager[user.Id].deafened_timespan += (float)(currenttime - deafened_start).TotalMinutes;
                }
            }

            return Task.CompletedTask;
            
        }
    }
}