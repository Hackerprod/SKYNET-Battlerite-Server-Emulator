using BloodGUI_Binding.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace SKYNET.Dispatchers
{
    public class MatchSession : MsgHandler
    {
        public override void AddHandlers(MsgDispatcher dispatcher)
        {
            dispatcher["matches/sessions/player/v1"] = OnMatchesSessionsPlayer;
            dispatcher["matches/regions/sessions/public/v1"] = OnMatchesRegionsSessions;
            
        }



        private void OnMatchesSessionsPlayer(RequestMessage request)
        {
            BaseResponse data = null;
            if (request.RequestType == RequestType.GET)
            {
                data = new GetActiveSessionResponse()
                {
                    connectURL = "Lidgren://10.31.0.2:29000",
                    matchId = "fd5a372a55c04fb5a7ecd114f1d97696",
                    matchType = StunShared.MatchType.VSAI
                };
            }
            else
            {
                if (request.ContainsBody)
                {
                    LeaveActiveSessionRequest LeaveSession = request.Body.Deserialize<LeaveActiveSessionRequest>();
                    data = new EmptyResponse()
                    {
                    };
                }
            }
            data = new EmptyResponse()
            {
            };
            //string json = "{\"connectURL\":\"SDR://EWrkwkCzcaUyGpIBDZf7GmAdAAAAADiy5B5Qh9MBWkhsaW0AAlpDMvDwP1WFMo3jQvNIIyAoH2T/9n7AJNDmGWCkaw5XD/PwNb5tLe87wb4/9VZQ6YFy3Rx3CwahRin1mMxCszC0fHpyGXN0ZWFtaWQ6NzY1NjExOTg4MTc2OTIxNzN6GXN0ZWFtaWQ6OTAxNDI0NjE3MjgxNjA3NzYiQNqoZplDXDt0Xh829onocfyUzoUk9m9UozsPllFDdTu5x0//jr5WiBa6nLX8ANDu2LkaNdIJLfsCX4tiEwOfuQQ=\",\"matchId\":\"fd5a372a55c04fb5a7ecd114f1d97696\",\"matchType\":\"VSAI\"}";
            //SendResponse(request.ListenerResponse, json);
            SendResponse(request.ListenerResponse, data);
        }
        private void OnMatchesRegionsSessions(RequestMessage request)
        {
            User user = GetUser(request.RemoteAddress);
            ilog.Warn("Ver para los Matchs");
            if (request.ContainsBody)
            {
                PublicCreateSessionRequest Request = request.Body.Deserialize<PublicCreateSessionRequest>();
                CreateMatchSessionResponse data = new CreateMatchSessionResponse()
                {
                    region = "SKYNET",
                    matchId = "564654654",
                    connectURLs = new List<PlayerConnectURL>()
                    {
                        new PlayerConnectURL()
                        {
                            connectURL = "Lidgren://10.31.0.2:29000",
                            //connectURL = "SteamIPv4://10.31.0.2:30000",
                            userId = 1000
                        }
                    }
                };
                SendResponse(request.ListenerResponse, data);
                string sdrconn = "EWrkwkCzcaUyGpIBDZf7GmAdAAAAADiy5B5Qh9MBWkhsaW0AAlpDMvDwP1WFMo3jQvNIIyAoH2T/9n7AJNDmGWCkaw5XD/PwNb5tLe87wb4/9VZQ6YFy3Rx3CwahRin1mMxCszC0fHpyGXN0ZWFtaWQ6NzY1NjExOTg4MTc2OTIxNzN6GXN0ZWFtaWQ6OTAxNDI0NjE3MjgxNjA3NzYiQNqoZplDXDt0Xh829onocfyUzoUk9m9UozsPllFDdTu5x0//jr5WiBa6nLX8ANDu2LkaNdIJLfsCX4tiEwOfuQQ=";

            }
            
        }
    }
}
