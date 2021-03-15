using System.Threading;

namespace Lidgren.Network
{
	internal sealed class NetUnreliableSenderChannel : NetSenderChannelBase
	{
		private NetConnection m_connection;

		private int m_windowStart;

		private int m_windowSize;

		private int m_sendStart;

		private NetBitVector m_receivedAcks;

		internal override int WindowSize => m_windowSize;

		internal NetUnreliableSenderChannel(NetConnection connection, int windowSize)
		{
			m_connection = connection;
			m_windowSize = windowSize;
			m_windowStart = 0;
			m_sendStart = 0;
			m_receivedAcks = new NetBitVector(1024);
			m_queuedSends = new NetQueue<NetOutgoingMessage>(8);
		}

		internal override int GetAllowedSends()
		{
			return m_windowSize - (m_sendStart + 1024 - m_windowStart) % m_windowSize;
		}

		internal override void Reset()
		{
			m_receivedAcks.Clear();
			m_queuedSends.Clear();
			m_windowStart = 0;
			m_sendStart = 0;
		}

		internal override NetSendResult Enqueue(NetOutgoingMessage message)
		{
			int num = m_queuedSends.Count + 1;
			int num2 = m_windowSize - (m_sendStart + 1024 - m_windowStart) % 1024;
			if (num > num2)
			{
				return NetSendResult.Dropped;
			}
			m_queuedSends.Enqueue(message);
			return NetSendResult.Sent;
		}

		internal override void SendQueuedMessages(float now)
		{
			int num = GetAllowedSends();
			if (num >= 1)
			{
				while (m_queuedSends.Count > 0 && num > 0)
				{
					if (m_queuedSends.TryDequeue(out NetOutgoingMessage item))
					{
						ExecuteSend(now, item);
					}
					num--;
				}
			}
		}

		private void ExecuteSend(float now, NetOutgoingMessage message)
		{
			int sendStart = m_sendStart;
			m_sendStart = (m_sendStart + 1) % 1024;
			m_connection.QueueSendMessage(message, sendStart);
			Interlocked.Decrement(ref message.m_recyclingCount);
			if (message.m_recyclingCount <= 0)
			{
				m_connection.m_peer.Recycle(message);
			}
		}

		internal override void ReceiveAcknowledge(float now, int seqNr)
		{
			int num = NetUtility.RelativeSequenceNumber(seqNr, m_windowStart);
			if (num >= 0)
			{
				if (num == 0)
				{
					m_receivedAcks[m_windowStart] = false;
					m_windowStart = (m_windowStart + 1) % 1024;
				}
				else
				{
					m_receivedAcks[seqNr] = true;
					while (m_windowStart != seqNr)
					{
						m_receivedAcks[m_windowStart] = false;
						m_windowStart = (m_windowStart + 1) % 1024;
					}
				}
			}
		}
	}
}
