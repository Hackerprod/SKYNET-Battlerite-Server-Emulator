using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Reflection;
using System.Text;

namespace Lidgren.Network
{
	[DebuggerDisplay("LengthBits={LengthBits}")]
	public sealed class NetOutgoingMessage
	{
		private const int c_overAllocateAmount = 4;

		internal NetMessageType m_messageType;

		internal bool m_isSent;

		internal int m_recyclingCount;

		internal int m_fragmentGroup;

		internal int m_fragmentGroupTotalBits;

		internal int m_fragmentChunkByteSize;

		internal int m_fragmentChunkNumber;

		private static Dictionary<Type, MethodInfo> s_writeMethods;

		internal byte[] m_data;

		internal int m_bitLength;

		public int LengthBytes
		{
			get
			{
				return m_bitLength + 7 >> 3;
			}
			set
			{
				m_bitLength = value * 8;
				InternalEnsureBufferSize(m_bitLength);
			}
		}

		public int LengthBits
		{
			get
			{
				return m_bitLength;
			}
			set
			{
				m_bitLength = value;
				InternalEnsureBufferSize(m_bitLength);
			}
		}

		internal NetOutgoingMessage()
		{
		}

		internal void Reset()
		{
			m_messageType = NetMessageType.LibraryError;
			m_bitLength = 0;
			m_isSent = false;
			m_recyclingCount = 0;
			m_fragmentGroup = 0;
		}

		internal int Encode(byte[] intoBuffer, int ptr, int sequenceNumber)
		{
			intoBuffer[ptr++] = (byte)m_messageType;
			byte b = (byte)((sequenceNumber << 1) | ((m_fragmentGroup != 0) ? 1 : 0));
			intoBuffer[ptr++] = b;
			intoBuffer[ptr++] = (byte)(sequenceNumber >> 7);
			if (m_fragmentGroup == 0)
			{
				intoBuffer[ptr++] = (byte)m_bitLength;
				intoBuffer[ptr++] = (byte)(m_bitLength >> 8);
				int num6 = NetUtility.BytesToHoldBits(m_bitLength);
				if (num6 > 0)
				{
					Buffer.BlockCopy(m_data, 0, intoBuffer, ptr, num6);
					ptr += num6;
				}
			}
			else
			{
				int num7 = ptr;
				intoBuffer[ptr++] = (byte)m_bitLength;
				intoBuffer[ptr++] = (byte)(m_bitLength >> 8);
				ptr = NetFragmentationHelper.WriteHeader(intoBuffer, ptr, m_fragmentGroup, m_fragmentGroupTotalBits, m_fragmentChunkByteSize, m_fragmentChunkNumber);
				int num10 = ptr - num7 - 2;
				int num11 = m_bitLength + num10 * 8;
				intoBuffer[num7] = (byte)num11;
				intoBuffer[num7 + 1] = (byte)(num11 >> 8);
				int num12 = NetUtility.BytesToHoldBits(m_bitLength);
				if (num12 > 0)
				{
					Buffer.BlockCopy(m_data, m_fragmentChunkNumber * m_fragmentChunkByteSize, intoBuffer, ptr, num12);
					ptr += num12;
				}
			}
			return ptr;
		}

		internal int GetEncodedSize()
		{
			int num = 5;
			if (m_fragmentGroup != 0)
			{
				num += NetFragmentationHelper.GetFragmentationHeaderSize(m_fragmentGroup, m_fragmentGroupTotalBits / 8, m_fragmentChunkByteSize, m_fragmentChunkNumber);
			}
			return num + LengthBytes;
		}

		public bool Encrypt(INetEncryption encryption)
		{
			return encryption.Encrypt(this);
		}

		public override string ToString()
		{
			return "[NetOutgoingMessage " + m_messageType + " " + LengthBytes + " bytes]";
		}

		static NetOutgoingMessage()
		{
			s_writeMethods = new Dictionary<Type, MethodInfo>();
			MethodInfo[] methods = typeof(NetOutgoingMessage).GetMethods(BindingFlags.Instance | BindingFlags.Public);
			MethodInfo[] array = methods;
			foreach (MethodInfo methodInfo in array)
			{
				if (methodInfo.Name.Equals("Write", StringComparison.InvariantCulture))
				{
					ParameterInfo[] parameters = methodInfo.GetParameters();
					if (parameters.Length == 1)
					{
						s_writeMethods[parameters[0].ParameterType] = methodInfo;
					}
				}
			}
		}

		public byte[] PeekDataBuffer()
		{
			return m_data;
		}

		public void EnsureBufferSize(int numberOfBits)
		{
			int num = numberOfBits + 7 >> 3;
			if (m_data == null)
			{
				m_data = new byte[num + 4];
			}
			else if (m_data.Length < num)
			{
				Array.Resize(ref m_data, num + 4);
			}
		}

		public void InternalEnsureBufferSize(int numberOfBits)
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

		public void Write(bool value)
		{
			EnsureBufferSize(m_bitLength + 1);
			NetBitWriter.WriteByte((byte)(value ? 1 : 0), 1, m_data, m_bitLength);
			m_bitLength++;
		}

		public void Write(byte source)
		{
			EnsureBufferSize(m_bitLength + 8);
			NetBitWriter.WriteByte(source, 8, m_data, m_bitLength);
			m_bitLength += 8;
		}

		[CLSCompliant(false)]
		public void Write(sbyte source)
		{
			EnsureBufferSize(m_bitLength + 8);
			NetBitWriter.WriteByte((byte)source, 8, m_data, m_bitLength);
			m_bitLength += 8;
		}

		public void Write(byte source, int numberOfBits)
		{
			EnsureBufferSize(m_bitLength + numberOfBits);
			NetBitWriter.WriteByte(source, numberOfBits, m_data, m_bitLength);
			m_bitLength += numberOfBits;
		}

		public void Write(byte[] source)
		{
			if (source == null)
			{
				throw new ArgumentNullException("source");
			}
			int num = source.Length * 8;
			EnsureBufferSize(m_bitLength + num);
			NetBitWriter.WriteBytes(source, 0, source.Length, m_data, m_bitLength);
			m_bitLength += num;
		}

		public void Write(byte[] source, int offsetInBytes, int numberOfBytes)
		{
			if (source == null)
			{
				throw new ArgumentNullException("source");
			}
			int num = numberOfBytes * 8;
			EnsureBufferSize(m_bitLength + num);
			NetBitWriter.WriteBytes(source, offsetInBytes, numberOfBytes, m_data, m_bitLength);
			m_bitLength += num;
		}

		[CLSCompliant(false)]
		public void Write(ushort source)
		{
			EnsureBufferSize(m_bitLength + 16);
			NetBitWriter.WriteUInt32(source, 16, m_data, m_bitLength);
			m_bitLength += 16;
		}

		[CLSCompliant(false)]
		public void Write(ushort source, int numberOfBits)
		{
			EnsureBufferSize(m_bitLength + numberOfBits);
			NetBitWriter.WriteUInt32(source, numberOfBits, m_data, m_bitLength);
			m_bitLength += numberOfBits;
		}

		public void Write(short source)
		{
			EnsureBufferSize(m_bitLength + 16);
			NetBitWriter.WriteUInt32((uint)source, 16, m_data, m_bitLength);
			m_bitLength += 16;
		}

		public void Write(int source)
		{
			EnsureBufferSize(m_bitLength + 32);
			NetBitWriter.WriteUInt32((uint)source, 32, m_data, m_bitLength);
			m_bitLength += 32;
		}

		[CLSCompliant(false)]
		public void Write(uint source)
		{
			EnsureBufferSize(m_bitLength + 32);
			NetBitWriter.WriteUInt32(source, 32, m_data, m_bitLength);
			m_bitLength += 32;
		}

		[CLSCompliant(false)]
		public void Write(uint source, int numberOfBits)
		{
			EnsureBufferSize(m_bitLength + numberOfBits);
			NetBitWriter.WriteUInt32(source, numberOfBits, m_data, m_bitLength);
			m_bitLength += numberOfBits;
		}

		public void Write(int source, int numberOfBits)
		{
			EnsureBufferSize(m_bitLength + numberOfBits);
			if (numberOfBits != 32)
			{
				int num = 1 << numberOfBits - 1;
				source = ((source >= 0) ? (source & ~num) : ((-source - 1) | num));
			}
			NetBitWriter.WriteUInt32((uint)source, numberOfBits, m_data, m_bitLength);
			m_bitLength += numberOfBits;
		}

		[CLSCompliant(false)]
		public void Write(ulong source)
		{
			EnsureBufferSize(m_bitLength + 64);
			NetBitWriter.WriteUInt64(source, 64, m_data, m_bitLength);
			m_bitLength += 64;
		}

		[CLSCompliant(false)]
		public void Write(ulong source, int numberOfBits)
		{
			EnsureBufferSize(m_bitLength + numberOfBits);
			NetBitWriter.WriteUInt64(source, numberOfBits, m_data, m_bitLength);
			m_bitLength += numberOfBits;
		}

		public void Write(long source)
		{
			EnsureBufferSize(m_bitLength + 64);
			NetBitWriter.WriteUInt64((ulong)source, 64, m_data, m_bitLength);
			m_bitLength += 64;
		}

		public void Write(long source, int numberOfBits)
		{
			EnsureBufferSize(m_bitLength + numberOfBits);
			NetBitWriter.WriteUInt64((ulong)source, numberOfBits, m_data, m_bitLength);
			m_bitLength += numberOfBits;
		}

		public void Write(float source)
		{
			byte[] bytes = BitConverter.GetBytes(source);
			Write(bytes);
		}

		public void Write(double source)
		{
			byte[] bytes = BitConverter.GetBytes(source);
			Write(bytes);
		}

		[CLSCompliant(false)]
		public int WriteVariableUInt32(uint value)
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

		public int WriteVariableInt32(int value)
		{
			uint value2 = (uint)((value << 1) ^ (value >> 31));
			return WriteVariableUInt32(value2);
		}

		public int WriteVariableInt64(long value)
		{
			ulong value2 = (ulong)((value << 1) ^ (value >> 63));
			return WriteVariableUInt64(value2);
		}

		[CLSCompliant(false)]
		public int WriteVariableUInt64(ulong value)
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

		public void WriteSignedSingle(float value, int numberOfBits)
		{
			float num = (value + 1f) * 0.5f;
			int num2 = (1 << numberOfBits) - 1;
			uint source = (uint)(num * (float)num2);
			Write(source, numberOfBits);
		}

		public void WriteUnitSingle(float value, int numberOfBits)
		{
			int num = (1 << numberOfBits) - 1;
			uint source = (uint)(value * (float)num);
			Write(source, numberOfBits);
		}

		public void WriteRangedSingle(float value, float min, float max, int numberOfBits)
		{
			float num = max - min;
			float num2 = (value - min) / num;
			int num3 = (1 << numberOfBits) - 1;
			Write((uint)((float)num3 * num2), numberOfBits);
		}

		public int WriteRangedInteger(int min, int max, int value)
		{
			uint value2 = (uint)(max - min);
			int num = NetUtility.BitsToHoldUInt(value2);
			uint source = (uint)(value - min);
			Write(source, num);
			return num;
		}

		public void Write(string source)
		{
			if (string.IsNullOrEmpty(source))
			{
				EnsureBufferSize(m_bitLength + 8);
				WriteVariableUInt32(0u);
			}
			else
			{
				byte[] bytes = Encoding.UTF8.GetBytes(source);
				EnsureBufferSize(m_bitLength + 8 + bytes.Length * 8);
				WriteVariableUInt32((uint)bytes.Length);
				Write(bytes);
			}
		}

		public void Write(IPEndPoint endPoint)
		{
			byte[] addressBytes = endPoint.Address.GetAddressBytes();
			Write((byte)addressBytes.Length);
			Write(addressBytes);
			Write((ushort)endPoint.Port);
		}

		public void WriteTime(double localTime, bool highPrecision)
		{
			if (highPrecision)
			{
				Write(localTime);
			}
			else
			{
				Write((float)localTime);
			}
		}

		public void WritePadBits()
		{
			m_bitLength = (m_bitLength + 7 >> 3) * 8;
			EnsureBufferSize(m_bitLength);
		}

		public void WritePadBits(int numberOfBits)
		{
			m_bitLength += numberOfBits;
			EnsureBufferSize(m_bitLength);
		}

		public void Write(NetOutgoingMessage message)
		{
			EnsureBufferSize(m_bitLength + message.LengthBytes * 8);
			Write(message.m_data, 0, message.LengthBytes);
			int num = message.m_bitLength % 8;
			if (num != 0)
			{
				int num2 = 8 - num;
				m_bitLength -= num2;
			}
		}

		public void Write(NetIncomingMessage message)
		{
			EnsureBufferSize(m_bitLength + message.LengthBytes * 8);
			Write(message.m_data, 0, message.LengthBytes);
			int num = message.m_bitLength % 8;
			if (num != 0)
			{
				int num2 = 8 - num;
				m_bitLength -= num2;
			}
		}

		public void WriteAllFields(object ob)
		{
			WriteAllFields(ob, BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		}

		public void WriteAllFields(object ob, BindingFlags flags)
		{
			if (ob == null)
			{
				return;
			}
			Type type = ob.GetType();
			FieldInfo[] fields = type.GetFields(flags);
			NetUtility.SortMembersList(fields);
			FieldInfo[] array = fields;
			int num = 0;
			FieldInfo fieldInfo;
			while (true)
			{
				if (num >= array.Length)
				{
					return;
				}
				fieldInfo = array[num];
				object value = fieldInfo.GetValue(ob);
				if (!s_writeMethods.TryGetValue(fieldInfo.FieldType, out MethodInfo value2))
				{
					break;
				}
				value2.Invoke(this, new object[1]
				{
					value
				});
				num++;
			}
			throw new NetException("Failed to find write method for type " + fieldInfo.FieldType);
		}

		public void WriteAllProperties(object ob)
		{
			WriteAllProperties(ob, BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		}

		public void WriteAllProperties(object ob, BindingFlags flags)
		{
			if (ob != null)
			{
				Type type = ob.GetType();
				PropertyInfo[] properties = type.GetProperties(flags);
				NetUtility.SortMembersList(properties);
				PropertyInfo[] array = properties;
				foreach (PropertyInfo propertyInfo in array)
				{
					MethodInfo getMethod = propertyInfo.GetGetMethod((flags & BindingFlags.NonPublic) == BindingFlags.NonPublic);
					object obj = getMethod.Invoke(ob, null);
					if (s_writeMethods.TryGetValue(propertyInfo.PropertyType, out MethodInfo value))
					{
						value.Invoke(this, new object[1]
						{
							obj
						});
					}
				}
			}
		}
	}
}
