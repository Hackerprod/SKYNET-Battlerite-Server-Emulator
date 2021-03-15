using BloodGUI_Binding.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace SKYNET.Dispatchers
{
    public class Twitch : MsgHandler
    {
        public override void AddHandlers(MsgDispatcher dispatcher)
        {
            dispatcher["twitch/all-streams"] = OnTwitchAllStreams;
            dispatcher["twitch/users"] = OnTwitchUsers;
        }

        private void OnTwitchUsers(RequestMessage request)
        {
            GetTwitchUserDataResponse data = new GetTwitchUserDataResponse()
            {
                message = "Ejasi",
                users = new List<TwitchUserData>()
                { 
                    new TwitchUserData()
                    {
                         name = "Hack",
                         twitchId = "twitchId",
                         userId = 1000
                    },
                    new TwitchUserData()
                    {
                         name = "Yoe",
                         twitchId = "twitchId",
                         userId = 1001
                    }
                }
            };
            string json = "{\"users\":[]}";
            SendResponse(request.ListenerResponse, json);
            //SendResponse(request.ListenerResponse, data);
        }

        private void OnTwitchAllStreams(RequestMessage request)
        {
            GetTwitchStreamsResponse data = new GetTwitchStreamsResponse()
            {
                streams = new List<TwitchStream>()
                {
                    new TwitchStream()
                    {
                        streamId = "76561197960266728",
                        twitchId = "76561197960266728",
                        url = "http://localhost:25000/url_puesto_por_mi",
                        status = "playing",
                        name = "Twitch",
                        description = "lol lol lol",
                        viewers = 57
                    }
                }
            };
            string json = "{\"streams\":[]}";
            SendResponse(request.ListenerResponse, json);
            //SendResponse(request.ListenerResponse, data);
        }
    }
}
