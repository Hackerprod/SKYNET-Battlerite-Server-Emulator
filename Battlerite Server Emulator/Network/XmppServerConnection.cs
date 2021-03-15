using System;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Text;
using System.Net.Sockets;
using Matrix.Xmpp.Bind;
using Matrix.Xmpp.Client;
using Matrix.Xmpp.Roster;
using Matrix.Xmpp.Sasl;
using Matrix.Xmpp.Session;
using Matrix.Xmpp.Stream;
using Matrix.Xml;
using Matrix.Xmpp;
using Matrix;

using Iq = Matrix.Xmpp.Base.Iq;
using Message = Matrix.Xmpp.Base.Message;
using MxAuth = Matrix.Xmpp.Sasl.Auth;
using Presence = Matrix.Xmpp.Base.Presence;
using Stream = Matrix.Xmpp.Client.Stream;
using System.Collections.Generic;

namespace SKYNET.Network
{
	/// <summary>
	/// XMPPSeverConnection class.
	/// </summary>
	public class XmppSeverConnection
	{
        public static List<XmppSeverConnection> ServerConnections = new List<XmppSeverConnection>();

        public XmppSeverConnection()
		{
		    SessionId = null;
		    streamParser = new XmppStreamParser();
            
            streamParser.OnStreamStart += streamParser_OnStreamStart;
            streamParser.OnStreamEnd += streamParser_OnStreamEnd;
            streamParser.OnStreamElement += streamParser_OnStreamElement;

            

        }

		public XmppSeverConnection(Socket sock) : this()
		{	
			m_Sock = sock;
			m_Sock.BeginReceive(buffer, 0, BUFFERSIZE, 0, ReadCallback, null);		
		}
       
        private XmppStreamParser			streamParser;
		private Socket					m_Sock;
        private const int BUFFERSIZE = 1024;
        private byte[] buffer = new byte[BUFFERSIZE];

        // Jid binded to this connection
	    public Jid Jid;
	    private bool InitialPresence;
	    private Presence LastPresence;
        private bool streamFooterSent;

	    public void ReadCallback(IAsyncResult ar)
	    {
	        // Retrieve the state object and the handler socket
	        // from the asynchronous state object

	        // Read data from the client socket. 
	        try
	        {
	            int bytesRead = m_Sock.EndReceive(ar);

	            if (bytesRead > 0)
	            {
	                //streamParser.Push(buffer, 0, bytesRead);
	                streamParser.Write(buffer, 0, bytesRead);
	                // Not all data received. Get more.
	                m_Sock.BeginReceive(buffer, 0, BUFFERSIZE, 0, ReadCallback, null);
	            }
	            else
	                Disconnect();
	        }
	        catch (Exception)
	        {
	            Disconnect();
	        }
	    }

	    /// <summary>
        /// Disconnect socket
        /// </summary>
        public void Disconnect()
        {
            Debug.WriteLine("Socket Disconnect");

	        if (!streamFooterSent && m_Sock.Connected)
	        {
                Send("</stream:stream>");
                streamFooterSent = true;    
	        }

            // return right away if have not created socket
            if (m_Sock == null)
                return;

            try
            {
                // first, shutdown the socket (when connected
                if (m_Sock.Connected)
                    m_Sock.Shutdown(SocketShutdown.Both);
            }
            catch (Exception)
            {
            }
            try
            {
                // next, close the socket which terminates any pending
                // async operations
                m_Sock.Close();
            }
            catch (Exception)
            {
            }

            if (ServerConnections.Contains(this))
                ServerConnections.Remove(this);
        }

		private void Send(string data) 
		{
			// Convert the string data to byte data using ASCII encoding.
			byte[] byteData = Encoding.UTF8.GetBytes(data);

			// Begin sending the data to the remote device.
			m_Sock.BeginSend(byteData, 0, byteData.Length, 0, SendCallback, null);
		}

		private void SendCallback(IAsyncResult ar) 
		{
			try 
			{
				// Complete sending the data to the remote device.
				int bytesSent = m_Sock.EndSend(ar);
				Console.WriteLine("Sent {0} bytes to client.", bytesSent);

			} 
			catch (Exception e) 
			{
				Console.WriteLine(e.ToString());
			}
		}
	
		
		public void Stop()
		{
		    Disconnect();
		}
			
		
	    public const string XmppDomain = "localhost";
        public string   User            { get; set; }
        public string   Resource        { get; set; }
	    public string   SessionId       { get; set; }
        public bool     IsAuthenticated { get; set; }
        public bool     IsBinded        { get; set; }
        

        void streamParser_OnStreamEnd(object sender, Matrix.EventArgs e)
        {
            Disconnect();
        }

        void streamParser_OnStreamElement(object sender, StanzaEventArgs e)
        {
            Console.WriteLine("OnStreamElement: " + e);

            if (e.Stanza is Presence)
            {
                ProcessPresence(e.Stanza as Presence);
            }
            else if (e.Stanza  is Message)
            {
                ProcessMessage(e.Stanza as Message);
            }
            else if (e.Stanza is Iq)
            {
                ProcessIq(e.Stanza as Iq);
            }

            if (e.Stanza is MxAuth)
            {
                var auth = e.Stanza as MxAuth;
                if (auth.SaslMechanism == SaslMechanism.Plain)
                    ProcessSaslPlainAuth(auth);
            }
        }

	    void streamParser_OnStreamStart(object sender, StanzaEventArgs e)
        {
            SendStreamHeader();
            Send(BuildStreamFeatures());
        }

        private void ProcessMessage(Message msg)
        {
            var to = msg.To;

            // check if the destination of this message is available
            var con = ServerConnections.FirstOrDefault(c => c.Jid.Equals(to, new BareJidComparer()));
            if (con != null)
            {
                // found connection, stamp packet with from and route it.
                msg.From = Jid;
                con.Send(msg);
            }
            else
            {
                // connection not found. Return the message to the sender and stamp it with error
                msg.Type = MessageType.Error;
                msg.To = Jid;
                msg.From = to;
                msg.Add(new Matrix.Xmpp.Client.Error(Matrix.Xmpp.Base.ErrorCondition.ServiceUnavailable));
                Send(msg);
            }
        }

	    private void ProcessPresence(Presence pres)
	    {

            if (pres.Type == PresenceType.Subscribe)
            {
                // RFC 3.1.2.  Server Processing of Outbound Subscription Request
            }
            else if (pres.Type == PresenceType.Subscribed)
            {
                // RFC 3.1.5.  Server Processing of Outbound Subscription Approval
            }
            else if (pres.Type == PresenceType.Unsubscribed)
            {
                // RFC 3.2.2.  Server Processing of Outbound Subscription Cancellation
            }
            else if (pres.Type == PresenceType.Unsubscribe)
            {
                // RFC 3.3.2.  Server Processing of Outbound Unsubscribe
            }
            else if (pres.Type == PresenceType.Available
                    || pres.Type == PresenceType.Unavailable)
            {
                ProcessOutboundPresence(pres);
            }
	    }

        private void ProcessOutboundPresence(Presence pres)
        {
            if (pres.To == null || pres.To.Equals(XmppDomain, new FullJidComparer()))
            {
                // a presence to the server
                if (pres.Type == PresenceType.Available)
                {
                    if (IsBinded && !InitialPresence)
                    {
                        InitialPresence = true;

                        // request all initial presences of the contacts
                        for (int i = 0; i < 11; i++)
                        {
                            // but not myself
                            if (Jid.User.EndsWith(i.ToString()))
                                continue;

                            var jid = new Jid("user" + i + "@" + XmppDomain);
                            var con = ServerConnections.FirstOrDefault(sc => sc.Jid.Equals(jid, new BareJidComparer()));
                            if (con != null && con.LastPresence != null)
                                Send(con.LastPresence);
                        }
                    }
                }
                else if (pres.Type == PresenceType.Unavailable)
                {
                }

                // distribute presence
                // to all own resources, 
                pres.From = Jid;
                pres.To = null;
                LastPresence = pres;
                

                /*
                    then to all subscribed contacts
                    4.2.2.  Server Processing of Outbound Initial Presence

                    Upon receiving initial presence from a client, the user's server MUST send the initial presence stanza from the
                    full JID <user@domainpart/resourcepart> of the user to all contacts that are subscribed to the user's presence;
                    such contacts are those for which a JID is present in the user's roster with the 'subscription' attribute set to
                    a value of "from" or "both". 
                */

                
                for (int i = 0; i < 11; i++)
                {
                    // but not myself
                    if (Jid.User.EndsWith(i.ToString()))
                        continue;

                    var jid = new Jid("user" + i + "@" + XmppDomain);
                    var con = ServerConnections.FirstOrDefault(sc => sc.Jid.Equals(jid, new BareJidComparer()));
                    if (con != null)
                        con.Send(pres);
                }

                // Send the presence to all my own connected resources (sessions)
                ServerConnections
                    .Where(sc => sc.Jid.Equals(Jid, new BareJidComparer()))
                    .ToList()
                    .ForEach(
                        c => c.Send(pres)
                        );
            }
        }

	    private void ProcessIq(Iq iq)
		{
            if (iq.Query is Roster)
                ProcessRosterIq(iq);
            else if (iq.Query is Bind)
                ProcessBind(iq);
            else if (iq.Query is Session)
                ProcessSession(iq);
            else if (iq.To != null && !iq.To.Equals(XmppDomain, new FullJidComparer()))
                RouteIq(iq);
            else
            {
                // something we don't understand or do not support, reply with error
                Send(
                    new Matrix.Xmpp.Client.Iq()
                    {
                        Type = IqType.Error,
                        Id = iq.Id,
                        Error = new Matrix.Xmpp.Client.Error(Matrix.Xmpp.Base.ErrorCondition.FeatureNotImplemented)
                    });
            }
		}

		private void ProcessRosterIq(Iq iq)
		{
			if (iq.Type == IqType.Get)
			{
				// Send the roster
				// we send a dummy roster here, you should retrieve it from a
				// database or some kind of directory (LDAP, AD etc...)
				iq.SwitchDirection();
				iq.Type = IqType.Result;
				for (int i = 1; i < 11;i++)
				{
                    // don't add yourself to the contact list (aka roster)
				    if (Jid.User.EndsWith(i.ToString()))
				        continue;

				    var ri = new RosterItem
				    {
				        Jid = new Jid("user" + i + "@" + XmppDomain),
				        Name = "User " + i,
				        Subscription = Subscription.Both
				    };
				    ri.AddGroup("Group 1");
					iq.Query.Add(ri);
				}
				Send(iq);
			}
            else if (iq.Type == IqType.Set)
            {
                // TODO, handle roster add, remove and update here.
            }
		}

        private void ProcessSaslPlainAuth(MxAuth auth)
        {
            string pass = null;
            string user = null;

            byte[] bytes = Convert.FromBase64String(auth.Value);
            string sasl = Encoding.UTF8.GetString(bytes);
            // trim nullchars
            sasl = sasl.Trim((char) 0);
            string[] split = sasl.Split((char) 0);

            if (split.Length == 3)
            {
                user = split[1];
                pass = split[2];
            }
            else if (split.Length == 2)
            {
                user = split[0];
                pass = split[1];
            }
         
            // here you should get the password from youdatabase or auth provider
            const string dbPass = "secret";
            
            // check if username and password is correct
            if (user != null
                && Regex.IsMatch(user, "user([0-9]|10)$")
                && pass == dbPass)
            {
                // pass correct
                User = user;
                streamParser.Reset();
                IsAuthenticated = true;
                Send(new Success());
            }
            else
            {
                {
                    // user does not exist or wrong password
                    Send(new Failure(FailureCondition.NotAuthorized));
                }
            }
        }

        private void ProcessBind(Iq iq)
        {
            var bind = iq.Query as Bind;

            string res = bind.Resource;
            if (!String.IsNullOrEmpty(res))
            {
                var jid = new Jid(User, XmppDomain, res);
                Jid = jid;
                var resIq = new BindIq
                {
                    Id = iq.Id,
                    Type = IqType.Result,
                    Bind = { Jid = jid }
                };

                Send(resIq);
                Resource = res;
                IsBinded = true;

                // connection is bindet now. Add it to our global list of connection.
                ServerConnections.Add(this);
            }
            else
            {
                // return error
            }

        }

        private void ProcessSession(Iq iq)
        {
            /*            
                <iq type="set" id="aabca" >
                    <session xmlns="urn:ietf:params:xml:ns:xmpp-session"/>
                </iq>            
             */
            if (iq.Type == IqType.Set)
                Send(new SessionIq { Id = iq.Id, Type = IqType.Result });
        }

	    private void RouteIq(Iq iq)
	    {
            // route the iq here
            var to = iq.To;

            // check if the destination of this message is available
            var con = ServerConnections.FirstOrDefault(c => c.Jid.Equals(to, new BareJidComparer()));
            if (con != null)
            {
                // found connection, stamp packet with from and route it.
                iq.From = Jid;
                con.Send(iq);
            }
            else
            {
                // connection not found. Return the message to the sender and stamp it with error
                iq.Type = IqType.Error;
                iq.To = Jid;
                iq.From = to;
                iq.Add(new Matrix.Xmpp.Client.Error(Matrix.Xmpp.Base.ErrorCondition.ServiceUnavailable));
                Send(iq);
            }
	    }
    
        /// <summary>
        /// sends the XMPP stream header
        /// </summary>
		private void SendStreamHeader()
		{
            var stream = new Stream
            {
                Version = "1.0",
                From = XmppDomain,
                Id = Guid.NewGuid().ToString()
            };

            Send(stream.StartTag());
		}

        private XmppXElement BuildStreamFeatures()
        {
            var feat = new StreamFeatures();
            //feat.Add(new StartTls());
            
            if (!IsAuthenticated)
            {
                var mechs = new Mechanisms();
                mechs.AddMechanism(SaslMechanism.Plain);
                feat.Mechanisms = mechs;
            }
            else if (!IsBinded && IsAuthenticated)
            {
                feat.Add(new Bind());
            }

            return feat;
        }

	    private void Send(XmppXElement el)
		{
			Send(el.ToString(false));
		}
	}
}