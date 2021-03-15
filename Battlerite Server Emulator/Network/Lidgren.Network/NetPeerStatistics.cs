using System.Diagnostics;
using System.Text;

namespace Lidgren.Network
{
	public sealed class NetPeerStatistics
	{
		private readonly NetPeer m_peer;

		internal int m_sentPackets;

		internal int m_receivedPackets;

		internal int m_sentMessages;

		internal int m_receivedMessages;

		internal int m_sentBytes;

		internal int m_receivedBytes;

		internal long m_bytesAllocated;

		public int SentPackets => m_sentPackets;

		public int ReceivedPackets => m_receivedPackets;

		public int SentMessages => m_sentMessages;

		public int ReceivedMessages => m_receivedMessages;

		public int SentBytes => m_sentBytes;

		public int ReceivedBytes => m_receivedBytes;

		public long StorageBytesAllocated => m_bytesAllocated;

		public int BytesInRecyclePool => m_peer.m_storagePoolBytes;

		internal NetPeerStatistics(NetPeer peer)
		{
			m_peer = peer;
			Reset();
		}

		internal void Reset()
		{
			m_sentPackets = 0;
			m_receivedPackets = 0;
			m_sentMessages = 0;
			m_receivedMessages = 0;
			m_sentBytes = 0;
			m_receivedBytes = 0;
			m_bytesAllocated = 0L;
		}

		[Conditional("DEBUG")]
		internal void PacketSent(int numBytes, int numMessages)
		{
			m_sentPackets++;
			m_sentBytes += numBytes;
			m_sentMessages += numMessages;
		}

		[Conditional("DEBUG")]
		internal void PacketReceived(int numBytes, int numMessages)
		{
			m_receivedPackets++;
			m_receivedBytes += numBytes;
			m_receivedMessages += numMessages;
		}

		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine(m_peer.ConnectionsCount.ToString() + " connections");
			stringBuilder.AppendLine("Sent " + m_sentBytes + " bytes in " + m_sentMessages + " messages in " + m_sentPackets + " packets");
			stringBuilder.AppendLine("Received " + m_receivedBytes + " bytes in " + m_receivedMessages + " messages in " + m_receivedPackets + " packets");
			stringBuilder.AppendLine("Storage allocated " + m_bytesAllocated + " bytes");
			stringBuilder.AppendLine("Recycled pool " + m_peer.m_storagePoolBytes + " bytes");
			return stringBuilder.ToString();
		}
	}
}
