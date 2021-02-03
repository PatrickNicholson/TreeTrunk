namespace TreeTrunk.DataObjects{
    public class Profile{
        public ulong id;
        public string name;
        public float sr_score;
        public float voice_total;
        public float voice_start;
        public float voice_end;
        public double num_messages;
        public float last_active;

        public Profile(ulong Id, string Name){
            id = Id;
            name = Name;
        }
        public Profile(ulong Id){
            id = Id;
        }

    }
}