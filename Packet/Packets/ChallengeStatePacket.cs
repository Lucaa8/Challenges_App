using Newtonsoft.Json.Linq;
using System;

namespace Challenges_App.Packet.Packets
{
    public class ChallengeStatePacket : IPacket
    {
        //Used into MainMenu.cs class with method toggleChallengesState
        public delegate void Callback(Boolean state);

        public enum Type
        {
            INFO, UPDATE
        }

        private Type type = Type.INFO;
        private Boolean state;

        public ChallengeStatePacket(Type type, Boolean updatedState)
        {
            this.type = type;
            if (type == Type.UPDATE)
            {
                state = updatedState;
            }
        }

        public ChallengeStatePacket(JObject json)
        {
            if (json.ContainsKey("ID"))
            {
                id = json.Value<Int32>("ID");
            }
            state = json.Value<Boolean>("State");
        }

        public Boolean getState()
        {
            return state;
        }

        private int id = -1;
        int IPacket.ID { get { return id; }  set { id = value; } }

        JObject IPacket.getPacket()
        {
            JObject json = new JObject();
            if (id != -1)
            {
                json["ID"] = id;
            }
            json["Type"] = type.ToString();
            if(type == Type.UPDATE)
            {
                json["State"] = state;
            }
            return json;
        }
    }
}
