using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace System.Net.Sockets
{
    public static class TcpClientExtension
    {
        public static bool IsConnected(this TcpClient sender)
        {
            return TcpClientExtension.GetState(sender) == TcpState.Established;
        }

        private static TcpState GetState(TcpClient sender)
        {
            bool flag = sender == null;
            TcpState result;
            if (flag)
            {
                result = TcpState.Unknown;
            }
            else
            {
                bool flag2 = sender.Client == null;
                if (flag2)
                {
                    result = TcpState.Closed;
                }
                else
                {
                    try
                    {
                        TcpConnectionInformation tcpConnectionInformation = IPGlobalProperties.GetIPGlobalProperties().GetActiveTcpConnections().SingleOrDefault((TcpConnectionInformation x) => x.LocalEndPoint.Equals(sender.Client.LocalEndPoint));
                        result = ((tcpConnectionInformation != null) ? tcpConnectionInformation.State : TcpState.Unknown);
                    }
                    catch
                    {
                        result = TcpState.Unknown;
                    }
                }
            }
            return result;
        }
    }
}

