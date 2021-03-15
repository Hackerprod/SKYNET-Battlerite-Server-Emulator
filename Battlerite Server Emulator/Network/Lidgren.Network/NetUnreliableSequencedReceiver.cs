namespace Lidgren.Network
{
	internal sealed class NetUnreliableSequencedReceiver : NetReceiverChannelBase
	{
		private int m_lastReceivedSequenceNumber;

		public NetUnreliableSequencedReceiver(NetConnection connection)
			: base(connection)
		{
		}

		internal override void ReceiveMessage(NetIncomingMessage msg)
		{
			int sequenceNumber = msg.m_sequenceNumber;
			m_connection.QueueAck(msg.m_receivedMessageType, sequenceNumber);
			int num = NetUtility.RelativeSequenceNumber(sequenceNumber, m_lastReceivedSequenceNumber);
			if (num >= 0)
			{
				m_lastReceivedSequenceNumber = sequenceNumber;
				m_peer.ReleaseMessage(msg);
			}
		}
	}
}
