using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using TreeTrunk.DataObjects;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Discord.WebSocket;
using System;
using Microsoft.Extensions.DependencyInjection;

//using System;

namespace TreeTrunk{
    public static class StaticFunctions{

        public static ConcurrentDictionary<ulong, GuildData> data = new ConcurrentDictionary<ulong, GuildData>();

        public static IServiceProvider services;
        
 /*       
        //returns null if it cant find the value
        public static void AddGuildData(ulong guildId, string key, object value){
            lock(data){
                if(data.ContainsKey(guildId)){
                    
                    if(data[guildId].storeddata.ContainsKey(key)){
                        data[guildId].storeddata[key] = value;
                    }
                    else{
                        data[guildId].storeddata.TryAdd(key,value);
                    }
                }
                else{
                    data.TryAdd(guildId, new GuildData(guildId));
                    data[guildId].storeddata.TryAdd(key, value);
                }
            }
        }
        
        //returns null if it cant find the value
        public static object GetGuildData(ulong guildId, string index){
            object value;
            lock(data){
                data.GetOrAdd(guildId,new GuildData(guildId)).storeddata.TryGetValue(index, out value);
            }
            return value;
        }
*/   
        //loads saved data into memory on startup
        public static Task LoadGuildData(){
            string json = File.ReadAllText("data.json");
            lock(data){            
                data = JsonConvert.DeserializeObject<ConcurrentDictionary<ulong, GuildData>>(json);
                data = data ?? new ConcurrentDictionary<ulong, GuildData>();
            }
            return Task.CompletedTask;
        }

        //writes data to harddisk
        public static Task WriteGuildData(){
            File.WriteAllText("data.json", JsonConvert.SerializeObject(data, Formatting.Indented));
            return Task.CompletedTask;
        }

        public static async Task InitializeData(){
            var _discord = services.GetRequiredService<DiscordSocketClient>();
            var context = _discord.Guilds;
            await _discord.DownloadUsersAsync(context);
            lock(StaticFunctions.data){
                
                foreach(var guild in context){
                    StaticFunctions.data.GetOrAdd(guild.Id,new GuildData(guild.Id));
                    foreach(var user in _discord.GetGuild(guild.Id).Users){
                        if(!StaticFunctions.data[guild.Id].usermanager.ContainsKey(user.Id) && !user.IsBot){
                            StaticFunctions.data[guild.Id].usermanager.Add(user.Id, new Profile(user.Id,user.Username));
                        }
                    }
                }

                WriteGuildData();
            }
        }

        public static async Task UpdateAR(){
            var context = services.GetRequiredService<DiscordSocketClient>();
            var datacopy = data;
            
            foreach(KeyValuePair<ulong, GuildData> guild in datacopy){
                var max_AR = guild.Value.armax;
                var min_AR = guild.Value.armin;
                var max_MMR = min_AR;
                var decay_value = guild.Value.decay;
                var guildcomms = context.GetGuild(guild.Key);
                TimeSpan placements = guild.Value.placementtime;

                
                foreach(KeyValuePair<ulong, Profile> user in guild.Value.usermanager){
                    if(user.Value.activityrating > max_MMR){
                        max_MMR = user.Value.activityrating;
                    }
                }
                
                foreach(KeyValuePair<ulong, Profile> user in guild.Value.usermanager){
                    
                    TimeSpan profiledate = user.Value.profilecreated - DateTime.Now;
                    if(TimeSpan.Compare(profiledate,placements) < 0){
                        Console.WriteLine(user.Value.name.ToString());
                        await guildcomms.GetUser(user.Key).RemoveRoleAsync(guildcomms.GetRole(guild.Value.bronze));
                        await guildcomms.GetUser(user.Key).RemoveRoleAsync(guildcomms.GetRole(guild.Value.silver));
                        await guildcomms.GetUser(user.Key).RemoveRoleAsync(guildcomms.GetRole(guild.Value.gold));
                        await guildcomms.GetUser(user.Key).RemoveRoleAsync(guildcomms.GetRole(guild.Value.plat));
                        await guildcomms.GetUser(user.Key).RemoveRoleAsync(guildcomms.GetRole(guild.Value.diamond));
                        await guildcomms.GetUser(user.Key).RemoveRoleAsync(guildcomms.GetRole(guild.Value.master));
                        await guildcomms.GetUser(user.Key).RemoveRoleAsync(guildcomms.GetRole(guild.Value.gm));
                        await guildcomms.GetUser(user.Key).AddRoleAsync(guildcomms.GetRole(guild.Value.unranked));
                        continue;
                    }

                    var points_gained = user.Value.points_earned;
                    var mmr = user.Value.activityrating;
                    mmr += (points_gained*((1-(mmr/max_MMR)) + (1 - (mmr/max_AR)))) - (decay_value*((mmr - min_AR)/max_AR));

                    if(mmr > max_AR) mmr = max_AR;
                    else if(mmr < min_AR) mmr = min_AR;

                    data[guild.Key].usermanager[user.Key].activityrating = mmr;

                    //bronze
                    if(mmr < guild.Value.bronze_ar){
                        await guildcomms.GetUser(user.Key).RemoveRoleAsync(guildcomms.GetRole(guild.Value.unranked));
                        await guildcomms.GetUser(user.Key).RemoveRoleAsync(guildcomms.GetRole(guild.Value.silver));
                        await guildcomms.GetUser(user.Key).AddRoleAsync(guildcomms.GetRole(guild.Value.bronze));
                    }
                    //silver
                    else if(mmr > guild.Value.bronze_ar && mmr < guild.Value.gold_ar){
                        await guildcomms.GetUser(user.Key).RemoveRoleAsync(guildcomms.GetRole(guild.Value.unranked));
                        await guildcomms.GetUser(user.Key).RemoveRoleAsync(guildcomms.GetRole(guild.Value.bronze));
                        await guildcomms.GetUser(user.Key).RemoveRoleAsync(guildcomms.GetRole(guild.Value.gold));
                        await guildcomms.GetUser(user.Key).AddRoleAsync(guildcomms.GetRole(guild.Value.silver));
                    }
                    //gold
                    else if(mmr > guild.Value.silver_ar && mmr < guild.Value.plat_ar){
                        await guildcomms.GetUser(user.Key).RemoveRoleAsync(guildcomms.GetRole(guild.Value.unranked));
                        await guildcomms.GetUser(user.Key).RemoveRoleAsync(guildcomms.GetRole(guild.Value.silver));
                        await guildcomms.GetUser(user.Key).RemoveRoleAsync(guildcomms.GetRole(guild.Value.plat));
                        await guildcomms.GetUser(user.Key).AddRoleAsync(guildcomms.GetRole(guild.Value.gold));
                    }
                    //plat
                    else if(mmr > guild.Value.gold_ar && mmr < guild.Value.diamond_ar){
                        await guildcomms.GetUser(user.Key).RemoveRoleAsync(guildcomms.GetRole(guild.Value.unranked));
                        await guildcomms.GetUser(user.Key).RemoveRoleAsync(guildcomms.GetRole(guild.Value.gold));
                        await guildcomms.GetUser(user.Key).RemoveRoleAsync(guildcomms.GetRole(guild.Value.diamond));
                        await guildcomms.GetUser(user.Key).AddRoleAsync(guildcomms.GetRole(guild.Value.plat));
                    }
                    //diamond
                    else if(mmr > guild.Value.plat_ar && mmr < guild.Value.master_ar){
                        await guildcomms.GetUser(user.Key).RemoveRoleAsync(guildcomms.GetRole(guild.Value.unranked));
                        await guildcomms.GetUser(user.Key).RemoveRoleAsync(guildcomms.GetRole(guild.Value.plat));
                        await guildcomms.GetUser(user.Key).RemoveRoleAsync(guildcomms.GetRole(guild.Value.master));
                        await guildcomms.GetUser(user.Key).AddRoleAsync(guildcomms.GetRole(guild.Value.diamond));
                    }
                    //master
                    else if(mmr > guild.Value.diamond_ar && mmr < guild.Value.gm_ar){
                        await guildcomms.GetUser(user.Key).RemoveRoleAsync(guildcomms.GetRole(guild.Value.unranked));
                        await guildcomms.GetUser(user.Key).RemoveRoleAsync(guildcomms.GetRole(guild.Value.diamond));
                        await guildcomms.GetUser(user.Key).RemoveRoleAsync(guildcomms.GetRole(guild.Value.gm));
                        await guildcomms.GetUser(user.Key).AddRoleAsync(guildcomms.GetRole(guild.Value.master));
                    }
                    //grand master
                    else if(mmr > guild.Value.master_ar){
                        await guildcomms.GetUser(user.Key).RemoveRoleAsync(guildcomms.GetRole(guild.Value.unranked));
                        await guildcomms.GetUser(user.Key).RemoveRoleAsync(guildcomms.GetRole(guild.Value.master));
                        await guildcomms.GetUser(user.Key).AddRoleAsync(guildcomms.GetRole(guild.Value.gm));
                    }
                    else{
                        await guildcomms.GetUser(user.Key).RemoveRoleAsync(guildcomms.GetRole(guild.Value.bronze));
                        await guildcomms.GetUser(user.Key).RemoveRoleAsync(guildcomms.GetRole(guild.Value.silver));
                        await guildcomms.GetUser(user.Key).RemoveRoleAsync(guildcomms.GetRole(guild.Value.gold));
                        await guildcomms.GetUser(user.Key).RemoveRoleAsync(guildcomms.GetRole(guild.Value.plat));
                        await guildcomms.GetUser(user.Key).RemoveRoleAsync(guildcomms.GetRole(guild.Value.diamond));
                        await guildcomms.GetUser(user.Key).RemoveRoleAsync(guildcomms.GetRole(guild.Value.master));
                        await guildcomms.GetUser(user.Key).RemoveRoleAsync(guildcomms.GetRole(guild.Value.gm));
                        await guildcomms.GetUser(user.Key).AddRoleAsync(guildcomms.GetRole(guild.Value.unranked));
                    }

                }
            }
            //return Task.CompletedTask;
        }
/*
        //changes config file on the fly
        public static Task WriteConfig(string key, string value){
            string json = File.ReadAllText("config.json");
            dynamic jsonObj = Newtonsoft.Json.JsonConvert.DeserializeObject(json);
            jsonObj[key] = value;
            string output = Newtonsoft.Json.JsonConvert.SerializeObject(jsonObj, Newtonsoft.Json.Formatting.Indented);
            File.WriteAllText("config.json", output);
            return Task.CompletedTask;
        }

        //
        public static void CastIt<T>(object value, out T target){
            target = (T) Convert.ChangeType(value, typeof(T));
        }
*/
    }
}