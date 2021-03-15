using BloodGUI_Binding.Web;
using StunShared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace SKYNET.Dispatchers
{
    public class RankingSeasons : MsgHandler
    {
        public override void AddHandlers(MsgDispatcher dispatcher)
        {
            dispatcher["ranking/seasons/rewards"] = OnRankingSeasonsRewards;
            dispatcher["ranking/seasons/current"] = OnRankingSeasonsCurrent;
            //dispatcher["ranking/seasons/finalizerewards"] = OnRankingSeasonsFinalizereWards;
        }

        private void OnRankingSeasonsFinalizereWards(RequestMessage request)
        {
            //string json = new JavaScriptSerializer().Serialize("");
            //SendResponse(request.ListenerResponse, data);
        }

        private void OnRankingSeasonsCurrent(RequestMessage request)
        {
            GetSeasonInfoResponse data = new GetSeasonInfoResponse()
            {
                seasonID = 890415,
                seasonNameKey = "SKYNET Season",
                seasonInfoUrl = "http://localhost:25000",
                secondsSinceStart = 16000,
                secondsToEnd = 50000
            };
            string json = "{\"seasonID\":15,\"secondsSinceStart\":-40586301,\"secondsToEnd\":-911628098}";
            SendResponse(request.ListenerResponse, json);
            //SendResponse(request.ListenerResponse, data);
        }

        private void OnRankingSeasonsRewards(RequestMessage request)
        {
            GetSeasonRewardsResponse data = new GetSeasonRewardsResponse()
            {
                seasonID = 890415,
                teamID = 2000,
                itemsReceived = new List<PlayerStackableData>(),
                newInventoryVersion = 415645,
                guid = Guid.NewGuid().ToString(),
                gotRewards = false
            };
            SendResponse(request.ListenerResponse, data);
        }
    }
}
