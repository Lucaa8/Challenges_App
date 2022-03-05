using Newtonsoft.Json.Linq;
using System;

namespace Challenges_App.Packet.Packets
{
    public class UpdateRequestPacket : IPacket
    {

        private String uuid;
        private IPacket.Target target;
        private JObject json;

        public UpdateRequestPacket(String uuid, IPacket.Target target, JObject json)
        {
            this.uuid = uuid;
            this.target = target;
            this.json = json;
        }

        int IPacket.ID { get { return -1; } set { } }

        JObject IPacket.getPacket()
        {
            JObject j = new JObject();
            j["UUID"] = uuid;
            j["Type"] = target.ToString();
            j["JSON"] = json.ToString();
            return j;
        }
    }
}
