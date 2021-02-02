using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Collections.Generic;
using TreeTrunk.DataObjects;

namespace TreeTrunk{
    public static class StaticFunctions{

        public static Dictionary<ulong, GuildData> data = new Dictionary<ulong, GuildData>();
        
        
        public static Task LoadGuildData(){
            string json = File.ReadAllText("data.json");
            data = JsonConvert.DeserializeObject<Dictionary<ulong, GuildData>>(json);
            return Task.CompletedTask;
        }

        public static Task WriteGuildData(){
            string json = JsonConvert.SerializeObject(data, Formatting.Indented);
            File.WriteAllText("data.json", json);
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