using System;
using System.Collections.Generic;

namespace Challenges_App.Packet
{
    public class CallbackManager
    {
        public delegate void Handle(IPacket packet);
        private int id = 0;
        private Dictionary<Int32, Handle> callbacks = new Dictionary<Int32, Handle>();

        public void addCallback(IPacket packet, Handle callback)
        {
            id++;
            packet.ID = id;
            callbacks.Add(id, callback);
        }

        public Boolean call(IPacket packet)
        {
            if (packet.ID != -1 && callbacks.ContainsKey(packet.ID))
            {
                Handle h = callbacks[packet.ID];
                callbacks.Remove(packet.ID);
                Ressource.synchronize(() =>
                {
                    h(packet);
                });
                return true;
            }
            return false;
        }
    }
}
