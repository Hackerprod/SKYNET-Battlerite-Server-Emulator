using Gameplay;
using Gameplay.GameObjects;
using SKYNET.Steam;
using StunNetwork;
using StunShared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SKYNET.Network
{
    public class UDPserver 
    {
        private UdpListener udpListener_0;
        private IPEndPoint ipendPoint_0;
        private static readonly ILog ilog_0 = BattleriteServer.ilog;
        public ServiceStatus Status { get; set; }

        private void PacketReceived(object sender, PacketReceivedEventArgs e)
        {
           // e.Stream.ReadInt32L();

            ipendPoint_0 = e.EndPoint;
            MemoryStream memoryStream_0 = new MemoryStream();
            e.Stream.CopyTo(memoryStream_0);

            byte[] bytes = memoryStream_0.ToArray();
            NetBufferIn netBufferIn_0 = new NetBufferIn(bytes, bytes.Length);
            string msg = netBufferIn_0.ReadString();

            BattleriteServer.ilog.Info("UDP Msg received [" + msg + "], " + bytes.Length + " bytes");
        }
        public bool Start(string host, int port)
        {
            if (ServiceStatus.Online == Status)
            {
                return false;
            }
            try
            {
                udpListener_0 = new UdpListener(IPAddress.Parse(host), port);
                udpListener_0.PacketReceived += PacketReceived;
                udpListener_0.Start();
                Status = ServiceStatus.Online;
            }
            catch
            {
                return false;
            }
            return true;
        }

        public void Stop()
        {
            if (Status == ServiceStatus.Online)
            {
                Status = ServiceStatus.Shuttingdown;
                try
                {
                    udpListener_0.Stop();
                }
                catch
                {
                }
                Status = ServiceStatus.Offline;
                ilog_0.Info((object)"Is Offline");
            }
        }

    }
    public enum ServiceStatus
    {
        Offline,
        Online,
        Shuttingdown
    }
}
