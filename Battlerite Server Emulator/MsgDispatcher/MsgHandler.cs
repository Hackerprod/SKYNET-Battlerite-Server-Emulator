using BloodGUI_Binding.Web;
using Ionic.Zlib;
using SKYNET;
using StunGUI;
using System.IO;
using System.Net;
using System.Text;
using System.Web.Script.Serialization;

public abstract class MsgHandler
{
    public BattleriteServer Server => BattleriteServer.Instance;
    public ILog ilog = BattleriteServer.ilog;
    public abstract void AddHandlers(MsgDispatcher dispatcher);

    public void SendResponse(WebSocketSharp.Net.HttpListenerResponse response, BaseResponse data)
    {
        string json = new JavaScriptSerializer().Serialize(data);
        SendResponse(response, json);
    }

    public void SendResponse(WebSocketSharp.Net.HttpListenerResponse response, string json)
    {
        byte[] bytes = Encoding.Default.GetBytes(json);

        SendResponse(response, bytes);
    }
    public void SendResponse(WebSocketSharp.Net.HttpListenerResponse response, byte[] bytes)
    {
        response.AppendHeader("Access-Control-Allow-Origin", "*");
        response.AppendHeader("Access-Control-Allow-Headers", "Content-Type, Accept, X-Requested-With");
        response.AppendHeader("Access-Control-Allow-Methods", "GET, POST");
        response.AppendHeader("Access-Control-Max-Age", "1728000");
        //response.ContentType = "application/json";

        response.OutputStream.Write(bytes, 0, bytes.Length);
        response.ContentLength64 = bytes.Length;
        response.StatusCode = (int)HttpStatusCode.OK;
        response.OutputStream.Close();
    }

    public User GetUser(IPAddress Address)
    {
        return BattleriteServer.DbManager.Users.GetByRemoteAddress(Address);
    }
    public User GetUser(ulong AccountId)
    {
        return BattleriteServer.DbManager.Users.GetByAccountId((uint)AccountId);
    }
}