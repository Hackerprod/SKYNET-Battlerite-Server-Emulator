using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Reflection;

namespace Lidgren.Network
{
	public static class NetUtility
	{
		public static IPEndPoint Resolve(string ipOrHost, int port)
		{
			IPAddress address = Resolve(ipOrHost);
			return new IPEndPoint(address, port);
		}

		public static IPAddress Resolve(string ipOrHost)
		{
			if (string.IsNullOrEmpty(ipOrHost))
			{
				throw new ArgumentException("Supplied string must not be empty", "ipOrHost");
			}
			ipOrHost = ipOrHost.Trim();
			IPAddress address = null;
			if (!IPAddress.TryParse(ipOrHost, out address))
			{
				try
				{
					IPHostEntry hostEntry = Dns.GetHostEntry(ipOrHost);
					if (hostEntry == null)
					{
						return null;
					}
					IPAddress[] addressList = hostEntry.AddressList;
					foreach (IPAddress iPAddress in addressList)
					{
						if (iPAddress.AddressFamily == AddressFamily.InterNetwork)
						{
							return iPAddress;
						}
					}
					return null;
				}
				catch (SocketException ex)
				{
					if (ex.SocketErrorCode != SocketError.HostNotFound)
					{
						throw;
					}
					return null;
				}
			}
			if (address.AddressFamily == AddressFamily.InterNetwork)
			{
				return address;
			}
			throw new ArgumentException("This method will not currently resolve other than ipv4 addresses");
		}

		private static NetworkInterface GetNetworkInterface()
		{
			NetworkInterface[] allNetworkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
			if (allNetworkInterfaces == null || allNetworkInterfaces.Length < 1)
			{
				return null;
			}
			NetworkInterface networkInterface = null;
			NetworkInterface[] array = allNetworkInterfaces;
			foreach (NetworkInterface networkInterface2 in array)
			{
				if (networkInterface2.NetworkInterfaceType != NetworkInterfaceType.Loopback && networkInterface2.NetworkInterfaceType != NetworkInterfaceType.Unknown && networkInterface2.Supports(NetworkInterfaceComponent.IPv4))
				{
					if (networkInterface == null)
					{
						networkInterface = networkInterface2;
					}
					if (networkInterface2.OperationalStatus == OperationalStatus.Up)
					{
						return networkInterface2;
					}
				}
			}
			return networkInterface;
		}

		public static PhysicalAddress GetMacAddress()
		{
			return GetNetworkInterface()?.GetPhysicalAddress();
		}

		public static string ToHexString(long data)
		{
			return ToHexString(BitConverter.GetBytes(data));
		}

		public static string ToHexString(byte[] data)
		{
			char[] array = new char[data.Length * 2];
			for (int i = 0; i < data.Length; i++)
			{
				byte b = (byte)(data[i] >> 4);
				array[i * 2] = (char)((b > 9) ? (b + 55) : (b + 48));
				b = (byte)(data[i] & 0xF);
				array[i * 2 + 1] = (char)((b > 9) ? (b + 55) : (b + 48));
			}
			return new string(array);
		}

		public static IPAddress GetBroadcastAddress()
		{
			try
			{
				NetworkInterface networkInterface = GetNetworkInterface();
				if (networkInterface == null)
				{
					return null;
				}
				IPInterfaceProperties iPProperties = networkInterface.GetIPProperties();
				foreach (UnicastIPAddressInformation unicastAddress in iPProperties.UnicastAddresses)
				{
					if (unicastAddress != null && unicastAddress.Address != null && unicastAddress.Address.AddressFamily == AddressFamily.InterNetwork)
					{
						IPAddress iPv4Mask = unicastAddress.IPv4Mask;
						byte[] addressBytes = unicastAddress.Address.GetAddressBytes();
						byte[] addressBytes2 = iPv4Mask.GetAddressBytes();
						if (addressBytes.Length != addressBytes2.Length)
						{
							throw new ArgumentException("Lengths of IP address and subnet mask do not match.");
						}
						byte[] array = new byte[addressBytes.Length];
						for (int i = 0; i < array.Length; i++)
						{
							array[i] = (byte)(addressBytes[i] | (addressBytes2[i] ^ 0xFF));
						}
						return new IPAddress(array);
					}
				}
			}
			catch
			{
				return IPAddress.Broadcast;
			}
			return IPAddress.Broadcast;
		}

		public static IPAddress GetMyAddress(out IPAddress mask)
		{
			mask = null;
			NetworkInterface networkInterface = GetNetworkInterface();
			if (networkInterface == null)
			{
				mask = null;
				return null;
			}
			IPInterfaceProperties iPProperties = networkInterface.GetIPProperties();
			foreach (UnicastIPAddressInformation unicastAddress in iPProperties.UnicastAddresses)
			{
				if (unicastAddress != null && unicastAddress.Address != null && unicastAddress.Address.AddressFamily == AddressFamily.InterNetwork)
				{
					mask = unicastAddress.IPv4Mask;
					return unicastAddress.Address;
				}
			}
			return null;
		}

		public static bool IsLocal(IPEndPoint endpoint)
		{
			if (endpoint == null)
			{
				return false;
			}
			return IsLocal(endpoint.Address);
		}

		public static bool IsLocal(IPAddress remote)
		{
			IPAddress mask;
			IPAddress myAddress = GetMyAddress(out mask);
			if (mask == null)
			{
				return false;
			}
			uint num = BitConverter.ToUInt32(mask.GetAddressBytes(), 0);
			uint num2 = BitConverter.ToUInt32(remote.GetAddressBytes(), 0);
			uint num3 = BitConverter.ToUInt32(myAddress.GetAddressBytes(), 0);
			return (num2 & num) == (num3 & num);
		}

		[CLSCompliant(false)]
		public static int BitsToHoldUInt(uint value)
		{
			int num = 1;
			while ((value >>= 1) != 0)
			{
				num++;
			}
			return num;
		}

		public static int BytesToHoldBits(int numBits)
		{
			return (numBits + 7) / 8;
		}

		internal static uint SwapByteOrder(uint value)
		{
			return ((uint)((int)value & -16777216) >> 24) | ((value & 0xFF0000) >> 8) | ((value & 0xFF00) << 8) | ((value & 0xFF) << 24);
		}

		internal static ulong SwapByteOrder(ulong value)
		{
			return ((ulong)((long)value & -72057594037927936L) >> 56) | ((value & 0xFF000000000000) >> 40) | ((value & 0xFF0000000000) >> 24) | ((value & 0xFF00000000) >> 8) | ((value & 4278190080u) << 8) | ((value & 0xFF0000) << 24) | ((value & 0xFF00) << 40) | ((value & 0xFF) << 56);
		}

		internal static bool CompareElements(byte[] one, byte[] two)
		{
			if (one.Length != two.Length)
			{
				return false;
			}
			for (int i = 0; i < one.Length; i++)
			{
				if (one[i] != two[i])
				{
					return false;
				}
			}
			return true;
		}

		public static byte[] ToByteArray(string hexString)
		{
			byte[] array = new byte[hexString.Length / 2];
			for (int i = 0; i < hexString.Length; i += 2)
			{
				array[i / 2] = Convert.ToByte(hexString.Substring(i, 2), 16);
			}
			return array;
		}

		public static string ToHumanReadable(long bytes)
		{
			if (bytes < 4000)
			{
				return bytes + " bytes";
			}
			if (bytes < 1000000)
			{
				return Math.Round((double)bytes / 1000.0, 2) + " kilobytes";
			}
			return Math.Round((double)bytes / 1000000.0, 2) + " megabytes";
		}

		internal static int RelativeSequenceNumber(int nr, int expected)
		{
			int num = (nr + 1024 - expected) % 1024;
			if (num > 512)
			{
				num -= 1024;
			}
			return num;
		}

		public static int GetWindowSize(NetDeliveryMethod method)
		{
			switch (method)
			{
			case NetDeliveryMethod.Unknown:
				return 0;
			case NetDeliveryMethod.Unreliable:
			case NetDeliveryMethod.UnreliableSequenced:
				return 128;
			case NetDeliveryMethod.ReliableOrdered:
				return 64;
			default:
				return 64;
			}
		}

		internal static void SortMembersList(MemberInfo[] list)
		{
			int num = 1;
			while (num * 3 + 1 <= list.Length)
			{
				num = 3 * num + 1;
			}
			while (num > 0)
			{
				for (int i = num - 1; i < list.Length; i++)
				{
					MemberInfo memberInfo = list[i];
					int num2 = i;
					while (num2 >= num && string.Compare(list[num2 - num].Name, memberInfo.Name, StringComparison.InvariantCulture) > 0)
					{
						list[num2] = list[num2 - num];
						num2 -= num;
					}
					list[num2] = memberInfo;
				}
				num /= 3;
			}
		}

		internal static NetDeliveryMethod GetDeliveryMethod(NetMessageType mtp)
		{
			if ((int)mtp >= 67)
			{
				return NetDeliveryMethod.ReliableOrdered;
			}
			if ((int)mtp >= 35)
			{
				return NetDeliveryMethod.ReliableSequenced;
			}
			if ((int)mtp >= 34)
			{
				return NetDeliveryMethod.ReliableUnordered;
			}
			if ((int)mtp >= 2)
			{
				return NetDeliveryMethod.UnreliableSequenced;
			}
			return NetDeliveryMethod.Unreliable;
		}
	}
}
