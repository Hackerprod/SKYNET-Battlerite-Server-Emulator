using SKYNET;
using System.IO;
using System.Net;
using System.Net.Sockets;
using WebSocketSharp;
using WebSocketSharp.Net.WebSockets;
using WebSocketSharp.Server;
using System;
using System.Collections.Generic;
using System.Text;
using StunShared;
using Volgo.Messages;
using Newtonsoft.Json;
using Volgo.Messages.Nexon;
using ProtoBuf;

public class WebSocketProcessor : WebSocketBehavior, IConnection
{
    public WebSocketProcessor()
    { }
    private Dictionary<string, Type> _PayloadTypes = new Dictionary<string, Type>()
    {
        {"inventory_update", typeof(InventoryUpdateMessage)},
        {"login", typeof(LoginMessage)},
        {"omni_auth", typeof(OmniAuthMessage)},
        {"server_announcement", typeof(ServerAnnouncement)},
        {"guss_terminate", typeof(GussMessage)},
    };
    public bool Connected
    {
        get
        {
            WebSocketContext context = this.Context;
            bool? obj;
            if (context == null)
            {
                obj = null;
            }
            else
            {
                WebSocket webSocket = context.WebSocket;
                obj = ((webSocket != null) ? new bool?(webSocket.IsAlive) : null);
            }
            return obj ?? false;
        }
    }
    public IPEndPoint RemoteEndPoint => this.Context.UserEndPoint;

    protected override void OnOpen()
    {
        BattleriteServer.Connections.CountConnections += 1;
        BattleriteServer.ilog.Info($"New client connection from {this.Context.UserEndPoint}");
    }
    protected override void OnError(WebSocketSharp.ErrorEventArgs e)
    {
        BattleriteServer.ilog.Error("Error: " + e.Message + "\r\n" + (e.Exception.StackTrace ?? string.Empty));
    }
    protected override void OnClose(CloseEventArgs e)
    {
        try
        {
            BattleriteServer.Connections.CountConnections -= 1;
            BattleriteServer.Connections.Remove(this);
        }
        catch (Exception ex)
        {
            BattleriteServer.ilog.Error(ex);
        }

    }
    protected override void OnMessage(MessageEventArgs args)
    {
        try
        {
            SubUnsubMessage msg = Serializer.Deserialize<SubUnsubMessage>(new MemoryStream(args.RawData));
            if (args.IsBinary && ProtoBufHelper.TryDeserialize(args.RawData, out SubUnsubMessage record))
            {
                string Topic = record.Topic;
                string text = record.type.ToString();
                ulong userId = record.userId;
                string MsgType = record.Topic.Replace(userId + "_", "");

                BattleriteServer.ilog.Warn($"PublishMessage, topic: {MsgType}, payloadType: {text}");

                BattleriteServer.Connections.AddOrUpdate(userId, this);

                ProcessMessage(MsgType, Topic, userId);

            }

        }
        catch (Exception ex)
        {
            BattleriteServer.ilog.Error(ex);
        }
    }

    private void ProcessMessage(string MsgType, string Topic, ulong userId)
    {
        string json = "";
        User user = BattleriteServer.DbManager.Users.GetByAccountId(userId);
        if (user == null) return;
        switch (MsgType)
        {
            case "login":
                LoginMessage LoginMessage = new LoginMessage()
                {
                    guss = new GussMessage()
                    {
                        messageType = GussMessageType.Login,
                        customErrorCode = 1
                    },
                    omni = new OmniLoginResult()
                    {
                        failed = false
                    }
                };
                //json = JsonConvert.SerializeObject(LoginMessage);

                break;
            case "inventory_update":
                InventoryUpdateMessage InventoryUpdate = new InventoryUpdateMessage()
                {
                    updateType = InventoryUpdateType.PARTNER_REWARD,
                    stackableUpdates = new List<PlayerStackableData>()
                    {
                        new PlayerStackableData()
                        {
                            amount = 130,
                            type = 400001
                        },
                        new PlayerStackableData()
                        {
                            amount = 230,
                            type = 40002
                        }
                    },
                };
                json = JsonConvert.SerializeObject(InventoryUpdate);
                break;
            case "omni_auth":
                OmniAuthMessage OmniAuth = new OmniAuthMessage()
                {
                    gameType = GameType.Arena,
                    sessionId = user.sessionID
                };
                json = JsonConvert.SerializeObject(OmniAuth); 
                break;
            case "server_announcement":
                ServerAnnouncement ServerAnnouncement = new ServerAnnouncement()
                {
                    level = ServerAnnouncementLevel.INFO,
                    message = "Welcome to [SKYNET] Battlerite server. By Hackerprod",
                    type = ServerAnnouncementType.ALL
                };
                json = JsonConvert.SerializeObject(ServerAnnouncement);
                break;
            case "guss_terminate":
                GussMessage GussMessage = new GussMessage()
                {
                     
                };
                json = JsonConvert.SerializeObject(GussMessage); 
                break;
            default:
                BattleriteServer.ilog.Warn($"Not found Handler for {MsgType} call");
                break;
        }

        if (!string.IsNullOrEmpty(json))
        {
            PublishMessage msg = new PublishMessage();
            msg.Payload = Encoding.UTF8.GetBytes(json);
            msg.payloadType = MsgType;
            msg.Topic = Topic;

            if (ProtoBufHelper.TrySerialize(msg, out byte[] data))
            {
                BattleriteServer.Connections.Send(userId, data);
            }

        }


    }

    public void Disconnect()
    {
        base.Close();
    }


    void IConnection.Send(byte[] msg)
    {
        base.Send(msg);
    }

    public void Send(byte[] msg, ulong steamId)
    {
        base.Send(msg);
    }
}
