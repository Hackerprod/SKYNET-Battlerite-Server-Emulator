using System.Diagnostics;
using System.Text;

namespace Lidgren.Network
{
	public sealed class NetConnectionStatistics
	{
		private readonly NetConnection m_connection;

		internal int m_sentPackets;

		internal int m_receivedPackets;

		internal int m_sentMessages;

		internal int m_receivedMessages;

		internal int m_sentBytes;

		internal int m_receivedBytes;

		internal int m_resentMessagesDueToDelay;

		internal int m_resentMessagesDueToHole;

		public int SentPackets => m_sentPackets;

		public int ReceivedPackets => m_receivedPackets;

		public int SentBytes => m_sentBytes;

		public int ReceivedBytes => m_receivedBytes;

		public int ResentMessages => m_resentMessagesDueToHole + m_resentMessagesDueToDelay;

		internal NetConnectionStatistics(NetConnection conn)
		{
			m_connection = conn;
			Reset();
		}

		internal void Reset()
		{
			m_sentPackets = 0;
			m_receivedPackets = 0;
			m_sentBytes = 0;
			m_receivedBytes = 0;
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

		[Conditional("DEBUG")]
		internal void MessageResent(MessageResendReason reason)
		{
			if (reason == MessageResendReason.Delay)
			{
				m_resentMessagesDueToDelay++;
			}
			else
			{
				m_resentMessagesDueToHole++;
			}
		}

		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("Sent " + m_sentBytes + " bytes in " + m_sentMessages + " messages in " + m_sentPackets + " packets");
			stringBuilder.AppendLine("Received " + m_receivedBytes + " bytes in " + m_receivedMessages + " messages in " + m_receivedPackets + " packets");
			if (m_resentMessagesDueToDelay > 0)
			{
				stringBuilder.AppendLine("Resent messages (delay): " + m_resentMessagesDueToDelay);
			}
			if (m_resentMessagesDueToDelay > 0)
			{
				stringBuilder.AppendLine("Resent messages (holes): " + m_resentMessagesDueToHole);
			}
			int num = 0;
			int num2 = 0;
			NetSenderChannelBase[] sendChannels = m_connection.m_sendChannels;
			foreach (NetSenderChannelBase netSenderChannelBase in sendChannels)
			{
				if (netSenderChannelBase != null)
				{
					num += netSenderChannelBase.m_queuedSends.Count;
					NetReliableSenderChannel netReliableSenderChannel = netSenderChannelBase as NetReliableSenderChannel;
					if (netReliableSenderChannel != null)
					{
						for (int j = 0; j < netReliableSenderChannel.m_storedMessages.Length; j++)
						{
							if (netReliableSenderChannel.m_storedMessages[j].Message != null)
							{
								num2++;
							}
						}
					}
				}
			}
			int num3 = 0;
			NetReceiverChannelBase[] receiveChannels = m_connection.m_receiveChannels;
			foreach (NetReceiverChannelBase netReceiverChannelBase in receiveChannels)
			{
				NetReliableOrderedReceiver netReliableOrderedReceiver = netReceiverChannelBase as NetReliableOrderedReceiver;
				if (netReliableOrderedReceiver != null)
				{
					for (int l = 0; l < netReliableOrderedReceiver.m_withheldMessages.Length; l++)
					{
						if (netReliableOrderedReceiver.m_withheldMessages[l] != null)
						{
							num3++;
						}
					}
				}
			}
			stringBuilder.AppendLine("Unsent messages: " + num);
			stringBuilder.AppendLine("Stored messages: " + num2);
			stringBuilder.AppendLine("Withheld messages: " + num3);
			return stringBuilder.ToString();
		}
	}
}
