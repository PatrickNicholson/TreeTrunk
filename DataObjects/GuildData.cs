using System.Collections.Generic;

namespace TreeTrunk.DataObjects{
    public class GuildData{
        public ulong guild_id;
        public string name;
        public Dictionary<ulong, Profile> users = new Dictionary<ulong, Profile>();
        public Dictionary<ulong, TextChannels> textchannels = new Dictionary<ulong, TextChannels>();

        public GuildData(ulong GuildId, string Name){
            guild_id = GuildId;
            name = Name;
        }

    }
}