using BloodGUI_Binding.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace SKYNET.Dispatchers
{
    class MatchLobby : MsgHandler
    {
        public override void AddHandlers(MsgDispatcher dispatcher)
        {
            dispatcher["match-lobby/public/get-all/v1"]    = OnMatchLobbyGetAll;
            dispatcher["match-lobby/public/get/v1"]        = OnMatchLobbyGet;
        }

        private void OnMatchLobbyGet(RequestMessage request)
        {
            GetPublicLobbyResponse data = new GetPublicLobbyResponse();
            if (request.ContainsBody)
            {
                GetPublicLobbyRequest Request = request.Body.Deserialize<GetPublicLobbyRequest>();
                MatchLobbyData lobby = BattleriteServer.Lobbies.Get(Request.lobbyId);
                data.lobby = lobby;
            }
            else
            {
                GetPublicLobbyRequest Request = request.Query.DeserializeQuery<GetPublicLobbyRequest>();
                MatchLobbyData lobby = BattleriteServer.Lobbies.Get(Request.lobbyId);
                data.lobby = lobby;
            }

            SendResponse(request.ListenerResponse, data);

        }

        private void OnMatchLobbyGetAll(RequestMessage request)
        {
            GetAllPublicLobbyResponse data = new GetAllPublicLobbyResponse()
            {
                lobbies = new List<MatchLobbyData>()
            };
            List<MatchLobbyData> lobbies = BattleriteServer.Lobbies.GetAll();
            data.lobbies.AddRange(lobbies);
            //string response = "{\"lobbies\":[{\"version\":102,\"id\":\"privatelobby_06a7c73db09e44228d7fb9c988c7fb56_1\",\"settings\":{\"name\":\"asas\",\"gameType\":1733162751,\"teamSize\":3,\"teamCount\":2,\"maxObservers\":6,\"map\":1608885260,\"region\":\"japan\",\"roundTime\":90,\"soulOrbEnabled\":true,\"suddenDeathEnabled\":true,\"cooldownModifier\":1.0,\"winScore\":3},\"players\":[{\"userId\":1355209287214964736,\"team\":2,\"slot\":2},{\"userId\":1355448632157028352,\"team\":2},{\"userId\":1155054895183863808,\"team\":1,\"slot\":1},{\"userId\":1356656016275681280,\"team\":2,\"slot\":1},{\"userId\":989382770063192064,\"team\":1,\"slot\":2},{\"userId\":1355224846660939776,\"team\":1}],\"observers\":[{\"userId\":1029560312132714496}],\"isInMatch\":true,\"session\":{}},{\"version\":45,\"id\":\"privatelobby_89ca249553c448ddac6c5cc40c3a05b8_1\",\"settings\":{\"name\":\"Shit on\",\"gameType\":1733162751,\"teamSize\":2,\"teamCount\":2,\"maxObservers\":6,\"map\":1608885260,\"region\":\"us_east\",\"roundTime\":90,\"soulOrbEnabled\":true,\"suddenDeathEnabled\":true,\"cooldownModifier\":1.0,\"winScore\":3},\"players\":[{\"userId\":958543969741062144,\"team\":1},{\"userId\":1356378690795220992,\"team\":2},{\"userId\":1356392285604032512,\"team\":1,\"slot\":1},{\"userId\":1356392157182828544,\"team\":2,\"slot\":1}],\"observers\":[],\"isInMatch\":true,\"session\":{\"teamOneScore\":1,\"teamTwoScore\":1}},{\"version\":205,\"id\":\"privatelobby_d8e2279af1f64a88bb9782276258a90e_1\",\"settings\":{\"name\":\"rrrr\",\"gameType\":1733162751,\"teamSize\":2,\"teamCount\":2,\"maxObservers\":6,\"map\":1646471093,\"region\":\"eu_east\",\"roundTime\":120,\"soulOrbEnabled\":true,\"suddenDeathEnabled\":true,\"cooldownModifier\":1.0,\"winScore\":3},\"players\":[{\"userId\":1302552380264554496,\"team\":1},{\"userId\":1357004388652195840,\"team\":2},{\"userId\":1357004136608079872,\"team\":1,\"slot\":1},{\"userId\":1357004531866681344,\"team\":2,\"slot\":1}],\"observers\":[],\"isInMatch\":true,\"session\":{\"teamTwoScore\":1}}]}";
            //SendResponse(request.ListenerResponse, response);
            SendResponse(request.ListenerResponse, data);
        }
    }
}
