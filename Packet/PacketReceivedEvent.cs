using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Challenges_App.Packet
{
    public class PacketReceivedEvent
    {
        public void receive(IPacket packet)
        {
            if (packet is Packets.ExitPacket)
            {
                MainWindow.Instance.packetManager.stop();
                MessageBox.Show("Le serveur termine la connexion. Raison: " + ((Packets.ExitPacket)packet).getExitReason(), "Stopping communication");
            }
            if(packet is Packets.ChallengeStatePacket)
            {
                if (MainWindow.Instance.menu != null)
                {
                    MainWindow.Instance.menu.ChallengesState = ((Packets.ChallengeStatePacket)packet).getState();
                }
            }
        }

    }
}
