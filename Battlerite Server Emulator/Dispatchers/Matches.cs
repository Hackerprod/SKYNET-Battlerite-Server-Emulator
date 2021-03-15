using BloodGUI_Binding.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace SKYNET.Dispatchers
{
    public class Matches : MsgHandler
    {
        public override void AddHandlers(MsgDispatcher dispatcher)
        {
            dispatcher["matches/regions/v1"] = OnMatchesRegions;
            dispatcher["auth/current-game-version/v1"] = OnCurrentGameVersion;

        }



        private void OnCurrentGameVersion(RequestMessage request)
        {
            GetCurrentGameVersionResponse data = new GetCurrentGameVersionResponse()
            {
                currentBloodgateVersion = 410,
                currentGameplayVersion = 409,
                //currentNexonVersion = 410,
                //skipVersionCheck = true
            };
            SendResponse(request.ListenerResponse, data);
        }

        private void OnMatchesRegions(RequestMessage request)
        {
            GetServerRegionsResponse data = new GetServerRegionsResponse()
            {
                regions = new List<MatchRegion>()
                {
                    new MatchRegion()
                    {
                        name = "SKYNET",
                        pingHost = "10.31.0.2",
                        sdrPOPID = 0
                    }
                }
            };
            string json = "{\"regions\":[{\"name\":\"south_america\",\"pingHost\":\"\",\"sdrPOPID\":6779509},{\"name\":\"us_west\",\"pingHost\":\"\",\"sdrPOPID\":6644084},{\"name\":\"poland\",\"pingHost\":\"\",\"sdrPOPID\":7823735},{\"name\":\"eu_east\",\"pingHost\":\"\",\"sdrPOPID\":7760229},{\"name\":\"asia\",\"pingHost\":\"\",\"sdrPOPID\":7563120},{\"name\":\"australia\",\"pingHost\":\"\",\"sdrPOPID\":7567716},{\"name\":\"eu_west\",\"pingHost\":\"\",\"sdrPOPID\":7107960},{\"name\":\"japan\",\"pingHost\":\"\",\"sdrPOPID\":7633263},{\"name\":\"us_east\",\"pingHost\":\"\",\"sdrPOPID\":6906212},{\"name\":\"peru\",\"pingHost\":\"\",\"sdrPOPID\":7104877},{\"name\":\"south_africa\",\"pingHost\":\"\",\"sdrPOPID\":6975074},{\"name\":\"spain\",\"pingHost\":\"\",\"sdrPOPID\":7168356},{\"name\":\"chile\",\"pingHost\":\"\",\"sdrPOPID\":7562092},{\"name\":\"eu_north\",\"pingHost\":\"\",\"sdrPOPID\":7566447},{\"name\":\"india_east\",\"pingHost\":\"\",\"sdrPOPID\":7168353},{\"name\":\"india\",\"pingHost\":\"\",\"sdrPOPID\":6451053},{\"name\":\"hong_kong\",\"pingHost\":\"\",\"sdrPOPID\":6843239}]}";
            //SendResponse(request.ListenerResponse, json);
            SendResponse(request.ListenerResponse, data);
        }
    }
}
