using System.Threading;

namespace Lidgren.Network
{
	internal sealed class NetReliableSenderChannel : NetSenderChannelBase
	{
		private NetConnection m_connection;

		private int m_windowStart;

		private int m_windowSize;

		private int m_sendStart;

		private NetBitVector m_receivedAcks;

		internal NetStoredReliableMessage[] m_storedMessages;

		internal float m_resendDelay;

		internal override int WindowSize => m_windowSize;

		internal NetReliableSenderChannel(NetConnection connection, int windowSize)
		{
			m_connection = connection;
			m_windowSize = windowSize;
			m_windowStart = 0;
			m_sendStart = 0;
			m_receivedAcks = new NetBitVector(1024);
			m_storedMessages = new NetStoredReliableMessage[m_windowSize];
			m_queuedSends = new NetQueue<NetOutgoingMessage>(8);
			m_resendDelay = m_connection.GetResendDelay();
		}

		internal override int GetAllowedSends()
		{
			return m_windowSize - (m_sendStart + 1024 - m_windowStart) % 1024;
		}

		internal override void Reset()
		{
			m_receivedAcks.Clear();
			for (int i = 0; i < m_storedMessages.Length; i++)
			{
				m_storedMessages[i].Reset();
			}
			m_queuedSends.Clear();
			m_windowStart = 0;
			m_sendStart = 0;
		}

		internal override NetSendResult Enqueue(NetOutgoingMessage message)
		{
			m_queuedSends.Enqueue(message);
			int count = m_queuedSends.Count;
			int num = m_windowSize - (m_sendStart + 1024 - m_windowStart) % 1024;
			if (count <= num)
			{
				return NetSendResult.Sent;
			}
			return NetSendResult.Queued;
		}

		internal override void SendQueuedMessages(float now)
		{
			for (int i = 0; i < m_storedMessages.Length; i++)
			{
				NetOutgoingMessage message = m_storedMessages[i].Message;
				if (message != null)
				{
					float lastSent = m_storedMessages[i].LastSent;
					if (lastSent > 0f && now - lastSent > m_resendDelay)
					{
						int num = m_windowStart % m_windowSize;
						int num2 = m_windowStart;
						while (num != i)
						{
							num--;
							if (num < 0)
							{
								num = m_windowSize - 1;
							}
							num2--;
						}
						m_connection.QueueSendMessage(message, num2);
						m_storedMessages[i].LastSent = now;
						m_storedMessages[i].NumSent++;
					}
				}
			}
			int num3 = GetAllowedSends();
			if (num3 >= 1)
			{
				while (m_queuedSends.Count > 0 && num3 > 0)
				{
					if (m_queuedSends.TryDequeue(out NetOutgoingMessage item))
					{
						ExecuteSend(now, item);
					}
					num3--;
				}
			}
		}

		private void ExecuteSend(float now, NetOutgoingMessage message)
		{
			int sendStart = m_sendStart;
			m_sendStart = (m_sendStart + 1) % 1024;
			m_connection.QueueSendMessage(message, sendStart);
			int num = sendStart % m_windowSize;
			m_storedMessages[num].NumSent++;
			m_storedMessages[num].Message = message;
			m_storedMessages[num].LastSent = now;
		}

		private void DestoreMessage(int storeIndex)
		{
			NetOutgoingMessage message = m_storedMessages[storeIndex].Message;
			Interlocked.Decrement(ref message.m_recyclingCount);
			if (message.m_recyclingCount <= 0)
			{
				m_connection.m_peer.Recycle(message);
			}
			m_storedMessages[storeIndex] = default(NetStoredReliableMessage);
		}

		internal override void ReceiveAcknowledge(float now, int seqNr)
		{
			int num = NetUtility.RelativeSequenceNumber(seqNr, m_windowStart);
			if (num >= 0)
			{
				if (num == 0)
				{
					m_receivedAcks[m_windowStart] = false;
					DestoreMessage(m_windowStart % m_windowSize);
					m_windowStart = (m_windowStart + 1) % 1024;
					while (m_receivedAcks.Get(m_windowStart))
					{
						m_receivedAcks[m_windowStart] = false;
						m_windowStart = (m_windowStart + 1) % 1024;
					}
				}
				else
				{
					int num2 = NetUtility.RelativeSequenceNumber(seqNr, m_sendStart);
					if (num2 <= 0)
					{
						if (!m_receivedAcks[seqNr])
						{
							DestoreMessage(seqNr % m_windowSize);
							m_receivedAcks[seqNr] = true;
						}
					}
					else if (num2 > 0)
					{
						return;
					}
					int num3 = seqNr;
					do
					{
						num3--;
						if (num3 < 0)
						{
							num3 = 1023;
						}
						if (!m_receivedAcks[num3])
						{
							int num4 = num3 % m_windowSize;
							if (m_storedMessages[num4].NumSent == 1)
							{
								NetOutgoingMessage message = m_storedMessages[num4].Message;
								if (!(now - m_storedMessages[num4].LastSent < m_resendDelay * 0.35f))
								{
									m_storedMessages[num4].LastSent = now;
									m_storedMessages[num4].NumSent++;
									m_connection.QueueSendMessage(message, num3);
								}
							}
						}
					}
					while (num3 != m_windowStart);
				}
			}
		}
	}
}
