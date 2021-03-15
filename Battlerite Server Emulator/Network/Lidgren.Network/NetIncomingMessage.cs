using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Reflection;
using System.Text;

namespace Lidgren.Network
{
	[DebuggerDisplay("Type={MessageType} LengthBits={LengthBits}")]
	public class NetIncomingMessage
	{
		private const string c_readOverflowError = "Trying to read past the buffer size - likely caused by mismatching Write/Reads, different size or order.";

		internal byte[] m_data;

		internal int m_bitLength;

		internal NetIncomingMessageType m_incomingMessageType;

		internal IPEndPoint m_senderEndpoint;

		internal NetConnection m_senderConnection;

		internal int m_sequenceNumber;

		internal NetMessageType m_receivedMessageType;

		internal bool m_isFragment;

		internal double m_receiveTime;

		private static readonly Dictionary<Type, MethodInfo> s_readMethods;

		internal int m_readPosition;

		public NetIncomingMessageType MessageType => m_incomingMessageType;

		public NetDeliveryMethod DeliveryMethod => NetUtility.GetDeliveryMethod(m_receivedMessageType);

		public int SequenceChannel => (int)m_receivedMessageType - (int)NetUtility.GetDeliveryMethod(m_receivedMessageType);

		public IPEndPoint SenderEndpoint => m_senderEndpoint;

		public NetConnection SenderConnection => m_senderConnection;

		public double ReceiveTime => m_receiveTime;

		public int LengthBytes => m_bitLength + 7 >> 3;

		public int LengthBits
		{
			get
			{
				return m_bitLength;
			}
			internal set
			{
				m_bitLength = value;
			}
		}

		public long Position
		{
			get
			{
				return m_readPosition;
			}
			set
			{
				m_readPosition = (int)value;
			}
		}

		public int PositionInBytes => m_readPosition / 8;

		internal NetIncomingMessage()
		{
		}

		internal NetIncomingMessage(NetIncomingMessageType tp)
		{
			m_incomingMessageType = tp;
		}

		internal void Reset()
		{
			m_incomingMessageType = NetIncomingMessageType.Error;
			m_readPosition = 0;
			m_receivedMessageType = NetMessageType.LibraryError;
			m_senderConnection = null;
			m_bitLength = 0;
			m_isFragment = false;
		}

		public bool Decrypt(INetEncryption encryption)
		{
			return encryption.Decrypt(this);
		}

		public override string ToString()
		{
			return "[NetIncomingMessage #" + m_sequenceNumber + " " + LengthBytes + " bytes]";
		}

		public byte[] PeekDataBuffer()
		{
			return m_data;
		}

		public bool PeekBoolean()
		{
			byte b = NetBitWriter.ReadByte(m_data, 1, m_readPosition);
			if (b <= 0)
			{
				return false;
			}
			return true;
		}

		public byte PeekByte()
		{
			return NetBitWriter.ReadByte(m_data, 8, m_readPosition);
		}

		[CLSCompliant(false)]
		public sbyte PeekSByte()
		{
			byte b = NetBitWriter.ReadByte(m_data, 8, m_readPosition);
			return (sbyte)b;
		}

		public byte PeekByte(int numberOfBits)
		{
			return NetBitWriter.ReadByte(m_data, numberOfBits, m_readPosition);
		}

		public byte[] PeekBytes(int numberOfBytes)
		{
			byte[] array = new byte[numberOfBytes];
			NetBitWriter.ReadBytes(m_data, numberOfBytes, m_readPosition, array, 0);
			return array;
		}

		public void PeekBytes(byte[] into, int offset, int numberOfBytes)
		{
			NetBitWriter.ReadBytes(m_data, numberOfBytes, m_readPosition, into, offset);
		}

		public short PeekInt16()
		{
			uint num = NetBitWriter.ReadUInt32(m_data, 16, m_readPosition);
			return (short)num;
		}

		[CLSCompliant(false)]
		public ushort PeekUInt16()
		{
			uint num = NetBitWriter.ReadUInt32(m_data, 16, m_readPosition);
			return (ushort)num;
		}

		public int PeekInt32()
		{
			return (int)NetBitWriter.ReadUInt32(m_data, 32, m_readPosition);
		}

		public int PeekInt32(int numberOfBits)
		{
			uint num = NetBitWriter.ReadUInt32(m_data, numberOfBits, m_readPosition);
			if (numberOfBits == 32)
			{
				return (int)num;
			}
			int num2 = 1 << numberOfBits - 1;
			if ((num & num2) == 0)
			{
				return (int)num;
			}
			uint num3 = uint.MaxValue >> 33 - numberOfBits;
			uint num4 = (num & num3) + 1;
			return (int)(0 - num4);
		}

		[CLSCompliant(false)]
		public uint PeekUInt32()
		{
			return NetBitWriter.ReadUInt32(m_data, 32, m_readPosition);
		}

		[CLSCompliant(false)]
		public uint PeekUInt32(int numberOfBits)
		{
			return NetBitWriter.ReadUInt32(m_data, numberOfBits, m_readPosition);
		}

		[CLSCompliant(false)]
		public ulong PeekUInt64()
		{
			ulong num = NetBitWriter.ReadUInt32(m_data, 32, m_readPosition);
			ulong num2 = NetBitWriter.ReadUInt32(m_data, 32, m_readPosition + 32);
			return num + (num2 << 32);
		}

		public long PeekInt64()
		{
			return (long)PeekUInt64();
		}

		[CLSCompliant(false)]
		public ulong PeekUInt64(int numberOfBits)
		{
			if (numberOfBits <= 32)
			{
				return NetBitWriter.ReadUInt32(m_data, numberOfBits, m_readPosition);
			}
			ulong num = NetBitWriter.ReadUInt32(m_data, 32, m_readPosition);
			return num | NetBitWriter.ReadUInt32(m_data, numberOfBits - 32, m_readPosition);
		}

		public long PeekInt64(int numberOfBits)
		{
			return (long)PeekUInt64(numberOfBits);
		}

		public float PeekFloat()
		{
			return PeekSingle();
		}

		public float PeekSingle()
		{
			if ((m_readPosition & 7) == 0)
			{
				return BitConverter.ToSingle(m_data, m_readPosition >> 3);
			}
			byte[] value = PeekBytes(4);
			return BitConverter.ToSingle(value, 0);
		}

		public double PeekDouble()
		{
			if ((m_readPosition & 7) == 0)
			{
				return BitConverter.ToDouble(m_data, m_readPosition >> 3);
			}
			byte[] value = PeekBytes(8);
			return BitConverter.ToDouble(value, 0);
		}

		public string PeekString()
		{
			int readPosition = m_readPosition;
			string result = ReadString();
			m_readPosition = readPosition;
			return result;
		}

		static NetIncomingMessage()
		{
			Type[] types = typeof(byte).Assembly.GetTypes();
			s_readMethods = new Dictionary<Type, MethodInfo>();
			MethodInfo[] methods = typeof(NetIncomingMessage).GetMethods(BindingFlags.Instance | BindingFlags.Public);
			MethodInfo[] array = methods;
			foreach (MethodInfo methodInfo in array)
			{
				if (methodInfo.GetParameters().Length == 0 && methodInfo.Name.StartsWith("Read", StringComparison.InvariantCulture))
				{
					string b = methodInfo.Name.Substring(4);
					Type[] array2 = types;
					foreach (Type type in array2)
					{
						if (type.Name == b)
						{
							s_readMethods[type] = methodInfo;
						}
					}
				}
			}
		}

		public bool ReadBoolean()
		{
			byte b = NetBitWriter.ReadByte(m_data, 1, m_readPosition);
			m_readPosition++;
			if (b <= 0)
			{
				return false;
			}
			return true;
		}

		public byte ReadByte()
		{
			byte result = NetBitWriter.ReadByte(m_data, 8, m_readPosition);
			m_readPosition += 8;
			return result;
		}

		[CLSCompliant(false)]
		public sbyte ReadSByte()
		{
			byte b = NetBitWriter.ReadByte(m_data, 8, m_readPosition);
			m_readPosition += 8;
			return (sbyte)b;
		}

		public byte ReadByte(int numberOfBits)
		{
			byte result = NetBitWriter.ReadByte(m_data, numberOfBits, m_readPosition);
			m_readPosition += numberOfBits;
			return result;
		}

		public byte[] ReadBytes(int numberOfBytes)
		{
			byte[] array = new byte[numberOfBytes];
			NetBitWriter.ReadBytes(m_data, numberOfBytes, m_readPosition, array, 0);
			m_readPosition += 8 * numberOfBytes;
			return array;
		}

		public void ReadBytes(byte[] into, int offset, int numberOfBytes)
		{
			NetBitWriter.ReadBytes(m_data, numberOfBytes, m_readPosition, into, offset);
			m_readPosition += 8 * numberOfBytes;
		}

		public void ReadBits(byte[] into, int offset, int numberOfBits)
		{
			int num = numberOfBits / 8;
			int num2 = numberOfBits - num * 8;
			NetBitWriter.ReadBytes(m_data, num, m_readPosition, into, offset);
			m_readPosition += 8 * num;
			if (num2 > 0)
			{
				into[offset + num] = ReadByte(num2);
			}
		}

		public short ReadInt16()
		{
			uint num = NetBitWriter.ReadUInt32(m_data, 16, m_readPosition);
			m_readPosition += 16;
			return (short)num;
		}

		[CLSCompliant(false)]
		public ushort ReadUInt16()
		{
			uint num = NetBitWriter.ReadUInt32(m_data, 16, m_readPosition);
			m_readPosition += 16;
			return (ushort)num;
		}

		public int ReadInt32()
		{
			uint result = NetBitWriter.ReadUInt32(m_data, 32, m_readPosition);
			m_readPosition += 32;
			return (int)result;
		}

		public int ReadInt32(int numberOfBits)
		{
			uint num = NetBitWriter.ReadUInt32(m_data, numberOfBits, m_readPosition);
			m_readPosition += numberOfBits;
			if (numberOfBits == 32)
			{
				return (int)num;
			}
			int num2 = 1 << numberOfBits - 1;
			if ((num & num2) == 0)
			{
				return (int)num;
			}
			uint num3 = uint.MaxValue >> 33 - numberOfBits;
			uint num4 = (num & num3) + 1;
			return (int)(0 - num4);
		}

		[CLSCompliant(false)]
		public uint ReadUInt32()
		{
			uint result = NetBitWriter.ReadUInt32(m_data, 32, m_readPosition);
			m_readPosition += 32;
			return result;
		}

		[CLSCompliant(false)]
		public uint ReadUInt32(int numberOfBits)
		{
			uint result = NetBitWriter.ReadUInt32(m_data, numberOfBits, m_readPosition);
			m_readPosition += numberOfBits;
			return result;
		}

		[CLSCompliant(false)]
		public ulong ReadUInt64()
		{
			ulong num = NetBitWriter.ReadUInt32(m_data, 32, m_readPosition);
			m_readPosition += 32;
			ulong num2 = NetBitWriter.ReadUInt32(m_data, 32, m_readPosition);
			ulong result = num + (num2 << 32);
			m_readPosition += 32;
			return result;
		}

		public long ReadInt64()
		{
			return (long)ReadUInt64();
		}

		[CLSCompliant(false)]
		public ulong ReadUInt64(int numberOfBits)
		{
			ulong result;
			if (numberOfBits <= 32)
			{
				result = NetBitWriter.ReadUInt32(m_data, numberOfBits, m_readPosition);
			}
			else
			{
				result = NetBitWriter.ReadUInt32(m_data, 32, m_readPosition);
				result |= NetBitWriter.ReadUInt32(m_data, numberOfBits - 32, m_readPosition);
			}
			m_readPosition += numberOfBits;
			return result;
		}

		public long ReadInt64(int numberOfBits)
		{
			return (long)ReadUInt64(numberOfBits);
		}

		public float ReadFloat()
		{
			return ReadSingle();
		}

		public float ReadSingle()
		{
			if ((m_readPosition & 7) == 0)
			{
				float result = BitConverter.ToSingle(m_data, m_readPosition >> 3);
				m_readPosition += 32;
				return result;
			}
			byte[] value = ReadBytes(4);
			return BitConverter.ToSingle(value, 0);
		}

		public double ReadDouble()
		{
			if ((m_readPosition & 7) == 0)
			{
				double result = BitConverter.ToDouble(m_data, m_readPosition >> 3);
				m_readPosition += 64;
				return result;
			}
			byte[] value = ReadBytes(8);
			return BitConverter.ToDouble(value, 0);
		}

		[CLSCompliant(false)]
		public uint ReadVariableUInt32()
		{
			int num = 0;
			int num2 = 0;
			byte b;
			do
			{
				b = ReadByte();
				num |= (b & 0x7F) << num2;
				num2 += 7;
			}
			while ((b & 0x80) != 0);
			return (uint)num;
		}

		public int ReadVariableInt32()
		{
			uint num = ReadVariableUInt32();
			return (int)((num >> 1) ^ (0 - (num & 1)));
		}

		public long ReadVariableInt64()
		{
			ulong num = ReadVariableUInt64();
			return (long)((num >> 1) ^ (0L - (num & 1)));
		}

		[CLSCompliant(false)]
		public ulong ReadVariableUInt64()
		{
			ulong num = 0uL;
			int num2 = 0;
			byte b;
			do
			{
				b = ReadByte();
				num = (ulong)((long)num | (((long)b & 127L) << num2));
				num2 += 7;
			}
			while ((b & 0x80) != 0);
			return num;
		}

		public float ReadSignedSingle(int numberOfBits)
		{
			uint num = ReadUInt32(numberOfBits);
			int num2 = (1 << numberOfBits) - 1;
			return ((float)(double)(num + 1) / (float)(num2 + 1) - 0.5f) * 2f;
		}

		public float ReadUnitSingle(int numberOfBits)
		{
			uint num = ReadUInt32(numberOfBits);
			int num2 = (1 << numberOfBits) - 1;
			return (float)(double)(num + 1) / (float)(num2 + 1);
		}

		public float ReadRangedSingle(float min, float max, int numberOfBits)
		{
			float num = max - min;
			int num2 = (1 << numberOfBits) - 1;
			float num3 = (float)(double)ReadUInt32(numberOfBits);
			float num4 = num3 / (float)num2;
			return min + num4 * num;
		}

		public int ReadRangedInteger(int min, int max)
		{
			uint value = (uint)(max - min);
			int numberOfBits = NetUtility.BitsToHoldUInt(value);
			uint num = ReadUInt32(numberOfBits);
			return (int)(min + num);
		}

		public string ReadString()
		{
			int num = (int)ReadVariableUInt32();
			if (num == 0)
			{
				return string.Empty;
			}
			if ((m_readPosition & 7) == 0)
			{
                string @string = "";
                try
                {
                    @string = Encoding.UTF8.GetString(m_data, m_readPosition >> 3, num);
                    m_readPosition += 8 * num;
                }
                catch { }
                
				return @string;
			}
			byte[] array = ReadBytes(num);
			return Encoding.UTF8.GetString(array, 0, array.Length);
		}

		public IPEndPoint ReadIPEndpoint()
		{
			byte numberOfBytes = ReadByte();
			byte[] address = ReadBytes(numberOfBytes);
			int port = ReadUInt16();
			IPAddress address2 = new IPAddress(address);
			return new IPEndPoint(address2, port);
		}

		public double ReadTime(bool highPrecision)
		{
			double num = highPrecision ? ReadDouble() : ((double)ReadSingle());
			if (m_senderConnection == null)
			{
				throw new NetException("Cannot call ReadTime() on message without a connected sender (ie. unconnected messages)");
			}
			return num - m_senderConnection.m_remoteTimeOffset;
		}

		public void SkipPadBits()
		{
			m_readPosition = (m_readPosition + 7 >> 3) * 8;
		}

		public void ReadPadBits()
		{
			m_readPosition = (m_readPosition + 7 >> 3) * 8;
		}

		public void SkipPadBits(int numberOfBits)
		{
			m_readPosition += numberOfBits;
		}

		public void ReadAllFields(object target)
		{
			ReadAllFields(target, BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		}

		public void ReadAllFields(object target, BindingFlags flags)
		{
			if (target != null)
			{
				Type type = target.GetType();
				FieldInfo[] fields = type.GetFields(flags);
				NetUtility.SortMembersList(fields);
				FieldInfo[] array = fields;
				foreach (FieldInfo fieldInfo in array)
				{
					if (s_readMethods.TryGetValue(fieldInfo.FieldType, out MethodInfo value))
					{
						object value2 = value.Invoke(this, null);
						fieldInfo.SetValue(target, value2);
					}
				}
			}
		}

		public void ReadAllProperties(object target)
		{
			ReadAllProperties(target, BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		}

		public void ReadAllProperties(object target, BindingFlags flags)
		{
			if (target == null)
			{
				throw new ArgumentNullException("target");
			}
			if (target != null)
			{
				Type type = target.GetType();
				PropertyInfo[] properties = type.GetProperties(flags);
				NetUtility.SortMembersList(properties);
				PropertyInfo[] array = properties;
				foreach (PropertyInfo propertyInfo in array)
				{
					if (s_readMethods.TryGetValue(propertyInfo.PropertyType, out MethodInfo value))
					{
						object obj = value.Invoke(this, null);
						MethodInfo setMethod = propertyInfo.GetSetMethod((flags & BindingFlags.NonPublic) == BindingFlags.NonPublic);
						setMethod.Invoke(target, new object[1]
						{
							obj
						});
					}
				}
			}
		}

		private void InternalEnsureBufferSize(int numberOfBits)
		{
			int num = numberOfBits + 7 >> 3;
			if (m_data == null)
			{
				m_data = new byte[num];
			}
			else if (m_data.Length < num)
			{
				Array.Resize(ref m_data, num);
			}
		}

		internal void Write(bool value)
		{
			InternalEnsureBufferSize(m_bitLength + 1);
			NetBitWriter.WriteByte((byte)(value ? 1 : 0), 1, m_data, m_bitLength);
			m_bitLength++;
		}

		internal void Write(byte source)
		{
			InternalEnsureBufferSize(m_bitLength + 8);
			NetBitWriter.WriteByte(source, 8, m_data, m_bitLength);
			m_bitLength += 8;
		}

		internal void Write(sbyte source)
		{
			InternalEnsureBufferSize(m_bitLength + 8);
			NetBitWriter.WriteByte((byte)source, 8, m_data, m_bitLength);
			m_bitLength += 8;
		}

		internal void Write(byte source, int numberOfBits)
		{
			InternalEnsureBufferSize(m_bitLength + numberOfBits);
			NetBitWriter.WriteByte(source, numberOfBits, m_data, m_bitLength);
			m_bitLength += numberOfBits;
		}

		internal void Write(byte[] source)
		{
			if (source == null)
			{
				throw new ArgumentNullException("source");
			}
			int num = source.Length * 8;
			InternalEnsureBufferSize(m_bitLength + num);
			NetBitWriter.WriteBytes(source, 0, source.Length, m_data, m_bitLength);
			m_bitLength += num;
		}

		internal void Write(byte[] source, int offsetInBytes, int numberOfBytes)
		{
			if (source == null)
			{
				throw new ArgumentNullException("source");
			}
			int num = numberOfBytes * 8;
			InternalEnsureBufferSize(m_bitLength + num);
			NetBitWriter.WriteBytes(source, offsetInBytes, numberOfBytes, m_data, m_bitLength);
			m_bitLength += num;
		}

		internal void Write(ushort source)
		{
			InternalEnsureBufferSize(m_bitLength + 16);
			NetBitWriter.WriteUInt32(source, 16, m_data, m_bitLength);
			m_bitLength += 16;
		}

		internal void Write(ushort source, int numberOfBits)
		{
			InternalEnsureBufferSize(m_bitLength + numberOfBits);
			NetBitWriter.WriteUInt32(source, numberOfBits, m_data, m_bitLength);
			m_bitLength += numberOfBits;
		}

		internal void Write(short source)
		{
			InternalEnsureBufferSize(m_bitLength + 16);
			NetBitWriter.WriteUInt32((uint)source, 16, m_data, m_bitLength);
			m_bitLength += 16;
		}

		internal void Write(int source)
		{
			InternalEnsureBufferSize(m_bitLength + 32);
			NetBitWriter.WriteUInt32((uint)source, 32, m_data, m_bitLength);
			m_bitLength += 32;
		}

		internal void Write(uint source)
		{
			InternalEnsureBufferSize(m_bitLength + 32);
			NetBitWriter.WriteUInt32(source, 32, m_data, m_bitLength);
			m_bitLength += 32;
		}

		internal void Write(uint source, int numberOfBits)
		{
			InternalEnsureBufferSize(m_bitLength + numberOfBits);
			NetBitWriter.WriteUInt32(source, numberOfBits, m_data, m_bitLength);
			m_bitLength += numberOfBits;
		}

		internal void Write(int source, int numberOfBits)
		{
			InternalEnsureBufferSize(m_bitLength + numberOfBits);
			if (numberOfBits != 32)
			{
				int num = 1 << numberOfBits - 1;
				source = ((source >= 0) ? (source & ~num) : ((-source - 1) | num));
			}
			NetBitWriter.WriteUInt32((uint)source, numberOfBits, m_data, m_bitLength);
			m_bitLength += numberOfBits;
		}

		internal void Write(ulong source)
		{
			InternalEnsureBufferSize(m_bitLength + 64);
			NetBitWriter.WriteUInt64(source, 64, m_data, m_bitLength);
			m_bitLength += 64;
		}

		internal void Write(ulong source, int numberOfBits)
		{
			InternalEnsureBufferSize(m_bitLength + numberOfBits);
			NetBitWriter.WriteUInt64(source, numberOfBits, m_data, m_bitLength);
			m_bitLength += numberOfBits;
		}

		internal void Write(long source)
		{
			InternalEnsureBufferSize(m_bitLength + 64);
			NetBitWriter.WriteUInt64((ulong)source, 64, m_data, m_bitLength);
			m_bitLength += 64;
		}

		internal void Write(long source, int numberOfBits)
		{
			InternalEnsureBufferSize(m_bitLength + numberOfBits);
			NetBitWriter.WriteUInt64((ulong)source, numberOfBits, m_data, m_bitLength);
			m_bitLength += numberOfBits;
		}

		internal void Write(float source)
		{
			byte[] bytes = BitConverter.GetBytes(source);
			Write(bytes);
		}

		internal void Write(double source)
		{
			byte[] bytes = BitConverter.GetBytes(source);
			Write(bytes);
		}

		internal int WriteVariableUInt32(uint value)
		{
			int num = 1;
			uint num2 = value;
			while (num2 >= 128)
			{
				Write((byte)(num2 | 0x80));
				num2 >>= 7;
				num++;
			}
			Write((byte)num2);
			return num;
		}

		internal int WriteVariableInt32(int value)
		{
			int num = 1;
			uint num2 = (uint)((value << 1) ^ (value >> 31));
			while (num2 >= 128)
			{
				Write((byte)(num2 | 0x80));
				num2 >>= 7;
				num++;
			}
			Write((byte)num2);
			return num;
		}

		internal int WriteVariableUInt64(ulong value)
		{
			int num = 1;
			ulong num2 = value;
			while (num2 >= 128)
			{
				Write((byte)(num2 | 0x80));
				num2 >>= 7;
				num++;
			}
			Write((byte)num2);
			return num;
		}

		internal void WriteSignedSingle(float value, int numberOfBits)
		{
			float num = (value + 1f) * 0.5f;
			int num2 = (1 << numberOfBits) - 1;
			uint source = (uint)(num * (float)num2);
			Write(source, numberOfBits);
		}

		internal void WriteUnitSingle(float value, int numberOfBits)
		{
			int num = (1 << numberOfBits) - 1;
			uint source = (uint)(value * (float)num);
			Write(source, numberOfBits);
		}

		internal void WriteRangedSingle(float value, float min, float max, int numberOfBits)
		{
			float num = max - min;
			float num2 = (value - min) / num;
			int num3 = (1 << numberOfBits) - 1;
			Write((uint)((float)num3 * num2), numberOfBits);
		}

		internal int WriteRangedInteger(int min, int max, int value)
		{
			uint value2 = (uint)(max - min);
			int num = NetUtility.BitsToHoldUInt(value2);
			uint source = (uint)(value - min);
			Write(source, num);
			return num;
		}

		internal void Write(string source)
		{
			if (string.IsNullOrEmpty(source))
			{
				InternalEnsureBufferSize(m_bitLength + 8);
				WriteVariableUInt32(0u);
			}
			else
			{
				byte[] bytes = Encoding.UTF8.GetBytes(source);
				int num = 1;
				uint num2 = (uint)bytes.Length;
				while (num2 >= 128)
				{
					num2 >>= 7;
					num++;
				}
				InternalEnsureBufferSize(m_bitLength + (bytes.Length + num) * 8);
				WriteVariableUInt32((uint)bytes.Length);
				Write(bytes);
			}
		}

		internal void Write(IPEndPoint endPoint)
		{
			byte[] addressBytes = endPoint.Address.GetAddressBytes();
			Write((byte)addressBytes.Length);
			Write(addressBytes);
			Write((ushort)endPoint.Port);
		}

		internal void WritePadBits()
		{
			m_bitLength = (m_bitLength + 7) / 8 * 8;
			InternalEnsureBufferSize(m_bitLength);
		}

		internal void WritePadBits(int numberOfBits)
		{
			m_bitLength += numberOfBits;
			InternalEnsureBufferSize(m_bitLength);
		}
	}
}
