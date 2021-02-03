using System.Collections.Generic;
using System;

namespace TreeTrunk.DataObjects{
    public class GuildData{
        public ulong guild_id;
        public string name;
        public ulong starboard_channel;
        public ulong streaming_role;
        public Dictionary<ulong, Profile> users = new Dictionary<ulong, Profile>();
        public Dictionary<ulong, TextChannels> textchannels = new Dictionary<ulong, TextChannels>();
        public Dictionary<ulong, Roles> roles = new Dictionary<ulong, Roles>();
        public Dictionary<ulong, string> starboard = new Dictionary<ulong, string>();

        public GuildData(ulong GuildId, string Name, IServiceProvider services){
            guild_id = GuildId;
            name = Name;
        }

    }
}