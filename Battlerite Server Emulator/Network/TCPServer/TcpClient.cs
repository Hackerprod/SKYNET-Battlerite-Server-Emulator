using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace SKYNET
{
	public class TCPClient : IDisposable
	{
        public event EventHandler ConnectionStatusChanged;

        private Thread ConnectionMonitoring = null;

        private Thread _rxThread = null;

		private List<byte> _queuedMsg = new List<byte>();

		private TcpClient _client = null;

		private bool disposedValue = false;

        private bool ConnectState = false;
        public bool Connected
        {
            get
            {
                bool flag = TcpClient.IsConnected();
                bool flag2 = flag == this.ConnectState;
                bool result;
                if (flag2)
                {
                    result = this.ConnectState;
                }
                else
                {
                    this.ConnectState = flag;
                    EventHandler connectionStatusChanged = this.ConnectionStatusChanged;
                    if (connectionStatusChanged != null)
                    {
                        connectionStatusChanged(this, null);
                    }
                    result = flag;
                }
                return result;
            }
            private set
            {
                bool flag = value == this.ConnectState;
                if (!flag)
                {
                    this.ConnectState = value;
                    EventHandler connectionStatusChanged = this.ConnectionStatusChanged;
                    if (connectionStatusChanged != null)
                    {
                        connectionStatusChanged(this, null);
                    }
                }
            }
        }
        internal bool QueueStop
		{
			get;
			set;
		}

		internal int ReadLoopIntervalMs
		{
			get;
			set;
		}

		public bool AutoTrimStrings
		{
			get;
			set;
		}

		public TcpClient TcpClient => _client;

		public event EventHandler<TCPMessage> DelimiterDataReceived;

		public event EventHandler<TCPMessage> DataReceived;

		public TCPClient()
		{
			ReadLoopIntervalMs = 10;
		}

		public bool Connect(string hostNameOrIpAddress, int port)
		{
			if (string.IsNullOrEmpty(hostNameOrIpAddress))
			{
                return false;
			}
			_client = new TcpClient();
            try
            {
                _client.Connect(hostNameOrIpAddress, port);
                StartRxThread();
                this.Connected = true;
                this.ConnectionMonitoring = new Thread(new ThreadStart(this.MonitorConnection));
                this.ConnectionMonitoring.Start();
                return true;
            }
            catch
            {
            }
            return false;
        }

		private void StartRxThread()
		{
			if (_rxThread == null)
			{
				_rxThread = new Thread(ListenerLoop);
				_rxThread.IsBackground = true;
				_rxThread.Start();
			}
		}

		public TCPClient Disconnect()
		{
			if (_client == null)
			{
				return this;
			}
			_client.Close();
			_client = null;
            this.Connected = false;
            return this;
		}
        private void MonitorConnection()
        {
            while (this.Connected)
            {
                Thread.Sleep(100);
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
			_rxThread = null;
		}

		private void RunLoopStep()
		{
			if (_client != null && _client.Connected)
			{
				TcpClient client = _client;
				if (client.Available == 0)
				{
					Thread.Sleep(10);
				}
				else
				{
					List<byte> list = new List<byte>();
					while (client.Available > 0 && client.Connected)
					{
						byte[] array = new byte[1];
						client.Client.Receive(array, 0, 1, SocketFlags.None);
						list.AddRange(array);

                        _queuedMsg.AddRange(array);
                    }
					if (list.Count > 0)
					{
						NotifyEndTransmissionRx(client, list.ToArray());
					}
				}
			}
		}

		private void NotifyDelimiterMessageRx(TcpClient client, byte[] msg)
		{
			if (this.DelimiterDataReceived != null)
			{
				TCPMessage e = new TCPMessage(msg, client);
				this.DelimiterDataReceived(this, e);
			}
		}

		private void NotifyEndTransmissionRx(TcpClient client, byte[] msg)
		{
			if (this.DataReceived != null)
			{
				TCPMessage e = new TCPMessage(msg, client);
				this.DataReceived(this, e);
			}
		}

		public void Write(byte[] data)
		{
			if (_client == null)
			{
				throw new Exception("Cannot send data to a null TcpClient (check to see if Connect was called)");
			}
			_client.GetStream().Write(data, 0, data.Length);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
				}
				QueueStop = true;
				if (_client != null)
				{
					try
					{
						_client.Close();
					}
					catch
					{
					}
					_client = null;
				}
				disposedValue = true;
			}
		}

		public void Dispose()
		{
			Dispose(disposing: true);
		}
	}
}
