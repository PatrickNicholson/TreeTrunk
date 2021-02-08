using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using TreeTrunk.DataObjects;
using System.Collections.Concurrent;
using Discord;
using System;
using Discord.Commands;
using System.Linq;


namespace TreeTrunk{
    public static class StaticFunctions{

        public static ConcurrentDictionary<ulong, GuildData> data = new ConcurrentDictionary<ulong, GuildData>();
        
        
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
        
        public static Task LoadGuildData(){
            string json = File.ReadAllText("data.json");            
            data = JsonConvert.DeserializeObject<ConcurrentDictionary<ulong, GuildData>>(json);
            data = data ?? new ConcurrentDictionary<ulong, GuildData>();
            return Task.CompletedTask;
        }

        public static Task WriteGuildData(){
            File.WriteAllText("data.json", JsonConvert.SerializeObject(data, Formatting.None));
            return Task.CompletedTask;
        }

        public static Task WriteConfig(string key, string value){
            string json = File.ReadAllText("config.json");
            dynamic jsonObj = Newtonsoft.Json.JsonConvert.DeserializeObject(json);
            jsonObj[key] = value;
            string output = Newtonsoft.Json.JsonConvert.SerializeObject(jsonObj, Newtonsoft.Json.Formatting.None);
            File.WriteAllText("config.json", output);
            return Task.CompletedTask;
        }

        public static async void retro(SocketCommandContext context){
            var guild_id = context.Guild.Id;
            var textchannels = context.Client.GetGuild(guild_id).TextChannels;
            IMessage message_index = null;
            foreach(var textchannel in textchannels){
                
                if(message_index == null){
                    foreach(var message in await textchannel.GetMessagesAsync(1).FlattenAsync()){
                        message_index = message;
                    }
                }
                IMessage last_message = message_index;
                //Console.WriteLine(textchannel.Name.ToString());

                int leng = 1;
                int count = 1;
                while(leng > 0){
                    var messages = await textchannel.GetMessagesAsync(last_message, Direction.Before, 100).FlattenAsync();
                    //Console.WriteLine(messages.Count());
                    foreach( var message in messages){
                        //Console.WriteLine(message.Author.ToString());
                        
                        //Console.WriteLine(message);
                        last_message = message;
                    }
                    //Console.WriteLine(last_message);
                    
                    leng = messages.Count();
                    count += leng;
                    
                }
                Console.WriteLine(textchannel.Name.ToString() + " " + count.ToString());
                message_index = null;
                
                    
            }
        }
        public static void CastIt<T>(object value, out T target){
            target = (T) Convert.ChangeType(value, typeof(T));
        }

    }
}