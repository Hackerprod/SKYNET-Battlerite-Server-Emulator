using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using SKYNET;

public class MsgDispatcher
{
    public delegate void HandlerDelegate(RequestMessage request);

    private readonly Dictionary<string, HandlerDelegate> HandlerList;

    private readonly ILog ilog;

    public MsgDispatcher()
    {
        this.HandlerList = new Dictionary<string, HandlerDelegate>();
    }

    public HandlerDelegate this[string e]
    {
        get
        {
            return this.HandlerList[e];
        }
        set
        {
            this.HandlerList[e] = value;
        }
    }


    public void Dispatch(RequestMessage request)
    {
        if (request == null)
        {
            return;
        }
        if (this.HandlerList.TryGetValue(request.RequestHandler, out HandlerDelegate handlerDelegate))
        {
            try
            {
                if (request.RequestHandler != "Chat")
                {
                    BattleriteServer.ilog.Info($"Received request for handler {request.RequestHandler}, {request.Body.Length} bytes");
                }

                handlerDelegate(request);
                request.Dispose();
            }
            catch (Exception ex)
            {
                BattleriteServer.ilog.Error(ex.Message + " " + ex.StackTrace);
            }
            return;
        }
        BattleriteServer.ilog.Error($"Dont exist handler for {request.RequestHandler} request.");
        if (request.ContainsBody)
        {
            if (!string.IsNullOrEmpty(request.Query))
            {
                BattleriteServer.ilog.Error($"Query {request.Query}");
            }
            BattleriteServer.ilog.Error($"Body {Encoding.UTF8.GetString(request.Body)}");
        }
        //ResponseError(request.ListenerResponse);
        ResponseError(request.ListenerResponse);
    }


    private void ResponseError(HttpListenerResponse response)
    {
        response.StatusCode = (int)HttpStatusCode.NotFound;
        response.Close();
    }
    private void ResponseError(WebSocketSharp.Net.HttpListenerResponse response)
    {
        response.StatusCode = (int)HttpStatusCode.NotFound;
        response.Close();
    }

    public void AddHandler(MsgHandler handler)
    {
        if (handler == null)
        {
            return;
        }
        handler.AddHandlers(this);
    }

}