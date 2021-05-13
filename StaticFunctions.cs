using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using TreeTrunk.DataObjects;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Discord.WebSocket;
using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using TreeTrunk.Services;


namespace TreeTrunk{
    public static class StaticFunctions{

        public static ConcurrentDictionary<ulong, GuildData> data = new ConcurrentDictionary<ulong, GuildData>();

        public static IServiceProvider services;

        public static void LoadGuildData(){
            Console.WriteLine(DateTime.Now.ToString() + ": Loading data");
            string json = File.ReadAllText("data.json");
            lock(data){
                data = JsonConvert.DeserializeObject<ConcurrentDictionary<ulong, GuildData>>(json);
                data = data ?? new ConcurrentDictionary<ulong, GuildData>();
            }
        }

        public static void WriteGuildData(){
            File.WriteAllText("data.json", JsonConvert.SerializeObject(data, Formatting.Indented));
            Console.WriteLine(DateTime.Now.ToString() + ": Saved Data");
        }

        public async static Task InitializeData(){
            Console.WriteLine(DateTime.Now.ToString() + ": Initializing data");
            var _discord = services.GetRequiredService<DiscordSocketClient>(); 
            var context = _discord.Guilds;
            await _discord.DownloadUsersAsync(context);
            LoadGuildData();
            lock(data){
                var d = data;
                foreach(var guild in context){
                    if(!data.ContainsKey(guild.Id)){
                        data.TryAdd(guild.Id,new GuildData(guild.Id));
                        foreach(var user in _discord.GetGuild(guild.Id).Users){
                            if(!data[guild.Id].usermanager.ContainsKey(user.Id) && !user.IsBot){
                                data[guild.Id].usermanager.Add(user.Id, new Profile(user.Id,user.Username));
                            }
                        }
                        WriteGuildData();
                    }
                }
            }
            TaskSchedule.Instance.ScheduleTask(23, 59, 24, () 
                => UpdateAR());
            _discord.Ready -= StaticFunctions.InitializeData;
            return;
        }

        public static Task UpdateAR(){
            var context = services.GetRequiredService<DiscordSocketClient>();
            var data_copy = data;
            foreach(KeyValuePair<ulong, GuildData> guild in data_copy){

                
                var guildcomms = context.GetGuild(guild.Key);

                if(guildcomms == null){
                    data.Remove(guild.Key, out var x);
                    continue;
                }
                double max_AR = Convert.ToDouble(guild.Value.armax);
                double min_AR = Convert.ToDouble(guild.Value.armin);
                double max_MMR = Convert.ToDouble(guild.Value.max_player_mmr);
                double decay_value = Convert.ToDouble(guild.Value.decay);
                

                var ranks = new Dictionary<ulong,int>(){
                    {guild.Value.quickplay,guild.Value.armin},
                    {guild.Value.bronze,guild.Value.bronze_ar},
                    {guild.Value.silver,guild.Value.silver_ar},
                    {guild.Value.gold,guild.Value.gold_ar},
                    {guild.Value.plat,guild.Value.plat_ar},
                    {guild.Value.diamond,guild.Value.diamond_ar},
                    {guild.Value.master,guild.Value.master_ar},
                    {guild.Value.gm,guild.Value.gm_ar}
                };
                var ar_ranks = ranks.Values;
                var rank = ranks.Keys; 

                TimeSpan placements = guild.Value.placementtime;

                Console.WriteLine(DateTime.Now.ToString() + ": Updating User Ranks");
                double max = 0;
                foreach(KeyValuePair<ulong, Profile> user in guild.Value.usermanager){
                    
                    if(guildcomms.GetUser(user.Key) == null){
                        data[guild.Key].usermanager.Remove(user.Key);
                        continue;
                    }



                    var roles = context.GetGuild(guild.Key).GetUser(user.Key).Roles;
                    if(roles == null){
                        continue;
                    }
                    else if(roles.Count() == 1){
                        guildcomms.GetUser(user.Key).AddRoleAsync(guildcomms.GetRole(guild.Value.unranked));
                        continue;
                    }
                    
                    
                    
                    TimeSpan profiledate = DateTime.Now - user.Value.profilecreated;
                    
                    int extra = 0;
                    if(user.Value.voice_start != DateTime.MinValue || user.Value.share_start != DateTime.MinValue){
                        DateTime currenttime = DateTime.Now;
                        DateTime voice_start = user.Value.voice_start;
                        DateTime share_start = user.Value.share_start;
                        double pointsregular = (guild.Value.vactive) / 60.0d;
                        double pointsshare = (guild.Value.vstream) / 60.0d;

                        if(voice_start != DateTime.MinValue){
                            extra = Convert.ToInt32((currenttime - voice_start).TotalMinutes*pointsshare);
                            data[guild.Key].usermanager[user.Key].voice_start = currenttime;
                        }
                        if(share_start != DateTime.MinValue){
                            extra = Convert.ToInt32((currenttime - share_start).TotalMinutes*pointsregular);
                            data[guild.Key].usermanager[user.Key].share_start = currenttime;
                        }
                    }
                    
                    double points_gained = Convert.ToDouble(user.Value.points_earned + extra);
                    double mmr = Convert.ToDouble(user.Value.activityrating);

                    mmr += (points_gained*(1-(mmr/max_MMR))) + (points_gained*(1-(mmr/max_AR))) - (decay_value*((((mmr - min_AR)/max_AR))*((mmr - min_AR)/max_AR)));
                    

                    if(mmr > max_AR) mmr = max_AR;
                    else if(mmr < min_AR) mmr = min_AR;

                    

                    data[guild.Key].usermanager[user.Key].activityrating = Convert.ToInt32(mmr);
                    data[guild.Key].usermanager[user.Key].points_earned = 0;

                    if(mmr > max) max = mmr;

                    
                    
                    if(TimeSpan.Compare(profiledate,placements) < 0 ){
                        Console.WriteLine(DateTime.Now.ToString() + ": UNRANKED - " + user.Value.name);
                        if(!roles.Contains(guildcomms.GetRole(guild.Value.unranked))){
                            guildcomms.GetUser(user.Key).AddRoleAsync(guildcomms.GetRole(guild.Value.unranked));
                            foreach(var role in roles){
                                if(ranks.ContainsKey(role.Id)){
                                    guildcomms.GetUser(user.Key).RemoveRoleAsync(guildcomms.GetRole(role.Id));
                                }
                            }
                        }
                    }
                    else{
                        Console.WriteLine(DateTime.Now.ToString() + ": RANKED - " + user.Value.name);

                        if(roles.Contains(guildcomms.GetRole(guild.Value.unranked))){
                            guildcomms.GetUser(user.Key).RemoveRoleAsync(guildcomms.GetRole(guild.Value.unranked));
                        }
                        if(!roles.Contains(guildcomms.GetRole(guild.Value.quickplay)) && mmr == 1){
                            foreach(var role in roles){
                                if(ranks.ContainsKey(role.Id)){
                                    guildcomms.GetUser(user.Key).RemoveRoleAsync(guildcomms.GetRole(role.Id));
                                }
                            }
                            guildcomms.GetUser(user.Key).AddRoleAsync(guildcomms.GetRole(guild.Value.quickplay));
                        }
                        else{
                            for(int i = 1; i < ranks.Count(); i++){
                                if(!roles.Contains(guildcomms.GetRole(rank.ElementAt(i))) &&  mmr > ar_ranks.ElementAt(i-1) && mmr <= ar_ranks.ElementAt(i)){
                                    foreach(var role in roles){
                                        if(ranks.ContainsKey(role.Id)){
                                            guildcomms.GetUser(user.Key).RemoveRoleAsync(guildcomms.GetRole(role.Id));
                                            Console.WriteLine(rank.ElementAt(i));
                                        }
                                    }
                                    guildcomms.GetUser(user.Key).AddRoleAsync(guildcomms.GetRole(rank.ElementAt(i)));
                                }
                            }
                        }
                    }
                }
                data[guild.Key].max_player_mmr = Convert.ToInt32(max);
            }
            WriteGuildData();
            Console.WriteLine(DateTime.Now.ToString() + ": Finished Updating User Ranks");
            
            return Task.CompletedTask;
        }

    }
}