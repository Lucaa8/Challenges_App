using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Challenges_App.Packet.Packets
{
    public class ChallengesListPacket : IPacket
    {

        private Dictionary<String, String> challenges = new Dictionary<String, String>();
        private Dictionary<String, String> categories = new Dictionary<String, String>();

        public ChallengesListPacket() {}

        public ChallengesListPacket(JObject json)
        {
            if (json.ContainsKey("ID"))
            {
                id = json.Value<Int32>("ID");
            }
            if (json.ContainsKey("Challenges"))
            {
                JArray jarr = json["Challenges"] as JArray;
                Dictionary<String, String> temp = new Dictionary<String, String>();
                foreach (JObject jobj in jarr)
                {
                    temp.Add(jobj.Value<String>("UUID"), jobj.Value<String>("Name"));
                }
                foreach (KeyValuePair<string, string> item in temp.OrderBy(key => key.Value))
                {
                    challenges.Add(item.Key, item.Value);
                }
            }
            if (json.ContainsKey("Categories"))
            {
                JArray jarr = json["Categories"] as JArray;
                Dictionary<String, String> temp = new Dictionary<String, String>();
                foreach (JObject jobj in jarr)
                {
                    temp.Add(jobj.Value<String>("UUID"), jobj.Value<String>("Name"));
                }
                foreach (KeyValuePair<string, string> item in temp.OrderBy(key => key.Value))
                {
                    categories.Add(item.Key, item.Value);
                }
            }
        }

        public Dictionary<String,String> getChallenges()
        {
            return challenges;
        }

        public Dictionary<String,String> getCategories()
        {
            return categories;
        }

        private int id = -1;
        int IPacket.ID { get{ return id; } set{ id = value; } }

        JObject IPacket.getPacket()
        {
            JObject json = new JObject();
            if (id != -1)
            {
                json["ID"] = id;
            }
            return json;
        }
    }
}
