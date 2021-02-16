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
        public DateTime started_playing;
        public DateTime last_active;
        public DateTime voice_start;
        public DateTime voiceshare_start;
        public DateTime voiceshare_end;
        public DateTime deafened_start;
        public float deafened_timespan;
        public float voiceshare_timespan;
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
            started_playing = DateTime.MinValue;
            last_active = DateTime.MinValue;
            voice_start = DateTime.MinValue;
            voiceshare_start = DateTime.MinValue;
            voiceshare_end = DateTime.MinValue;
            voiceshare_timespan = 0;
            deafened_start = DateTime.MinValue;
            deafened_timespan = 0;
        }
    }
}