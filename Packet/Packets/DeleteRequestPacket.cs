using Newtonsoft.Json.Linq;
using System;

namespace Challenges_App.Packet.Packets
{
    public class DeleteRequestPacket : IPacket
    {

        private String uuid;
        private IPacket.Target target;

        private Boolean success;
        private String? errorMsg;

        public DeleteRequestPacket(String uuid, IPacket.Target target)
        {
            this.uuid = uuid;
            this.target = target;
        }

        public DeleteRequestPacket(JObject json)
        {
            if (json.ContainsKey("ID"))
            {
                id = json.Value<Int32>("ID");
            }
            success = json.Value<Boolean>("Success");
            if (!success && json.ContainsKey("ErrorMsg"))
            {
                errorMsg = json.Value<String>("ErrorMsg");
            }
        }

        private int id = -1;
        int IPacket.ID { get { return id; } set { id = value; } }

        public Boolean withSuccess()
        {
            return success;
        }

        public String? getErrorMsg()
        {
            return errorMsg;
        }

        JObject IPacket.getPacket()
        {
            JObject json = new JObject();
            if (id != -1)
            {
                json["ID"] = id;
            }
            json["UUID"] = uuid;
            json["Type"] = target.ToString();
            return json;
        }
    }
}
