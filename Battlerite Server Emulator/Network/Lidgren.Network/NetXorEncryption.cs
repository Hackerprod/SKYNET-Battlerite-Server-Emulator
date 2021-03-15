using System.Text;

namespace Lidgren.Network
{
	public class NetXorEncryption : INetEncryption
	{
		private byte[] m_key;

		public NetXorEncryption(byte[] key)
		{
			m_key = key;
		}

		public NetXorEncryption(string key)
		{
			m_key = Encoding.UTF8.GetBytes(key);
		}

		public bool Encrypt(NetOutgoingMessage msg)
		{
			int lengthBytes = msg.LengthBytes;
			for (int i = 0; i < lengthBytes; i++)
			{
				int num = i % m_key.Length;
				msg.m_data[i] = (byte)(msg.m_data[i] ^ m_key[num]);
			}
			return true;
		}

		public bool Decrypt(NetIncomingMessage msg)
		{
			int lengthBytes = msg.LengthBytes;
			for (int i = 0; i < lengthBytes; i++)
			{
				int num = i % m_key.Length;
				msg.m_data[i] = (byte)(msg.m_data[i] ^ m_key[num]);
			}
			return true;
		}
	}
}
