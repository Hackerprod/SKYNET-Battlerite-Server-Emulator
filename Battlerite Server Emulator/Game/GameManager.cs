using Lidgren.Network;
using StunNetwork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SKYNET
{
    public class GameManager : GameHandler
    {
        ILog ilog = BattleriteServer.ilog;
        private enum LobbyStatus
        {
            Waiting = 0,
            Starting = 1
        }

        private LobbyStatus status = LobbyStatus.Waiting;

        private readonly IdManager idMan = new IdManager();

        private readonly List<Player> players = new List<Player>();
        private readonly PlayerConnectionLookup connections = new PlayerConnectionLookup();
        private readonly Player me;

        private void startGame()
        {
            var connectionsToKill = this.connections
                .Where(c => c.Status != NetConnectionStatus.Connected)
                .ToList();
            foreach (var connection in connectionsToKill)
            {
                var id = this.connections[connection];
                connection.Disconnect("");
                this.players.RemoveAt(this.players.FindIndex(p => p.ID == id));
            }

            if (this.connections.Count > 0)
            {
                var startMessage = this.server.CreateMessage();
                startMessage.Write((byte)LobbyMessageType.StartGameBuilding);
                //this.server.SendMessage(startMessage, this.connections, NetDeliveryMethod.ReliableOrdered, 0);
            }

            //this.Stopped(new BuildGameHandler(this.server, new PlayerLookup(this.players), this.connections, this.me.ID));
        }


        public void onClientStatusChanged(NetIncomingMessage message)
        {
            switch (message.SenderConnection.Status)
            {
                case NetConnectionStatus.Connected:
                    {
                        this.onClientFullyConnected(message.SenderConnection);
                        break;
                    }
                case NetConnectionStatus.Disconnected:
                    {
                        ilog.Warn("Player disconnected, we cannot handle that yet!");
                        ilog.Warn("Prepare for unforeseen consequences.");
                        break;
                    }
            }
        }

        private void onClientFullyConnected(NetConnection connection)
        {
            var newPlayerId = this.connections[connection];
            var newPlayer = this.players.First(p => p.ID == newPlayerId);

            var otherClientConnections = this.connections
                .Where(c => c != connection)
                .Where(c => c.Status == NetConnectionStatus.Connected)
                .ToList();

            // tell other clients about this player
            if (otherClientConnections.Count > 0)
            {
                var newPlayerMessage = this.server.CreateMessage();
                newPlayerMessage.Write((byte)LobbyMessageType.NewPlayer);
                //newPlayerMessage.Write(newPlayer.ID.Simple);
                newPlayerMessage.Write(newPlayer.ID.value);
                newPlayerMessage.Write(newPlayer.Name);

                this.server.SendMessage(newPlayerMessage, otherClientConnections, NetDeliveryMethod.ReliableOrdered, 0);
            }

            var allOthersMessage = this.server.CreateMessage();
            allOthersMessage.Write((byte)LobbyMessageType.NewPlayers);
            allOthersMessage.Write((byte)(otherClientConnections.Count + 1));
            foreach (var p in this.players.Where(p => p.ID != newPlayerId)
                .Where(p =>
                {
                    var c = this.connections[p.ID];
                    return c == null || c.Status == NetConnectionStatus.Connected;
                }))
            {
                // collect all players
                allOthersMessage.Write(p.ID.value);
                //allOthersMessage.Write(p.ID.Simple);
                allOthersMessage.Write(p.Name);
            }
            // tell this client about other players
            connection.SendMessage(allOthersMessage, NetDeliveryMethod.ReliableOrdered, 0);

        }

        public void tryApproveClient(NetServer server, NetConnection senderConnection, NetIncomingMessage message)
        {
            string name;
            name = message.ReadString();
            {
                var trimmed = name.Trim();
                if (trimmed != "" && trimmed == name)
                {
                    this.approveClient(server, senderConnection, name);
                    return;
                }
            }
            message.SenderConnection.Deny();
        }
        private void approveClient(NetServer server, NetConnection connection, string name)
        {
            int playerId = 1000;
            var p = new Player(this.idMan.GetNext<Player>(), name);
            this.players.Add(p);
            this.connections.Add(p.ID, connection);

            var message = server.CreateMessage(4);
            //message.Write(p.ID.Simple);
            NetBufferIn struct37_0 = new NetBufferIn();
            message.Write(struct37_0.LengthBits);
            message.Write(struct37_0.Data, 0, struct37_0.LengthBytes);

            connection.Approve(message);
        }



        public void ProcessData(NetIncomingMessage msg)
        {
            int num = msg.ReadInt32();
            int num2 = (int)Math.Ceiling((double)num / 8.0);

            ilog.Error(num);
            ilog.Error(num2);

            msg.ReadBytes(msg.m_data, 0, num2);
            NetBufferIn netBufferIn = new NetBufferIn(msg.m_data, num, 0);
            MessageType messageType = (MessageType)netBufferIn.ReadRangedInteger(0, 63);

            switch (messageType)
            {
                case MessageType.Game:

                    ilog.Error(netBufferIn.ReadString());
                    //if (this.bool_0)
                    //{
                    //    //if (this.stack_0.Count == 0)
                    //    //{
                    //    //    this.stack_0.Push(new byte[32768]);
                    //    //    ilog.Warn("Allocated new packet received buffer. _DelayedPackets.Count: " + this.list_2.Count);
                    //    //}
                    //    //byte[] array = this.stack_0.Pop();
                    //    Array.Copy(netBufferIn.Data, array, netBufferIn.LengthBytes);
                    //    NetBufferIn item = new NetBufferIn(array, netBufferIn.LengthBits, netBufferIn.Position);
                    //    for (int i = this.list_2.Count - 1; i >= 0; i--)
                    //    {
                    //        if (this.list_2[i].Data == null)
                    //        {
                    //            this.list_2.RemoveAt(i);
                    //        IL_116:
                    //            if (this.list_2.Count >= 2)
                    //            {
                    //                NetBufferIn netBufferIn = this.list_2[0];
                    //                this.stack_0.Push(netBufferIn.Data);
                    //                this.list_2.RemoveAt(0);
                    //            }
                    //            this.list_2.Add(item);
                    //            return;
                    //        }
                    //    }
                    //    goto IL_116;
                    //}
                    //this.int_1++;
                    //if (!netBufferIn.ReadBoolean())
                    //{
                    //    return;
                    //}
                    //using (StunProfiler.Sample("DeserializeGameState"))
                    //{
                    //    this.method_13(ref netBufferIn);
                    //    return;
                    //}
                    
                    break;
                case MessageType.Debug:
                    break;
                case MessageType.GameMessage:
                    //               this.method_12(ref netBufferIn, ref bool_2);
                    return;
                case MessageType.ConnectData:
                    //                this.method_9(ref netBufferIn);
                    return;
                case MessageType.DisconnectData:
                    //                this.method_10(ref netBufferIn);
                    return;
                case MessageType.AuthResponseOk:
                    return;
                default:
                    ilog.Error("Invalid data received. MessageType: " + messageType);
                    return;
            }

        }

    }
}
