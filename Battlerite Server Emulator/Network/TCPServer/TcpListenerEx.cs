using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SKYNET
{
    public class TcpListenerEx : TcpListener
    {
        public new bool Active => base.Active;

        public TcpListenerEx(IPEndPoint localEP) : base(localEP)
        {
        }

        public TcpListenerEx(IPAddress localaddr, int port) : base(localaddr, port)
        {
        }
    }
}
