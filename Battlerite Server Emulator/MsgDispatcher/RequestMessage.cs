using CodeProject.ObjectPool;
using SKYNET;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using WebSocketSharp.Net;

[Serializable]
public class RequestMessage :  PooledObject
{
    public string RequestHandler { get; set; }
    public System.Net.IPAddress RemoteAddress { get; set; }
    public RequestType RequestType { get; set; }
    public string Query { get; set; }
    public HttpListenerResponse ListenerResponse { get; set; }
    public byte[] Body { get; internal set; } 
    public bool ContainsBody { get => Body.Length > 0; }

    public RequestMessage()
    {
        base.OnReleaseResources = new Action(this.OnReleaseAndReset);
        base.OnResetState = new Action(this.OnReleaseAndReset);
    }

    private void OnReleaseAndReset()
    {
        this.RequestHandler = null;
        this.RemoteAddress = null;
        this.ListenerResponse = null;
    }
    private byte[] GetBody(HttpListenerRequest request)
    {
        Stream inputStream = request.InputStream;
        MemoryStream bodyStream = new MemoryStream();
        using (Stream stream = inputStream)
        {
            stream.CopyTo(bodyStream);
        }
        return bodyStream.ToArray();
    }
}