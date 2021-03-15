using BloodGUI_Binding;
using BloodGUI_Binding.Web;
using Matrix;
using Matrix.Xml;
using Matrix.Xmpp.Base;
using Matrix.Xmpp.Sasl;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Xml;
using WebSocketSharp.Net;

namespace SKYNET.Dispatchers
{
    public class Chat : MsgHandler
    {
        private XmppStreamParser streamParser;

        public Chat()
        {
            const string LIC = @"YOUR LICENSE";
            Matrix.License.LicenseManager.SetLicense(LIC);

            // when something is wrong with your license you can find the error here
            //ilog.Info(Matrix.License.LicenseManager.LicenseError);

            //Matrix.Xml.
            streamParser = new XmppStreamParser();
            streamParser.OnStreamStart += streamParser_OnStreamStart;
            streamParser.OnStreamEnd += streamParser_OnStreamEnd;
            streamParser.OnStreamElement += streamParser_OnStreamElement;
            streamParser.OnError += StreamParser_OnError;
            streamParser.OnStreamStart += StreamParser_OnStreamStart;
        }
        public override void AddHandlers(MsgDispatcher dispatcher)
        {
            dispatcher["Chat"] = OnChat;
        }
        private void OnChat(RequestMessage request)
        {
            byte[] Body = default(byte[]);
            if (request.ContainsBody)
            {
                Body = request.Body;
            }
            else if (!string.IsNullOrEmpty(request.Query))
            {
                Body = Encoding.UTF8.GetBytes(request.Query);
            }

            byte[] bytes = new byte[Body.Length - 1];
            ChatMessageType type = (ChatMessageType)Body[0];
            Array.Copy(Body, 1, bytes, 0, bytes.Length);

            //ProcessMessage(bytes, request.ListenerResponse);

            //ilog.Error($"Received chat type {type}");
            //ilog.Error($"{Encoding.UTF8.GetString(bytes)}");
            XmppXElement Stanza = Matrix.Xml.XmppXElement.LoadXml($"<stream>{Encoding.UTF8.GetString(bytes)}</stream>");
            //ilog.Error($"{Stanza.}");

            if (Stanza is BloodGUI.Chat.CustomStatus)
            {
                ilog.Error("Presence");
            }
            if (Stanza is BloodGUI.Chat.FriendData)
            {
                ilog.Error("Presence");
            }
            if (Stanza is BloodGUI.Chat.FriendRequest)
            {
                ilog.Error("Presence");
            }
            if (Stanza is BloodGUI.Chat.PrivateMatchGroupData)
            {
                ilog.Error("Presence");
            }
            if (Stanza is BloodGUI.Chat.RoomBlobUpdateData)
            {
                ilog.Error("Presence");
            }
            if (Stanza is BloodGUI.Chat.RoomData)
            {
                ilog.Error("Presence");
            }
            if (Stanza is BloodGUI.Chat.RoomUser)
            {
                ilog.Error("Presence");
            }

            if (Stanza is Presence)
            {
                ilog.Error("Presence");
                //ProcessPresence(e.Stanza as Presence);
            }
            else if (Stanza is Message)
            {
                ilog.Error("Message");
                //ProcessMessage(e.Stanza as Message);
            }
            else if (Stanza is Iq)
            {
                ilog.Error("Iq");
                //ProcessIq(e.Stanza as Iq);
            }

            if (Stanza is Matrix.Xmpp.Sasl.Auth)
            {
                var auth = Stanza as Matrix.Xmpp.Sasl.Auth;
                if (auth.SaslMechanism == SaslMechanism.Plain)
                    ilog.Error("ProcessSaslPlainAuth");  //ProcessSaslPlainAuth(auth);
            }

        }
        private void StreamParser_OnStreamStart(object sender, StanzaEventArgs e)
        {
            ilog.Error($"OnStreamStart");
        }

        private void StreamParser_OnError(object sender, ExceptionEventArgs e)
        {
            ilog.Error($"OnError");
        }






        private void ProcessMessage(byte[] bytes, HttpListenerResponse listenerResponse)
        {
            string msg = Encoding.UTF8.GetString(bytes);
            if (!msg.StartsWith("<"))
            {
                streamParser.Write(Encoding.UTF8.GetBytes("<stream:stream>"), 0, Encoding.UTF8.GetBytes("<stream:stream>").Length);
                streamParser.Write(bytes, 0, bytes.Length);
                streamParser.Write(Encoding.UTF8.GetBytes("</stream:stream>"), 0, Encoding.UTF8.GetBytes("</stream:stream>").Length);
                return;
            }
            streamParser.Write(bytes, 0, bytes.Length);

        }
        void streamParser_OnStreamStart(object sender, StanzaEventArgs e)
        {
            ilog.Error("OnStreamStart: ");
            SendStreamHeader();
            //Send(BuildStreamFeatures());
        }
        void streamParser_OnStreamEnd(object sender, Matrix.EventArgs e)
        {
            ilog.Error("OnStreamEnd: ");
            //Disconnect();
        }
        void streamParser_OnStreamElement(object sender, StanzaEventArgs e)
        {
            ilog.Error("OnStreamElement: " + e.Stanza.GetType());
            e.Stanza.Save("c:/ss.txt");

            if (e.Stanza is Presence)
            {
                ilog.Error("Presence");
                //ProcessPresence(e.Stanza as Presence);
            }
            else if (e.Stanza  is Message)
            {
                ilog.Error("Message");
                //ProcessMessage(e.Stanza as Message);
            }
            else if (e.Stanza is Iq)
            {
                ilog.Error("Iq");
                //ProcessIq(e.Stanza as Iq);
            }

            if (e.Stanza is Matrix.Xmpp.Sasl.Auth)
            {
                var auth = e.Stanza as Matrix.Xmpp.Sasl.Auth;
                if (auth.SaslMechanism == SaslMechanism.Plain)
                    ilog.Error("ProcessSaslPlainAuth");  //ProcessSaslPlainAuth(auth);
            }
        }
        private void SendStreamHeader()
        {
            var stream = new Matrix.Xmpp.Client.Stream
            {
                Version = "1.0",
                From = "localhost",
                Id = Guid.NewGuid().ToString()
            };

            Send(stream.StartTag());
        }
        private void Send(string data)
        {
            // Convert the string data to byte data using ASCII encoding.
            byte[] byteData = Encoding.UTF8.GetBytes(data);

            // Begin sending the data to the remote device.
            //m_Sock.BeginSend(byteData, 0, byteData.Length, 0, SendCallback, null);
            ilog.Error("Send");
        }
    }
}
