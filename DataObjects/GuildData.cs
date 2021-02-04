using System.Collections.Generic;
using System;

namespace TreeTrunk.DataObjects{
    public class GuildData{
        public ulong guild_id;
        public ulong starboard;

        public Dictionary<ulong, Profile> users = new Dictionary<ulong, Profile>();
        public Dictionary<ulong, int> textchannel_data = new Dictionary<ulong, int>();
        public Dictionary<ulong, int> voicechannel_data = new Dictionary<ulong, int>();

        public Dictionary<string, object> data = new Dictionary<string, object>();
        //to get different types of data from the data dictionary do: <type> a = data["key1"] as <type>;
        

        public GuildData(ulong GuildId){
            guild_id = GuildId;
        }

    }
}