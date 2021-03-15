using System;

namespace Lidgren.Network
{
	public abstract class NetBlockEncryptionBase : INetEncryption
	{
		private byte[] m_tmp;

		public abstract int BlockSize
		{
			get;
		}

		public NetBlockEncryptionBase()
		{
			m_tmp = new byte[BlockSize];
		}

		public bool Encrypt(NetOutgoingMessage msg)
		{
			int lengthBits = msg.LengthBits;
			int lengthBytes = msg.LengthBytes;
			int blockSize = BlockSize;
			int num = (int)Math.Ceiling((double)lengthBytes / (double)blockSize);
			int num2 = num * blockSize;
			msg.EnsureBufferSize(num2 * 8 + 2);
			msg.LengthBits = num2 * 8;
			for (int i = 0; i < num; i++)
			{
				EncryptBlock(msg.m_data, i * blockSize, m_tmp);
				Buffer.BlockCopy(m_tmp, 0, msg.m_data, i * blockSize, m_tmp.Length);
			}
			msg.Write((ushort)lengthBits);
			return true;
		}

		public bool Decrypt(NetIncomingMessage msg)
		{
			int num = msg.LengthBytes - 2;
			int blockSize = BlockSize;
			int num2 = num / blockSize;
			if (num2 * blockSize != num)
			{
				return false;
			}
			for (int i = 0; i < num2; i++)
			{
				DecryptBlock(msg.m_data, i * blockSize, m_tmp);
				Buffer.BlockCopy(m_tmp, 0, msg.m_data, i * blockSize, m_tmp.Length);
			}
			uint num3 = (uint)(msg.m_bitLength = (int)NetBitWriter.ReadUInt32(msg.m_data, 16, num * 8));
			return true;
		}

		protected abstract void EncryptBlock(byte[] source, int sourceOffset, byte[] destination);

		protected abstract void DecryptBlock(byte[] source, int sourceOffset, byte[] destination);
	}
}
