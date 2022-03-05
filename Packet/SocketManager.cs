using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Challenges_App.Packet
{
    public class SocketManager
    {
        private String addr;
        private int port;
        private CallbackManager callbackManager;
        private PacketReceivedEvent receiveEvent;
        private List<IPacket> queue = new List<IPacket>();
        private Socket socket;
        public Boolean isOnline = false; //Est capable d'envoyer et recevoir via la méthode sendPacket et receive
        public Boolean isSessionValid = false; //Est une session valide grâce à la clé du joueur (Client et server side)
        private long lastReceived = -1;
        private long lastSent = -1;

        public SocketManager(String addr, int port)
        {
            this.callbackManager = new CallbackManager();
            this.receiveEvent = new PacketReceivedEvent();
            this.addr = addr;
            this.port = port;
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);          
            try
            {
                socket.Connect(addr, port);
                isOnline = true;
                new Thread(() => listen()).Start();
                sendQueue();
                keepAlive();
            }catch (Exception ex)
            {
                MessageBox.Show("Une erreur est intervenue lors de la connexion au serveur.\nUne session a-elle été crée ?\n\nErreur: "+ex.Message, "Server error connexion");
                stop();
            }
        }

        public void queuePacket(IPacket packet, CallbackManager.Handle? callback)
        {
            if (!isOnline)
            {
                MessageBox.Show("Impossible d'envoyer un packet: le serveur n'est pas à l'écoute.", "Error not ready");
                return;
            }
            if(!isSessionValid && packet is not Packets.LoginPacket && packet is not Packets.KeepAlivePacket && packet is not Packets.ExitPacket)
            {
                MessageBox.Show("Impossible d'envoyer un packet: la session actuelle n'est pas valide.", "Error invalid session");
                return;
            }
            if(callback != null)
            {
                callbackManager.addCallback(packet, callback);
            }
            queue.Add(packet);
        }

        private void send(JObject json)
        {
            byte[] b = Encoding.UTF8.GetBytes(json.ToString());
            byte[] size = BitConverter.GetBytes(b.Length);
            Array.Reverse(size);
            try
            {
                socket.Send(size, 0, size.Length, SocketFlags.None);
                socket.Send(b, 0, b.Length, SocketFlags.None);
            }catch (Exception ex)
            {
                stop();
                MessageBox.Show("La connexion avec le serveur semble avoir été interrompue.\n" + ex.Message, "Error can't send socket");
            }

        }

        private byte[] read(int size)
        {
            byte[] buf = new byte[size];
            int offset = 0;//offset pour écrire dans le tableau de byte "buf"
            int bytesLeft = size;
            try
            {
                while (bytesLeft > 0)
                {
                    int bRead = socket.Receive(buf, offset, bytesLeft, SocketFlags.None);
                    offset += bRead;
                    bytesLeft -= bRead;
                }
            }catch(Exception ex)
            {
                stop();
                MessageBox.Show("La connexion avec le serveur semble avoir été interrompue.\n" + ex.Message, "Error can't receive socket");
            }
            return buf;
        }

        private byte[] read()
        {
            byte[] header = read(4);
            Array.Reverse(header);
            return read(BitConverter.ToInt32(header, 0));
        }

        private IPacket? receive(byte[] b)
        {
            JObject json = JObject.Parse(Encoding.UTF8.GetString(b));
            IPacket? packet = IPacket.fromJson(json);
            if (packet != null && MainWindow.Instance.menu != null)
            {
                MainWindow.Instance.menu.getLogger().addText(LogManager.LogType.IN, packet.GetType().Name);
            }
            if (packet == null || callbackManager.call(packet)) //Ne retourne pas le packet pour receiveEvent si un callback existe
            {
                return null;
            }
            return packet;
        }

        public void sendStop(String reason)
        {
            queuePacket(new Packets.ExitPacket(reason), null);
        }

        public void stop()
        {
            isOnline = false;
            MainWindow.Instance.init(false);
        }

        private async void sendQueue()
        {
            while (isOnline)
            {
                if(queue.Count > 0)
                {
                    IPacket packet = queue[0];
                    queue.RemoveAt(0);
                    if (packet != null)
                    {
                        lastSent = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                        send(IPacket.toJson(packet));
                        if (packet is not Packets.KeepAlivePacket && MainWindow.Instance.menu != null)
                        {
                            MainWindow.Instance.menu.getLogger().addText(LogManager.LogType.OUT, packet.GetType().Name);
                        }
                        if (packet is Packets.ExitPacket)
                        {
                            stop();
                        }
                    }
                }
                await Task.Delay(1);
            }
        }

        private void listen()
        {
            while (isOnline)
            {
                int data = socket.Available;
                if (data > 0)
                {
                    IPacket? packet = receive(read());
                    lastReceived = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                    if (packet != null)//Le packet sera null ici si un callback s'occupe déjà du packet
                    {
                        Ressource.synchronize(() => receiveEvent.receive(packet));                 
                    }
                }
            }
            socket.Close();
        }

        private async void keepAlive()
        {
            while (isOnline)
            {
                await Task.Delay(5000);
                if (isOnline)//Peut crash durant le Delay(5000);
                {
                    queuePacket(new Packets.KeepAlivePacket(), null);
                }
            }
        }

        public String getAdress()
        {
            return addr;
        }

        public long getLastReceived()
        {
            return lastReceived;
        }

        public long getLastSent()
        {
            return lastSent;
        }
    }
}
