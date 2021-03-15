using System.Net;
using System.Net.Sockets;

namespace SKYNET
{
    public interface IConnection
    {
        bool Connected { get; }
        IPEndPoint RemoteEndPoint { get; }
        void Send(byte[] msg);
        void Send(byte[] msg, ulong steamId);
        void Disconnect();
    }
}