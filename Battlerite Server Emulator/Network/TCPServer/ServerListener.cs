using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace SKYNET
{
	internal class ServerListener
	{
		private TcpListenerEx _listener;

		private List<TcpClient> _connectedClients = new List<TcpClient>();

		private List<TcpClient> _disconnectedClients = new List<TcpClient>();

		private TcpServer _parent = null;

		private List<byte> _queuedMsg = new List<byte>();

		private Thread _rxThread = null;



		public int ConnectedClientsCount => _connectedClients.Count;

		public IEnumerable<TcpClient> ConnectedClients => _connectedClients;

		internal bool QueueStop
		{
			get;
			set;
		}

		internal IPAddress IPAddress
		{
			get;
			private set;
		}

		internal int Port
		{
			get;
			private set;
		}

		internal int ReadLoopIntervalMs
		{
			get;
			set;
		}

		internal TcpListenerEx Listener => _listener;

		internal ServerListener(TcpServer parentServer, IPAddress ipAddress, int port)
		{
            Lenght = new List<byte>();
            Message = new List<byte>();
            ToLenght = true;

            QueueStop = false;
			_parent = parentServer;
			IPAddress = ipAddress;
			Port = port;
			ReadLoopIntervalMs = 10;
			_listener = new TcpListenerEx(ipAddress, port);
			_listener.Start();
			ThreadPool.QueueUserWorkItem(ListenerLoop);
		}

		private void StartThread()
		{
			if (_rxThread == null)
			{
				_rxThread = new Thread(ListenerLoop);
				_rxThread.IsBackground = true;
				_rxThread.Start();
			}
		}

		private void ListenerLoop(object state)
		{
			while (!QueueStop)
			{
				try
				{
					RunLoopStep();
				}
				catch
				{
				}
				Thread.Sleep(ReadLoopIntervalMs);
			}
			_listener.Stop();
		}

		private bool IsSocketConnected(Socket s)
		{
			bool flag = s.Poll(1000, SelectMode.SelectRead);
			bool flag2 = s.Available == 0;
			if ((flag && flag2) || !s.Connected)
			{
				return false;
			}
			return true;
		}

		private void RunLoopStep()
		{
			if (_disconnectedClients.Count > 0)
			{
				TcpClient[] array = _disconnectedClients.ToArray();
				_disconnectedClients.Clear();
				TcpClient[] array2 = array;
				foreach (TcpClient tcpClient in array2)
				{
					_connectedClients.Remove(tcpClient);
					_parent.NotifyClientDisconnected(this, tcpClient);
				}
			}
			if (_listener.Pending())
			{
				TcpClient tcpClient2 = _listener.AcceptTcpClient();
				_connectedClients.Add(tcpClient2);
				_parent.NotifyClientConnected(this, tcpClient2);
			}

			foreach (TcpClient connectedClient in _connectedClients)
			{
				if (!IsSocketConnected(connectedClient.Client))
				{
					_disconnectedClients.Add(connectedClient);
				}
				if (connectedClient.Available != 0)
				{
					List<byte> list = new List<byte>();
					while (connectedClient.Available > 0 && connectedClient.Connected)
					{
						byte[] array3 = new byte[1];
						connectedClient.Client.Receive(array3, 0, 1, SocketFlags.None);
						list.AddRange(array3);

                        ProcessByte(array3, connectedClient);

                        _queuedMsg.AddRange(array3);
                    }
                    ToLenght = true;
                    if (list.Count > 0)
					{
						_parent.NotifyEndTransmissionRx(this, connectedClient, list.ToArray());
					}
				}
			}
		}
        bool ToLenght;
        int MessageLenght;
        List<byte> Lenght;
        List<byte> Message;
        private void ProcessByte(byte[] Byte, TcpClient connectedClient)
        {
            if (ToLenght)
            {
                Lenght.AddRange(Byte);
            }
            else
                Message.AddRange(Byte);

            if (Lenght.Count == 5)
            {
                string length = Encoding.Default.GetString(Lenght.ToArray());
                if (int.TryParse(length, out int lenght))
                {
                    MessageLenght = lenght;
                    ToLenght = false;
                    Lenght.Clear();
                }
            }
            if (Message.Count == MessageLenght)
            {
                ToLenght = true;
                _parent.NotifyGCMessage(this, connectedClient, Message.ToArray());
                Message.Clear();
            }
            if (Lenght.Count > 5)
            {
                ToLenght = false;
                Lenght.Clear();
            }
            if (Message.Count > MessageLenght)
            {
                ToLenght = true;
                Message.Clear();
            }

        }
    }
}
