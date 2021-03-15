using System;
using System.Net;
using System.Net.Sockets;

namespace SKYNET
{
    public static class NetHelpers
    {
        public static IPAddress GetLocalIP(Socket activeSocket)
        {
            IPEndPoint iPEndPoint = activeSocket.LocalEndPoint as IPEndPoint;
            if (iPEndPoint == null || iPEndPoint.Address == IPAddress.Any)
            {
                throw new Exception("Socket not connected");
            }
            return iPEndPoint.Address;
        }

        public static IPAddress GetIPAddress(uint ipAddr)
        {
            byte[] bytes = BitConverter.GetBytes(ipAddr);
            Array.Reverse(bytes);
            return new IPAddress(bytes);
        }

        public static uint GetIPAddress(IPAddress ipAddr)
        {
            byte[] addressBytes = ipAddr.GetAddressBytes();
            Array.Reverse(addressBytes);
            return BitConverter.ToUInt32(addressBytes, 0);
        }

        public static uint EndianSwap(uint input)
        {
            return (uint)IPAddress.NetworkToHostOrder((int)input);
        }

        public static ulong EndianSwap(ulong input)
        {
            return (ulong)IPAddress.NetworkToHostOrder((long)input);
        }

        public static ushort EndianSwap(ushort input)
        {
            return (ushort)IPAddress.NetworkToHostOrder((short)input);
        }

        public static bool TryParseIPEndPoint(string stringValue, out IPEndPoint endPoint)
        {
            string[] array = stringValue.Split(':');
            if (array.Length != 2)
            {
                endPoint = null;
                return false;
            }
            if (!IPAddress.TryParse(array[0], out IPAddress address))
            {
                endPoint = null;
                return false;
            }
            if (!int.TryParse(array[1], out int result))
            {
                endPoint = null;
                return false;
            }
            endPoint = new IPEndPoint(address, result);
            return true;
        }

        public static IPAddress GetBroadcastAddress(this IPAddress address, IPAddress subnetMask)
        {
            byte[] addressBytes = address.GetAddressBytes();
            byte[] addressBytes2 = subnetMask.GetAddressBytes();
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

        public static IPAddress GetNetworkAddress(this IPAddress address, IPAddress subnetMask)
        {
            byte[] addressBytes = address.GetAddressBytes();
            byte[] addressBytes2 = subnetMask.GetAddressBytes();
            if (addressBytes.Length != addressBytes2.Length)
            {
                throw new ArgumentException("Lengths of IP address and subnet mask do not match.");
            }
            byte[] array = new byte[addressBytes.Length];
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = (byte)(addressBytes[i] & addressBytes2[i]);
            }
            return new IPAddress(array);
        }

        public static bool IsInSameSubnet(this IPAddress address2, IPAddress address, IPAddress subnetMask)
        {
            IPAddress networkAddress = GetNetworkAddress(address, subnetMask);
            IPAddress networkAddress2 = GetNetworkAddress(address2, subnetMask);
            return networkAddress.Equals(networkAddress2);
        }
    }
}