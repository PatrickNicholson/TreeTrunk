using System;
namespace TreeTrunk.DataObjects{
    public class Profile{
        public string name;
        public ulong id;
        public ulong money;
        public double voiceminutetotal;
        public double messagetotal;
        public int activityrating;
        public int points_earned;
        public DateTime last_message;
        public DateTime started_playing;
        public DateTime last_active;
        public DateTime voice_start;
        public DateTime share_start;
        public float voice_timespan;
        public float share_timespan;
        public readonly DateTime profilecreated;

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
            share_timespan = 0;
            voice_timespan = 0;
        }
    }
}