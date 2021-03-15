using System;
using System.Security.Cryptography;
using System.Text;

namespace Lidgren.Network
{
	public sealed class NetXtea : NetBlockEncryptionBase
	{
		private const int c_blockSize = 8;

		private const int c_keySize = 16;

		private const int c_delta = -1640531527;

		private readonly int m_numRounds;

		private readonly uint[] m_sum0;

		private readonly uint[] m_sum1;

		public override int BlockSize => 8;

		public NetXtea(byte[] key, int rounds)
		{
			if (key.Length < 16)
			{
				throw new NetException("Key too short!");
			}
			m_numRounds = rounds;
			m_sum0 = new uint[m_numRounds];
			m_sum1 = new uint[m_numRounds];
			uint[] array = new uint[8];
			int num;
			int num2 = num = 0;
			while (num2 < 4)
			{
				array[num2] = BitConverter.ToUInt32(key, num);
				num2++;
				num += 4;
			}
			for (num2 = (num = 0); num2 < 32; num2++)
			{
				m_sum0[num2] = (uint)(num + (int)array[num & 3]);
				num += -1640531527;
				m_sum1[num2] = (uint)(num + (int)array[(num >> 11) & 3]);
			}
		}

		public NetXtea(byte[] key)
			: this(key, 32)
		{
		}

		public NetXtea(string key)
			: this(SHA1.Create().ComputeHash(Encoding.UTF8.GetBytes(key)), 32)
		{
		}

		protected override void EncryptBlock(byte[] source, int sourceOffset, byte[] destination)
		{
			uint num = BytesToUInt(source, sourceOffset);
			uint num2 = BytesToUInt(source, sourceOffset + 4);
			for (int i = 0; i != m_numRounds; i++)
			{
				num += ((((num2 << 4) ^ (num2 >> 5)) + num2) ^ m_sum0[i]);
				num2 += ((((num << 4) ^ (num >> 5)) + num) ^ m_sum1[i]);
			}
			UIntToBytes(num, destination, 0);
			UIntToBytes(num2, destination, 4);
		}

		protected override void DecryptBlock(byte[] source, int sourceOffset, byte[] destination)
		{
			uint num = BytesToUInt(source, sourceOffset);
			uint num2 = BytesToUInt(source, sourceOffset + 4);
			for (int num3 = m_numRounds - 1; num3 >= 0; num3--)
			{
				num2 -= ((((num << 4) ^ (num >> 5)) + num) ^ m_sum1[num3]);
				num -= ((((num2 << 4) ^ (num2 >> 5)) + num2) ^ m_sum0[num3]);
			}
			UIntToBytes(num, destination, 0);
			UIntToBytes(num2, destination, 4);
		}

		private static uint BytesToUInt(byte[] bytes, int offset)
		{
			uint num = (uint)(bytes[offset] << 24);
			num = (uint)((int)num | (bytes[++offset] << 16));
			num = (uint)((int)num | (bytes[++offset] << 8));
			return num | bytes[++offset];
		}

		private static void UIntToBytes(uint value, byte[] destination, int destinationOffset)
		{
			destination[destinationOffset++] = (byte)(value >> 24);
			destination[destinationOffset++] = (byte)(value >> 16);
			destination[destinationOffset++] = (byte)(value >> 8);
			destination[destinationOffset++] = (byte)value;
		}
	}
}
