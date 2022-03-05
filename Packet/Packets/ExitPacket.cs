using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Challenges_App.Packet.Packets
{
    public class ExitPacket : IPacket
    {

        private String exit = "Non spécifiée";

        public ExitPacket(String reason)
        {
            exit = reason;
        }

        public ExitPacket(JObject json)
        {
            if (json.ContainsKey("Reason"))
            {
                exit = json.Value<String>("Reason");
            }
        }

        public String getExitReason()
        {
            return exit;
        }

        int IPacket.ID { get { return -1; } set { } }

        JObject IPacket.getPacket()
        {
            JObject json = new JObject();
            json["Reason"] = exit;
            return json;
        }
    }
}
