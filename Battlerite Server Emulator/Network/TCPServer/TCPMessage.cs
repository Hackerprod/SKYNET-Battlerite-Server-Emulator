using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace SKYNET
{
	public class TCPMessage
	{
		private TcpClient _tcpClient;

		public byte[] Data
		{
			get;
			private set;
		}

		public TcpClient TcpClient => _tcpClient;

		internal TCPMessage(byte[] data, TcpClient tcpClient)
		{
			Data = data;
			_tcpClient = tcpClient;
		}


		public void Reply(byte[] data)
		{
			_tcpClient.GetStream().Write(data, 0, data.Length);
		}
	}
}
