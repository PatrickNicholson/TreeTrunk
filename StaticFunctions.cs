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

//using System;

namespace TreeTrunk{
    public static class StaticFunctions{

        public static ConcurrentDictionary<ulong, GuildData> data = new ConcurrentDictionary<ulong, GuildData>();

        public static IServiceProvider services;
        
        //loads saved data into memory on startup
        public static void LoadGuildData(){
            string json = File.ReadAllText("data.json");
            lock(data){            
                data = JsonConvert.DeserializeObject<ConcurrentDictionary<ulong, GuildData>>(json);
                data = data ?? new ConcurrentDictionary<ulong, GuildData>();
            }
        }

        //writes data to harddisk
        public static void WriteGuildData(){
            File.WriteAllText("data.json", JsonConvert.SerializeObject(data, Formatting.Indented));
            Console.WriteLine(DateTime.Now.ToString() + ": Saved Data");
        }

        public static Task InitializeData(){
            var _discord = services.GetRequiredService<DiscordSocketClient>();
            var context = _discord.Guilds;
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
            //execute AR update
            TaskSchedule.Instance.ScheduleTask(23, 59, 24, async () 
                => await UpdateAR());
            _discord.Ready -= StaticFunctions.InitializeData;
            return Task.CompletedTask;
        }

        public static Task UpdateAR(){
            var context = services.GetRequiredService<DiscordSocketClient>();
            //var tasks = new List<Task>();
            lock(data){
                foreach(KeyValuePair<ulong, GuildData> guild in data){
                    var max_AR = guild.Value.armax;
                    var min_AR = guild.Value.armin;
                    var max_MMR = min_AR;
                    var decay_value = guild.Value.decay;
                    var guildcomms = context.GetGuild(guild.Key);


                    var ranks = new Dictionary<ulong,int>(){
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

                    if(guild.Value.max_player_mmr == 0){
                        foreach(KeyValuePair<ulong, Profile> user in guild.Value.usermanager){
                            if(user.Value.activityrating > max_MMR){
                                max_MMR = user.Value.activityrating;
                                guild.Value.max_player_mmr = max_MMR;
                                data[guild.Key].max_player_mmr = max_MMR;
                            }
                        }
                    }
                    Console.WriteLine(DateTime.Now.ToString() + ": Updating User Ranks");
                    foreach(KeyValuePair<ulong, Profile> user in guild.Value.usermanager){

                        var roles = context.GetGuild(guild.Key).GetUser(user.Key).Roles;
                        if(roles == null){
                            guildcomms.GetUser(user.Key).AddRoleAsync(guildcomms.GetRole(guild.Value.unranked));
                            continue;
                        }
                        
                        
                        TimeSpan profiledate = DateTime.Now - user.Value.profilecreated;
                        
                        
                        
                        var points_gained = user.Value.points_earned;
                        var mmr = user.Value.activityrating;
                        mmr += (points_gained*((1-(mmr/max_MMR)) + (1 - (mmr/max_AR)))) - (decay_value*((((mmr - min_AR)/max_AR))*((mmr - min_AR)/max_AR)));

                        if(mmr > max_AR) mmr = max_AR;
                        else if(mmr < min_AR) mmr = min_AR;

                        data[guild.Key].usermanager[user.Key].activityrating = mmr;
                        data[guild.Key].usermanager[user.Key].points_earned = 0;

                        if(mmr > max_MMR) data[guild.Key].max_player_mmr = mmr;

                        
                        
                        if(TimeSpan.Compare(profiledate,placements) < 0 ){
                            if(!roles.Contains(guildcomms.GetRole(guild.Value.unranked))){
                                Console.WriteLine(DateTime.Now.ToString() + ": UNRANKED - " + user.Value.name);
                                guildcomms.GetUser(user.Key).AddRoleAsync(guildcomms.GetRole(guild.Value.unranked));
                                foreach(var role in roles){
                                    if(ranks.ContainsKey(role.Id)){
                                        guildcomms.GetUser(user.Key).RemoveRoleAsync(guildcomms.GetRole(role.Id));
                                    }
                                }
                            }
                        }
                        else{
                            if(roles.Contains(guildcomms.GetRole(guild.Value.unranked))){
                                guildcomms.GetUser(user.Key).RemoveRoleAsync(guildcomms.GetRole(guild.Value.unranked));
                            }                           
                            Console.WriteLine(DateTime.Now.ToString() + ": RANKED - " + user.Value.name);
                            for(int i = 0; i < ranks.Count(); i++){
                                if(i == 0 && !roles.Contains(guildcomms.GetRole(rank.ElementAt(i))) && mmr <= ar_ranks.ElementAt(i)){
                                    guildcomms.GetUser(user.Key).AddRoleAsync(guildcomms.GetRole(rank.ElementAt(i)));
                                    if(roles.Contains(guildcomms.GetRole(rank.ElementAt(i+1)))){
                                        guildcomms.GetUser(user.Key).RemoveRoleAsync(guildcomms.GetRole(rank.ElementAt(i+1)));
                                    }
                                }
                                else if(i+1 >= ranks.Count() && !roles.Contains(guildcomms.GetRole(rank.ElementAt(i))) && mmr > ar_ranks.ElementAt(i-1)){
                                    guildcomms.GetUser(user.Key).AddRoleAsync(guildcomms.GetRole(rank.ElementAt(i)));
                                    if(roles.Contains(guildcomms.GetRole(rank.ElementAt(i-1)))){
                                        guildcomms.GetUser(user.Key).RemoveRoleAsync(guildcomms.GetRole(rank.ElementAt(i-1)));
                                    }
                                }
                                else if(!roles.Contains(guildcomms.GetRole(rank.ElementAt(i))) && mmr > ar_ranks.ElementAt(i-1) && mmr <= ar_ranks.ElementAt(i)){
                                    guildcomms.GetUser(user.Key).AddRoleAsync(guildcomms.GetRole(rank.ElementAt(i)));
                                    if(roles.Contains(guildcomms.GetRole(rank.ElementAt(i-1)))){
                                        guildcomms.GetUser(user.Key).RemoveRoleAsync(guildcomms.GetRole(rank.ElementAt(i-1)));
                                    }
                                    if(roles.Contains(guildcomms.GetRole(rank.ElementAt(i+1)))){
                                        guildcomms.GetUser(user.Key).RemoveRoleAsync(guildcomms.GetRole(rank.ElementAt(i+1)));
                                    }
                                }
                            }
                        }
                    }
                }
                WriteGuildData();
            }
            //await Task.WhenAll(tasks);
            Console.WriteLine(DateTime.Now.ToString() + ": Finished Updating User Ranks");
            
            return Task.CompletedTask;
        }

    }
}