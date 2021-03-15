using System;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace SKYNET.Network
{
    public class UdpListener
    {
        public delegate void ExceptionThrownDelegate(object sender, ExceptionEventArgs e);

        public delegate void PacketReceivedDelegate(object sender, PacketReceivedEventArgs e);

        private readonly byte[] _buffer = new byte[4096];

        private readonly IPEndPoint _localEndPoint;

        private bool _running;

        private Socket _socket;

        public int Port
        {
            get;
        }

        public bool EnableBroadcast
        {
            get;
            set;
        }

        public event PacketReceivedDelegate PacketReceived;

        public event ExceptionThrownDelegate ExceptionThrown;

        public UdpListener(IPAddress ip, int port)
        {
            Port = port;
            _localEndPoint = new IPEndPoint(ip, port);
            _running = false;
        }

        public UdpListener(int port)
        {
            Port = port;
            _localEndPoint = new IPEndPoint(IPAddress.Any, port);
            _running = false;
        }

        private void CreateSocket()
        {
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            _socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, optionValue: true);
            if (EnableBroadcast)
            {
                _socket.EnableBroadcast = true;
            }
            _socket.Bind(_localEndPoint);
        }

        protected void OnPacketReceived(IPEndPoint endPoint, MemoryStream dataStream)
        {
            this.PacketReceived?.Invoke(this, new PacketReceivedEventArgs(endPoint, dataStream));
        }

        protected virtual void OnExceptionThrown(ExceptionEventArgs e)
        {
            this.ExceptionThrown?.Invoke(this, e);
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                Socket socket = (Socket)ar.AsyncState;
                EndPoint endPoint = new IPEndPoint(IPAddress.Any, 0);
                if (_running)
                {
                    int num = socket.EndReceiveFrom(ar, ref endPoint);
                    if (num == 0 && _running)
                    {
                        StartReceiving();
                    }
                    else if (_running)
                    {
                        using (MemoryStream dataStream = RecyclableStreams.Manager.GetStream("udp_listener_packet", _buffer, 0, num))
                        {
                            OnPacketReceived((IPEndPoint)endPoint, dataStream);
                        }
                        StartReceiving();
                    }
                }
            }
            catch (ObjectDisposedException)
            {
                StartReceiving(restart: true);
            }
            catch (SocketException)
            {
                StartReceiving(restart: true);
            }
            catch (Exception ex3)
            {
                OnExceptionThrown(new ExceptionEventArgs(ex3));
                StartReceiving();
            }
        }

        public void Send(byte[] data, IPEndPoint endpoint)
        {
            try
            {
                _socket.BeginSendTo(data, 0, data.Length, SocketFlags.None, endpoint, delegate (IAsyncResult ar)
                {
                    try
                    {
                        _socket.EndSendTo(ar);
                    }
                    catch
                    {
                    }
                }, null);
            }
            catch (ObjectDisposedException)
            {
            }
            catch (Exception ex2)
            {
                OnExceptionThrown(new ExceptionEventArgs(ex2));
            }
        }

        public void Send(byte[] data, int offset, int length, IPEndPoint endpoint)
        {
            try
            {
                _socket.BeginSendTo(data, offset, length, SocketFlags.None, endpoint, delegate (IAsyncResult ar)
                {
                    try
                    {
                        _socket.EndSendTo(ar);
                    }
                    catch
                    {
                    }
                }, null);
            }
            catch (ObjectDisposedException)
            {
            }
            catch (Exception ex2)
            {
                OnExceptionThrown(new ExceptionEventArgs(ex2));
            }
        }

        public bool Start()
        {
            if (_running)
            {
                return false;
            }
            _running = true;
            try
            {
                CreateSocket();
                StartReceiving();
            }
            catch
            {
                return false;
            }
            return true;
        }

        private void StartReceiving(bool restart = false)
        {
            if (_running)
            {
                try
                {
                    if (restart)
                    {
                        try
                        {
                            _socket.Close();
                        }
                        catch
                        {
                        }
                        CreateSocket();
                    }
                    EndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
                    _socket.BeginReceiveFrom(_buffer, 0, _buffer.Length, SocketFlags.None, ref remoteEP, ReceiveCallback, _socket);
                }
                catch (ObjectDisposedException)
                {
                    StartReceiving(restart: true);
                }
                catch (SocketException)
                {
                    StartReceiving(restart: true);
                }
                catch (Exception ex3)
                {
                    OnExceptionThrown(new ExceptionEventArgs(ex3));
                    StartReceiving(restart: true);
                }
            }
        }

        public void Stop()
        {
            if (_running)
            {
                _running = false;
                try
                {
                    _socket.Shutdown(SocketShutdown.Receive);
                    _socket.Close();
                    _socket.Dispose();
                }
                catch
                {
                }
            }
        }
    }
}

