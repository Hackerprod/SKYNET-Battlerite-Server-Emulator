using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace SKYNET
{
    public class TcpServer
    {
        private List<ServerListener> _listeners = new List<ServerListener>();

        private Stopwatch _debugInfoTime;

        public bool IsStarted => _listeners.Any((ServerListener l) => l.Listener.Active);

        public int ConnectedClientsCount => _listeners.Sum((ServerListener l) => l.ConnectedClientsCount);

        public event EventHandler<TcpClient> ClientConnected;

        public event EventHandler<TcpClient> ClientDisconnected;

        public event EventHandler<TCPMessage> DelimiterDataReceived;

        public event EventHandler<TCPMessage> DataReceived;
        public event EventHandler<TCPMessage> GCMessageReceived;

        public TcpServer()
        {
        }

        public IEnumerable<IPAddress> GetIPAddresses()
        {
            List<IPAddress> list = new List<IPAddress>();
            IEnumerable<NetworkInterface> enumerable = from nic in NetworkInterface.GetAllNetworkInterfaces()
                                                       where nic.OperationalStatus == OperationalStatus.Up
                                                       select nic;
            foreach (NetworkInterface item in enumerable)
            {
                IPInterfaceProperties iPProperties = item.GetIPProperties();
                foreach (UnicastIPAddressInformation unicastAddress in iPProperties.UnicastAddresses)
                {
                    if (!list.Contains(unicastAddress.Address))
                    {
                        list.Add(unicastAddress.Address);
                    }
                }
            }
            return (from ip in list
                    orderby RankIpAddress(ip) descending
                    select ip).ToList();
        }

        public List<IPAddress> GetListeningIPs()
        {
            List<IPAddress> list = new List<IPAddress>();
            foreach (ServerListener listener in _listeners)
            {
                if (!list.Contains(listener.IPAddress))
                {
                    list.Add(listener.IPAddress);
                }
            }
            return (from ip in list
                    orderby RankIpAddress(ip) descending
                    select ip).ToList();
        }

        public void Broadcast(byte[] data)
        {
            foreach (TcpClient item in _listeners.SelectMany((ServerListener x) => x.ConnectedClients))
            {
                item.GetStream().Write(data, 0, data.Length);
            }
        }

        private int RankIpAddress(IPAddress addr)
        {
            int num = 1000;
            if (IPAddress.IsLoopback(addr))
            {
                num = 300;
            }
            else if (addr.AddressFamily == AddressFamily.InterNetwork)
            {
                num += 100;
                if (addr.GetAddressBytes().Take(2).SequenceEqual(new byte[2]
                {
                    169,
                    254
                }))
                {
                    num = 0;
                }
            }
            if (num > 500)
            {
                foreach (NetworkInterface item in TryGetCurrentNetworkInterfaces())
                {
                    IPInterfaceProperties iPProperties = item.GetIPProperties();
                    if (iPProperties.GatewayAddresses.Any())
                    {
                        if (iPProperties.UnicastAddresses.Any((UnicastIPAddressInformation u) => u.Address.Equals(addr)))
                        {
                            num += 1000;
                        }
                        break;
                    }
                }
            }
            return num;
        }

        private static IEnumerable<NetworkInterface> TryGetCurrentNetworkInterfaces()
        {
            try
            {
                return from ni in NetworkInterface.GetAllNetworkInterfaces()
                       where ni.OperationalStatus == OperationalStatus.Up
                       select ni;
            }
            catch (NetworkInformationException)
            {
                return Enumerable.Empty<NetworkInterface>();
            }
        }

        public bool Start(int port, bool ignoreNicsWithOccupiedPorts = true)
        {
            IEnumerable<IPAddress> iPAddresses = GetIPAddresses();
            bool flag = false;
            foreach (IPAddress item in iPAddresses)
            {
                try
                {
                    Start(item, port);
                }
                catch (SocketException ex)
                {
                    DebugInfo(ex.ToString());
                    flag = true;
                    return false;
                }
            }
            if (!IsStarted)
            {
                return false;
            }
            if (flag && !ignoreNicsWithOccupiedPorts)
            {
                Stop();
                return false;
            }
            return true;
        }

        public TcpServer Start(int port, AddressFamily addressFamilyFilter)
        {
            IEnumerable<IPAddress> enumerable = from ip in GetIPAddresses()
                                                where ip.AddressFamily == addressFamilyFilter
                                                select ip;
            foreach (IPAddress item in enumerable)
            {
                try
                {
                    Start(item, port);
                }
                catch
                {
                }
            }
            return this;
        }

        public TcpServer Start(IPAddress ipAddress, int port)
        {
            ServerListener item = new ServerListener(this, ipAddress, port);
            _listeners.Add(item);
            return this;
        }

        public void Stop()
        {
            _listeners.All((ServerListener l) => l.QueueStop = true);
            while (_listeners.Any((ServerListener l) => l.Listener.Active))
            {
                Thread.Sleep(100);
            }
            _listeners.Clear();
        }

        internal void NotifyEndTransmissionRx(ServerListener listener, TcpClient client, byte[] msg)
        {
            if (this.DataReceived != null)
            {
                TCPMessage e = new TCPMessage(msg, client);
                this.DataReceived(this, e);
            }
        }
        internal void NotifyGCMessage(ServerListener listener, TcpClient client, byte[] msg)
        {
            if (this.GCMessageReceived != null)
            {
                TCPMessage e = new TCPMessage(msg, client);
                this.GCMessageReceived(this, e);
            }
        }

        internal void NotifyClientConnected(ServerListener listener, TcpClient newClient)
        {
            if (this.ClientConnected != null)
            {
                this.ClientConnected(this, newClient);
            }
        }

        internal void NotifyClientDisconnected(ServerListener listener, TcpClient disconnectedClient)
        {
            if (this.ClientDisconnected != null)
            {
                this.ClientDisconnected(this, disconnectedClient);
            }
        }

        [Conditional("DEBUG")]
        private void DebugInfo(string format, params object[] args)
        {
            if (_debugInfoTime == null)
            {
                _debugInfoTime = new Stopwatch();
                _debugInfoTime.Start();
            }
            Debug.WriteLine(_debugInfoTime.ElapsedMilliseconds + ": " + format, args);
        }
    }
}
