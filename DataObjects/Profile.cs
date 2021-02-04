using System;
namespace TreeTrunk.DataObjects{
    public class Profile{
        public ulong id;
        public int sr_score;
        public ulong voice_total;
        public DateTime voice_start;
        public DateTime voice_end;
        public ulong num_messages;
        public DateTime last_active;
        public int points_earned;

        public Profile(ulong Id){
            num_messages = 0;
            voice_total = 0;
            sr_score = 1;
            id = Id;
            points_earned = 0;
        }
    }
}