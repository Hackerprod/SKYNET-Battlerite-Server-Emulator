using System;
using System.Net;
using System.Net.Sockets;

namespace Lidgren.Network
{
	public class PlatformSocket
	{
		private Socket socket;

		public bool Broadcast
		{
			set
			{
				socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, value);
			}
		}

		public int Available => socket.Available;

		public int ReceiveBufferSize
		{
			get
			{
				return socket.ReceiveBufferSize;
			}
			set
			{
				socket.ReceiveBufferSize = value;
			}
		}

		public int SendBufferSize
		{
			get
			{
				return socket.SendBufferSize;
			}
			set
			{
				socket.SendBufferSize = value;
			}
		}

		public bool Blocking
		{
			get
			{
				return socket.Blocking;
			}
			set
			{
				socket.Blocking = value;
			}
		}

		public EndPoint LocalEndPoint => socket.LocalEndPoint;

		public bool IsBound => socket.IsBound;

		public bool DontFragment
		{
			get
			{
				return socket.DontFragment;
			}
			set
			{
				socket.DontFragment = value;
			}
		}

		public PlatformSocket()
		{
			socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
		}

		internal void Bind(EndPoint ep)
		{
			socket.Bind(ep);
		}

		public void Close(int timeout)
		{
			socket.Close(timeout);
		}

		public bool Poll(int microseconds)
		{
			return socket.Poll(microseconds, SelectMode.SelectRead);
		}

		public int ReceiveFrom(byte[] receiveBuffer, int offset, int numBytes, ref EndPoint senderRemote)
		{
			return socket.ReceiveFrom(receiveBuffer, offset, numBytes, SocketFlags.None, ref senderRemote);
		}

		public int SendTo(byte[] data, int offset, int numBytes, EndPoint target)
		{
			return socket.SendTo(data, offset, numBytes, SocketFlags.None, target);
		}

		public void Shutdown(SocketShutdown socketShutdown)
		{
			socket.Shutdown(socketShutdown);
		}

		public void Setup()
		{
		}

		public void EndSendTo(IAsyncResult res)
		{
		}
	}
}
