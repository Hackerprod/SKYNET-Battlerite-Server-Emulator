using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Lidgren.Network
{
	public class NetRC2Encryption : INetEncryption
	{
		private readonly byte[] m_key;

		private readonly byte[] m_iv;

		private readonly int m_bitSize;

		private static readonly List<int> m_keysizes;

		private static readonly List<int> m_blocksizes;

		static NetRC2Encryption()
		{
			RC2CryptoServiceProvider rC2CryptoServiceProvider = new RC2CryptoServiceProvider();
			List<int> list = new List<int>();
			KeySizes[] legalKeySizes = rC2CryptoServiceProvider.LegalKeySizes;
			foreach (KeySizes keySizes in legalKeySizes)
			{
				for (int j = keySizes.MinSize; j <= keySizes.MaxSize; j += keySizes.SkipSize)
				{
					if (!list.Contains(j))
					{
						list.Add(j);
					}
					if (j == keySizes.MaxSize)
					{
						break;
					}
				}
			}
			m_keysizes = list;
			list = new List<int>();
			KeySizes[] legalBlockSizes = rC2CryptoServiceProvider.LegalBlockSizes;
			foreach (KeySizes keySizes2 in legalBlockSizes)
			{
				for (int l = keySizes2.MinSize; l <= keySizes2.MaxSize; l += keySizes2.SkipSize)
				{
					if (!list.Contains(l))
					{
						list.Add(l);
					}
					if (l == keySizes2.MaxSize)
					{
						break;
					}
				}
			}
			m_blocksizes = list;
		}

		public NetRC2Encryption(byte[] key, byte[] iv)
		{
			if (!m_keysizes.Contains(key.Length * 8))
			{
				string text = m_keysizes.Aggregate("", (string current, int i) => current + $"{i}, ");
				text = text.Remove(text.Length - 3);
				throw new NetException($"Not a valid key size. (Valid values are: {text})");
			}
			if (!m_blocksizes.Contains(iv.Length * 8))
			{
				string text2 = m_blocksizes.Aggregate("", (string current, int i) => current + $"{i}, ");
				text2 = text2.Remove(text2.Length - 3);
				throw new NetException($"Not a valid iv size. (Valid values are: {text2})");
			}
			m_key = key;
			m_iv = iv;
			m_bitSize = m_key.Length * 8;
		}

		public NetRC2Encryption(string key, int bitsize)
		{
			if (!m_keysizes.Contains(bitsize))
			{
				string text = m_keysizes.Aggregate("", (string current, int i) => current + $"{i}, ");
				text = text.Remove(text.Length - 3);
				throw new NetException($"Not a valid key size. (Valid values are: {text})");
			}
			byte[] array = Encoding.UTF32.GetBytes(key);
			HMACSHA512 hMACSHA = new HMACSHA512(Convert.FromBase64String("i88NEiez3c50bHqr3YGasDc4p8jRrxJAaiRiqixpvp4XNAStP5YNoC2fXnWkURtkha6M8yY901Gj07IRVIRyGL=="));
			hMACSHA.Initialize();
			for (int j = 0; j < 1000; j++)
			{
				array = hMACSHA.ComputeHash(array);
			}
			int num = bitsize / 8;
			m_key = new byte[num];
			Buffer.BlockCopy(array, 0, m_key, 0, num);
			m_iv = new byte[m_blocksizes[0] / 8];
			Buffer.BlockCopy(array, array.Length - m_iv.Length - 1, m_iv, 0, m_iv.Length);
			m_bitSize = bitsize;
		}

		public NetRC2Encryption(string key)
			: this(key, m_keysizes.Max())
		{
		}

		public bool Encrypt(NetOutgoingMessage msg)
		{
			try
			{
				RC2CryptoServiceProvider rC2CryptoServiceProvider = new RC2CryptoServiceProvider();
				rC2CryptoServiceProvider.KeySize = m_bitSize;
				rC2CryptoServiceProvider.Mode = CipherMode.CBC;
				using (RC2CryptoServiceProvider rC2CryptoServiceProvider2 = rC2CryptoServiceProvider)
				{
					using (ICryptoTransform transform = rC2CryptoServiceProvider2.CreateEncryptor(m_key, m_iv))
					{
						using (MemoryStream memoryStream = new MemoryStream())
						{
							using (CryptoStream cryptoStream = new CryptoStream(memoryStream, transform, CryptoStreamMode.Write))
							{
								cryptoStream.Write(msg.m_data, 0, msg.m_data.Length);
							}
							msg.m_data = memoryStream.ToArray();
						}
					}
				}
			}
			catch
			{
				return false;
			}
			return true;
		}

		public bool Decrypt(NetIncomingMessage msg)
		{
			try
			{
				RC2CryptoServiceProvider rC2CryptoServiceProvider = new RC2CryptoServiceProvider();
				rC2CryptoServiceProvider.KeySize = m_bitSize;
				rC2CryptoServiceProvider.Mode = CipherMode.CBC;
				using (RC2CryptoServiceProvider rC2CryptoServiceProvider2 = rC2CryptoServiceProvider)
				{
					using (ICryptoTransform transform = rC2CryptoServiceProvider2.CreateDecryptor(m_key, m_iv))
					{
						using (MemoryStream memoryStream = new MemoryStream())
						{
							using (CryptoStream cryptoStream = new CryptoStream(memoryStream, transform, CryptoStreamMode.Write))
							{
								cryptoStream.Write(msg.m_data, 0, msg.m_data.Length);
							}
							msg.m_data = memoryStream.ToArray();
						}
					}
				}
			}
			catch
			{
				return false;
			}
			return true;
		}
	}
}
