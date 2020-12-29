using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Collections.Generic;
using TreeTrunk.DataObjects;

namespace TreeTrunk.StaticFunc{
    public static class StaticFunctions{

        public static GuildData ReadGuildData(ulong guildID){
            var json = File.ReadAllText("data.json");
            Dictionary<ulong, GuildData> data = JsonConvert.DeserializeObject<Dictionary<ulong, GuildData>>(json);
            return data[guildID];
        }
        public static Task WriteGuildData(GuildData data){
            var json = File.ReadAllText("data.json");
            Dictionary<ulong, GuildData> alldata = JsonConvert.DeserializeObject<Dictionary<ulong, GuildData>>(json);

            if(alldata.ContainsKey(data.guild_id)){
                alldata[data.guild_id] = data;
            }
            else{
                alldata.Add(data.guild_id, data);
            }

            string json2 = JsonConvert.SerializeObject(alldata, Formatting.Indented);
            File.WriteAllText("data.json", json2);
            return Task.CompletedTask;
        }


        public static Task WriteConfig(string key, string value){
            string json = File.ReadAllText("config.json");
            dynamic jsonObj = Newtonsoft.Json.JsonConvert.DeserializeObject(json);
            jsonObj[key] = value;
            string output = Newtonsoft.Json.JsonConvert.SerializeObject(jsonObj, Newtonsoft.Json.Formatting.Indented);
            File.WriteAllText("config.json", output);
            return Task.CompletedTask;
        }
    }
}