using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using BloodGUI_Binding.Web;
using SKYNET.Db;
using StunShared;

namespace SKYNET.Dispatchers
{
    public class Account : MsgHandler
    {
        public override void AddHandlers(MsgDispatcher dispatcher)
        {
            dispatcher["account/v1"] = OnAccountV1;
            dispatcher["account/freerotation/v1"] = OnAccountFreerotationHandler;
            dispatcher["account/steam-info"] = OnAccountSteamInfoHandler;
            dispatcher["quest/user/v1"] = OnQuestUserHandler;
            dispatcher["account/legalterms/agree/v1"] = OnLegalTermsV1;
            dispatcher["account/profile/email-verification-state/v1"]  = OnEmailVerification;
            dispatcher["account/rewards/get/v1"] = OnAccountRewards;
            dispatcher["account/stats/ui/events"] = OnAccountStats;
            dispatcher["account/news/info/v1"] = OnAccountNews;
            dispatcher["account/legalterms/agree/v1"] = OnAccountLegalTerms;
            dispatcher["account/profile/public/v1"] = OnAccountProfilePublic;
            dispatcher["ranking/teams/v2"] = OnRankingTeams;
            dispatcher["account/profile/set-email-flags/v2"] = OnAccountSetEmailFlags;
            dispatcher["quest/getsponsor/v1"] = OnQuestGetSponsor;
            dispatcher["account/profile/id/v1"] = OnAccountProfileId;
            dispatcher["account/matchmaking-ticket/v1"] = OnAccountMatchmakingTicket;
            dispatcher["account/public/v1"] = OnAccountPublic;
            dispatcher["account/profile/title/v1"] = OnAccountProfileTitle;
            dispatcher["account/profile/picture/v1"] = OnAccountProfilePicture;
            dispatcher["account/lootbox/open"] = OnAccountLootboxOpen;
            dispatcher["ranking/teams/create"] = OnRankingTeamsCreate;
            dispatcher["account/buy"] = OnAccountBuy;
            dispatcher["ranking/team"] = OnRankingTeam;
        }

        private void OnRankingTeam(RequestMessage request)
        {
            GetTeamRequest Request = request.Body.Deserialize<GetTeamRequest>();
            GetTeamResponse data = new GetTeamResponse();

            Team team = BattleriteServer.DbManager.Teams.GetByTeamId((ulong)Request.teamID);
            if (team != null)
            {
                PublicTeamDataV2 TeamData = team.GetPublicTeamDataV2();
                data.team = TeamData;
            }
            SendResponse(request.ListenerResponse, data);
        }

        private void OnAccountBuy(RequestMessage request)
        {
            BuyItemRequest Request = request.Body.Deserialize<BuyItemRequest>();
            BuyItemResponse data = new BuyItemResponse()
            {
                newInventoryVersion = 5644654,
            };

            bool CanBuy = false;
            User user = GetUser(request.RemoteAddress);
            if (user != null)
            {
                CanBuy = user.CanBuy(Request.expectedPrice);
                if (CanBuy)
                {
                    PlayerStackableData stackableData = new PlayerStackableData()
                    {
                        type = (int)Request.stackableId,
                        amount = 1
                    };
                    user.AddStackableData(stackableData);
                    user = user.DiscountOnWallet(Request.expectedPrice);
                    data.stackableModifications = user.Account.inventory.stackables;
                }
                data.success = CanBuy;
            }
            
            SendResponse(request.ListenerResponse, data);
        }

        private void OnRankingTeamsCreate(RequestMessage request)
        {
            CreateTeamRequest Request = request.Body.Deserialize<CreateTeamRequest>();
            PublicTeamDataV2 team = new PublicTeamDataV2()
            {
                members = Request.members,
                avatar = 39074,
                freeNameChangesOwned = 1,
                name = "",
                teamID = modCommon.RandomID(),
                teamVersion = 1
            };
            CreateTeamResponse data = new CreateTeamResponse()
            {
                team = team
            };
            BattleriteServer.DbManager.Teams.AddOrUpdate((ulong)team.teamID, team);
            SendResponse(request.ListenerResponse, data);
        }

        private void OnAccountLootboxOpen(RequestMessage request)
        {
            OpenLootboxRequest Request = request.Body.Deserialize<OpenLootboxRequest>();
            OpenLootboxResponse data = new OpenLootboxResponse()
            {
                success = true,
                itemsReceived = new List<PlayerStackableData>()
                {
                    new PlayerStackableData()
                    {
                        type = 60166,
                        amount = 1
                    },
                    new PlayerStackableData()
                    {
                        type = 10025,
                        amount = 1
                    },
                    new PlayerStackableData()
                    {
                        type = 100075,
                        amount = 1
                    },

                }
            };

            User user = GetUser(request.RemoteAddress);
            if (user != null)
            {
            }
            SendResponse(request.ListenerResponse, data);        
        }

        private void OnAccountV1(RequestMessage request)
        {
            User user = GetUser(request.RemoteAddress);
            if (user != null)
            {
                GetAccountResponse data = user.Account;
                
                SendResponse(request.ListenerResponse, data);
            }
            //string json = File.ReadAllText("c:/login.json");
            //SendResponse(request.ListenerResponse, json);
            //SendResponse(request.ListenerResponse, data);

        }
        private void OnAccountFreerotationHandler(RequestMessage request)
        {
            GetFreeRotationResponse data = new GetFreeRotationResponse()
            {
                currentRotationStackables = new List<int>(),
                secondsToNextRotation = 0,
                nextRotationStackables = new List<int>()
            };

            string json = "{\"currentRotationStackables\":[10017,10015,10003,10016,10009,10019,10072,10067,10053,10069,10061,10051],\"secondsToNextRotation\":115299,\"nextRotationStackables\":[10043,10012,10014,10008,10009,10039,10068,10064,10066,10060,10061,10075]}";
            SendResponse(request.ListenerResponse, json);
            //SendResponse(request.ListenerResponse, data);
        }
        private void OnAccountProfilePicture(RequestMessage request)
        {
            if (request.ContainsBody)
            {
                SetPlayerPictureRequest Request = request.Body.Deserialize<SetPlayerPictureRequest>();
                User user = GetUser(request.RemoteAddress);
                if (user != null)
                {
                    BattleriteServer.DbManager.Users.SetPicture(user.AccountId, Request.picture); 
                }
            }
            EmptyResponse data = new EmptyResponse()
            {

            };
            
            SendResponse(request.ListenerResponse, data);
        }

        private void OnAccountProfileTitle(RequestMessage request)
        {
            if (request.ContainsBody)
            {
                SetPlayerTitleRequest Request = request.Body.Deserialize<SetPlayerTitleRequest>();
            }
            EmptyResponse data = new EmptyResponse()
            {

            };
            
            SendResponse(request.ListenerResponse, data);
        }

        private void OnAccountMatchmakingTicket(RequestMessage request)
        {
            if (request.ContainsBody)
            {
                CreateMatchmakingTicketRequest Request = request.Body.Deserialize<CreateMatchmakingTicketRequest>();
            }
            CreateMatchmakingTicketResponse data = new CreateMatchmakingTicketResponse()
            {
                matchmakingTicket   = "E5C9ED3879D4012442CF5E3C41E5168F1155E6090F9F139CC93AF94AF4A68E88F311AA3D042F9643496C77F5CA88C6084AD0AA9A8EFCF7BCD4165CA348B0DB8384131FED247E7F47D979E42DDA21C5F3C0E494FB43DD8C53331400EB009F337B22889B97693E5FF9A9141C4B8E221ADC6C75CC2D8C9C622B804B22AF1968E101707BDE2E46B59E7A3D4FDBFB5AFC29F93BE30369D1E32737D6304E73141C485E70CB3B6D47E0DC1CDAE7CECDDF2D4854EB5F7B8F72F94AB4777242300B8F7609F1C02FE73676D1EBB372BC457E02B32281915EC9857C28D4374CD9A2259B5B543CFFAEB0B21269AF31E5B7CA083BBF329064D2FD4247363D44660AE9EADFE45B311CE770A75819EC2E9BC27586BB2F93F50F720C9A761C67391EFAE6254E8B612034BB9348CAD146FFE663E7BD20B34452FB1DE94C6CB93DA332176EB8C04BFB10B01E9D22800982897AD4F2F87FAE403C0A0D4A1356BCAF597F87D54C130F68ED79DB8A854F895B14C81A48FE1F5F6AC80C5A64C019445EB57F3B7C32CC50575FC57A44FF88CBA32E14B2F833F3557B1377E62ED70EF9C483FFE4B95871C3703C8E7F7DFE6A7266816A3C237EF6D2D3B6C8E772DDE93A7B19CD406F636F361062B846A82F78AA7201DA38A393287BEE5EA78980052FB0C432E75C822C0F8AD1E056AEE2F5E93C51249277AC5E97188389C9E28BF32C63A01EF28775573A184C4B6C586A1A3565C375A6F3AC652BDE0C048C86529C191F0CC26BD773A10FB07F9BD649502632AE82EF530CE7F93A07DC83C422B3094C889BB4C4943D7DECD2C82E250D733918A6E1E284EB201F2C0566A99024ED0E1EBC1B41EC6C0E9F70E72E60AF4C031D14A24AF0E645DC23F8589371B944CFAED8D6C5A2404A80ABF96EC35F95810C0A756657260C7A73EF83D3FE5A96D106A942CAA6CEA5018F56972A1324CF4062B9FD41A5E78394816F679F8230596B678803DC9D1E0ACB4E1D8EF51BA3D4660F3D00098220D130C9369F64797C95347357DCCECB5DF613B55EC4DCB1CD69ACABBEAD8A3710B5867A05CD3BA6D3D628AF7023DA10A25765A967EC213EB8C83DFF996E68F24D28B0ED56C6590FDC5742CD68A4EA84B56FFF1DA3A9D39216C02C3ADBF3DA789727A346BE8D3C5EAF6521434251154500AE01CD8128ED20917A65379B0D73BECEF94C381D759ADAD5F40BEF4F200AC27E9C76E3EC4D37210FD2DDC4348AF0CCEF2B8A3C3B7854C4F1FDFB25FF77E738D6F609681E0AFB619DA51975EA5F6B6B66F4AFF9D8DB0CC9",
                playMatchTicket     = "E4D5A911713EB561ED77AC41B061BF41AF82EEBFAD2E46786F38A9666252134B06693072F582AFA7DE55B15A37647B0E5579222E4A6725D52DF620B618B9361B5C6E817CB20E5F0D4E0028771C3146897CFF612A1C1E63F3BB851D74DEC55191"
            };
            SendResponse(request.ListenerResponse, data);
        }

        private void OnAccountProfileId(RequestMessage request)
        {
            if (request.ContainsBody)
            {
                GetUserIdForNameRequest Request = request.Body.Deserialize<GetUserIdForNameRequest>();
            }
            GetUserIdForNameResponse data = new GetUserIdForNameResponse()
            {
                userId = 1000
            };
            
            SendResponse(request.ListenerResponse, data);
        }

        private void OnQuestGetSponsor(RequestMessage request)
        {
            GetSelectedSponsorResponse data = new GetSelectedSponsorResponse()
            {
                sponsorId = 54,
                schedule = "",
                sponsorHistory = new List<int>() { 4, 55, 465, 2845}
            };
            
            SendResponse(request.ListenerResponse, data);
        }

        private void OnAccountSetEmailFlags(RequestMessage request)
        {
            SuccessResponse data = new SuccessResponse()
            {
                success = true,
            };
            
            SendResponse(request.ListenerResponse, data);
        }

        private void OnRankingTeams(RequestMessage request)
        {
            List<ulong> users = new List<ulong>();
            int season = 0;

            if (request.ContainsBody)
            {
                GetTeamsRequest Request = request.Body.Deserialize<GetTeamsRequest>();
                users.AddRange(Request.users);
                season = Request.season;
            }
            else if (!string.IsNullOrEmpty(request.Query))
            {
                GetTeamsRequest Request = request.Query.DeserializeQuery<GetTeamsRequest>();
                users.AddRange(Request.users);
                season = Request.season;
            }
            GetTeamsResponse data = new GetTeamsResponse()
            {
                teams = new List<PublicTeamDataV2>()
            };
            foreach (var item in users)
            {
                User user = GetUser(request.RemoteAddress);
                if (user != null)
                {
                    List<Team> teams = BattleriteServer.DbManager.Teams.FindTeamsFromAccountId(user.AccountId);
                    data.teams.AddRange(from team in teams select team.TeamData.Get());
                }
            }
            
            SendResponse(request.ListenerResponse, data);
        }

        private void OnAccountLegalTerms(RequestMessage request)
        {
            AcceptLegalTermsResponse data = new AcceptLegalTermsResponse()
            {
                //success = true,
            };
            
            SendResponse(request.ListenerResponse, data);
        }

        private void OnAccountNews(RequestMessage request)
        {
            GetNewsInfoRequest Request = request.Body.Deserialize<GetNewsInfoRequest>();

            GetNewsInfoResponse data = new GetNewsInfoResponse()
            {
                region = "south_america",
                language = "spanish"
            };
            
            SendResponse(request.ListenerResponse, data);
        }

        private void OnLegalTermsV1(RequestMessage request)
        {
            AcceptLegalTermsResponse data = new AcceptLegalTermsResponse()
            {

            };
            
            SendResponse(request.ListenerResponse, data);
        }



        private void OnQuestUserHandler(RequestMessage request)
        {
            GetQuestStateResponse data = new GetQuestStateResponse()
            {
                pools = new List<QuestPoolMetaState>(),
                quests = new List<QuestState>(),
                secondsUntilQuestsChanges = 22505,
                version = 5
            };

            string json = "{\"quests\":[{\"guid\":\"3aaf38ee7b674edcb21c16b4249fa3b8\",\"ID\":10,\"completionState\":\"ACTIVE\",\"requirements\":[{\"sourceType\":3,\"sourceID\":10,\"progression\":1,\"events\":[]}],\"totalQuestProgress\":1,\"questPoolID\":1},{\"guid\":\"9cf1637b13294192a83ce6c0c8fd6e98\",\"ID\":11,\"completionState\":\"ACTIVE\",\"requirements\":[{\"sourceType\":3,\"sourceID\":10,\"progression\":0,\"events\":[]},{\"sourceType\":2,\"sourceID\":10,\"progression\":0,\"events\":[]}],\"totalQuestProgress\":1,\"questPoolID\":1},{\"guid\":\"54f9f7f550744efc87763d4fc3f7b3eb\",\"ID\":12,\"completionState\":\"ACTIVE\",\"requirements\":[{\"sourceType\":3,\"sourceID\":10,\"progression\":0,\"events\":[]},{\"sourceType\":2,\"sourceID\":11,\"progression\":0,\"events\":[]}],\"totalQuestProgress\":1,\"questPoolID\":1},{\"guid\":\"833b8da5168043a0897db95403dcce81\",\"ID\":13,\"completionState\":\"ACTIVE\",\"requirements\":[{\"sourceType\":3,\"sourceID\":10,\"progression\":0,\"events\":[]},{\"sourceType\":2,\"sourceID\":12,\"progression\":0,\"events\":[]}],\"totalQuestProgress\":1,\"questPoolID\":1},{\"guid\":\"c0a633d8b4d742a681c8f82311c7f9e3\",\"ID\":14,\"completionState\":\"ACTIVE\",\"requirements\":[{\"sourceType\":3,\"sourceID\":10,\"progression\":0,\"events\":[]},{\"sourceType\":2,\"sourceID\":13,\"progression\":0,\"events\":[]}],\"totalQuestProgress\":1,\"questPoolID\":1},{\"guid\":\"d9ade7a2dfb3417f8d8b98160ffc0b51\",\"ID\":20,\"completionState\":\"ACTIVE\",\"requirements\":[{\"sourceType\":3,\"sourceID\":100,\"progression\":0,\"events\":[]}],\"totalQuestProgress\":0,\"questPoolID\":1},{\"guid\":\"4d886b7b991e477da5041ca3d1d76ef6\",\"ID\":21,\"completionState\":\"ACTIVE\",\"requirements\":[{\"sourceType\":3,\"sourceID\":100,\"progression\":0,\"events\":[]},{\"sourceType\":2,\"sourceID\":20,\"progression\":0,\"events\":[]}],\"totalQuestProgress\":0,\"questPoolID\":1},{\"guid\":\"b5a4892ee7d8453ab23029e3db78127c\",\"ID\":22,\"completionState\":\"ACTIVE\",\"requirements\":[{\"sourceType\":3,\"sourceID\":100,\"progression\":0,\"events\":[]},{\"sourceType\":2,\"sourceID\":21,\"progression\":0,\"events\":[]}],\"totalQuestProgress\":0,\"questPoolID\":1},{\"guid\":\"0cca62b0b7ea43c39d46b54320db0d1f\",\"ID\":23,\"completionState\":\"ACTIVE\",\"requirements\":[{\"sourceType\":3,\"sourceID\":100,\"progression\":0,\"events\":[]},{\"sourceType\":2,\"sourceID\":22,\"progression\":0,\"events\":[]}],\"totalQuestProgress\":0,\"questPoolID\":1},{\"guid\":\"7edeca2860af4078affa839918c84375\",\"ID\":30,\"completionState\":\"ACTIVE\",\"requirements\":[{\"sourceType\":3,\"sourceID\":101,\"progression\":0,\"events\":[]}],\"totalQuestProgress\":0,\"questPoolID\":1},{\"guid\":\"9e95999bb0184807b8bc39fecc54eaa7\",\"ID\":31,\"completionState\":\"ACTIVE\",\"requirements\":[{\"sourceType\":3,\"sourceID\":101,\"progression\":0,\"events\":[]},{\"sourceType\":2,\"sourceID\":30,\"progression\":0,\"events\":[]}],\"totalQuestProgress\":0,\"questPoolID\":1},{\"guid\":\"2c3b9f2cea024350a3436cfe9afe3514\",\"ID\":32,\"completionState\":\"ACTIVE\",\"requirements\":[{\"sourceType\":3,\"sourceID\":101,\"progression\":0,\"events\":[]},{\"sourceType\":2,\"sourceID\":31,\"progression\":0,\"events\":[]}],\"totalQuestProgress\":0,\"questPoolID\":1},{\"guid\":\"82bb9966e45d440695490466f0a5e60c\",\"ID\":33,\"completionState\":\"ACTIVE\",\"requirements\":[{\"sourceType\":3,\"sourceID\":101,\"progression\":0,\"events\":[]},{\"sourceType\":2,\"sourceID\":32,\"progression\":0,\"events\":[]}],\"totalQuestProgress\":0,\"questPoolID\":1},{\"guid\":\"df230fefec744dc496649f4364790113\",\"ID\":40,\"completionState\":\"ACTIVE\",\"requirements\":[{\"sourceType\":3,\"sourceID\":112,\"progression\":0,\"events\":[]}],\"totalQuestProgress\":0,\"questPoolID\":1},{\"guid\":\"941c5c38d15d46ffb281768447ad96a8\",\"ID\":41,\"completionState\":\"ACTIVE\",\"requirements\":[{\"sourceType\":3,\"sourceID\":114,\"progression\":0,\"events\":[]}],\"totalQuestProgress\":0,\"questPoolID\":1},{\"guid\":\"6c38ca619c504f16a6b50d41af6dad64\",\"ID\":42,\"completionState\":\"ACTIVE\",\"requirements\":[{\"sourceType\":3,\"sourceID\":118,\"progression\":0,\"events\":[]}],\"totalQuestProgress\":0,\"questPoolID\":1},{\"guid\":\"9e31f64df7714009a7a6f08aec501df6\",\"ID\":50,\"completionState\":\"ACTIVE\",\"requirements\":[{\"sourceType\":3,\"sourceID\":11,\"progression\":0,\"events\":[]}],\"totalQuestProgress\":0,\"questPoolID\":1},{\"guid\":\"cf3d0602abf540baac87d8eed224a9f5\",\"ID\":51,\"completionState\":\"ACTIVE\",\"requirements\":[{\"sourceType\":3,\"sourceID\":11,\"progression\":0,\"events\":[]},{\"sourceType\":2,\"sourceID\":50,\"progression\":0,\"events\":[]}],\"totalQuestProgress\":0,\"questPoolID\":1},{\"guid\":\"3602dbb297834485b9b5103a22122388\",\"ID\":52,\"completionState\":\"ACTIVE\",\"requirements\":[{\"sourceType\":3,\"sourceID\":11,\"progression\":0,\"events\":[]},{\"sourceType\":2,\"sourceID\":51,\"progression\":0,\"events\":[]}],\"totalQuestProgress\":0,\"questPoolID\":1},{\"guid\":\"d93ea2b1a6cf48fcbca3fd293593417d\",\"ID\":53,\"completionState\":\"ACTIVE\",\"requirements\":[{\"sourceType\":3,\"sourceID\":11,\"progression\":0,\"events\":[]},{\"sourceType\":2,\"sourceID\":52,\"progression\":0,\"events\":[]}],\"totalQuestProgress\":0,\"questPoolID\":1},{\"guid\":\"05a6ff9c18104b55a7f6a90db185811f\",\"ID\":110,\"completionState\":\"ACTIVE\",\"requirements\":[{\"sourceType\":3,\"sourceID\":1001,\"progression\":0,\"events\":[]}],\"totalQuestProgress\":0,\"questPoolID\":1},{\"guid\":\"8e1e88211f5b4dd6ad44bdf79fddd988\",\"ID\":111,\"completionState\":\"ACTIVE\",\"requirements\":[{\"sourceType\":3,\"sourceID\":1001,\"progression\":0,\"events\":[]},{\"sourceType\":2,\"sourceID\":110,\"progression\":0,\"events\":[]}],\"totalQuestProgress\":0,\"questPoolID\":1},{\"guid\":\"980bb9c55c604a8b87ee6b46baf29550\",\"ID\":112,\"completionState\":\"ACTIVE\",\"requirements\":[{\"sourceType\":3,\"sourceID\":1001,\"progression\":0,\"events\":[]},{\"sourceType\":2,\"sourceID\":111,\"progression\":0,\"events\":[]}],\"totalQuestProgress\":0,\"questPoolID\":1},{\"guid\":\"3dd89b61904b420fb8bd7e875fc536b0\",\"ID\":120,\"completionState\":\"ACTIVE\",\"requirements\":[{\"sourceType\":3,\"sourceID\":1002,\"progression\":0,\"events\":[]}],\"totalQuestProgress\":0,\"questPoolID\":1},{\"guid\":\"f42953f91a7c4fd49ba9f4ef086a3fe8\",\"ID\":121,\"completionState\":\"ACTIVE\",\"requirements\":[{\"sourceType\":3,\"sourceID\":1002,\"progression\":0,\"events\":[]},{\"sourceType\":2,\"sourceID\":120,\"progression\":0,\"events\":[]}],\"totalQuestProgress\":0,\"questPoolID\":1},{\"guid\":\"82dbd610eba14821859cdc7de75fe34f\",\"ID\":130,\"completionState\":\"ACTIVE\",\"requirements\":[{\"sourceType\":3,\"sourceID\":1003,\"progression\":0,\"events\":[]}],\"totalQuestProgress\":0,\"questPoolID\":1},{\"guid\":\"df1fd80fc00448e1901179008d1e30d8\",\"ID\":131,\"completionState\":\"ACTIVE\",\"requirements\":[{\"sourceType\":3,\"sourceID\":1003,\"progression\":0,\"events\":[]},{\"sourceType\":2,\"sourceID\":130,\"progression\":0,\"events\":[]}],\"totalQuestProgress\":0,\"questPoolID\":1},{\"guid\":\"e2769dcabfde4e3094c96408257eb189\",\"ID\":140,\"completionState\":\"ACTIVE\",\"requirements\":[{\"sourceType\":3,\"sourceID\":1004,\"progression\":0,\"events\":[]}],\"totalQuestProgress\":0,\"questPoolID\":1},{\"guid\":\"d057c4763e514488847f79c41a7d8592\",\"ID\":141,\"completionState\":\"ACTIVE\",\"requirements\":[{\"sourceType\":3,\"sourceID\":1004,\"progression\":0,\"events\":[]},{\"sourceType\":2,\"sourceID\":140,\"progression\":0,\"events\":[]}],\"totalQuestProgress\":0,\"questPoolID\":1},{\"guid\":\"44baf02b197d4f14b94071fb39003be3\",\"ID\":150,\"completionState\":\"ACTIVE\",\"requirements\":[{\"sourceType\":3,\"sourceID\":1005,\"progression\":0,\"events\":[]}],\"totalQuestProgress\":0,\"questPoolID\":1},{\"guid\":\"0ae005f7327c4ebbbddf7c0051972d38\",\"ID\":151,\"completionState\":\"ACTIVE\",\"requirements\":[{\"sourceType\":3,\"sourceID\":1005,\"progression\":0,\"events\":[]},{\"sourceType\":2,\"sourceID\":150,\"progression\":0,\"events\":[]}],\"totalQuestProgress\":0,\"questPoolID\":1},{\"guid\":\"76b203f3ffcf45cbba6436276dab2430\",\"ID\":210,\"completionState\":\"ACTIVE\",\"requirements\":[{\"sourceType\":3,\"sourceID\":1101,\"progression\":0,\"events\":[]}],\"totalQuestProgress\":0,\"questPoolID\":1},{\"guid\":\"78e95d2e7a8a422bae1d822c98315d56\",\"ID\":211,\"completionState\":\"ACTIVE\",\"requirements\":[{\"sourceType\":3,\"sourceID\":1101,\"progression\":0,\"events\":[]},{\"sourceType\":2,\"sourceID\":210,\"progression\":0,\"events\":[]}],\"totalQuestProgress\":0,\"questPoolID\":1},{\"guid\":\"5ee27085789249f28b4fdf09017df7f6\",\"ID\":220,\"completionState\":\"ACTIVE\",\"requirements\":[{\"sourceType\":3,\"sourceID\":1102,\"progression\":0,\"events\":[]}],\"totalQuestProgress\":0,\"questPoolID\":1},{\"guid\":\"4cec492e171e4b3e84b98be411f07f92\",\"ID\":221,\"completionState\":\"ACTIVE\",\"requirements\":[{\"sourceType\":3,\"sourceID\":1102,\"progression\":0,\"events\":[]},{\"sourceType\":2,\"sourceID\":220,\"progression\":0,\"events\":[]}],\"totalQuestProgress\":0,\"questPoolID\":1},{\"guid\":\"1650c5bd191d46c680bb04912bf32d5b\",\"ID\":1001,\"completionState\":\"ACTIVE\",\"requirements\":[{\"sourceType\":1,\"sourceID\":1,\"progression\":0,\"events\":[]}],\"totalQuestProgress\":0,\"questPoolID\":1},{\"guid\":\"700dbf55bffd488d91f0ca4d3d028144\",\"ID\":1002,\"completionState\":\"ACTIVE\",\"requirements\":[{\"sourceType\":1,\"sourceID\":2,\"progression\":0,\"events\":[]}],\"totalQuestProgress\":0,\"questPoolID\":1},{\"guid\":\"34cfcedcffee4cb3b50a59c15610a9a8\",\"ID\":1003,\"completionState\":\"ACTIVE\",\"requirements\":[{\"sourceType\":1,\"sourceID\":3,\"progression\":0,\"events\":[]}],\"totalQuestProgress\":0,\"questPoolID\":1},{\"guid\":\"6cadaee7df0a48718e9ada469e13be72\",\"ID\":1004,\"completionState\":\"ACTIVE\",\"requirements\":[{\"sourceType\":1,\"sourceID\":4,\"progression\":0,\"events\":[]}],\"totalQuestProgress\":0,\"questPoolID\":1},{\"guid\":\"5ee0e3e9f46043b1bfb5b009e7a5b23d\",\"ID\":1006,\"completionState\":\"ACTIVE\",\"requirements\":[{\"sourceType\":1,\"sourceID\":6,\"progression\":0,\"events\":[]}],\"totalQuestProgress\":0,\"questPoolID\":1},{\"guid\":\"88c70cb7ee544bcda19de36942e0ebc7\",\"ID\":7001,\"completionState\":\"ACTIVE\",\"requirements\":[{\"sourceType\":3,\"sourceID\":9002,\"progression\":0,\"events\":[]}],\"totalQuestProgress\":0,\"questPoolID\":1},{\"guid\":\"ef5731490a2c4246ab4acffe6a2c6cc7\",\"ID\":2000,\"completionState\":\"ACTIVE\",\"requirements\":[{\"sourceType\":1,\"sourceID\":100,\"progression\":0,\"events\":[]}],\"totalQuestProgress\":0,\"questPoolID\":2},{\"guid\":\"5d7d404817504f4785314c0997deb73f\",\"ID\":2001,\"completionState\":\"ACTIVE\",\"requirements\":[{\"sourceType\":1,\"sourceID\":101,\"progression\":0,\"events\":[]}],\"totalQuestProgress\":0,\"questPoolID\":2},{\"guid\":\"dec4f7a4dbc44e3592578859b022d42e\",\"ID\":2010,\"completionState\":\"ACTIVE\",\"requirements\":[{\"sourceType\":1,\"sourceID\":110,\"progression\":0,\"events\":[]},{\"sourceType\":2,\"sourceID\":2001,\"progression\":0,\"events\":[]}],\"totalQuestProgress\":0,\"questPoolID\":2},{\"guid\":\"a190aae9021c4736bf09e7cb316639e3\",\"ID\":2011,\"completionState\":\"ACTIVE\",\"requirements\":[{\"sourceType\":1,\"sourceID\":111,\"progression\":0,\"events\":[]},{\"sourceType\":2,\"sourceID\":2001,\"progression\":0,\"events\":[]}],\"totalQuestProgress\":0,\"questPoolID\":2},{\"guid\":\"3fde4b24c25d483b834920c0573cad8a\",\"ID\":2020,\"completionState\":\"ACTIVE\",\"requirements\":[{\"sourceType\":1,\"sourceID\":120,\"progression\":0,\"events\":[]},{\"sourceType\":2,\"sourceID\":2011,\"progression\":0,\"events\":[]}],\"totalQuestProgress\":0,\"questPoolID\":2},{\"guid\":\"0823ce9217d04903862f842fd797beeb\",\"ID\":2030,\"completionState\":\"ACTIVE\",\"requirements\":[{\"sourceType\":1,\"sourceID\":130,\"progression\":0,\"events\":[]},{\"sourceType\":2,\"sourceID\":2020,\"progression\":0,\"events\":[]}],\"totalQuestProgress\":0,\"questPoolID\":2},{\"guid\":\"16900b8ee5724b9f8acbaceb30a4695c\",\"ID\":2040,\"completionState\":\"ACTIVE\",\"requirements\":[{\"sourceType\":1,\"sourceID\":140,\"progression\":0,\"events\":[]},{\"sourceType\":2,\"sourceID\":2030,\"progression\":0,\"events\":[]}],\"totalQuestProgress\":0,\"questPoolID\":2},{\"guid\":\"31f565b1327047a5bb35c0ac5d482c3c\",\"ID\":5001,\"completionState\":\"FINISHED\",\"requirements\":[{\"sourceType\":1,\"sourceID\":10,\"progression\":1,\"events\":[]}],\"totalQuestProgress\":1,\"secondsSinceStart\":63895,\"secondsToEnd\":22505,\"timeSinceCompleted\":334,\"questPoolID\":6},{\"guid\":\"33f013d235dc4c62941b4e5b1165f1c2\",\"ID\":1004505,\"completionState\":\"ACTIVE\",\"requirements\":[{\"sourceType\":1,\"sourceID\":1055004,\"progression\":0,\"events\":[]}],\"totalQuestProgress\":0,\"secondsSinceStart\":235157,\"secondsToEnd\":24043,\"questPoolID\":103},{\"guid\":\"83d6f61dedae4e9694687ef6b6f802e0\",\"ID\":1004504,\"completionState\":\"ACTIVE\",\"requirements\":[{\"sourceType\":1,\"sourceID\":1055003,\"progression\":0,\"events\":[]}],\"totalQuestProgress\":0,\"secondsSinceStart\":235157,\"secondsToEnd\":24043,\"questPoolID\":103},{\"guid\":\"cf24c96decb642038863dfde6857cb14\",\"ID\":1004502,\"completionState\":\"ACTIVE\",\"requirements\":[{\"sourceType\":1,\"sourceID\":1055002,\"progression\":0,\"events\":[]}],\"totalQuestProgress\":0,\"secondsSinceStart\":148757,\"secondsToEnd\":110443,\"questPoolID\":103},{\"guid\":\"2d0aa1482aff4546a53eaeb606e57f48\",\"ID\":1004507,\"completionState\":\"ACTIVE\",\"requirements\":[{\"sourceType\":1,\"sourceID\":1055006,\"progression\":0,\"events\":[]}],\"totalQuestProgress\":0,\"secondsSinceStart\":148757,\"secondsToEnd\":110443,\"questPoolID\":103},{\"guid\":\"041cf344d3c94458b50756107eb84981\",\"ID\":1004509,\"completionState\":\"ACTIVE\",\"requirements\":[{\"sourceType\":1,\"sourceID\":1055008,\"progression\":0,\"events\":[]}],\"totalQuestProgress\":0,\"secondsSinceStart\":62357,\"secondsToEnd\":196843,\"questPoolID\":103},{\"guid\":\"b5b50aec0913448396c6115a6ab48161\",\"ID\":1004508,\"completionState\":\"ACTIVE\",\"requirements\":[{\"sourceType\":1,\"sourceID\":1055007,\"progression\":0,\"events\":[]}],\"totalQuestProgress\":0,\"secondsSinceStart\":62357,\"secondsToEnd\":196843,\"questPoolID\":103},{\"guid\":\"7f9b15a9f1f84fe5b7ce1fdc10976f2b\",\"ID\":60004,\"completionState\":\"ACTIVE\",\"requirements\":[{\"sourceType\":1,\"sourceID\":60002,\"progression\":0,\"events\":[]}],\"totalQuestProgress\":0,\"questPoolID\":30002},{\"guid\":\"37777a0371b740bba18f7d58fa67d071\",\"ID\":60005,\"completionState\":\"ACTIVE\",\"requirements\":[{\"sourceType\":1,\"sourceID\":35,\"progression\":1,\"events\":[]},{\"sourceType\":3,\"sourceID\":1200,\"progression\":0,\"events\":[]}],\"totalQuestProgress\":1,\"questPoolID\":30003},{\"guid\":\"842ae4243418426b839be5252d33e18f\",\"ID\":60006,\"completionState\":\"ACTIVE\",\"requirements\":[{\"sourceType\":1,\"sourceID\":35,\"progression\":1,\"events\":[]},{\"sourceType\":3,\"sourceID\":1201,\"progression\":0,\"events\":[]}],\"totalQuestProgress\":1,\"questPoolID\":30003},{\"guid\":\"82893d4c912943dca0f2d560df5f239c\",\"ID\":60007,\"completionState\":\"ACTIVE\",\"requirements\":[{\"sourceType\":1,\"sourceID\":10,\"progression\":1,\"events\":[]},{\"sourceType\":3,\"sourceID\":1202,\"progression\":0,\"events\":[]}],\"totalQuestProgress\":1,\"questPoolID\":30003},{\"guid\":\"3f433560bf924862bff87cc51f5e92a1\",\"ID\":60008,\"completionState\":\"ACTIVE\",\"requirements\":[{\"sourceType\":1,\"sourceID\":60002,\"progression\":0,\"events\":[]},{\"sourceType\":3,\"sourceID\":10,\"progression\":0,\"events\":[]}],\"totalQuestProgress\":0,\"questPoolID\":30003},{\"guid\":\"b1029052617640aebe5d89e013a368b2\",\"ID\":1060005,\"completionState\":\"ACTIVE\",\"requirements\":[{\"sourceType\":1,\"sourceID\":1000027,\"progression\":0,\"events\":[]},{\"sourceType\":3,\"sourceID\":1203,\"progression\":0,\"events\":[]}],\"totalQuestProgress\":0,\"questPoolID\":30003},{\"guid\":\"d14e09b376d044abad849037f8e10155\",\"ID\":1000010,\"completionState\":\"ACTIVE\",\"requirements\":[{\"sourceType\":3,\"sourceID\":1000010,\"progression\":1,\"events\":[]}],\"totalQuestProgress\":1,\"questPoolID\":100001},{\"guid\":\"8bafb82d115f47f1bbd896275c572e4b\",\"ID\":1000011,\"completionState\":\"ACTIVE\",\"requirements\":[{\"sourceType\":3,\"sourceID\":1000010,\"progression\":0,\"events\":[]},{\"sourceType\":2,\"sourceID\":1000010,\"progression\":0,\"events\":[]}],\"totalQuestProgress\":1,\"questPoolID\":100001},{\"guid\":\"d4f85d28a9cd4009bda8c5de15d513ba\",\"ID\":1000012,\"completionState\":\"ACTIVE\",\"requirements\":[{\"sourceType\":3,\"sourceID\":1000010,\"progression\":0,\"events\":[]},{\"sourceType\":2,\"sourceID\":1000011,\"progression\":0,\"events\":[]}],\"totalQuestProgress\":1,\"questPoolID\":100001},{\"guid\":\"a1c480b58f774595982ffebb18156e3b\",\"ID\":1000013,\"completionState\":\"ACTIVE\",\"requirements\":[{\"sourceType\":3,\"sourceID\":1000010,\"progression\":0,\"events\":[]},{\"sourceType\":2,\"sourceID\":1000012,\"progression\":0,\"events\":[]}],\"totalQuestProgress\":1,\"questPoolID\":100001},{\"guid\":\"5f741c2eaeab49faa2bad5b22e6b7ca4\",\"ID\":1000014,\"completionState\":\"ACTIVE\",\"requirements\":[{\"sourceType\":3,\"sourceID\":1000010,\"progression\":0,\"events\":[]},{\"sourceType\":2,\"sourceID\":1000013,\"progression\":0,\"events\":[]}],\"totalQuestProgress\":1,\"questPoolID\":100001},{\"guid\":\"418598f5d4aa42839b7f6d2f64cf2dfc\",\"ID\":1000050,\"completionState\":\"ACTIVE\",\"requirements\":[{\"sourceType\":3,\"sourceID\":11,\"progression\":0,\"events\":[]}],\"totalQuestProgress\":0,\"questPoolID\":100001},{\"guid\":\"ccdb7208559a46afbbbc3ac641b4443e\",\"ID\":1000051,\"completionState\":\"ACTIVE\",\"requirements\":[{\"sourceType\":3,\"sourceID\":11,\"progression\":0,\"events\":[]},{\"sourceType\":2,\"sourceID\":1000050,\"progression\":0,\"events\":[]}],\"totalQuestProgress\":0,\"questPoolID\":100001},{\"guid\":\"4b0c3a8826884cf982ae1a78797613b4\",\"ID\":1000052,\"completionState\":\"ACTIVE\",\"requirements\":[{\"sourceType\":3,\"sourceID\":11,\"progression\":0,\"events\":[]},{\"sourceType\":2,\"sourceID\":1000051,\"progression\":0,\"events\":[]}],\"totalQuestProgress\":0,\"questPoolID\":100001},{\"guid\":\"0726ebad214a4344bba0bc308dce850c\",\"ID\":1000053,\"completionState\":\"ACTIVE\",\"requirements\":[{\"sourceType\":3,\"sourceID\":11,\"progression\":0,\"events\":[]},{\"sourceType\":2,\"sourceID\":1000052,\"progression\":0,\"events\":[]}],\"totalQuestProgress\":0,\"questPoolID\":100001},{\"guid\":\"f694aaaff2714852b15b113c16a6d36e\",\"ID\":1001003,\"completionState\":\"ACTIVE\",\"requirements\":[{\"sourceType\":1,\"sourceID\":1000003,\"progression\":0,\"events\":[]}],\"totalQuestProgress\":0,\"questPoolID\":100001},{\"guid\":\"096721aedc754bcaa41670d02d51a331\",\"ID\":1001004,\"completionState\":\"ACTIVE\",\"requirements\":[{\"sourceType\":1,\"sourceID\":1000013,\"progression\":0,\"events\":[]}],\"totalQuestProgress\":0,\"questPoolID\":100001},{\"guid\":\"6c7ac905596645cbb989c09d12dcf8f7\",\"ID\":1001005,\"completionState\":\"ACTIVE\",\"requirements\":[{\"sourceType\":1,\"sourceID\":1000014,\"progression\":0,\"events\":[]}],\"totalQuestProgress\":0,\"questPoolID\":100001},{\"guid\":\"bbf29413f0f247b5b50088e094c7619d\",\"ID\":1001006,\"completionState\":\"ACTIVE\",\"requirements\":[{\"sourceType\":1,\"sourceID\":1000015,\"progression\":0,\"events\":[]}],\"totalQuestProgress\":0,\"questPoolID\":100001},{\"guid\":\"5d7fc59a66784a05b8bd1dd253587c14\",\"ID\":1001007,\"completionState\":\"ACTIVE\",\"requirements\":[{\"sourceType\":1,\"sourceID\":1000016,\"progression\":0,\"events\":[]}],\"totalQuestProgress\":0,\"questPoolID\":100001},{\"guid\":\"eb81438513f6402da56e39a3f56752bd\",\"ID\":1001008,\"completionState\":\"ACTIVE\",\"requirements\":[{\"sourceType\":1,\"sourceID\":1000017,\"progression\":0,\"events\":[]}],\"totalQuestProgress\":0,\"questPoolID\":100001},{\"guid\":\"ca4b172c5ab94898ae487ee1c194cbf8\",\"ID\":1001009,\"completionState\":\"ACTIVE\",\"requirements\":[{\"sourceType\":1,\"sourceID\":1000010,\"progression\":0,\"events\":[]}],\"totalQuestProgress\":0,\"questPoolID\":100001},{\"guid\":\"bac141cefb8b46e4a753b18f12f7594d\",\"ID\":1001010,\"completionState\":\"ACTIVE\",\"requirements\":[{\"sourceType\":1,\"sourceID\":1000010,\"progression\":0,\"events\":[]}],\"totalQuestProgress\":0,\"questPoolID\":100001},{\"guid\":\"88c4c3ba687b46a980dae87656ac47fc\",\"ID\":1001011,\"completionState\":\"ACTIVE\",\"requirements\":[{\"sourceType\":1,\"sourceID\":1000011,\"progression\":0,\"events\":[]}],\"totalQuestProgress\":0,\"questPoolID\":100001},{\"guid\":\"63b2861073894f46aeb3cfbf4e162df6\",\"ID\":1001012,\"completionState\":\"ACTIVE\",\"requirements\":[{\"sourceType\":1,\"sourceID\":1000011,\"progression\":0,\"events\":[]}],\"totalQuestProgress\":0,\"questPoolID\":100001},{\"guid\":\"4989cd9b16a14977a9bbc89fb6bde90a\",\"ID\":1001013,\"completionState\":\"ACTIVE\",\"requirements\":[{\"sourceType\":1,\"sourceID\":1000007,\"progression\":0,\"events\":[]}],\"totalQuestProgress\":0,\"questPoolID\":100001},{\"guid\":\"ac6b1a0f5df1423aaa774a712be969f6\",\"ID\":1001014,\"completionState\":\"ACTIVE\",\"requirements\":[{\"sourceType\":1,\"sourceID\":1000007,\"progression\":0,\"events\":[]}],\"totalQuestProgress\":0,\"questPoolID\":100001},{\"guid\":\"499aed7e4a914c6ab824acfdd5b8dd3d\",\"ID\":1001015,\"completionState\":\"ACTIVE\",\"requirements\":[{\"sourceType\":1,\"sourceID\":1000007,\"progression\":0,\"events\":[]}],\"totalQuestProgress\":0,\"questPoolID\":100001},{\"guid\":\"b4556352c69d4c06a130539ecb5c0f71\",\"ID\":1001016,\"completionState\":\"ACTIVE\",\"requirements\":[{\"sourceType\":1,\"sourceID\":1000018,\"progression\":0,\"events\":[]}],\"totalQuestProgress\":0,\"questPoolID\":100001},{\"guid\":\"182b51a1350848d8bca0aa60758b2498\",\"ID\":1001017,\"completionState\":\"ACTIVE\",\"requirements\":[{\"sourceType\":1,\"sourceID\":1000019,\"progression\":0,\"events\":[]}],\"totalQuestProgress\":0,\"questPoolID\":100001},{\"guid\":\"3de5a2b629284412a95b74fa5fb90adc\",\"ID\":1001018,\"completionState\":\"ACTIVE\",\"requirements\":[{\"sourceType\":1,\"sourceID\":1000020,\"progression\":0,\"events\":[]}],\"totalQuestProgress\":0,\"questPoolID\":100001},{\"guid\":\"599df8503510462fa6d0a8cc3aef64f0\",\"ID\":1001019,\"completionState\":\"ACTIVE\",\"requirements\":[{\"sourceType\":1,\"sourceID\":1000021,\"progression\":0,\"events\":[]}],\"totalQuestProgress\":0,\"questPoolID\":100001},{\"guid\":\"85fe1c8ff22647099af83b5160afe27e\",\"ID\":1001020,\"completionState\":\"ACTIVE\",\"requirements\":[{\"sourceType\":1,\"sourceID\":1000022,\"progression\":0,\"events\":[]}],\"totalQuestProgress\":0,\"questPoolID\":100001},{\"guid\":\"7198a6dbad074a118dff7600fdcb2675\",\"ID\":1001021,\"completionState\":\"ACTIVE\",\"requirements\":[{\"sourceType\":1,\"sourceID\":1000023,\"progression\":0,\"events\":[]}],\"totalQuestProgress\":0,\"questPoolID\":100001},{\"guid\":\"a7b93b0c85674e19a1c49b5233acb4a8\",\"ID\":1001022,\"completionState\":\"ACTIVE\",\"requirements\":[{\"sourceType\":1,\"sourceID\":1000024,\"progression\":0,\"events\":[]}],\"totalQuestProgress\":0,\"questPoolID\":100001},{\"guid\":\"ab5348bdeb4549f0ba5778e513c1561b\",\"ID\":1001023,\"completionState\":\"ACTIVE\",\"requirements\":[{\"sourceType\":1,\"sourceID\":1000005,\"progression\":0,\"events\":[]}],\"totalQuestProgress\":0,\"questPoolID\":100001},{\"guid\":\"45b8dfe9111949f8b29001f984cf2018\",\"ID\":1001024,\"completionState\":\"ACTIVE\",\"requirements\":[{\"sourceType\":1,\"sourceID\":1000005,\"progression\":0,\"events\":[]}],\"totalQuestProgress\":0,\"questPoolID\":100001},{\"guid\":\"b9cfb5d27907409593f2ae16feee59be\",\"ID\":1001025,\"completionState\":\"ACTIVE\",\"requirements\":[{\"sourceType\":1,\"sourceID\":1000005,\"progression\":0,\"events\":[]}],\"totalQuestProgress\":0,\"questPoolID\":100001},{\"guid\":\"6a701ac9ac1c4208b97a3b137a7e2cf1\",\"ID\":1001026,\"completionState\":\"ACTIVE\",\"requirements\":[{\"sourceType\":1,\"sourceID\":1000006,\"progression\":0,\"events\":[]}],\"totalQuestProgress\":0,\"questPoolID\":100001},{\"guid\":\"522f7b2339734d2e9dd4af96967e9663\",\"ID\":1001027,\"completionState\":\"ACTIVE\",\"requirements\":[{\"sourceType\":1,\"sourceID\":1000006,\"progression\":0,\"events\":[]}],\"totalQuestProgress\":0,\"questPoolID\":100001},{\"guid\":\"8f61500718e0473ebb858089d8d599b4\",\"ID\":1001028,\"completionState\":\"ACTIVE\",\"requirements\":[{\"sourceType\":1,\"sourceID\":1000006,\"progression\":0,\"events\":[]}],\"totalQuestProgress\":0,\"questPoolID\":100001},{\"guid\":\"f3335e4c6218482499435033670863de\",\"ID\":1001029,\"completionState\":\"ACTIVE\",\"requirements\":[{\"sourceType\":1,\"sourceID\":1000025,\"progression\":0,\"events\":[]}],\"totalQuestProgress\":0,\"questPoolID\":100001},{\"guid\":\"2f13bb1f0f154bd098954af56d23f856\",\"ID\":1001030,\"completionState\":\"ACTIVE\",\"requirements\":[{\"sourceType\":1,\"sourceID\":1000008,\"progression\":0,\"events\":[]}],\"totalQuestProgress\":0,\"questPoolID\":100001},{\"guid\":\"00fdb58329bc43e1b22949e7072ab057\",\"ID\":1001031,\"completionState\":\"ACTIVE\",\"requirements\":[{\"sourceType\":1,\"sourceID\":1000009,\"progression\":0,\"events\":[]}],\"totalQuestProgress\":0,\"questPoolID\":100001},{\"guid\":\"abf8dbce43fe4fecb8d34c994d032a74\",\"ID\":1001032,\"completionState\":\"ACTIVE\",\"requirements\":[{\"sourceType\":1,\"sourceID\":1000026,\"progression\":0,\"events\":[]}],\"totalQuestProgress\":0,\"questPoolID\":100001},{\"guid\":\"73e37bb0cc2844128c4dfba8d3e3de85\",\"ID\":1001033,\"completionState\":\"ACTIVE\",\"requirements\":[{\"sourceType\":1,\"sourceID\":1000100,\"progression\":0,\"events\":[]}],\"totalQuestProgress\":0,\"questPoolID\":100001},{\"guid\":\"e893d1ba159047e5bf7f42bcfeea3e6b\",\"ID\":1001034,\"completionState\":\"ACTIVE\",\"requirements\":[{\"sourceType\":1,\"sourceID\":1000028,\"progression\":0,\"events\":[]}],\"totalQuestProgress\":0,\"questPoolID\":100001},{\"guid\":\"eb1142a8bf514f41829f29d631b20ebe\",\"ID\":1060004,\"completionState\":\"ACTIVE\",\"requirements\":[{\"sourceType\":1,\"sourceID\":1060002,\"progression\":0,\"events\":[]}],\"totalQuestProgress\":0,\"questPoolID\":130002}],\"pools\":[{\"ID\":1},{\"ID\":2},{\"ID\":3},{\"ID\":4},{\"ID\":5},{\"ID\":6,\"secondsUntilRefresh\":22504},{\"ID\":7},{\"ID\":8},{\"ID\":9},{\"ID\":10},{\"ID\":11},{\"ID\":12},{\"ID\":13},{\"ID\":14},{\"ID\":15},{\"ID\":16},{\"ID\":17},{\"ID\":18},{\"ID\":19},{\"ID\":20},{\"ID\":22},{\"ID\":23},{\"ID\":24},{\"ID\":25},{\"ID\":26},{\"ID\":27},{\"ID\":28},{\"ID\":29},{\"ID\":30},{\"ID\":31},{\"ID\":32},{\"ID\":33},{\"ID\":34},{\"ID\":35},{\"ID\":36},{\"ID\":37},{\"ID\":38},{\"ID\":39},{\"ID\":40},{\"ID\":41},{\"ID\":42},{\"ID\":43},{\"ID\":44},{\"ID\":45},{\"ID\":46},{\"ID\":47},{\"ID\":48},{\"ID\":49},{\"ID\":50},{\"ID\":51},{\"ID\":52},{\"ID\":53},{\"ID\":54},{\"ID\":55},{\"ID\":56},{\"ID\":57},{\"ID\":58},{\"ID\":59},{\"ID\":60},{\"ID\":61},{\"ID\":103,\"secondsUntilRefresh\":24042},{\"ID\":10001},{\"ID\":10002},{\"ID\":10003},{\"ID\":10004},{\"ID\":10005},{\"ID\":10006},{\"ID\":10007},{\"ID\":10008},{\"ID\":10009},{\"ID\":10010},{\"ID\":10011},{\"ID\":10012},{\"ID\":10013},{\"ID\":10014},{\"ID\":10015},{\"ID\":10016},{\"ID\":10017},{\"ID\":10018},{\"ID\":10019},{\"ID\":10020},{\"ID\":10021},{\"ID\":10022},{\"ID\":10023},{\"ID\":10024},{\"ID\":10025},{\"ID\":10026},{\"ID\":10027},{\"ID\":10028},{\"ID\":10029},{\"ID\":10030},{\"ID\":10031},{\"ID\":10032},{\"ID\":10033},{\"ID\":10034},{\"ID\":10035},{\"ID\":10036},{\"ID\":10037},{\"ID\":10038},{\"ID\":10039},{\"ID\":10040},{\"ID\":10041},{\"ID\":10042},{\"ID\":10043},{\"ID\":10044},{\"ID\":10045},{\"ID\":30001},{\"ID\":30002},{\"ID\":30003},{\"ID\":30004},{\"ID\":100001},{\"ID\":100002},{\"ID\":100003},{\"ID\":100004},{\"ID\":100005},{\"ID\":100006},{\"ID\":100007},{\"ID\":100051},{\"ID\":100052},{\"ID\":100053},{\"ID\":100054},{\"ID\":100055},{\"ID\":100056},{\"ID\":100057},{\"ID\":100058},{\"ID\":100059},{\"ID\":100060},{\"ID\":100061},{\"ID\":100062},{\"ID\":100063},{\"ID\":111001},{\"ID\":111002},{\"ID\":111003},{\"ID\":111004},{\"ID\":111005},{\"ID\":111006},{\"ID\":111007},{\"ID\":111008},{\"ID\":111009},{\"ID\":111010},{\"ID\":111011},{\"ID\":111012},{\"ID\":111013},{\"ID\":111014},{\"ID\":111015},{\"ID\":111016},{\"ID\":111017},{\"ID\":111018},{\"ID\":111019},{\"ID\":111020},{\"ID\":111021},{\"ID\":111022},{\"ID\":111023},{\"ID\":111024},{\"ID\":111025},{\"ID\":111026},{\"ID\":111027},{\"ID\":111028},{\"ID\":111029},{\"ID\":111030},{\"ID\":111031},{\"ID\":111032},{\"ID\":111033},{\"ID\":111034},{\"ID\":111035},{\"ID\":112000},{\"ID\":112001},{\"ID\":112002},{\"ID\":112003},{\"ID\":112004},{\"ID\":112005},{\"ID\":112006},{\"ID\":112007},{\"ID\":112008},{\"ID\":112009},{\"ID\":112010},{\"ID\":112011},{\"ID\":112012},{\"ID\":112013},{\"ID\":112014},{\"ID\":112015},{\"ID\":112016},{\"ID\":112017},{\"ID\":112018},{\"ID\":112019},{\"ID\":112020},{\"ID\":112021},{\"ID\":112022},{\"ID\":112023},{\"ID\":112024},{\"ID\":112025},{\"ID\":112026},{\"ID\":112027},{\"ID\":112028},{\"ID\":112029},{\"ID\":112030},{\"ID\":112031},{\"ID\":112032},{\"ID\":112033},{\"ID\":112034},{\"ID\":130001},{\"ID\":130002},{\"ID\":10000024},{\"ID\":10000026},{\"ID\":10000028}],\"version\":5,\"secondsUntilQuestsChanges\":22505}";
            //SendResponse(request.ListenerResponse, json);
            SendResponse(request.ListenerResponse, data);
        }

        private void OnAccountSteamInfoHandler(RequestMessage request)
        {
            GetSteamInfoResponse data = new GetSteamInfoResponse()
            {
                country = "BR",
                currency = "BRL"
            };
            //string json = "{\"profile\":{\"userId\":1000,\"name\":\"Hackerprod\",\"title\":504,\"picture\":30000,\"displayFlags\":1},\"inventory\":{\"userId\":1357145039997902848,\"version\":5,\"stackables\":[{\"type\":20,\"amount\":1},{\"type\":91,\"amount\":1612403765},{\"type\":93,\"amount\":1},{\"type\":100,\"amount\":50},{\"type\":199,\"amount\":222},{\"type\":201,\"amount\":1612403765},{\"type\":205,\"amount\":1612403596},{\"type\":400001,\"amount\":2},{\"type\":400037}]},\"emailVerificationState\":3,\"stackableChanges\":[],\"createTime\":1612403589000}";
            //SendResponse(request.ListenerResponse, json);
            SendResponse(request.ListenerResponse, data);

        }

        private void OnEmailVerification(RequestMessage request)
        {
            EmailVerificationStateResponse data = new EmailVerificationStateResponse()
            {
                email = "hackerprodlive@gmail.com",
                flags = (int)EmailVerificationFlag.AcceptPrivacyPolicy,
                firstVerification = false,
                resendTime = 0,
                state = 3,
                useEmailVerification = false,
                verifyTime = 0
            };
            
            SendResponse(request.ListenerResponse, data);
        }
        private void OnAccountRewards(RequestMessage request)
        {
            User user = GetUser(request.RemoteAddress);
            GetRewardsStackablesRequest Request = request.Body.Deserialize<GetRewardsStackablesRequest>();
            GetRewardsStackablesResponse data = new GetRewardsStackablesResponse()
            {
                rewards = new List<PublicRewardData>()
                {
                    new PublicRewardData()
                    {
                        guid = Request.rewardGuids[0],
                        stackables = new List<PlayerStackableData>()
                        {
                            new PlayerStackableData()   //Bloodcoins
                            {
                               type = 100,
                               amount = 5000
                            },
                        }
                    },
                    new PublicRewardData()
                    {
                        guid = Request.rewardGuids[0],
                        stackables = new List<PlayerStackableData>()
                        {
                            new PlayerStackableData()   //Bloodcoins
                            {
                               type = 100,
                               amount = 200
                            },
                        }
                    },
                }
            };

            //string json = "{\"rewards\":[{\"guid\":\"0841abb2-6098-46a1-97c9-139e3801691a\",\"stackables\":[{\"type\":132,\"amount\":1}]},{\"guid\":\"25aac242-d38c-4e18-a94b-e091b6373d24\",\"stackables\":[{\"type\":400002,\"amount\":1}]},{\"guid\":\"95fc0593-b759-4704-b9a0-69ec52023b1f\",\"stackables\":[{\"type\":400001,\"amount\":1}]},{\"guid\":\"81fc0170-1543-4c1b-8314-9d05d62a6c08\",\"stackables\":[{\"type\":400003,\"amount\":1}]},{\"guid\":\"151451d1-d728-45b5-bcfa-f54aa1f0bcd8\",\"stackables\":[{\"type\":100,\"amount\":500}]},{\"guid\":\"21953f85-0742-4b12-bb97-39ab5955da59\",\"stackables\":[{\"type\":101,\"amount\":5000}]},{\"guid\":\"a580af1d-62fe-48ce-b2d1-82a679b61633\",\"stackables\":[{\"type\":100,\"amount\":250}]},{\"guid\":\"28337254-b664-4e13-8feb-2a03525706aa\",\"stackables\":[{\"type\":100,\"amount\":100}]},{\"guid\":\"564e7dd9-b28b-4a4f-b0e8-aeecdea08fff\",\"stackables\":[{\"type\":100029,\"amount\":1}]}]}";
            //SendResponse(request.ListenerResponse, json);
            SendResponse(request.ListenerResponse, data);
        }

        private void OnAccountStats(RequestMessage request)
        {
            if (request.ContainsBody)
            {
                UIEventsRequest Request = request.Body.Deserialize<UIEventsRequest>();
            }
            EmptyResponse data = new EmptyResponse()
            {
                
            };
            
            SendResponse(request.ListenerResponse, data);
        }

        private void OnAccountProfilePublic(RequestMessage request)
        {
            GetPublicProfilesRequest Request = request.Body.Deserialize<GetPublicProfilesRequest>();
            GetPublicProfilesResponse data = new GetPublicProfilesResponse()
            {
                profiles = new List<PublicProfileData>()
            };
            foreach (var item in Request.users)
            {
                User user = GetUser(item);
                if (user != null)
                {
                    data.profiles.Add(new PublicProfileData()
                    {
                        name = user.PersonaName,
                        picture = user.Account.profile.picture,
                        title = user.Account.profile.title,
                        userId = user.AccountId,
                        displayFlags = user.Account.profile.displayFlags
                    });
                }
            }
            SendResponse(request.ListenerResponse, data);
        }
        private void OnAccountPublic(RequestMessage request)
        {
            GetPublicAccountResponse data = new GetPublicAccountResponse()
            {
                inventories = new List<InventoryData>()
            };

            if (request.ContainsBody)
            {
                GetPublicAccountRequest Request = request.Body.Deserialize<GetPublicAccountRequest>();
                foreach (var item in Request.users)
                {
                    User user = GetUser(item);
                    if (user != null)
                    {
                        data.inventories.Add(user.Account.inventory);
                    }
                }
            }
            SendResponse(request.ListenerResponse, data);
        }

    }
}
