namespace TreeTrunk.DataObjects{
    public class Profile{
        public ulong id;
        public string name;
        public float sr_score;
        public float voice_minutes;
        public double num_messages;

        public Profile(ulong Id, string Name){
            id = Id;
            name = Name;
        }
        public Profile(ulong Id){
            id = Id;
        }


        public void AddSR(float num){
            sr_score += num;
        }
        public void AddSR(int num){
            sr_score += num;
        }
        public void AddVMinutes(float time){
            voice_minutes += time;
        }
        public void AddVMinutes(int time){
            voice_minutes += time;
        }
        public void AddMessageCount(int message){
            num_messages += message;
        }

    }
}