using Newtonsoft.Json.Linq;
using System;

namespace Challenges_App.Packet
{
    public interface IPacket
    {
        public enum Target {
            CATEGORY, CHALLENGE
        }

        public int ID { get; set; }

        JObject getPacket();

        public static IPacket? fromJson(JObject json)
        {
            if (json.ContainsKey("Type")&&json.ContainsKey("Body"))
            {
                return (IPacket)Activator.CreateInstance(Type.GetType(json.Value<String>("Type")), json["Body"] as JObject);
            }
            return null;
        }

        public static JObject toJson(IPacket packet)
        {
            JObject json = new JObject();
            json["Type"] = packet.GetType().ToString();
            json["Body"] = packet.getPacket();
            return json;
        }

    }
}
