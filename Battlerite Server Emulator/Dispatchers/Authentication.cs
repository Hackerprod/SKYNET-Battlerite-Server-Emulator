using BloodGUI_Binding.Web;
using SKYNET.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using WebSocketSharp.Net;

namespace SKYNET
{
    public class Authentication : MsgHandler
    {
        public override void AddHandlers(MsgDispatcher dispatcher)
        {
            dispatcher["auth/steam-users/users/v1"] = OnAuthSteamUsers;
            dispatcher["auth/steam-async/v1"] = OnAuthSteamAsync;
            dispatcher["auth/refresh/v1"] = OnAuthRefreshV1;
            dispatcher["auth/client"] = OnClient;

        }

        private void OnClient(RequestMessage request)
        {
            if (!string.IsNullOrEmpty(request.Query))
            {
                Auth loginRequest = request.Query.DeserializeQuery<Auth>();
                LoginResponse loginResponse = new LoginResponse();

                User user = BattleriteServer.DbManager.Users.GetByAccountName(loginRequest.AccountName);
                if (user == null)
                {
                    loginResponse.Result = false;
                    loginResponse.ErrorMessage = $"Account {loginRequest.AccountName} not found";
                    BattleriteServer.ilog.Warn($"Account {loginRequest.AccountName} not found");
                    SendLoginResponse(request.ListenerResponse, loginResponse);
                    return;
                }

                if (user.Password != loginRequest.Password)
                {
                    loginResponse.Result = false;
                    loginResponse.ErrorMessage = $"Invalid authentication from account {loginRequest.AccountName}";
                    BattleriteServer.ilog.Warn($"Invalid authentication from account {loginRequest.AccountName}");
                    SendLoginResponse(request.ListenerResponse, loginResponse);
                    return;
                }

                BattleriteServer.DbManager.Users.SetRemoteAddress(user.AccountId, request.RemoteAddress);

                loginResponse.Result = true;
                loginResponse.ErrorMessage = $"";
                BattleriteServer.ilog.Info($"User {loginRequest.AccountName} successfully authenticated");
                SendLoginResponse(request.ListenerResponse, loginResponse);

            }
        }

        private void SendLoginResponse(HttpListenerResponse listenerResponse, LoginResponse loginResponse)
        {
            string json = new JavaScriptSerializer().Serialize(loginResponse);
            SendResponse(listenerResponse, json);
        }


        private void OnAuthSteamUsers(RequestMessage request)
        {
            if (request.ContainsBody)
            {
                GetUsersFromSteamIdsResponse data = new GetUsersFromSteamIdsResponse()
                {
                    users = new List<SteamUserData>()
                };

                GetSteamUsersFromUserIdsRequest Request = request.Body.Deserialize<GetSteamUsersFromUserIdsRequest>(); 
                foreach (var item in Request.users)
                {
                    User user = GetUser(item);
                    if (user != null)
                    {
                        SteamUserData userData = new SteamUserData()
                        {
                            steamId = user.SteamId,
                            userId = user.AccountId
                        };
                    }
                }
                SendResponse(request.ListenerResponse, data);

            }
        }
        private void OnAuthSteamAsync(RequestMessage request)
        {
            if (request.ContainsBody)
            {
                SteamAuthRequest Request = request.Body.Deserialize<SteamAuthRequest>();
            }
            User user = GetUser(request.RemoteAddress);
            BattleriteServer.DbManager.Users.SetRemoteAddress(user.AccountId, request.RemoteAddress);

            if (user != null)
            {
                SteamAuthResponse data = new SteamAuthResponse()
                {
                    sessionID = string.IsNullOrEmpty(user.sessionID) ? "xxx" : user.sessionID, //Generar cuando entre sesion
                    refreshToken = Guid.NewGuid().ToString(),
                    timeUntilExpire = 900,
                    userID = user.AccountId,
                };
                SendResponse(request.ListenerResponse, data);
            }
            string json = "{\"sessionID\":\"43DB700FADEDA06306EF995B2EE79F0F\",\"refreshToken\":\"6BEF5084C7CC16634BD862CBAB1E4FCB\",\"timeUntilExpire\":900,\"userId\":1000}";
            //SendResponse(request.ListenerResponse, json);
        }
        private void OnAuthRefreshV1(RequestMessage request)
        {
            if (request.ContainsBody)
            {
                RefreshSessionRequest Request = request.Body.Deserialize<RefreshSessionRequest>();

                RefreshSessionResponse data = new RefreshSessionResponse()
                {
                    refreshToken = Guid.NewGuid().ToString(),
                    timeUntilExpire = 900, //segundos
                    userID = Request.userId
                };
                SendResponse(request.ListenerResponse, data);
            }
        }

    }
}
