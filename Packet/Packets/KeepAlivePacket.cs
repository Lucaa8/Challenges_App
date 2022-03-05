using Newtonsoft.Json.Linq;

namespace Challenges_App.Packet.Packets
{
    public class KeepAlivePacket : IPacket
    {
        public KeepAlivePacket()
        {

        }

        public KeepAlivePacket(JObject json)
        {

        }

        int IPacket.ID { get { return -1; } set { } }

        JObject IPacket.getPacket()
        {
            return new JObject();
        }
    }
}
