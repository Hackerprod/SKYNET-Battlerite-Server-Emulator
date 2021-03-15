using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace Lidgren.Network
{
	public class NetPeer
	{
		private static int s_initializedPeersCount;

		private int m_listenPort;

		private object m_tag;

		internal readonly List<NetConnection> m_connections;

		private readonly Dictionary<IPEndPoint, NetConnection> m_connectionLookup;

		private string m_shutdownReason;

		private int m_lastUsedFragmentGroup;

		private Dictionary<NetConnection, Dictionary<int, ReceivedFragmentGroup>> m_receivedFragmentGroups;

		private NetPeerStatus m_status;

		private Thread m_networkThread;

		private PlatformSocket m_socket;

		internal byte[] m_sendBuffer;

		internal byte[] m_receiveBuffer;

		internal NetIncomingMessage m_readHelperMessage;

		private EndPoint m_senderRemote;

		private object m_initializeLock = new object();

		private uint m_frameCounter;

		private double m_lastHeartbeat;

		private NetUPnP m_upnp;

		internal readonly NetPeerConfiguration m_configuration;

		private readonly NetQueue<NetIncomingMessage> m_releasedIncomingMessages;

		internal readonly NetQueue<NetTuple<IPEndPoint, NetOutgoingMessage>> m_unsentUnconnectedMessages;

		internal Dictionary<IPEndPoint, NetConnection> m_handshakes;

		internal readonly NetPeerStatistics m_statistics;

		internal long m_uniqueIdentifier;

		private AutoResetEvent m_messageReceivedEvent = new AutoResetEvent(initialState: false);

		private List<NetTuple<SynchronizationContext, SendOrPostCallback>> m_receiveCallbacks;

		private List<byte[]> m_storagePool;

		private NetQueue<NetOutgoingMessage> m_outgoingMessagesPool;

		private NetQueue<NetIncomingMessage> m_incomingMessagesPool;

		internal int m_storagePoolBytes;

		public NetPeerStatus Status => m_status;

		public AutoResetEvent MessageReceivedEvent => m_messageReceivedEvent;

		public long UniqueIdentifier => m_uniqueIdentifier;

		public int Port => m_listenPort;

		public NetUPnP UPnP => m_upnp;

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

		public List<NetConnection> Connections
		{
			get
			{
				lock (m_connections)
				{
					return new List<NetConnection>(m_connections);
				}
			}
		}

		public int ConnectionsCount => m_connections.Count;

		public NetPeerStatistics Statistics => m_statistics;

		public NetPeerConfiguration Configuration => m_configuration;

		public PlatformSocket Socket => m_socket;

		public void Introduce(IPEndPoint hostInternal, IPEndPoint hostExternal, IPEndPoint clientInternal, IPEndPoint clientExternal, string token)
		{
			NetOutgoingMessage netOutgoingMessage = CreateMessage(10 + token.Length + 1);
			netOutgoingMessage.m_messageType = NetMessageType.NatIntroduction;
			netOutgoingMessage.Write(value: false);
			netOutgoingMessage.WritePadBits();
			netOutgoingMessage.Write(hostInternal);
			netOutgoingMessage.Write(hostExternal);
			netOutgoingMessage.Write(token);
			m_unsentUnconnectedMessages.Enqueue(new NetTuple<IPEndPoint, NetOutgoingMessage>(clientExternal, netOutgoingMessage));
			netOutgoingMessage = CreateMessage(10 + token.Length + 1);
			netOutgoingMessage.m_messageType = NetMessageType.NatIntroduction;
			netOutgoingMessage.Write(value: true);
			netOutgoingMessage.WritePadBits();
			netOutgoingMessage.Write(clientInternal);
			netOutgoingMessage.Write(clientExternal);
			netOutgoingMessage.Write(token);
			m_unsentUnconnectedMessages.Enqueue(new NetTuple<IPEndPoint, NetOutgoingMessage>(hostExternal, netOutgoingMessage));
		}

		private void HandleNatIntroduction(int ptr)
		{
			NetIncomingMessage netIncomingMessage = SetupReadHelperMessage(ptr, 1000);
			byte b = netIncomingMessage.ReadByte();
			IPEndPoint item = netIncomingMessage.ReadIPEndpoint();
			IPEndPoint item2 = netIncomingMessage.ReadIPEndpoint();
			string source = netIncomingMessage.ReadString();
			if (b != 0 || m_configuration.IsMessageTypeEnabled(NetIncomingMessageType.NatIntroductionSuccess))
			{
				NetOutgoingMessage netOutgoingMessage = CreateMessage(1);
				netOutgoingMessage.m_messageType = NetMessageType.NatPunchMessage;
				netOutgoingMessage.Write(b);
				netOutgoingMessage.Write(source);
				m_unsentUnconnectedMessages.Enqueue(new NetTuple<IPEndPoint, NetOutgoingMessage>(item, netOutgoingMessage));
				netOutgoingMessage = CreateMessage(1);
				netOutgoingMessage.m_messageType = NetMessageType.NatPunchMessage;
				netOutgoingMessage.Write(b);
				netOutgoingMessage.Write(source);
				m_unsentUnconnectedMessages.Enqueue(new NetTuple<IPEndPoint, NetOutgoingMessage>(item2, netOutgoingMessage));
			}
		}

		private void HandleNatPunch(int ptr, IPEndPoint senderEndpoint)
		{
			NetIncomingMessage netIncomingMessage = SetupReadHelperMessage(ptr, 1000);
			if (netIncomingMessage.ReadByte() != 0)
			{
				string source = netIncomingMessage.ReadString();
				NetIncomingMessage netIncomingMessage2 = CreateIncomingMessage(NetIncomingMessageType.NatIntroductionSuccess, 10);
				netIncomingMessage2.m_senderEndpoint = senderEndpoint;
				netIncomingMessage2.Write(source);
				ReleaseMessage(netIncomingMessage2);
			}
		}

		public NetPeer(NetPeerConfiguration config)
		{
			m_configuration = config;
			m_statistics = new NetPeerStatistics(this);
			m_releasedIncomingMessages = new NetQueue<NetIncomingMessage>(4);
			m_unsentUnconnectedMessages = new NetQueue<NetTuple<IPEndPoint, NetOutgoingMessage>>(2);
			m_connections = new List<NetConnection>();
			m_connectionLookup = new Dictionary<IPEndPoint, NetConnection>();
			m_handshakes = new Dictionary<IPEndPoint, NetConnection>();
			m_senderRemote = new IPEndPoint(IPAddress.Any, 0);
			m_status = NetPeerStatus.NotRunning;
			m_receivedFragmentGroups = new Dictionary<NetConnection, Dictionary<int, ReceivedFragmentGroup>>();
		}

		public void Start()
		{
			if (m_status != 0)
			{
				LogWarning("Start() called on already running NetPeer - ignoring.");
			}
			else
			{
				m_status = NetPeerStatus.Starting;
				if (m_configuration.NetworkThreadName == "Lidgren network thread")
				{
					int num = Interlocked.Increment(ref s_initializedPeersCount);
					m_configuration.NetworkThreadName = "Lidgren network thread " + num.ToString();
				}
				InitializeNetwork();
				m_networkThread = new Thread(NetworkLoop);
				m_networkThread.Name = m_configuration.NetworkThreadName;
				m_networkThread.IsBackground = true;
				m_networkThread.Start();
				if (m_upnp != null)
				{
					m_upnp.Discover(this);
				}
				Thread.Sleep(50);
			}
		}

		public NetConnection GetConnection(IPEndPoint ep)
		{
			m_connectionLookup.TryGetValue(ep, out NetConnection value);
			return value;
		}

		public NetIncomingMessage WaitMessage(int maxMillis)
		{
			if (m_messageReceivedEvent != null)
			{
				m_messageReceivedEvent.WaitOne(maxMillis);
			}
			return ReadMessage();
		}

		public NetIncomingMessage ReadMessage()
		{
			if (m_releasedIncomingMessages.TryDequeue(out NetIncomingMessage item) && item.MessageType == NetIncomingMessageType.StatusChanged)
			{
				NetConnectionStatus visibleStatus = (NetConnectionStatus)item.PeekByte();
				item.SenderConnection.m_visibleStatus = visibleStatus;
			}
			return item;
		}

		internal void SendLibrary(NetOutgoingMessage msg, IPEndPoint recipient)
		{
			int numBytes = msg.Encode(m_sendBuffer, 0, 0);
			SendPacket(numBytes, recipient, 1, out bool _);
		}

		public NetConnection Connect(string host, int port)
		{
			return Connect(new IPEndPoint(NetUtility.Resolve(host), port), null);
		}

		public NetConnection Connect(string host, int port, NetOutgoingMessage hailMessage)
		{
			return Connect(new IPEndPoint(NetUtility.Resolve(host), port), hailMessage);
		}

		public NetConnection Connect(IPEndPoint remoteEndpoint)
		{
			return Connect(remoteEndpoint, null);
		}

		public virtual NetConnection Connect(IPEndPoint remoteEndpoint, NetOutgoingMessage hailMessage)
		{
			if (remoteEndpoint == null)
			{
				throw new ArgumentNullException("remoteEndpoint");
			}
			lock (m_connections)
			{
				if (m_status == NetPeerStatus.NotRunning)
				{
					throw new NetException("Must call Start() first");
				}
				if (m_connectionLookup.ContainsKey(remoteEndpoint))
				{
					throw new NetException("Already connected to that endpoint!");
				}
				if (m_handshakes.TryGetValue(remoteEndpoint, out NetConnection value))
				{
					switch (value.Status)
					{
					case NetConnectionStatus.InitiatedConnect:
						value.m_connectRequested = true;
						break;
					case NetConnectionStatus.RespondedConnect:
						value.SendConnectResponse((float)NetTime.Now, onLibraryThread: false);
						break;
					default:
						LogWarning("Weird situation; Connect() already in progress to remote endpoint; but hs status is " + value.Status);
						break;
					}
					return value;
				}
				NetConnection netConnection = new NetConnection(this, remoteEndpoint);
				netConnection.m_status = NetConnectionStatus.InitiatedConnect;
				netConnection.m_localHailMessage = hailMessage;
				netConnection.m_connectRequested = true;
				netConnection.m_connectionInitiator = true;
				m_handshakes.Add(remoteEndpoint, netConnection);
				return netConnection;
			}
		}

		internal void RawSend(byte[] arr, int offset, int length, IPEndPoint destination)
		{
			Array.Copy(arr, offset, m_sendBuffer, 0, length);
			SendPacket(length, destination, 1, out bool _);
		}

		public void Shutdown(string bye)
		{
			if (m_socket != null)
			{
				m_shutdownReason = bye;
				m_status = NetPeerStatus.ShutdownRequested;
			}
		}

		public void DiscoverLocalPeers(int serverPort)
		{
			NetOutgoingMessage netOutgoingMessage = CreateMessage(0);
			netOutgoingMessage.m_messageType = NetMessageType.Discovery;
			m_unsentUnconnectedMessages.Enqueue(new NetTuple<IPEndPoint, NetOutgoingMessage>(new IPEndPoint(IPAddress.Broadcast, serverPort), netOutgoingMessage));
		}

		public bool DiscoverKnownPeer(string host, int serverPort)
		{
			IPAddress iPAddress = NetUtility.Resolve(host);
			if (iPAddress == null)
			{
				return false;
			}
			DiscoverKnownPeer(new IPEndPoint(iPAddress, serverPort));
			return true;
		}

		public void DiscoverKnownPeer(IPEndPoint endpoint)
		{
			NetOutgoingMessage netOutgoingMessage = CreateMessage(0);
			netOutgoingMessage.m_messageType = NetMessageType.Discovery;
			m_unsentUnconnectedMessages.Enqueue(new NetTuple<IPEndPoint, NetOutgoingMessage>(endpoint, netOutgoingMessage));
		}

		public void SendDiscoveryResponse(NetOutgoingMessage msg, IPEndPoint recipient)
		{
			if (recipient == null)
			{
				throw new ArgumentNullException("recipient");
			}
			if (msg == null)
			{
				msg = CreateMessage(0);
			}
			else if (msg.m_isSent)
			{
				throw new NetException("Message has already been sent!");
			}
			if (msg.LengthBytes >= m_configuration.MaximumTransmissionUnit)
			{
				throw new NetException("Cannot send discovery message larger than MTU (currently " + m_configuration.MaximumTransmissionUnit + " bytes)");
			}
			msg.m_messageType = NetMessageType.DiscoveryResponse;
			m_unsentUnconnectedMessages.Enqueue(new NetTuple<IPEndPoint, NetOutgoingMessage>(recipient, msg));
		}

		private void SendFragmentedMessage(NetOutgoingMessage msg, IList<NetConnection> recipients, NetDeliveryMethod method, int sequenceChannel)
		{
			int num = Interlocked.Increment(ref m_lastUsedFragmentGroup);
			if (num >= 65534)
			{
				m_lastUsedFragmentGroup = 1;
				num = 1;
			}
			msg.m_fragmentGroup = num;
			int lengthBytes = msg.LengthBytes;
			int mTU = GetMTU(recipients);
			int bestChunkSize = NetFragmentationHelper.GetBestChunkSize(num, lengthBytes, mTU);
			int num2 = lengthBytes / bestChunkSize;
			if (num2 * bestChunkSize < lengthBytes)
			{
				num2++;
			}
			int num3 = bestChunkSize * 8;
			int num4 = msg.LengthBits;
			for (int i = 0; i < num2; i++)
			{
				NetOutgoingMessage netOutgoingMessage = CreateMessage(mTU);
				netOutgoingMessage.m_bitLength = ((num4 > num3) ? num3 : num4);
				netOutgoingMessage.m_data = msg.m_data;
				netOutgoingMessage.m_fragmentGroup = num;
				netOutgoingMessage.m_fragmentGroupTotalBits = lengthBytes * 8;
				netOutgoingMessage.m_fragmentChunkByteSize = bestChunkSize;
				netOutgoingMessage.m_fragmentChunkNumber = i;
				Interlocked.Add(ref netOutgoingMessage.m_recyclingCount, recipients.Count);
				foreach (NetConnection recipient in recipients)
				{
					recipient.EnqueueMessage(netOutgoingMessage, method, sequenceChannel);
				}
				num4 -= num3;
			}
		}

		private void HandleReleasedFragment(NetIncomingMessage im)
		{
			int group;
			int totalBits;
			int chunkByteSize;
			int chunkNumber;
			int num = NetFragmentationHelper.ReadHeader(im.m_data, 0, out group, out totalBits, out chunkByteSize, out chunkNumber);
			int num2 = NetUtility.BytesToHoldBits(totalBits);
			int num3 = num2 / chunkByteSize;
			if (num3 * chunkByteSize < num2)
			{
				num3++;
			}
			if (chunkNumber >= num3)
			{
				LogWarning("Index out of bounds for chunk " + chunkNumber + " (total chunks " + num3 + ")");
			}
			else
			{
				if (!m_receivedFragmentGroups.TryGetValue(im.SenderConnection, out Dictionary<int, ReceivedFragmentGroup> value))
				{
					value = new Dictionary<int, ReceivedFragmentGroup>();
					m_receivedFragmentGroups[im.SenderConnection] = value;
				}
				if (!value.TryGetValue(group, out ReceivedFragmentGroup value2))
				{
					value2 = new ReceivedFragmentGroup();
					value2.Data = new byte[num2];
					value2.ReceivedChunks = new NetBitVector(num3);
					value[group] = value2;
				}
				value2.ReceivedChunks[chunkNumber] = true;
				value2.LastReceived = (float)NetTime.Now;
				int dstOffset = chunkNumber * chunkByteSize;
				Buffer.BlockCopy(im.m_data, num, value2.Data, dstOffset, im.LengthBytes - num);
				value2.ReceivedChunks.Count();
				if (value2.ReceivedChunks.Count() == num3)
				{
					im.m_data = value2.Data;
					im.m_bitLength = totalBits;
					im.m_isFragment = false;
					ReleaseMessage(im);
				}
				else
				{
					Recycle(im);
				}
			}
		}

		public void RegisterReceivedCallback(SendOrPostCallback callback)
		{
			if (m_receiveCallbacks == null)
			{
				m_receiveCallbacks = new List<NetTuple<SynchronizationContext, SendOrPostCallback>>();
			}
			m_receiveCallbacks.Add(new NetTuple<SynchronizationContext, SendOrPostCallback>(SynchronizationContext.Current, callback));
		}

		internal void ReleaseMessage(NetIncomingMessage msg)
		{
			if (msg.m_isFragment)
			{
				HandleReleasedFragment(msg);
			}
			else
			{
				m_releasedIncomingMessages.Enqueue(msg);
				if (m_messageReceivedEvent != null)
				{
					m_messageReceivedEvent.Set();
				}
				if (m_receiveCallbacks != null)
				{
					foreach (NetTuple<SynchronizationContext, SendOrPostCallback> receiveCallback in m_receiveCallbacks)
					{
						NetTuple<SynchronizationContext, SendOrPostCallback> current = receiveCallback;
						current.Item1.Post(current.Item2, this);
					}
				}
			}
		}

		private void InitializeNetwork()
		{
			lock (m_initializeLock)
			{
				m_configuration.Lock();
				if (m_status != NetPeerStatus.Running)
				{
					if (m_configuration.m_enableUPnP)
					{
						m_upnp = new NetUPnP(this);
					}
					InitializePools();
					m_releasedIncomingMessages.Clear();
					m_unsentUnconnectedMessages.Clear();
					m_handshakes.Clear();
					IPEndPoint iPEndPoint = null;
					iPEndPoint = new IPEndPoint(m_configuration.LocalAddress, m_configuration.Port);
					EndPoint ep = iPEndPoint;
					m_socket = new PlatformSocket();
					m_socket.ReceiveBufferSize = m_configuration.ReceiveBufferSize;
					m_socket.SendBufferSize = m_configuration.SendBufferSize;
					m_socket.Blocking = false;
					m_socket.Bind(ep);
					m_socket.Setup();
					IPEndPoint iPEndPoint2 = m_socket.LocalEndPoint as IPEndPoint;
					m_listenPort = iPEndPoint2.Port;
					m_receiveBuffer = new byte[m_configuration.ReceiveBufferSize];
					m_sendBuffer = new byte[m_configuration.SendBufferSize];
					m_readHelperMessage = new NetIncomingMessage(NetIncomingMessageType.Error);
					m_readHelperMessage.m_data = m_receiveBuffer;
					byte[] array = new byte[8];
					NetRandom.Instance.NextBytes(array);
					try
					{
						PhysicalAddress macAddress = NetUtility.GetMacAddress();
						if (macAddress != null)
						{
							array = macAddress.GetAddressBytes();
						}
						else
						{
							LogWarning("Failed to get Mac address");
						}
					}
					catch (NotSupportedException)
					{
					}
					byte[] bytes = BitConverter.GetBytes(iPEndPoint2.GetHashCode());
					byte[] array2 = new byte[bytes.Length + array.Length];
					Array.Copy(bytes, 0, array2, 0, bytes.Length);
					Array.Copy(array, 0, array2, bytes.Length, array.Length);
					m_uniqueIdentifier = BitConverter.ToInt64(SHA1.Create().ComputeHash(array2), 0);
					m_status = NetPeerStatus.Running;
				}
			}
		}

		private void NetworkLoop()
		{
			do
			{
				try
				{
					Heartbeat();
				}
				catch (Exception ex)
				{
					LogWarning(ex.ToString());
				}
			}
			while (m_status == NetPeerStatus.Running);
			ExecutePeerShutdown();
		}

		private void ExecutePeerShutdown()
		{
			List<NetConnection> list = new List<NetConnection>(m_handshakes.Count + m_connections.Count);
			lock (m_connections)
			{
				foreach (NetConnection connection in m_connections)
				{
					if (connection != null)
					{
						list.Add(connection);
					}
				}
				lock (m_handshakes)
				{
					foreach (NetConnection value in m_handshakes.Values)
					{
						if (value != null)
						{
							list.Add(value);
						}
					}
					foreach (NetConnection item in list)
					{
						item.Shutdown(m_shutdownReason);
					}
				}
			}
			Heartbeat();
			lock (m_initializeLock)
			{
				try
				{
					if (m_socket != null)
					{
						try
						{
							m_socket.Shutdown(SocketShutdown.Receive);
						}
						catch (Exception)
						{
						}
						m_socket.Close(2);
					}
					if (m_messageReceivedEvent != null)
					{
						m_messageReceivedEvent.Set();
						m_messageReceivedEvent.Close();
						m_messageReceivedEvent = null;
					}
				}
				finally
				{
					m_socket = null;
					m_status = NetPeerStatus.NotRunning;
				}
				m_receiveBuffer = null;
				m_sendBuffer = null;
				m_unsentUnconnectedMessages.Clear();
				m_connections.Clear();
				m_handshakes.Clear();
			}
		}

		private void Heartbeat()
		{
			double now = NetTime.Now;
			float now2 = (float)now;
			double num = now - m_lastHeartbeat;
			int num2 = 1250 - m_connections.Count;
			if (num2 < 250)
			{
				num2 = 250;
			}
			if (num > 1.0 / (double)num2)
			{
				m_frameCounter++;
				m_lastHeartbeat = now;
				if (m_frameCounter % 3u == 0)
				{
					foreach (KeyValuePair<IPEndPoint, NetConnection> handshake in m_handshakes)
					{
						NetConnection value = handshake.Value;
						value.UnconnectedHeartbeat(now2);
						if (value.m_status == NetConnectionStatus.Connected || value.m_status == NetConnectionStatus.Disconnected)
						{
							break;
						}
					}
				}
				lock (m_connections)
				{
					foreach (NetConnection connection in m_connections)
					{
						connection.Heartbeat(now2, m_frameCounter);
						if (connection.m_status == NetConnectionStatus.Disconnected)
						{
							m_connections.Remove(connection);
							m_connectionLookup.Remove(connection.RemoteEndpoint);
							break;
						}
					}
				}
				NetTuple<IPEndPoint, NetOutgoingMessage> item;
				while (m_unsentUnconnectedMessages.TryDequeue(out item))
				{
					NetOutgoingMessage item2 = item.Item2;
					int numBytes = item2.Encode(m_sendBuffer, 0, 0);
					SendPacket(numBytes, item.Item1, 1, out bool _);
					Interlocked.Decrement(ref item2.m_recyclingCount);
					if (item2.m_recyclingCount <= 0)
					{
						Recycle(item2);
					}
				}
			}
			if (m_socket != null && m_socket.Poll(1000))
			{
				do
				{
					int num3 = 0;
					try
					{
						num3 = m_socket.ReceiveFrom(m_receiveBuffer, 0, m_receiveBuffer.Length, ref m_senderRemote);
					}
					catch (SocketException ex)
					{
						if (ex.SocketErrorCode != SocketError.ConnectionReset)
						{
							LogWarning(ex.ToString());
						}
						return;
					}
					if (num3 < 5)
					{
						break;
					}
					IPEndPoint iPEndPoint = (IPEndPoint)m_senderRemote;
					if (iPEndPoint.Port == 1900)
					{
						try
						{
							string @string = Encoding.UTF8.GetString(m_receiveBuffer, 0, num3);
							if (@string.Contains("upnp:rootdevice"))
							{
								@string = @string.Substring(@string.ToLower().IndexOf("location:") + 9);
								@string = @string.Substring(0, @string.IndexOf("\r")).Trim();
								m_upnp.ExtractServiceUrl(@string);
								return;
							}
						}
						catch
						{
						}
					}
					NetConnection value2 = null;
					m_connectionLookup.TryGetValue(iPEndPoint, out value2);
					double now3 = NetTime.Now;
					int num4 = 0;
					int num11;
					for (int i = 0; num3 - i >= 5; i += num11)
					{
						num4++;
						NetMessageType netMessageType = (NetMessageType)m_receiveBuffer[i++];
						byte b = m_receiveBuffer[i++];
						byte b2 = m_receiveBuffer[i++];
						bool isFragment = (b & 1) == 1;
						ushort sequenceNumber = (ushort)((b >> 1) | (b2 << 7));
						ushort num10 = (ushort)(m_receiveBuffer[i++] | (m_receiveBuffer[i++] << 8));
						num11 = NetUtility.BytesToHoldBits(num10);
						if (num3 - i < num11)
						{
							LogWarning("Malformed packet; stated payload length " + num11 + ", remaining bytes " + (num3 - i));
							return;
						}
						try
						{
							if ((int)netMessageType >= 128)
							{
								if (value2 != null)
								{
									value2.ReceivedLibraryMessage(netMessageType, i, num11);
								}
								else
								{
									ReceivedUnconnectedLibraryMessage(now3, iPEndPoint, netMessageType, i, num11);
								}
							}
							else
							{
								if (value2 == null && !m_configuration.IsMessageTypeEnabled(NetIncomingMessageType.UnconnectedData))
								{
									return;
								}
								NetIncomingMessage netIncomingMessage = CreateIncomingMessage(NetIncomingMessageType.Data, num11);
								netIncomingMessage.m_isFragment = isFragment;
								netIncomingMessage.m_receiveTime = now3;
								netIncomingMessage.m_sequenceNumber = sequenceNumber;
								netIncomingMessage.m_receivedMessageType = netMessageType;
								netIncomingMessage.m_senderConnection = value2;
								netIncomingMessage.m_senderEndpoint = iPEndPoint;
								netIncomingMessage.m_bitLength = num10;
								Buffer.BlockCopy(m_receiveBuffer, i, netIncomingMessage.m_data, 0, num11);
								if (value2 != null)
								{
									if (netMessageType == NetMessageType.Unconnected)
									{
										netIncomingMessage.m_incomingMessageType = NetIncomingMessageType.UnconnectedData;
										ReleaseMessage(netIncomingMessage);
									}
									else
									{
										value2.ReceivedMessage(netIncomingMessage);
									}
								}
								else
								{
									netIncomingMessage.m_incomingMessageType = NetIncomingMessageType.UnconnectedData;
									ReleaseMessage(netIncomingMessage);
								}
							}
						}
						catch (Exception ex2)
						{
							LogError("Packet parsing error: " + ex2.Message + " from " + iPEndPoint);
						}
					}
				}
				while (m_socket.Available > 0);
			}
		}

		internal void HandleIncomingDiscoveryRequest(double now, IPEndPoint senderEndpoint, int ptr, int payloadByteLength)
		{
			if (m_configuration.IsMessageTypeEnabled(NetIncomingMessageType.DiscoveryRequest))
			{
				NetIncomingMessage netIncomingMessage = CreateIncomingMessage(NetIncomingMessageType.DiscoveryRequest, payloadByteLength);
				if (payloadByteLength > 0)
				{
					Buffer.BlockCopy(m_receiveBuffer, ptr, netIncomingMessage.m_data, 0, payloadByteLength);
				}
				netIncomingMessage.m_receiveTime = now;
				netIncomingMessage.m_bitLength = payloadByteLength * 8;
				netIncomingMessage.m_senderEndpoint = senderEndpoint;
				ReleaseMessage(netIncomingMessage);
			}
		}

		internal void HandleIncomingDiscoveryResponse(double now, IPEndPoint senderEndpoint, int ptr, int payloadByteLength)
		{
			if (m_configuration.IsMessageTypeEnabled(NetIncomingMessageType.DiscoveryResponse))
			{
				NetIncomingMessage netIncomingMessage = CreateIncomingMessage(NetIncomingMessageType.DiscoveryResponse, payloadByteLength);
				if (payloadByteLength > 0)
				{
					Buffer.BlockCopy(m_receiveBuffer, ptr, netIncomingMessage.m_data, 0, payloadByteLength);
				}
				netIncomingMessage.m_receiveTime = now;
				netIncomingMessage.m_bitLength = payloadByteLength * 8;
				netIncomingMessage.m_senderEndpoint = senderEndpoint;
				ReleaseMessage(netIncomingMessage);
			}
		}

		private void ReceivedUnconnectedLibraryMessage(double now, IPEndPoint senderEndpoint, NetMessageType tp, int ptr, int payloadByteLength)
		{
			if (m_handshakes.TryGetValue(senderEndpoint, out NetConnection value))
			{
				value.ReceivedHandshake(now, tp, ptr, payloadByteLength);
			}
			else
			{
				switch (tp)
				{
				case NetMessageType.Disconnect:
					break;
				case NetMessageType.Discovery:
					HandleIncomingDiscoveryRequest(now, senderEndpoint, ptr, payloadByteLength);
					break;
				case NetMessageType.DiscoveryResponse:
					HandleIncomingDiscoveryResponse(now, senderEndpoint, ptr, payloadByteLength);
					break;
				case NetMessageType.NatIntroduction:
					HandleNatIntroduction(ptr);
					break;
				case NetMessageType.NatPunchMessage:
					HandleNatPunch(ptr, senderEndpoint);
					break;
				case NetMessageType.ConnectResponse:
					lock (m_handshakes)
					{
						foreach (KeyValuePair<IPEndPoint, NetConnection> handshake in m_handshakes)
						{
							if (handshake.Key.Address.Equals(senderEndpoint.Address) && handshake.Value.m_connectionInitiator)
							{
								NetConnection value2 = handshake.Value;
								m_connectionLookup.Remove(handshake.Key);
								m_handshakes.Remove(handshake.Key);
								value2.MutateEndpoint(senderEndpoint);
								m_connectionLookup.Add(senderEndpoint, value2);
								m_handshakes.Add(senderEndpoint, value2);
								value2.ReceivedHandshake(now, tp, ptr, payloadByteLength);
								return;
							}
						}
					}
					LogWarning("Received unhandled library message " + tp + " from " + senderEndpoint);
					break;
				default:
					LogWarning("Received unhandled library message " + tp + " from " + senderEndpoint);
					break;
				case NetMessageType.Connect:
				{
					int num = m_handshakes.Count + m_connections.Count;
					if (num >= m_configuration.m_maximumConnections)
					{
						NetOutgoingMessage netOutgoingMessage = CreateMessage("Server full");
						netOutgoingMessage.m_messageType = NetMessageType.Disconnect;
						SendLibrary(netOutgoingMessage, senderEndpoint);
					}
					else
					{
						NetConnection netConnection = new NetConnection(this, senderEndpoint);
						m_handshakes.Add(senderEndpoint, netConnection);
						netConnection.ReceivedHandshake(now, tp, ptr, payloadByteLength);
					}
					break;
				}
				}
			}
		}

		internal void AcceptConnection(NetConnection conn)
		{
			conn.InitExpandMTU(NetTime.Now);
			if (!m_handshakes.Remove(conn.m_remoteEndpoint))
			{
				LogWarning("AcceptConnection called but m_handshakes did not contain it!");
			}
			lock (m_connections)
			{
				if (m_connections.Contains(conn))
				{
					LogWarning("AcceptConnection called but m_connection already contains it!");
				}
				else
				{
					m_connections.Add(conn);
					m_connectionLookup.Add(conn.m_remoteEndpoint, conn);
				}
			}
		}

		[Conditional("DEBUG")]
		internal void VerifyNetworkThread()
		{
			Thread currentThread = Thread.CurrentThread;
			if (Thread.CurrentThread != m_networkThread)
			{
				throw new NetException("Executing on wrong thread! Should be library system thread (is " + currentThread.Name + " mId " + currentThread.ManagedThreadId + ")");
			}
		}

		internal NetIncomingMessage SetupReadHelperMessage(int ptr, int payloadLength)
		{
			m_readHelperMessage.m_bitLength = (ptr + payloadLength) * 8;
			m_readHelperMessage.m_readPosition = ptr * 8;
			return m_readHelperMessage;
		}

		internal bool SendMTUPacket(int numBytes, IPEndPoint target)
		{
			try
			{
				m_socket.DontFragment = true;
				int num = m_socket.SendTo(m_sendBuffer, 0, numBytes, target);
				if (numBytes != num)
				{
					LogWarning("Failed to send the full " + numBytes + "; only " + num + " bytes sent in packet!");
				}
			}
			catch (SocketException ex)
			{
				if (ex.SocketErrorCode == SocketError.MessageSize)
				{
					return false;
				}
				if (ex.SocketErrorCode == SocketError.WouldBlock)
				{
					LogWarning("Socket threw exception; would block - send buffer full? Increase in NetPeerConfiguration");
					return true;
				}
				if (ex.SocketErrorCode == SocketError.ConnectionReset)
				{
					return true;
				}
				LogError("Failed to send packet: (" + ex.SocketErrorCode + ") " + ex);
			}
			catch (Exception arg)
			{
				LogError("Failed to send packet: " + arg);
			}
			finally
			{
				m_socket.DontFragment = false;
			}
			return true;
		}

		internal void SendPacket(int numBytes, IPEndPoint target, int numMessages, out bool connectionReset)
		{
			connectionReset = false;
			try
			{
				if (target.Address == IPAddress.Broadcast)
				{
					m_socket.Broadcast = true;
				}
				int num = m_socket.SendTo(m_sendBuffer, 0, numBytes, target);
				if (numBytes != num)
				{
					LogWarning("Failed to send the full " + numBytes + "; only " + num + " bytes sent in packet!");
				}
			}
			catch (SocketException ex)
			{
				if (ex.SocketErrorCode == SocketError.WouldBlock)
				{
					LogWarning("Socket threw exception; would block - send buffer full? Increase in NetPeerConfiguration");
				}
				else if (ex.SocketErrorCode == SocketError.ConnectionReset)
				{
					connectionReset = true;
				}
				else
				{
					LogError("Failed to send packet: " + ex);
				}
			}
			catch (Exception arg)
			{
				LogError("Failed to send packet: " + arg);
			}
			finally
			{
				if (target.Address == IPAddress.Broadcast)
				{
					m_socket.Broadcast = false;
				}
			}
		}

		private void SendCallBack(IAsyncResult res)
		{
			m_socket.EndSendTo(res);
		}

		[Conditional("DEBUG")]
		internal void LogVerbose(string message)
		{
			if (m_configuration.IsMessageTypeEnabled(NetIncomingMessageType.VerboseDebugMessage))
			{
				ReleaseMessage(CreateIncomingMessage(NetIncomingMessageType.VerboseDebugMessage, message));
			}
		}

		[Conditional("DEBUG")]
		internal void LogDebug(string message)
		{
			if (m_configuration.IsMessageTypeEnabled(NetIncomingMessageType.DebugMessage))
			{
				ReleaseMessage(CreateIncomingMessage(NetIncomingMessageType.DebugMessage, message));
			}
		}

		internal void LogWarning(string message)
		{
			if (m_configuration.IsMessageTypeEnabled(NetIncomingMessageType.WarningMessage))
			{
				ReleaseMessage(CreateIncomingMessage(NetIncomingMessageType.WarningMessage, message));
			}
		}

		internal void LogError(string message)
		{
			if (m_configuration.IsMessageTypeEnabled(NetIncomingMessageType.ErrorMessage))
			{
				ReleaseMessage(CreateIncomingMessage(NetIncomingMessageType.ErrorMessage, message));
			}
		}

		private void InitializePools()
		{
			if (m_configuration.UseMessageRecycling)
			{
				m_storagePool = new List<byte[]>(16);
				m_outgoingMessagesPool = new NetQueue<NetOutgoingMessage>(4);
				m_incomingMessagesPool = new NetQueue<NetIncomingMessage>(4);
			}
			else
			{
				m_storagePool = null;
				m_outgoingMessagesPool = null;
				m_incomingMessagesPool = null;
			}
		}

		internal byte[] GetStorage(int minimumCapacity)
		{
			if (m_storagePool == null)
			{
				return new byte[minimumCapacity];
			}
			lock (m_storagePool)
			{
				for (int i = 0; i < m_storagePool.Count; i++)
				{
					byte[] array = m_storagePool[i];
					if (array != null && array.Length >= minimumCapacity)
					{
						m_storagePool[i] = null;
						m_storagePoolBytes -= array.Length;
						return array;
					}
				}
			}
			m_statistics.m_bytesAllocated += minimumCapacity;
			return new byte[minimumCapacity];
		}

		internal void Recycle(byte[] storage)
		{
			if (m_storagePool != null)
			{
				lock (m_storagePool)
				{
					for (int i = 0; i < m_storagePool.Count; i++)
					{
						if (m_storagePool[i] == null)
						{
							m_storagePoolBytes += storage.Length;
							m_storagePool[i] = storage;
							return;
						}
					}
					m_storagePoolBytes += storage.Length;
					m_storagePool.Add(storage);
				}
			}
		}

		public NetOutgoingMessage CreateMessage()
		{
			return CreateMessage(m_configuration.m_defaultOutgoingMessageCapacity);
		}

		public NetOutgoingMessage CreateMessage(string content)
		{
			byte[] bytes = Encoding.UTF8.GetBytes(content);
			NetOutgoingMessage netOutgoingMessage = CreateMessage(2 + bytes.Length);
			netOutgoingMessage.WriteVariableUInt32((uint)bytes.Length);
			netOutgoingMessage.Write(bytes);
			return netOutgoingMessage;
		}

		public NetOutgoingMessage CreateMessage(int initialCapacity)
		{
			if (m_outgoingMessagesPool == null || !m_outgoingMessagesPool.TryDequeue(out NetOutgoingMessage item))
			{
				item = new NetOutgoingMessage();
			}
			byte[] array = item.m_data = GetStorage(initialCapacity);
			return item;
		}

		internal NetIncomingMessage CreateIncomingMessage(NetIncomingMessageType tp, byte[] useStorageData)
		{
			if (m_incomingMessagesPool == null || !m_incomingMessagesPool.TryDequeue(out NetIncomingMessage item))
			{
				item = new NetIncomingMessage(tp);
			}
			else
			{
				item.m_incomingMessageType = tp;
			}
			item.m_data = useStorageData;
			return item;
		}

		internal NetIncomingMessage CreateIncomingMessage(NetIncomingMessageType tp, int minimumByteSize)
		{
			if (m_incomingMessagesPool == null || !m_incomingMessagesPool.TryDequeue(out NetIncomingMessage item))
			{
				item = new NetIncomingMessage(tp);
			}
			else
			{
				item.m_incomingMessageType = tp;
			}
			item.m_data = GetStorage(minimumByteSize);
			return item;
		}

		public void Recycle(NetIncomingMessage msg)
		{
			if (m_incomingMessagesPool != null)
			{
				byte[] data = msg.m_data;
				msg.m_data = null;
				Recycle(data);
				msg.Reset();
				m_incomingMessagesPool.Enqueue(msg);
			}
		}

		internal void Recycle(NetOutgoingMessage msg)
		{
			if (m_outgoingMessagesPool != null)
			{
				byte[] data = msg.m_data;
				msg.m_data = null;
				if (msg.m_fragmentGroup == 0)
				{
					Recycle(data);
				}
				msg.Reset();
				m_outgoingMessagesPool.Enqueue(msg);
			}
		}

		internal NetIncomingMessage CreateIncomingMessage(NetIncomingMessageType tp, string text)
		{
			NetIncomingMessage netIncomingMessage;
			if (string.IsNullOrEmpty(text))
			{
				netIncomingMessage = CreateIncomingMessage(tp, 1);
				netIncomingMessage.Write("");
				return netIncomingMessage;
			}
			byte[] bytes = Encoding.UTF8.GetBytes(text);
			netIncomingMessage = CreateIncomingMessage(tp, bytes.Length + ((bytes.Length <= 127) ? 1 : 2));
			netIncomingMessage.Write(text);
			return netIncomingMessage;
		}

		public NetSendResult SendMessage(NetOutgoingMessage msg, NetConnection recipient, NetDeliveryMethod method)
		{
			return SendMessage(msg, recipient, method, 0);
		}

		public NetSendResult SendMessage(NetOutgoingMessage msg, NetConnection recipient, NetDeliveryMethod method, int sequenceChannel)
		{
			if (msg == null)
			{
				throw new ArgumentNullException("msg");
			}
			if (recipient == null)
			{
				throw new ArgumentNullException("recipient");
			}
			if (sequenceChannel >= 32)
			{
				throw new ArgumentOutOfRangeException("sequenceChannel");
			}
			if (msg.m_isSent)
			{
				throw new NetException("This message has already been sent! Use NetPeer.SendMessage() to send to multiple recipients efficiently");
			}
			msg.m_isSent = true;
			int num = 5 + msg.LengthBytes;
			if (num <= recipient.m_currentMTU)
			{
				Interlocked.Increment(ref msg.m_recyclingCount);
				return recipient.EnqueueMessage(msg, method, sequenceChannel);
			}
			SendFragmentedMessage(msg, new NetConnection[1]
			{
				recipient
			}, method, sequenceChannel);
			return NetSendResult.Queued;
		}

		internal int GetMTU(IList<NetConnection> recipients)
		{
			int num = 2147483647;
			foreach (NetConnection recipient in recipients)
			{
				int currentMTU = recipient.m_currentMTU;
				if (currentMTU < num)
				{
					num = currentMTU;
				}
			}
			return num;
		}

		public void SendMessage(NetOutgoingMessage msg, IList<NetConnection> recipients, NetDeliveryMethod method, int sequenceChannel)
		{
			if (msg == null)
			{
				throw new ArgumentNullException("msg");
			}
			if (recipients == null)
			{
				throw new ArgumentNullException("recipients");
			}
			if (method == NetDeliveryMethod.Unreliable)
			{
			}
			if (msg.m_isSent)
			{
				throw new NetException("This message has already been sent! Use NetPeer.SendMessage() to send to multiple recipients efficiently");
			}
			GetMTU(recipients);
			msg.m_isSent = true;
			int lengthBytes = msg.LengthBytes;
			if (lengthBytes <= m_configuration.MaximumTransmissionUnit)
			{
				Interlocked.Add(ref msg.m_recyclingCount, recipients.Count);
				foreach (NetConnection recipient in recipients)
				{
					if (recipient == null)
					{
						Interlocked.Decrement(ref msg.m_recyclingCount);
					}
					else
					{
						NetSendResult netSendResult = recipient.EnqueueMessage(msg, method, sequenceChannel);
						if (netSendResult == NetSendResult.Dropped)
						{
							Interlocked.Decrement(ref msg.m_recyclingCount);
						}
					}
				}
			}
			else
			{
				SendFragmentedMessage(msg, recipients, method, sequenceChannel);
			}
		}

		public void SendUnconnectedMessage(NetOutgoingMessage msg, string host, int port)
		{
			if (msg == null)
			{
				throw new ArgumentNullException("msg");
			}
			if (host == null)
			{
				throw new ArgumentNullException("host");
			}
			if (msg.m_isSent)
			{
				throw new NetException("This message has already been sent! Use NetPeer.SendMessage() to send to multiple recipients efficiently");
			}
			if (msg.LengthBytes > m_configuration.MaximumTransmissionUnit)
			{
				throw new NetException("Unconnected messages too long! Must be shorter than NetConfiguration.MaximumTransmissionUnit (currently " + m_configuration.MaximumTransmissionUnit + ")");
			}
			IPAddress iPAddress = NetUtility.Resolve(host);
			if (iPAddress == null)
			{
				throw new NetException("Failed to resolve " + host);
			}
			msg.m_messageType = NetMessageType.Unconnected;
			Interlocked.Increment(ref msg.m_recyclingCount);
			m_unsentUnconnectedMessages.Enqueue(new NetTuple<IPEndPoint, NetOutgoingMessage>(new IPEndPoint(iPAddress, port), msg));
		}

		public void SendUnconnectedMessage(NetOutgoingMessage msg, IPEndPoint recipient)
		{
			if (msg == null)
			{
				throw new ArgumentNullException("msg");
			}
			if (recipient == null)
			{
				throw new ArgumentNullException("recipient");
			}
			if (msg.m_isSent)
			{
				throw new NetException("This message has already been sent! Use NetPeer.SendMessage() to send to multiple recipients efficiently");
			}
			if (msg.LengthBytes > m_configuration.MaximumTransmissionUnit)
			{
				throw new NetException("Unconnected messages too long! Must be shorter than NetConfiguration.MaximumTransmissionUnit (currently " + m_configuration.MaximumTransmissionUnit + ")");
			}
			msg.m_messageType = NetMessageType.Unconnected;
			msg.m_isSent = true;
			Interlocked.Increment(ref msg.m_recyclingCount);
			m_unsentUnconnectedMessages.Enqueue(new NetTuple<IPEndPoint, NetOutgoingMessage>(recipient, msg));
		}

		public void SendUnconnectedMessage(NetOutgoingMessage msg, IList<IPEndPoint> recipients)
		{
			if (msg == null)
			{
				throw new ArgumentNullException("msg");
			}
			if (recipients == null)
			{
				throw new ArgumentNullException("recipients");
			}
			if (msg.m_isSent)
			{
				throw new NetException("This message has already been sent! Use NetPeer.SendMessage() to send to multiple recipients efficiently");
			}
			if (msg.LengthBytes > m_configuration.MaximumTransmissionUnit)
			{
				throw new NetException("Unconnected messages too long! Must be shorter than NetConfiguration.MaximumTransmissionUnit (currently " + m_configuration.MaximumTransmissionUnit + ")");
			}
			msg.m_messageType = NetMessageType.Unconnected;
			msg.m_isSent = true;
			Interlocked.Add(ref msg.m_recyclingCount, recipients.Count);
			foreach (IPEndPoint recipient in recipients)
			{
				m_unsentUnconnectedMessages.Enqueue(new NetTuple<IPEndPoint, NetOutgoingMessage>(recipient, msg));
			}
		}
	}
}
