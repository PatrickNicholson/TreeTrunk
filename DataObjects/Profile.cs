using System;
namespace TreeTrunk.DataObjects{
    public class Profile{
        public string name;
        public ulong id;
        public ulong money;
        public ulong voiceminutetotal;
        public ulong messagetotal;
        public ulong reactions;
        public int activityrating;
        public int points_earned;
        public DateTime last_message;
        public DateTime started_playing;
        public DateTime last_active;
        public DateTime voice_start;
        public DateTime share_start;
        public DateTime profilecreated;
        public DateTime total_voiceminute_marker;

        public Profile(ulong Id, string Name){
            name = Name;
            id = Id;
            money = 0;
            voiceminutetotal = 0;
            messagetotal = 0;
            activityrating = 1;
            points_earned = 0;
            profilecreated = DateTime.Now;
            last_message = DateTime.MinValue;
            started_playing = DateTime.MinValue;
            last_active = DateTime.MinValue;
            voice_start = DateTime.MinValue;
            share_start = DateTime.MinValue;
            total_voiceminute_marker = DateTime.MinValue;
        }

    }
}