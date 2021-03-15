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
    public class Matchmaking : MsgHandler
    {
        public override void AddHandlers(MsgDispatcher dispatcher)
        {
            dispatcher["matchmaking/leave/v1"] = OnMatchmakingLeave;
            dispatcher["matchmaking/queue/v1"] = OnMatchmakingQueue;
        }

        private void OnMatchmakingQueue(RequestMessage request)
        {
            MatchmakingQueueRequest Request = request.Body.Deserialize<MatchmakingQueueRequest>();
            MatchmakingQueueResponse data = new MatchmakingQueueResponse()
            {
                matchId = "sdfsdfsdf",
                connectURLs = new List<PlayerConnectURL>()
                {
                    new PlayerConnectURL()
                    {
                        connectURL = "Lidgren://10.31.0.2:29000",
                        userId = 1000
                    }
                },
                timeInQueue = 20,
                regionAvailable = true,
                loadIssues = false,
            };
            SendResponse(request.ListenerResponse, data);
        }

        private void OnMatchmakingLeave(RequestMessage request)
        {
            LeaveMatchmakingQueueResponse data = new LeaveMatchmakingQueueResponse()
            {

            };
            string json = "{\"timeInQueue\":0,\"regionAvailable\":true}{\"timeInQueue\":8,\"regionAvailable\":true}";
            SendResponse(request.ListenerResponse, json);
            //SendResponse(request.ListenerResponse, data);        
        }
        }
    }
