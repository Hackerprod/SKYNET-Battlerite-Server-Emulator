using Newtonsoft.Json;
using SKYNET.Network;
using SKYNET.Network.Example;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using WebSocketSharp.Server;

namespace SKYNET
{
    public class ConnectionsManager
    {
        ILog ilog;
        public ConcurrentDictionary<ulong, IConnection> TCPConnectionsWithSteamId;
        WebSocketServer webSocketServer;
        private WebApi wApi;
        public ConnectionsManager()
        {
            this.TCPConnectionsWithSteamId = new ConcurrentDictionary<ulong, IConnection>();
            ilog = BattleriteServer.ilog;
        }

        public int CountConnections { get; set; }
        internal void Start()
        {
            webSocketServer = new WebSocketServer(26000);
            webSocketServer.WebSocketServices.AddService<WebSocketProcessor>("/connect", (Action<WebSocketProcessor>)null);
            webSocketServer.Start();

            wApi = new WebApi(25000);
            wApi.Start();

            LidgrenServer server = new LidgrenServer();

            TcpServer Tcpserver = new TcpServer();
            Tcpserver.ClientConnected += Tcp_ClientConnected;
            Tcpserver.ClientDisconnected += Tcp_ClientDisconnected;
            Tcpserver.DataReceived += Tcp_DataReceived;
            if (Tcpserver.Start(28000))
            {
                BattleriteServer.ilog.Info($"TCP server started successfully on port 28000");
            }
            else BattleriteServer.ilog.Info($"Error starting TCP server on port 28000");

            UDPserver Udpserver = new UDPserver();
            if (Udpserver.Start("10.31.0.2", 30000))
            {
                BattleriteServer.ilog.Info($"UDP server started successfully on port 30000");
            }
            else BattleriteServer.ilog.Info($"Error starting UDP server on port 30000");

            //XmppServer xmppServer = new XmppServer();
            //if (xmppServer.Start())
            //{
            //    BattleriteServer.ilog.Info($"Xmpp Server started successfully on port 5222");
            //}
            //else BattleriteServer.ilog.Info($"Error starting Xmpp Server on port 5222");
        }

        private object method_2(HttpRequestMessage httpRequestMessage_0)
        {
            ulong steamId;
            if (ulong.TryParse((from pair in httpRequestMessage_0.GetQueryNameValuePairs()
                                where pair.Key == "steamid"
                                select pair.Value).FirstOrDefault<string>(), out steamId))
            {
                return JsonConvert.SerializeObject(/*this.Stats.GetServerRealtimeStats(steamId)*/ "", Formatting.Indented, new JsonSerializerSettings
                {
                    //ContractResolver = new ShouldSerializeContractResolver(true)
                });
            }
            return null;
        }
        private void Tcp_DataReceived(object sender, TCPMessage e)
        {
            BattleriteServer.ilog.Info($"Received msg \"{ Encoding.UTF8.GetString(e.Data)}\", {e.Data.Length} bytes");
        }

        private void Tcp_ClientDisconnected(object sender, System.Net.Sockets.TcpClient e)
        {
            BattleriteServer.ilog.Info($"TCP Client disconnected from {e.Client.RemoteEndPoint}");
        }

        private void Tcp_ClientConnected(object sender, System.Net.Sockets.TcpClient e)
        {
            BattleriteServer.ilog.Info($"New client TCP connection from {e.Client.RemoteEndPoint}");
            new XmppSeverConnection(e.Client);
        }

        public void AddOrUpdate(ulong AccountId, IConnection conn)
        {
            if (AccountId == 0) return;

            if (TCPConnectionsWithSteamId.TryGetValue(AccountId, out IConnection cmConnection))
            {
                this.TCPConnectionsWithSteamId[AccountId] = cmConnection;
            }
            else
            {
                TCPConnectionsWithSteamId.TryAdd(AccountId, conn);
            }
        }
        public IConnection Get(ulong steamId)
        {
            this.TCPConnectionsWithSteamId.TryGetValue(steamId, out IConnection result);
            return result;
        }
        public void Remove(IConnection connection)
        {
            IConnection conn = null;

            ulong num = (from c in this.TCPConnectionsWithSteamId where object.Equals(c.Value.RemoteEndPoint, connection.RemoteEndPoint) select c.Key).FirstOrDefault<ulong>();
            if (num == 0UL)
            {
                return;
            }
            if (this.TCPConnectionsWithSteamId.TryRemove(num, out conn))
            {
                ilog.Info(string.Format("Connection for {0} was removed successfully.", num));
            }
            else
            {
                ilog.Info(string.Format("Connection for {0} was not found.", num));
            }
            //Por mi
            /*User user = Dota2GameCoordinator.Instance.DbManager.Users.GetBySteamId(num);
            if (user != null)
            {
                user.LastLogoff = (uint)DateHelpers.DateTimeToUnixTime(DateTime.Now);
                Dota2GameCoordinator.Instance.DbManager.Users.SetPlayingState(user.SteamId, false);
            }*/
        }
        public void Remove(ulong steamId)
        {
            if (steamId == 0) return;
            IConnection cmConnection;
            bool flag = this.TCPConnectionsWithSteamId.TryRemove(steamId, out cmConnection);
            ilog.Info(flag ? string.Format("Connection for {0} was removed successfully.", steamId) : string.Format("Connection for {0} was not found.", steamId));
        }
        public void ForEach(Action<IConnection> action)
        {
            foreach (KeyValuePair<ulong, IConnection> keyValuePair in this.TCPConnectionsWithSteamId)
            {
                IConnection value = keyValuePair.Value;
                if (value != null && value.Connected)
                {
                    action(value);
                }
            }
        }
        public void ForEach(IEnumerable<ulong> steamIds, Action<ulong, IConnection> action)
        {
            foreach (ulong num in steamIds.ToArray<ulong>())
            {
                IConnection cmConnection;
                if (this.TCPConnectionsWithSteamId.TryGetValue(num, out cmConnection) && cmConnection != null && cmConnection.Connected)
                {
                    action(num, this.TCPConnectionsWithSteamId[num]);
                }
            }
        }
        public bool IsConnected(ulong steamId)
        {
            if (steamId == 0UL)
            {
                return false;
            }
            IConnection result;
            if (!this.TCPConnectionsWithSteamId.TryGetValue(steamId, out result))
            {
                return false;
            }
            return true;
        }
        public IConnection GetByAccountId(ulong steamId)
        {
            if (steamId == 0UL)
            {
                return null;
            }
            IConnection result;
            if (!this.TCPConnectionsWithSteamId.TryGetValue(steamId, out result))
            {
                return null;
            }
            return result;
        }

        public void Send(ulong steamId, byte[] msg)
        {
            IConnection bySteamId = this.GetByAccountId(steamId);
            if (bySteamId == null)
            {
                return;
            }
            bySteamId.Send(msg);
        }
    }
}