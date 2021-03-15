using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Lidgren.Network;
using StunNetwork;

namespace SKYNET.Network
{
    public class LidgrenServer : GameHandler
    {
        GameManager gameManager;
        
        Dictionary<long, IPEndPoint[]> registeredHosts = new Dictionary<long, IPEndPoint[]>();
        

        public LidgrenServer()
        {
            

            NetPeerConfiguration config = new NetPeerConfiguration("ProjectH");
            config.LocalAddress = IPAddress.Parse("10.31.0.2");
            config.Port = 29000;

            //config.SetMessageTypeEnabled(Lidgren.NetIncomingMessageType.UnconnectedData, true);
            //config.SetMessageTypeEnabled(Lidgren.NetIncomingMessageType.DiscoveryResponse, true);
            //config.SetMessageTypeEnabled(Lidgren.NetIncomingMessageType.DiscoveryRequest, true);
            //config.SetMessageTypeEnabled(Lidgren.NetIncomingMessageType.ConnectionApproval, true);
            //config.SetMessageTypeEnabled(Lidgren.NetIncomingMessageType.StatusChanged, true);
            
            config.EnableMessageType(NetIncomingMessageType.UnconnectedData);
            config.EnableMessageType(NetIncomingMessageType.DiscoveryResponse);
            config.EnableMessageType(NetIncomingMessageType.DiscoveryRequest);
            config.EnableMessageType(NetIncomingMessageType.ConnectionApproval);
            config.EnableMessageType(NetIncomingMessageType.StatusChanged);
            config.EnableMessageType(NetIncomingMessageType.NatIntroductionSuccess);
            config.EnableMessageType(NetIncomingMessageType.Receipt);

            base.server = new NetServer(config);
            config.AcceptIncomingConnections = true;
            server.Start();

            gameManager = new GameManager();

            Thread packets = new Thread(new ThreadStart(this.PacketProcessor));
            packets.Start();

            

            //Thread receiveThread = new Thread(Receive) { IsBackground = true };
            //receiveThread.Start();


            //NetConnection con = Client.Connect(new IPEndPoint(IPAddress.Parse("10.31.0.2"), 27000));
            //ilog.Error(con.Status);
        }

        private void PacketProcessor()
        {
            for (; ; )
            {
                Update();
                System.Threading.Thread.Sleep(50);  // temp code
            }
        }
        private void Update()
        {
            NetIncomingMessage msg;
            while ((msg = base.server.ReadMessage()) != null)
            {
                long senderUid = msg.SenderConnection == null ? 0 : msg.SenderConnection.RemoteUniqueIdentifier;
                string senderIpAddress = msg.SenderEndpoint?.Address.ToString();
                BattleriteServer.ilog.Info("UDP Msg received [" + msg.m_incomingMessageType + "], ");

                switch (msg.MessageType)
                {
                    case NetIncomingMessageType.ConnectionApproval:
                        //gameManager.tryApproveClient(server, msg.SenderConnection, msg.SenderConnection.m_remoteHailMessage);
                        break;
                    case NetIncomingMessageType.ErrorMessage:
                        BattleriteServer.ilog.Info(msg.ReadString());
                        break;
                    case NetIncomingMessageType.Data:
                        //ProcessData(msg);
                        break;
                    case NetIncomingMessageType.StatusChanged:
                        //gameManager.onClientStatusChanged(msg.SenderConnection.m_remoteHailMessage);
                        ilog.Debug(msg.SenderConnection.Status);
                        break;
                    default:
                        ilog.Error("Unhandled type: " + msg.MessageType + "; Read string: " + msg.ReadString());
                        break;
                }
                server.Recycle(msg);
            }
        }

    }

}
