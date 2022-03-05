using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;

namespace Challenges_App.Packet.Packets
{
    public class InfoRequestPacket : IPacket
    {
        //OUT
        private String uuid;
        private IPacket.Target target;

        //IN
        private Boolean _doesExist;
        private JObject json;

        public InfoRequestPacket(String uuid, IPacket.Target target)
        {
            this.uuid = uuid;
            this.target = target;
        }

        public InfoRequestPacket(JObject json)
        {
            if (json.ContainsKey("ID"))
            {
                id = json.Value<Int32>("ID");
            }
            _doesExist = json.Value<Boolean>("Exists");
            if(_doesExist && json.ContainsKey("JSON"))
            {
                this.json = JObject.Parse(json.Value<String>("JSON"));
            }
        }

        private int id = -1;
        int IPacket.ID { get { return id; } set { id = value; } }

        JObject IPacket.getPacket()
        {
            JObject j = new JObject();
            if (id != -1)
            {
                j["ID"] = id;
            }
            j["UUID"] = uuid;
            j["Type"] = target.ToString();
            return j;
        }

        public Boolean doesExist()
        {
            return _doesExist;
        }

        public JObject getJson()
        {
            return json;
        }
    }
}
