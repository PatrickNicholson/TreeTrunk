using System.Collections.Generic;
using System;

namespace TreeTrunk.DataObjects{
    public class GuildData{
        public ulong guildID;
        public Dictionary<string, object> storeddata = new Dictionary<string, object>();
        

        public GuildData(ulong guildId){
            guildID = guildId;
        }

    }
}