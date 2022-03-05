using Newtonsoft.Json.Linq;
using System;

namespace Challenges_App.Packet.Packets
{
    public class LoginPacket : IPacket
    { 

        //OUT
        private String key;

        //IN
        private Boolean allowed = false;
        private String? reason = "unspecifed";

        private int id = -1;
        int IPacket.ID { get { return id; } set { id = value; } }

        //OUT
        public LoginPacket(String key)
        {
            this.key = key;
        }

        //IN
        public LoginPacket(JObject json)
        {
            if (json.ContainsKey("ID"))
            {
                id = json.Value<Int32>("ID");
            }
            allowed = json.ContainsKey("Allowed") && json.Value<Boolean>("Allowed");
            if (!allowed && json.ContainsKey("Reason"))
            {
                reason = json.Value<String>("Reason");
            }
        }

        //OUT
        JObject IPacket.getPacket()
        {
            JObject json = new JObject();
            if (id != -1)
            {
                json["ID"] = id;
            }
            json["Key"] = key;
            json["Version"] = MainWindow.v.ToString();
            return json;
        }

        //IN
        public Boolean isAllowed()
        {
            return allowed;
        }
        public String? getReason()
        {
            return reason;
        }
    }
}
