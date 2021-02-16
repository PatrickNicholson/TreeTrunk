using System.Collections.Generic;
using System;

namespace TreeTrunk.DataObjects{
//will implement a database later
    public class GuildData_Example{
        public ulong guildID;

        //quick fix start
        public string prefix;

        public ulong streamrole;

        public ulong modchat;
        public ulong unranked;
        public ulong bronze;
        public ulong silver;
        public ulong gold;
        public ulong plat;
        public ulong diamond;
        public ulong master;
        public ulong gm;

        public int bronze_ar;
        public int silver_ar;
        public int gold_ar;
        public int plat_ar;
        public int diamond_ar;
        public int master_ar;
        public int gm_ar;

        public int mshort;
        public int mlong;
        public int embeds;
        public int attach;
        public float vactive;
        public float vstream;
        public int decay;
        public int armax;
        public int armin;
        public int reactbuff;
        public int reactbuff_limit;

        public TimeSpan placementtime;

        public Dictionary<ulong, Profile> usermanager = new Dictionary<ulong, Profile>();
        public Dictionary<ulong, ulong> channelmessages = new Dictionary<ulong, ulong>();
        public Dictionary<ulong, ulong> voicechatminutes = new Dictionary<ulong, ulong>();
        public Dictionary<string, ulong> gameactivity = new Dictionary<string, ulong>();
        //quick fix end

        //public Dictionary<string, object> storeddata = new Dictionary<string, object>();
    
        public GuildData_Example(ulong guildId){
            guildID = guildId;
            
            //quick fix start
            prefix = "!";

            streamrole = 0;

            unranked = 0;
            bronze = 0;
            silver = 0;
            gold = 0;
            plat = 0;
            diamond = 0;
            master = 0;
            gm = 0;
            modchat = 0;

            bronze_ar = 0;
            silver_ar = 0;
            gold_ar = 0;
            plat_ar = 0;
            diamond_ar = 0;
            master_ar = 0;
            gm_ar = 0;

            mshort = 0;
            mlong = 0;
            embeds = 0;
            attach = 0;
            vactive = 0; //every 60min
            vstream = 0; //every 60min
            decay = 0;
            armax = 0;
            armin = 0;
            reactbuff = 0;
            reactbuff_limit = 0;

            placementtime = new TimeSpan(1,0,0);
            //quick fix end
        }

    }
}