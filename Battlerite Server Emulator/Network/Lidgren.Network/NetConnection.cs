using SKYNET;
using System;
using System.Diagnostics;
using System.Net;

namespace Lidgren.Network
{
	[DebuggerDisplay("RemoteUniqueIdentifier={RemoteUniqueIdentifier} RemoteEndpoint={RemoteEndpoint}")]
	public class NetConnection
	{
		private enum ExpandMTUStatus
		{
			None,
			InProgress,
			Finished
		}

		private const int c_protocolMaxMTU = 8190;

		internal NetPeer m_peer;

		internal NetPeerConfiguration m_peerConfiguration;

		internal NetConnectionStatus m_status;

		internal NetConnectionStatus m_visibleStatus;

		internal IPEndPoint m_remoteEndpoint;

		internal NetSenderChannelBase[] m_sendChannels;

		internal NetReceiverChannelBase[] m_receiveChannels;

		internal NetOutgoingMessage m_localHailMessage;

		internal long m_remoteUniqueIdentifier;

		internal NetQueue<NetTuple<NetMessageType, int>> m_queuedAcks;

		private int m_sendBufferWritePtr;

		private int m_sendBufferNumMessages;

		private object m_tag;

		internal NetConnectionStatistics m_statistics;

		internal bool m_connectRequested;

		internal bool m_disconnectRequested;

		internal bool m_connectionInitiator;

		internal string m_disconnectMessage;

		internal NetIncomingMessage m_remoteHailMessage;

		internal float m_lastHandshakeSendTime;

		internal int m_handshakeAttempts;

		private float m_sentPingTime;

		private int m_sentPingNumber;

		private float m_averageRoundtripTime;

		private float m_timeoutDeadline = 3.40282347E+38f;

		internal double m_remoteTimeOffset;

		private ExpandMTUStatus m_expandMTUStatus;

		private int m_largestSuccessfulMTU;

		private int m_smallestFailedMTU;

		private int m_lastSentMTUAttemptSize;

		private double m_lastSentMTUAttemptTime;

		private int m_mtuAttemptFails;

		internal int m_currentMTU;

		public object Tag
		{
			get
			{
				return m_tag;
			}
			set
			{
				m_tag = value;
			}
		}

		public NetPeer Peer => m_peer;

		public NetConnectionStatus Status => m_visibleStatus;

		public NetConnectionStatistics Statistics => m_statistics;

		public IPEndPoint RemoteEndpoint => m_remoteEndpoint;

		public long RemoteUniqueIdentifier => m_remoteUniqueIdentifier;

		public NetOutgoingMessage LocalHailMessage => m_localHailMessage;

		public NetIncomingMessage RemoteHailMessage => m_remoteHailMessage;

		public float AverageRoundtripTime => m_averageRoundtripTime;

		public float RemoteTimeOffset => (float)m_remoteTimeOffset;

		internal float GetResendDelay()
		{
			float num = m_averageRoundtripTime;
			if (num <= 0f)
			{
				num = 0.1f;
			}
			return 0.02f + num * 2f;
		}

		internal NetConnection(NetPeer peer, IPEndPoint remoteEndpoint)
		{
			m_peer = peer;
			m_peerConfiguration = m_peer.Configuration;
			m_status = NetConnectionStatus.None;
			m_visibleStatus = NetConnectionStatus.None;
			m_remoteEndpoint = remoteEndpoint;
			m_sendChannels = new NetSenderChannelBase[99];
			m_receiveChannels = new NetReceiverChannelBase[99];
			m_queuedAcks = new NetQueue<NetTuple<NetMessageType, int>>(4);
			m_statistics = new NetConnectionStatistics(this);
			m_averageRoundtripTime = -1f;
			m_currentMTU = m_peerConfiguration.MaximumTransmissionUnit;
		}

		internal void MutateEndpoint(IPEndPoint endpoint)
		{
			m_remoteEndpoint = endpoint;
		}

		internal void SetStatus(NetConnectionStatus status, string reason)
		{
			if (status != m_status)
			{
				m_status = status;
				if (reason == null)
				{
					reason = string.Empty;
				}
				if (m_status == NetConnectionStatus.Connected)
				{
					m_timeoutDeadline = (float)NetTime.Now + m_peerConfiguration.m_connectionTimeout;
				}
				if (m_peerConfiguration.IsMessageTypeEnabled(NetIncomingMessageType.StatusChanged))
				{
					NetIncomingMessage netIncomingMessage = m_peer.CreateIncomingMessage(NetIncomingMessageType.StatusChanged, 4 + reason.Length + ((reason.Length <= 126) ? 1 : 2));
					netIncomingMessage.m_senderConnection = this;
					netIncomingMessage.m_senderEndpoint = m_remoteEndpoint;
					netIncomingMessage.Write((byte)m_status);
					netIncomingMessage.Write(reason);
					m_peer.ReleaseMessage(netIncomingMessage);
				}
				else
				{
					m_visibleStatus = m_status;
				}
			}
		}

		internal void Heartbeat(float now, uint frameCounter)
		{
			if (frameCounter % 5u == 0)
			{
				if (now > m_timeoutDeadline)
				{
					ExecuteDisconnect("Connection timed out", sendByeMessage: true);
				}
				if (m_status == NetConnectionStatus.Connected)
				{
					if (now > m_sentPingTime + m_peer.m_configuration.m_pingInterval)
					{
						SendPing();
					}
					MTUExpansionHeartbeat((double)now);
				}
				if (m_disconnectRequested)
				{
					ExecuteDisconnect(m_disconnectMessage, sendByeMessage: true);
					return;
				}
			}
			byte[] sendBuffer = m_peer.m_sendBuffer;
			int currentMTU = m_currentMTU;
			bool connectionReset;
			if (frameCounter % 3u == 0)
			{
				while (m_queuedAcks.Count > 0)
				{
					int num = (currentMTU - (m_sendBufferWritePtr + 5)) / 3;
					if (num > m_queuedAcks.Count)
					{
						num = m_queuedAcks.Count;
					}
					m_sendBufferNumMessages++;
					sendBuffer[m_sendBufferWritePtr++] = 134;
					sendBuffer[m_sendBufferWritePtr++] = 0;
					sendBuffer[m_sendBufferWritePtr++] = 0;
					int num2 = num * 3 * 8;
					sendBuffer[m_sendBufferWritePtr++] = (byte)num2;
					sendBuffer[m_sendBufferWritePtr++] = (byte)(num2 >> 8);
					for (int i = 0; i < num; i++)
					{
						m_queuedAcks.TryDequeue(out NetTuple<NetMessageType, int> item);
						sendBuffer[m_sendBufferWritePtr++] = (byte)item.Item1;
						sendBuffer[m_sendBufferWritePtr++] = (byte)item.Item2;
						sendBuffer[m_sendBufferWritePtr++] = (byte)(item.Item2 >> 8);
					}
					if (m_queuedAcks.Count > 0)
					{
						m_peer.SendPacket(m_sendBufferWritePtr, m_remoteEndpoint, m_sendBufferNumMessages, out connectionReset);
						m_sendBufferWritePtr = 0;
						m_sendBufferNumMessages = 0;
					}
				}
			}
			for (int num3 = m_sendChannels.Length - 1; num3 >= 0; num3--)
			{
				m_sendChannels[num3]?.SendQueuedMessages(now);
			}
			if (m_sendBufferWritePtr > 0)
			{
				m_peer.SendPacket(m_sendBufferWritePtr, m_remoteEndpoint, m_sendBufferNumMessages, out connectionReset);
				m_sendBufferWritePtr = 0;
				m_sendBufferNumMessages = 0;
			}
		}

		internal void QueueSendMessage(NetOutgoingMessage om, int seqNr)
		{
			int encodedSize = om.GetEncodedSize();
			if (encodedSize > m_currentMTU)
			{
				m_peer.LogWarning("Message larger than MTU! Fragmentation must have failed!");
			}
			if (m_sendBufferWritePtr + encodedSize > m_currentMTU)
			{
				m_peer.SendPacket(m_sendBufferWritePtr, m_remoteEndpoint, m_sendBufferNumMessages, out bool _);
				m_sendBufferWritePtr = 0;
				m_sendBufferNumMessages = 0;
			}
			m_sendBufferWritePtr = om.Encode(m_peer.m_sendBuffer, m_sendBufferWritePtr, seqNr);
			m_sendBufferNumMessages++;
		}

		public NetSendResult SendMessage(NetOutgoingMessage msg, NetDeliveryMethod method, int sequenceChannel)
		{
			return m_peer.SendMessage(msg, this, method, sequenceChannel);
		}

		internal NetSendResult EnqueueMessage(NetOutgoingMessage msg, NetDeliveryMethod method, int sequenceChannel)
		{
			NetMessageType tp = msg.m_messageType = (NetMessageType)((int)method + sequenceChannel);
			int num = (int)(method - 1) + sequenceChannel;
			NetSenderChannelBase netSenderChannelBase = m_sendChannels[num];
			if (netSenderChannelBase == null)
			{
				netSenderChannelBase = CreateSenderChannel(tp);
			}
			if (msg.GetEncodedSize() > m_currentMTU)
			{
				throw new NetException("Message too large! Fragmentation failure?");
			}
			return netSenderChannelBase.Enqueue(msg);
		}

		private NetSenderChannelBase CreateSenderChannel(NetMessageType tp)
		{
			NetDeliveryMethod deliveryMethod = NetUtility.GetDeliveryMethod(tp);
			int num = (int)tp - (int)deliveryMethod;
			NetSenderChannelBase netSenderChannelBase;
			switch (deliveryMethod)
			{
			case NetDeliveryMethod.Unreliable:
			case NetDeliveryMethod.UnreliableSequenced:
				netSenderChannelBase = new NetUnreliableSenderChannel(this, NetUtility.GetWindowSize(deliveryMethod));
				break;
			case NetDeliveryMethod.ReliableOrdered:
				netSenderChannelBase = new NetReliableSenderChannel(this, NetUtility.GetWindowSize(deliveryMethod));
				break;
			default:
				netSenderChannelBase = new NetReliableSenderChannel(this, NetUtility.GetWindowSize(deliveryMethod));
				break;
			}
			int num2 = (int)(deliveryMethod - 1) + num;
			m_sendChannels[num2] = netSenderChannelBase;
			return netSenderChannelBase;
		}

		internal void ReceivedLibraryMessage(NetMessageType tp, int ptr, int payloadLength)
		{
			float num = (float)NetTime.Now;
			switch (tp)
			{
			case NetMessageType.Disconnect:
			{
				NetIncomingMessage netIncomingMessage3 = m_peer.SetupReadHelperMessage(ptr, payloadLength);
				ExecuteDisconnect(netIncomingMessage3.ReadString(), sendByeMessage: false);
				break;
			}
			case NetMessageType.Acknowledge:
				for (int i = 0; i < payloadLength; i += 3)
				{
					NetMessageType netMessageType = (NetMessageType)m_peer.m_receiveBuffer[ptr++];
					int num5 = m_peer.m_receiveBuffer[ptr++];
					num5 |= m_peer.m_receiveBuffer[ptr++] << 8;
					NetSenderChannelBase netSenderChannelBase = m_sendChannels[(uint)(netMessageType - 1)];
					if (netSenderChannelBase == null)
					{
						netSenderChannelBase = CreateSenderChannel(netMessageType);
					}
					netSenderChannelBase.ReceiveAcknowledge(num, num5);
				}
				break;
			case NetMessageType.Ping:
			{
				int pingNumber = m_peer.m_receiveBuffer[ptr++];
				SendPong(pingNumber);
				break;
			}
			case NetMessageType.Pong:
			{
				NetIncomingMessage netIncomingMessage2 = m_peer.SetupReadHelperMessage(ptr, payloadLength);
				int pongNumber = netIncomingMessage2.ReadByte();
				float remoteSendTime = netIncomingMessage2.ReadSingle();
				ReceivedPong(num, pongNumber, remoteSendTime);
				break;
			}
			case NetMessageType.ExpandMTURequest:
				SendMTUSuccess(payloadLength);
				break;
			case NetMessageType.ExpandMTUSuccess:
			{
				NetIncomingMessage netIncomingMessage = m_peer.SetupReadHelperMessage(ptr, payloadLength);
				int size = netIncomingMessage.ReadInt32();
				HandleExpandMTUSuccess((double)num, size);
				break;
			}
			default:
				m_peer.LogWarning("Connection received unhandled library message: " + tp);
				break;
			}
		}

		internal void ReceivedMessage(NetIncomingMessage msg)
		{
			NetMessageType receivedMessageType = msg.m_receivedMessageType;
			int num = (int)(receivedMessageType - 1);
			NetReceiverChannelBase netReceiverChannelBase = m_receiveChannels[num];
			if (netReceiverChannelBase == null)
			{
				netReceiverChannelBase = CreateReceiverChannel(receivedMessageType);
			}
			netReceiverChannelBase.ReceiveMessage(msg);
		}

		private NetReceiverChannelBase CreateReceiverChannel(NetMessageType tp)
		{
			NetReceiverChannelBase netReceiverChannelBase;
			switch (NetUtility.GetDeliveryMethod(tp))
			{
			case NetDeliveryMethod.Unreliable:
				netReceiverChannelBase = new NetUnreliableUnorderedReceiver(this);
				break;
			case NetDeliveryMethod.ReliableOrdered:
				netReceiverChannelBase = new NetReliableOrderedReceiver(this, 64);
				break;
			case NetDeliveryMethod.UnreliableSequenced:
				netReceiverChannelBase = new NetUnreliableSequencedReceiver(this);
				break;
			case NetDeliveryMethod.ReliableUnordered:
				netReceiverChannelBase = new NetReliableUnorderedReceiver(this, 64);
				break;
			case NetDeliveryMethod.ReliableSequenced:
				netReceiverChannelBase = new NetReliableSequencedReceiver(this, 64);
				break;
			default:
				throw new NetException("Unhandled NetDeliveryMethod!");
			}
			int num = (int)(tp - 1);
			m_receiveChannels[num] = netReceiverChannelBase;
			return netReceiverChannelBase;
		}

		internal void QueueAck(NetMessageType tp, int sequenceNumber)
		{
			m_queuedAcks.Enqueue(new NetTuple<NetMessageType, int>(tp, sequenceNumber));
		}

		public void GetSendQueueInfo(NetDeliveryMethod method, int sequenceChannel, out int windowSize, out int freeWindowSlots)
		{
			int num = (int)(method - 1) + sequenceChannel;
			NetSenderChannelBase netSenderChannelBase = m_sendChannels[num];
			if (netSenderChannelBase == null)
			{
				windowSize = NetUtility.GetWindowSize(method);
				freeWindowSlots = windowSize;
			}
			else
			{
				windowSize = netSenderChannelBase.WindowSize;
				freeWindowSlots = netSenderChannelBase.GetAllowedSends() - netSenderChannelBase.m_queuedSends.Count;
			}
		}

		internal void Shutdown(string reason)
		{
			ExecuteDisconnect(reason, sendByeMessage: true);
		}

		public override string ToString()
		{
			return "[NetConnection to " + m_remoteEndpoint + "]";
		}

		internal void UnconnectedHeartbeat(float now)
		{
			if (m_disconnectRequested)
			{
				ExecuteDisconnect(m_disconnectMessage, sendByeMessage: true);
			}
			if (m_connectRequested)
			{
				switch (m_status)
				{
				case NetConnectionStatus.Disconnecting:
					break;
				case NetConnectionStatus.RespondedConnect:
				case NetConnectionStatus.Connected:
					ExecuteDisconnect("Reconnecting", sendByeMessage: true);
					break;
				case NetConnectionStatus.InitiatedConnect:
					SendConnect(now);
					break;
				case NetConnectionStatus.Disconnected:
					throw new NetException("This connection is Disconnected; spent. A new one should have been created");
				default:
					SendConnect(now);
					break;
				}
			}
			else if (now - m_lastHandshakeSendTime > m_peerConfiguration.m_resendHandshakeInterval)
			{
				if (m_handshakeAttempts >= m_peerConfiguration.m_maximumHandshakeAttempts)
				{
					ExecuteDisconnect("Failed to establish connection - no response from remote host", sendByeMessage: true);
				}
				else
				{
					switch (m_status)
					{
					case NetConnectionStatus.InitiatedConnect:
						SendConnect(now);
						break;
					case NetConnectionStatus.RespondedConnect:
						SendConnectResponse(now, onLibraryThread: true);
						break;
					case NetConnectionStatus.None:
						if (!m_peerConfiguration.IsMessageTypeEnabled(NetIncomingMessageType.ConnectionApproval))
						{
							m_peer.LogWarning("Time to resend handshake, but status is " + m_status);
						}
						break;
					default:
						m_peer.LogWarning("Time to resend handshake, but status is " + m_status);
						break;
					}
				}
			}
		}

		internal void ExecuteDisconnect(string reason, bool sendByeMessage)
		{
			for (int i = 0; i < m_sendChannels.Length; i++)
			{
				m_sendChannels[i]?.Reset();
			}
			if (sendByeMessage)
			{
				SendDisconnect(reason, onLibraryThread: true);
			}
			SetStatus(NetConnectionStatus.Disconnected, reason);
			lock (m_peer.m_handshakes)
			{
				m_peer.m_handshakes.Remove(m_remoteEndpoint);
			}
			m_disconnectRequested = false;
			m_connectRequested = false;
			m_handshakeAttempts = 0;
		}

		internal void SendConnect(float now)
		{
			NetOutgoingMessage netOutgoingMessage = m_peer.CreateMessage(m_peerConfiguration.AppIdentifier.Length + 1 + 4);
			netOutgoingMessage.m_messageType = NetMessageType.Connect;
			netOutgoingMessage.Write(m_peerConfiguration.AppIdentifier);
			netOutgoingMessage.Write(m_peer.m_uniqueIdentifier);
			netOutgoingMessage.Write(now);
			WriteLocalHail(netOutgoingMessage);
			m_peer.SendLibrary(netOutgoingMessage, m_remoteEndpoint);
			m_connectRequested = false;
			m_lastHandshakeSendTime = now;
			m_handshakeAttempts++;
			int handshakeAttempt = m_handshakeAttempts;
			SetStatus(NetConnectionStatus.InitiatedConnect, "Locally requested connect");
		}

		internal void SendConnectResponse(float now, bool onLibraryThread)
		{
			NetOutgoingMessage netOutgoingMessage = m_peer.CreateMessage(m_peerConfiguration.AppIdentifier.Length + 1 + 4);
			netOutgoingMessage.m_messageType = NetMessageType.ConnectResponse;
			netOutgoingMessage.Write(m_peerConfiguration.AppIdentifier);
			netOutgoingMessage.Write(m_peer.m_uniqueIdentifier);
			netOutgoingMessage.Write(now);
			WriteLocalHail(netOutgoingMessage);
			if (onLibraryThread)
			{
				m_peer.SendLibrary(netOutgoingMessage, m_remoteEndpoint);
			}
			else
			{
				m_peer.m_unsentUnconnectedMessages.Enqueue(new NetTuple<IPEndPoint, NetOutgoingMessage>(m_remoteEndpoint, netOutgoingMessage));
			}
			m_lastHandshakeSendTime = now;
			m_handshakeAttempts++;
			int handshakeAttempt = m_handshakeAttempts;
			SetStatus(NetConnectionStatus.RespondedConnect, "Remotely requested connect");
		}

		internal void SendDisconnect(string reason, bool onLibraryThread)
		{
			NetOutgoingMessage netOutgoingMessage = m_peer.CreateMessage(reason);
			netOutgoingMessage.m_messageType = NetMessageType.Disconnect;
			if (onLibraryThread)
			{
				m_peer.SendLibrary(netOutgoingMessage, m_remoteEndpoint);
			}
			else
			{
				m_peer.m_unsentUnconnectedMessages.Enqueue(new NetTuple<IPEndPoint, NetOutgoingMessage>(m_remoteEndpoint, netOutgoingMessage));
			}
		}

		private void WriteLocalHail(NetOutgoingMessage om)
		{
			if (m_localHailMessage != null)
			{
				byte[] array = m_localHailMessage.PeekDataBuffer();
				if (array != null && array.Length >= m_localHailMessage.LengthBytes)
				{
					if (om.LengthBytes + m_localHailMessage.LengthBytes > m_peerConfiguration.m_maximumTransmissionUnit - 10)
					{
						throw new NetException("Hail message too large; can maximally be " + (m_peerConfiguration.m_maximumTransmissionUnit - 10 - om.LengthBytes));
					}
					om.Write(m_localHailMessage.PeekDataBuffer(), 0, m_localHailMessage.LengthBytes);
				}
			}
		}

		internal void SendConnectionEstablished()
		{
			NetOutgoingMessage netOutgoingMessage = m_peer.CreateMessage(0);
			netOutgoingMessage.m_messageType = NetMessageType.ConnectionEstablished;
			netOutgoingMessage.Write((float)NetTime.Now);
			m_peer.SendLibrary(netOutgoingMessage, m_remoteEndpoint);
			m_handshakeAttempts = 0;
			InitializePing();
			if (m_status != NetConnectionStatus.Connected)
			{
				SetStatus(NetConnectionStatus.Connected, "Connected to " + NetUtility.ToHexString(m_remoteUniqueIdentifier));
			}
		}

		public void Approve()
		{
			m_localHailMessage = null;
			m_handshakeAttempts = 0;
			SendConnectResponse((float)NetTime.Now, onLibraryThread: false);
		}

		public void Approve(NetOutgoingMessage localHail)
		{
			m_localHailMessage = localHail;
			m_handshakeAttempts = 0;
			SendConnectResponse((float)NetTime.Now, onLibraryThread: false);
		}

		public void Deny()
		{
			Deny("");
		}

		public void Deny(string reason)
		{
			SendDisconnect(reason, onLibraryThread: false);
			m_peer.m_handshakes.Remove(m_remoteEndpoint);
		}

		internal void ReceivedHandshake(double now, NetMessageType tp, int ptr, int payloadLength)
		{
			byte[] hail;
			switch (tp)
			{
			case NetMessageType.Ping:
			case NetMessageType.Pong:
			case NetMessageType.Acknowledge:
				break;
			case NetMessageType.Connect:
				if (m_status == NetConnectionStatus.None)
				{
					if (ValidateHandshakeData(ptr, payloadLength, out hail))
					{
						if (hail != null)
						{
							m_remoteHailMessage = m_peer.CreateIncomingMessage(NetIncomingMessageType.Data, hail);
							m_remoteHailMessage.LengthBits = hail.Length * 8;
						}
						else
						{
							m_remoteHailMessage = null;
						}
						if (m_peerConfiguration.IsMessageTypeEnabled(NetIncomingMessageType.ConnectionApproval))
						{
							NetIncomingMessage netIncomingMessage2 = m_peer.CreateIncomingMessage(NetIncomingMessageType.ConnectionApproval, (m_remoteHailMessage != null) ? m_remoteHailMessage.LengthBytes : 0);
							netIncomingMessage2.m_receiveTime = now;
							netIncomingMessage2.m_senderConnection = this;
							netIncomingMessage2.m_senderEndpoint = m_remoteEndpoint;
							if (m_remoteHailMessage != null)
							{
								netIncomingMessage2.Write(m_remoteHailMessage.m_data, 0, m_remoteHailMessage.LengthBytes);
							}
							m_peer.ReleaseMessage(netIncomingMessage2);
						}
						else
						{
							SendConnectResponse((float)now, onLibraryThread: true);
						}
					}
				}
				else if (m_status == NetConnectionStatus.RespondedConnect)
				{
					SendConnectResponse((float)now, onLibraryThread: true);
				}
				break;
			case NetMessageType.ConnectResponse:
				switch (m_status)
				{
				case NetConnectionStatus.None:
				case NetConnectionStatus.RespondedConnect:
				case NetConnectionStatus.Disconnecting:
				case NetConnectionStatus.Disconnected:
					break;
				case NetConnectionStatus.InitiatedConnect:
					if (ValidateHandshakeData(ptr, payloadLength, out hail))
					{
						if (hail != null)
						{
							m_remoteHailMessage = m_peer.CreateIncomingMessage(NetIncomingMessageType.Data, hail);
							m_remoteHailMessage.LengthBits = hail.Length * 8;
						}
						else
						{
							m_remoteHailMessage = null;
						}
						m_peer.AcceptConnection(this);
						SendConnectionEstablished();
					}
					break;
				case NetConnectionStatus.Connected:
					SendConnectionEstablished();
					break;
				}
				break;
			case NetMessageType.ConnectionEstablished:
				switch (m_status)
				{
				case NetConnectionStatus.None:
				case NetConnectionStatus.InitiatedConnect:
				case NetConnectionStatus.Connected:
				case NetConnectionStatus.Disconnecting:
				case NetConnectionStatus.Disconnected:
					break;
				case NetConnectionStatus.RespondedConnect:
				{
					NetIncomingMessage netIncomingMessage3 = m_peer.SetupReadHelperMessage(ptr, payloadLength);
					InitializeRemoteTimeOffset(netIncomingMessage3.ReadSingle());
					m_peer.AcceptConnection(this);
					InitializePing();
					SetStatus(NetConnectionStatus.Connected, "Connected to " + NetUtility.ToHexString(m_remoteUniqueIdentifier));
					break;
				}
				}
				break;
			case NetMessageType.Disconnect:
			{
				string reason = "Ouch";
				try
				{
					NetIncomingMessage netIncomingMessage = m_peer.SetupReadHelperMessage(ptr, payloadLength);
					reason = netIncomingMessage.ReadString();
				}
				catch
				{
				}
				ExecuteDisconnect(reason, sendByeMessage: false);
				break;
			}
			case NetMessageType.Discovery:
				m_peer.HandleIncomingDiscoveryRequest(now, m_remoteEndpoint, ptr, payloadLength);
				break;
			case NetMessageType.DiscoveryResponse:
				m_peer.HandleIncomingDiscoveryResponse(now, m_remoteEndpoint, ptr, payloadLength);
				break;
			}
		}

		private bool ValidateHandshakeData(int ptr, int payloadLength, out byte[] hail)
		{
			hail = null;
			NetIncomingMessage netIncomingMessage = m_peer.SetupReadHelperMessage(ptr, payloadLength);
			try
			{
				string a = netIncomingMessage.ReadString();
				long remoteUniqueIdentifier = netIncomingMessage.ReadInt64();
				InitializeRemoteTimeOffset(netIncomingMessage.ReadSingle());
				int num = payloadLength - (netIncomingMessage.PositionInBytes - ptr);
				if (num > 0)
				{
					hail = netIncomingMessage.ReadBytes(num);
				}
                if (a != m_peer.m_configuration.AppIdentifier)
				{
					ExecuteDisconnect("Wrong application identifier!", sendByeMessage: true);
					return false;
				}
				m_remoteUniqueIdentifier = remoteUniqueIdentifier;
			}
			catch (Exception ex)
			{
				ExecuteDisconnect("Handshake data validation failed", sendByeMessage: true);
				m_peer.LogWarning("ReadRemoteHandshakeData failed: " + ex.Message);
				return false;
			}
			return true;
		}

		public void Disconnect(string byeMessage)
		{
			if (m_status != 0 && m_status != NetConnectionStatus.Disconnected)
			{
				m_disconnectMessage = byeMessage;
				if (m_status != NetConnectionStatus.Disconnected && m_status != 0)
				{
					SetStatus(NetConnectionStatus.Disconnecting, byeMessage);
				}
				m_handshakeAttempts = 0;
				m_disconnectRequested = true;
			}
		}

		internal void InitializeRemoteTimeOffset(float remoteSendTime)
		{
			m_remoteTimeOffset = (double)remoteSendTime + (double)m_averageRoundtripTime / 2.0 - NetTime.Now;
		}

		public double GetLocalTime(double remoteTimestamp)
		{
			return remoteTimestamp - m_remoteTimeOffset;
		}

		public double GetRemoteTime(double localTimestamp)
		{
			return localTimestamp + m_remoteTimeOffset;
		}

		internal void InitializePing()
		{
			float num = m_sentPingTime = (float)NetTime.Now;
			m_sentPingTime -= m_peerConfiguration.PingInterval * 0.25f;
			m_sentPingTime -= NetRandom.Instance.NextSingle() * (m_peerConfiguration.PingInterval * 0.75f);
			m_timeoutDeadline = num + m_peerConfiguration.m_connectionTimeout * 2f;
			SendPing();
		}

		internal void SendPing()
		{
			m_sentPingNumber++;
			m_sentPingTime = (float)NetTime.Now;
			NetOutgoingMessage netOutgoingMessage = m_peer.CreateMessage(1);
			netOutgoingMessage.Write((byte)m_sentPingNumber);
			netOutgoingMessage.m_messageType = NetMessageType.Ping;
			int numBytes = netOutgoingMessage.Encode(m_peer.m_sendBuffer, 0, 0);
			m_peer.SendPacket(numBytes, m_remoteEndpoint, 1, out bool _);
		}

		internal void SendPong(int pingNumber)
		{
			NetOutgoingMessage netOutgoingMessage = m_peer.CreateMessage(5);
			netOutgoingMessage.Write((byte)pingNumber);
			netOutgoingMessage.Write((float)NetTime.Now);
			netOutgoingMessage.m_messageType = NetMessageType.Pong;
			int numBytes = netOutgoingMessage.Encode(m_peer.m_sendBuffer, 0, 0);
			m_peer.SendPacket(numBytes, m_remoteEndpoint, 1, out bool _);
		}

		internal void ReceivedPong(float now, int pongNumber, float remoteSendTime)
		{
			if ((byte)pongNumber == (byte)m_sentPingNumber)
			{
				m_timeoutDeadline = now + m_peerConfiguration.m_connectionTimeout;
				float num = now - m_sentPingTime;
				double num2 = (double)remoteSendTime + (double)num / 2.0 - (double)now;
				if (m_averageRoundtripTime < 0f)
				{
					m_remoteTimeOffset = num2;
					m_averageRoundtripTime = num;
				}
				else
				{
					m_averageRoundtripTime = m_averageRoundtripTime * 0.7f + num * 0.3f;
					m_remoteTimeOffset = (m_remoteTimeOffset * (double)(m_sentPingNumber - 1) + num2) / (double)m_sentPingNumber;
				}
				float resendDelay = GetResendDelay();
				NetSenderChannelBase[] sendChannels = m_sendChannels;
				foreach (NetSenderChannelBase netSenderChannelBase in sendChannels)
				{
					NetReliableSenderChannel netReliableSenderChannel = netSenderChannelBase as NetReliableSenderChannel;
					if (netReliableSenderChannel != null)
					{
						netReliableSenderChannel.m_resendDelay = resendDelay;
					}
				}
				if (m_peer.m_configuration.IsMessageTypeEnabled(NetIncomingMessageType.ConnectionLatencyUpdated))
				{
					NetIncomingMessage netIncomingMessage = m_peer.CreateIncomingMessage(NetIncomingMessageType.ConnectionLatencyUpdated, 4);
					netIncomingMessage.m_senderConnection = this;
					netIncomingMessage.m_senderEndpoint = m_remoteEndpoint;
					netIncomingMessage.Write(num);
					m_peer.ReleaseMessage(netIncomingMessage);
				}
			}
		}

		internal void InitExpandMTU(double now)
		{
			m_lastSentMTUAttemptTime = now + (double)m_peerConfiguration.m_expandMTUFrequency + 1.5 + (double)m_averageRoundtripTime;
			m_largestSuccessfulMTU = 512;
			m_smallestFailedMTU = -1;
			m_currentMTU = m_peerConfiguration.MaximumTransmissionUnit;
		}

		private void MTUExpansionHeartbeat(double now)
		{
			if (m_expandMTUStatus != ExpandMTUStatus.Finished)
			{
				if (m_expandMTUStatus == ExpandMTUStatus.None)
				{
					if (!m_peerConfiguration.m_autoExpandMTU)
					{
						FinalizeMTU(m_currentMTU);
					}
					else
					{
						ExpandMTU(now, succeeded: true);
					}
				}
				else if (now > m_lastSentMTUAttemptTime + (double)m_peerConfiguration.ExpandMTUFrequency)
				{
					m_mtuAttemptFails++;
					if (m_mtuAttemptFails == 3)
					{
						FinalizeMTU(m_currentMTU);
					}
					else
					{
						m_smallestFailedMTU = m_lastSentMTUAttemptSize;
						ExpandMTU(now, succeeded: false);
					}
				}
			}
		}

		private void ExpandMTU(double now, bool succeeded)
		{
			int num = (m_smallestFailedMTU != -1) ? ((int)(((float)m_smallestFailedMTU + (float)m_largestSuccessfulMTU) / 2f)) : ((int)((float)m_currentMTU * 1.25f));
			if (num > 8190)
			{
				num = 8190;
			}
			if (num == m_largestSuccessfulMTU)
			{
				FinalizeMTU(m_largestSuccessfulMTU);
			}
			else
			{
				SendExpandMTU(now, num);
			}
		}

		private void SendExpandMTU(double now, int size)
		{
			NetOutgoingMessage netOutgoingMessage = m_peer.CreateMessage(size);
			byte[] source = new byte[size];
			netOutgoingMessage.Write(source);
			netOutgoingMessage.m_messageType = NetMessageType.ExpandMTURequest;
			int numBytes = netOutgoingMessage.Encode(m_peer.m_sendBuffer, 0, 0);
			if (!m_peer.SendMTUPacket(numBytes, m_remoteEndpoint))
			{
				if (m_smallestFailedMTU == -1 || size < m_smallestFailedMTU)
				{
					m_smallestFailedMTU = size;
					m_mtuAttemptFails++;
					if (m_mtuAttemptFails >= m_peerConfiguration.ExpandMTUFailAttempts)
					{
						FinalizeMTU(m_largestSuccessfulMTU);
						return;
					}
				}
				ExpandMTU(now, succeeded: false);
			}
			else
			{
				m_lastSentMTUAttemptSize = size;
				m_lastSentMTUAttemptTime = now;
			}
		}

		private void FinalizeMTU(int size)
		{
			if (m_expandMTUStatus != ExpandMTUStatus.Finished)
			{
				m_expandMTUStatus = ExpandMTUStatus.Finished;
				m_currentMTU = size;
				int currentMTU = m_currentMTU;
				int maximumTransmissionUnit = m_peerConfiguration.m_maximumTransmissionUnit;
			}
		}

		private void SendMTUSuccess(int size)
		{
			NetOutgoingMessage netOutgoingMessage = m_peer.CreateMessage(1);
			netOutgoingMessage.Write(size);
			netOutgoingMessage.m_messageType = NetMessageType.ExpandMTUSuccess;
			int numBytes = netOutgoingMessage.Encode(m_peer.m_sendBuffer, 0, 0);
			m_peer.SendPacket(numBytes, m_remoteEndpoint, 1, out bool _);
		}

		private void HandleExpandMTUSuccess(double now, int size)
		{
			if (size > m_largestSuccessfulMTU)
			{
				m_largestSuccessfulMTU = size;
			}
			if (size >= m_currentMTU)
			{
				m_currentMTU = size;
				ExpandMTU(now, succeeded: true);
			}
		}
	}
}
