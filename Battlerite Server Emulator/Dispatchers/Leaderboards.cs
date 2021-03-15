using BloodGUI_Binding.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace SKYNET.Dispatchers
{
    public class Leaderboards : MsgHandler
    {
        public override void AddHandlers(MsgDispatcher dispatcher)
        {
            dispatcher["leaderboards/get/v1"] = OnLeaderboardsGet;
        }

        private void OnLeaderboardsGet(RequestMessage request)
        {
            if (request.ContainsBody)
            {
                GetLeaderboardRequest Request = request.Body.Deserialize<GetLeaderboardRequest>();
            }
            
            GetLeaderboardResponse data = new GetLeaderboardResponse()
            {
                seasonId = 890415,
                entryData = new List<LeaderboardEntryData>()
                {
                     new LeaderboardEntryData()
                     {
                          teamId = 2000,
                     }
                }
            };
            SendResponse(request.ListenerResponse, data);
        }

    }
}
