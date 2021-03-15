using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Xml;

namespace Lidgren.Network
{
	public class NetUPnP
	{
		private const int c_discoveryTimeOutMillis = 1000;

		private string m_serviceUrl;

		private NetPeer m_peer;

		private ManualResetEvent m_discoveryComplete = new ManualResetEvent(initialState: false);

		public NetUPnP(NetPeer peer)
		{
			m_peer = peer;
		}

		internal void Discover(NetPeer peer)
		{
			string s = "M-SEARCH * HTTP/1.1\r\nHOST: 239.255.255.250:1900\r\nST:upnp:rootdevice\r\nMAN:\"ssdp:discover\"\r\nMX:3\r\n\r\n";
			byte[] bytes = Encoding.UTF8.GetBytes(s);
			peer.Socket.Broadcast = true;
			peer.RawSend(bytes, 0, bytes.Length, new IPEndPoint(IPAddress.Broadcast, 1900));
			peer.Socket.Broadcast = false;
		}

		internal void ExtractServiceUrl(string resp)
		{
			try
			{
				XmlDocument xmlDocument = new XmlDocument();
				xmlDocument.Load(WebRequest.Create(resp).GetResponse().GetResponseStream());
				XmlNamespaceManager xmlNamespaceManager = new XmlNamespaceManager(xmlDocument.NameTable);
				xmlNamespaceManager.AddNamespace("tns", "urn:schemas-upnp-org:device-1-0");
				XmlNode xmlNode = xmlDocument.SelectSingleNode("//tns:device/tns:deviceType/text()", xmlNamespaceManager);
				if (xmlNode.Value.Contains("InternetGatewayDevice"))
				{
					XmlNode xmlNode2 = xmlDocument.SelectSingleNode("//tns:service[tns:serviceType=\"urn:schemas-upnp-org:service:WANIPConnection:1\"]/tns:controlURL/text()", xmlNamespaceManager);
					if (xmlNode2 != null)
					{
						m_serviceUrl = CombineUrls(resp, xmlNode2.Value);
						m_discoveryComplete.Set();
					}
				}
			}
			catch
			{
			}
		}

		private static string CombineUrls(string gatewayURL, string subURL)
		{
			if (subURL.Contains("http:") || subURL.Contains("."))
			{
				return subURL;
			}
			gatewayURL = gatewayURL.Replace("http://", "");
			int num = gatewayURL.IndexOf("/");
			if (num != -1)
			{
				gatewayURL = gatewayURL.Substring(0, num);
			}
			return "http://" + gatewayURL + subURL;
		}

		public bool ForwardPort(int port, string description)
		{
			if (m_serviceUrl == null && !m_discoveryComplete.WaitOne(1000))
			{
				return false;
			}
			IPAddress mask;
			IPAddress myAddress = NetUtility.GetMyAddress(out mask);
			if (myAddress == null)
			{
				return false;
			}
			try
			{
				SOAPRequest(m_serviceUrl, "<u:AddPortMapping xmlns:u=\"urn:schemas-upnp-org:service:WANIPConnection:1\"><NewRemoteHost></NewRemoteHost><NewExternalPort>" + port.ToString() + "</NewExternalPort><NewProtocol>" + ProtocolType.Udp.ToString().ToUpper() + "</NewProtocol><NewInternalPort>" + port.ToString() + "</NewInternalPort><NewInternalClient>" + myAddress.ToString() + "</NewInternalClient><NewEnabled>1</NewEnabled><NewPortMappingDescription>" + description + "</NewPortMappingDescription><NewLeaseDuration>0</NewLeaseDuration></u:AddPortMapping>", "AddPortMapping");
				Thread.Sleep(50);
			}
			catch (Exception ex)
			{
				m_peer.LogWarning("UPnP port forward failed: " + ex.Message);
				return false;
			}
			return true;
		}

		public bool DeleteForwardingRule(int port)
		{
			if (m_serviceUrl != null || m_discoveryComplete.WaitOne(1000))
			{
				try
				{
					SOAPRequest(m_serviceUrl, "<u:DeletePortMapping xmlns:u=\"urn:schemas-upnp-org:service:WANIPConnection:1\"><NewRemoteHost></NewRemoteHost><NewExternalPort>" + port + "</NewExternalPort><NewProtocol>" + ProtocolType.Udp.ToString().ToUpper() + "</NewProtocol></u:DeletePortMapping>", "DeletePortMapping");
					return true;
				}
				catch (Exception ex)
				{
					m_peer.LogWarning("UPnP delete forwarding rule failed: " + ex.Message);
					return false;
				}
			}
			return false;
		}

		public IPAddress GetExternalIP()
		{
			if (m_serviceUrl != null || m_discoveryComplete.WaitOne(1000))
			{
				try
				{
					XmlDocument xmlDocument = SOAPRequest(m_serviceUrl, "<u:GetExternalIPAddress xmlns:u=\"urn:schemas-upnp-org:service:WANIPConnection:1\"></u:GetExternalIPAddress>", "GetExternalIPAddress");
					XmlNamespaceManager xmlNamespaceManager = new XmlNamespaceManager(xmlDocument.NameTable);
					xmlNamespaceManager.AddNamespace("tns", "urn:schemas-upnp-org:device-1-0");
					string value = xmlDocument.SelectSingleNode("//NewExternalIPAddress/text()", xmlNamespaceManager).Value;
					return IPAddress.Parse(value);
				}
				catch (Exception ex)
				{
					m_peer.LogWarning("Failed to get external IP: " + ex.Message);
					return null;
				}
			}
			return null;
		}

		private XmlDocument SOAPRequest(string url, string soap, string function)
		{
			string s = "<?xml version=\"1.0\"?><s:Envelope xmlns:s=\"http://schemas.xmlsoap.org/soap/envelope/\" s:encodingStyle=\"http://schemas.xmlsoap.org/soap/encoding/\"><s:Body>" + soap + "</s:Body></s:Envelope>";
			WebRequest webRequest = WebRequest.Create(url);
			webRequest.Method = "POST";
			byte[] bytes = Encoding.UTF8.GetBytes(s);
			webRequest.Headers.Add("SOAPACTION", "\"urn:schemas-upnp-org:service:WANIPConnection:1#" + function + "\"");
			webRequest.ContentType = "text/xml; charset=\"utf-8\"";
			webRequest.ContentLength = bytes.Length;
			webRequest.GetRequestStream().Write(bytes, 0, bytes.Length);
			XmlDocument xmlDocument = new XmlDocument();
			WebResponse response = webRequest.GetResponse();
			Stream responseStream = response.GetResponseStream();
			xmlDocument.Load(responseStream);
			return xmlDocument;
		}
	}
}
