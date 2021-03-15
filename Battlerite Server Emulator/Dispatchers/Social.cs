using BloodGUI_Binding.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace SKYNET.Dispatchers
{
    class Social : MsgHandler
    {
        public override void AddHandlers(MsgDispatcher dispatcher)
        {
            dispatcher["account/schedules"] = OnAccountSchedulesHandler;
            dispatcher["social/home_timeline"] = OnSocialHomeTimeLine;
            dispatcher["social/trending"] = OnSocialTrending;
            dispatcher["social/new_timeline"] = OnSocialNewTimeLine;
            dispatcher["social/following"] = OnSocialFollowing;
            dispatcher["social/followers"] = OnSocialFollowers;
            dispatcher["social/user_timeline"] = OnSocialUserTimeLine;
            dispatcher["social/views"] = OnSocialViews;
            dispatcher["social/post/delete"] = OnSocialPostDelete;
            dispatcher["social/users"] = OnSocialUsers;

        }

        private void OnSocialUsers(RequestMessage request)
        {
            GetSocialUserDataRequest Request = request.Body.Deserialize<GetSocialUserDataRequest>();
            GetSocialUserDataResponse data = new GetSocialUserDataResponse()
            {
                users = new List<UserSocialProfile>()
            };
            foreach (var userid in Request.users)
            {
                User user = GetUser(userid);
                if (user != null)
                {
                    data.users.Add(user.GetUserSocialProfile());
                }
            }

            SendResponse(request.ListenerResponse, data);
        }

        private void OnSocialPostDelete(RequestMessage request)
        {
            DeletePostRequest Request = request.Body.Deserialize<DeletePostRequest>();
            ulong ModToDelete = Request.contentId;
            // ahora eliminar el post
            DeletePostResponse data = new DeletePostResponse()
            {
                contentId = ModToDelete
            };
            SendResponse(request.ListenerResponse, data);
        }

        private void OnSocialViews(RequestMessage request)
        {
            SocialViewContentResponse data = new SocialViewContentResponse()
            {
                success = true,
                newQuestVersion = 235
            };
            //string json = "{\"success\":true,\"newQuestVersion\":235}";
            //SendResponse(request.ListenerResponse, json);
            SendResponse(request.ListenerResponse, data);        
        }

            private void OnSocialUserTimeLine(RequestMessage request)
        {
            if (request.ContainsBody)
            {
                GetUserTimelineRequest Request = request.Body.Deserialize<GetUserTimelineRequest>();
            }
            GetTimelineResponse data = new GetTimelineResponse()
            {
                entries = new List<TimelineEntry>()
                {
                    new TimelineEntry()
                    {
                        createTime = 1487336949251,
                        userId = 1001,
                        contentId = 832577626047660032,
                        text = "30 sec to ulty",
                        description = "increase watching rate (x2)",
                        numLikes = 1104,
                        numReposts = 313,
                        reposterId = 1002,
                        repostId = 832653117375000576,
                        media = new List<MediaEntry>()
                        {
                            new MediaEntry()
                            {
                                url = "http://10.31.0.2:25000/lolo",
                                type = 1
                            }
                        },
                        liked = false,
                        reposted = false,
                    }
                },
                users = new List<UserProfile>()
                {
                    new UserProfile()
                    {
                        name = "Hackerprod",
                        picture = 55453,
                        title = 503,
                        userId = 1000
                    },
                    new UserProfile()
                    {
                        name = "AtillaLifeson",
                        picture = 34502,
                        title = 503,
                        userId = 1001
                    },
                }
            };
            //string json = "{\"entries\":[{\"createTime\":1516759350823,\"userId\":853518086270230528,\"contentId\":955984122650705920,\"text\":\"acheivementplease\",\"description\":\"\",\"numLikes\":0,\"numReposts\":0,\"media\":[{\"url\":\"https://replays.battlerite.net/39e166cbdbc45f15f6801d7c9952a063cdc99d7875274e38c9b256547b915d68\",\"type\":1}],\"liked\":false,\"reposted\":false},{\"createTime\":1510350790267,\"userId\":780293253999239168,\"contentId\":9291046714710015664,\"text\":\"eat this\",\"description\":\"\",\"numLikes\":0,\"numReposts\":0,\"media\":[{\"url\":\"https://replays.battlerite.net/7383bd5956a4a512d8221bc0c74ad4f2df6fde8b86c66d264bafb177a3e28ba9\",\"type\":1}],\"liked\":false,\"reposted\":false},{\"createTime\":1487299057772,\"userId\":779528393816432640,\"contentId\":832418697665724416,\"text\":\"Operation: Sonic the Hedgehog  #DuHatStrat\",\"description\":\"\",\"numLikes\":3567,\"numReposts\":1263,\"reposterId\":1002,\"repostId\":832653438784520192,\"media\":[{\"url\":\"https://replays.battlerite.net/aaf7f60826110481fa1ffbd85ce3b990d6386289220370108525ee7ec71457d0\",\"type\":1}],\"liked\":false,\"reposted\":false},{\"createTime\":1487336949251,\"userId\":1001,\"contentId\":832577626047660032,\"text\":\"30 sec to ulty\",\"description\":\"increase watching rate (x2)\",\"numLikes\":1104,\"numReposts\":313,\"reposterId\":1002,\"repostId\":832653117375000576,\"media\":[{\"url\":\"https://replays.battlerite.net/41ea6b9f2ce8c556e3c13e1605f4baaf3fce6268348f9f7934ad9b0e39b39875\",\"type\":1}],\"liked\":false,\"reposted\":false},{\"createTime\":1487305513857,\"userId\":11000,\"contentId\":832445776448864256,\"text\":\"Batata Quente Show\",\"description\":\"\",\"numLikes\":702,\"numReposts\":189,\"reposterId\":1002,\"repostId\":832652642437177344,\"media\":[{\"url\":\"https://replays.battlerite.net/3341e3bed3154bcfb22d05035f8e7c996ee8096dd2585cd3c891a25989f3d2c6\",\"type\":1}],\"liked\":false,\"reposted\":false},{\"createTime\":1487235119106,\"userId\":780739168669167616,\"contentId\":832150519463165952,\"text\":\"Simple Geometry\",\"description\":\"\",\"numLikes\":785,\"numReposts\":237,\"reposterId\":1002,\"repostId\":832234759823437824,\"media\":[{\"url\":\"https://replays.battlerite.net/3158cf4ad87a365d2a1efd4572d36ff7725527aa5c7f40f28842d273d90033f9\",\"type\":1}],\"liked\":false,\"reposted\":false},{\"createTime\":1486594368543,\"userId\":79,\"contentId\":829463016813768704,\"text\":\"1hp comeback vs croak\",\"description\":\"\",\"numLikes\":271,\"numReposts\":77,\"reposterId\":1002,\"repostId\":831815827970994176,\"media\":[{\"url\":\"https://replays.battlerite.net/df217c52a9b73f4f9b3c355f9163e829076416b957eb8ddd0a7091e47bc18e30\",\"type\":1}],\"liked\":false,\"reposted\":false},{\"createTime\":1486307495703,\"userId\":142,\"contentId\":828259784913469440,\"text\":\"Varesh damage\",\"description\":\"He dem bois\",\"numLikes\":440,\"numReposts\":126,\"reposterId\":1002,\"repostId\":829226920028811264,\"media\":[{\"url\":\"https://replays.battlerite.net/dc93bf5c9e760679a638e75186418e101693c50ae861ca83abd021bff3ece7b4\",\"type\":1}],\"liked\":false,\"reposted\":false},{\"createTime\":1485441932254,\"userId\":499,\"contentId\":824629348677062656,\"text\":\"When the yoloqueue is just right\",\"description\":\"\",\"numLikes\":901,\"numReposts\":295,\"reposterId\":1002,\"repostId\":824689048605044736,\"media\":[{\"url\":\"https://replays.battlerite.net/879d2287650d5704845c117e3a69264d963947addd86e4db7c0ab7765033a277\",\"type\":1}],\"liked\":false,\"reposted\":false},{\"createTime\":1480563750585,\"userId\":4248,\"contentId\":804168771790057472,\"text\":\"damo online 1v2 \",\"description\":\"\",\"numLikes\":2284,\"numReposts\":842,\"reposterId\":1002,\"repostId\":805217458108645376,\"media\":[{\"url\":\"https://replays.battlerite.net/e9c983b7cdd84aa1d00fffb6a513fac5a45961ed8e3e453f8af439e52605ab4d\",\"type\":1}],\"liked\":false,\"reposted\":false},{\"createTime\":1480788147067,\"userId\":627,\"contentId\":805109958852100096,\"text\":\"Special Delivery! [3s]\",\"description\":\"Good ulti with Croak in 3s.\",\"numLikes\":1444,\"numReposts\":397,\"reposterId\":1002,\"repostId\":805111167012970496,\"media\":[{\"url\":\"https://replays.battlerite.net/efd2daf23e4343ab052bee7537eefa3877e0c93d3b4a381298803582e632be61\",\"type\":1}],\"liked\":false,\"reposted\":false},{\"createTime\":1480357809392,\"userId\":796210373396148224,\"contentId\":803304991820484608,\"text\":\"Bakko ult vs Bakko ult\",\"description\":\"\",\"numLikes\":693,\"numReposts\":212,\"reposterId\":1002,\"repostId\":805078140308299776,\"media\":[{\"url\":\"https://replays.battlerite.net/7dcb84a183ea43e961a954550317a9e29db9d445f796454244d4c9a1396d9f6e\",\"type\":1}],\"liked\":false,\"reposted\":false}],\"users\":[{\"userId\":79,\"name\":\"ViktorY\",\"picture\":39034,\"title\":500},{\"userId\":142,\"name\":\"Kraetyz\",\"picture\":30043,\"title\":31025},{\"userId\":499,\"name\":\"Melanoleuca\",\"picture\":39504,\"title\":31019},{\"userId\":627,\"name\":\"Ikos\",\"picture\":39238,\"title\":500},{\"userId\":1001,\"name\":\"Bo4\",\"picture\":39026,\"title\":31013},{\"userId\":4248,\"name\":\"Damocles\",\"picture\":39507,\"title\":31021},{\"userId\":11000,\"name\":\"Aklo\",\"picture\":34001,\"title\":60130},{\"userId\":1002,\"name\":\"Stunlock\",\"picture\":38002,\"title\":503},{\"userId\":779528393816432640,\"name\":\"MrHuDat\",\"picture\":39273,\"title\":60095},{\"userId\":780293253999239168,\"name\":\"Goldberg\",\"picture\":39038,\"title\":503},{\"userId\":780739168669167616,\"name\":\"pip.\",\"picture\":37021,\"title\":503},{\"userId\":796210373396148224,\"name\":\"ThornBronze5\",\"picture\":39042,\"title\":60234},{\"userId\":853518086270230528,\"name\":\"Mojochy\",\"picture\":39021,\"title\":503}]}";
            //SendResponse(request.ListenerResponse, json);
            SendResponse(request.ListenerResponse, data);

        }

        private void OnSocialFollowers(RequestMessage request)
        {
            GetFollowersRequest Request = request.Body.Deserialize<GetFollowersRequest>();

            UserListResponse data = new UserListResponse()
            {
                users = new List<UserSocialProfile>(),
                nextCursor = 999609960360833025
            };
            if (Request == null)
            {
                SendResponse(request.ListenerResponse, new UserListResponse());
                return;
            }

            User user = GetUser(Request.user_id);
            if (user != null)
            {
                List<ulong> Followers = user.Followers;
                foreach (var userid in Followers)
                {
                    User _user = GetUser(userid);
                    if (_user != null)
                    {
                        data.users.Add(_user.GetUserSocialProfile());
                    }
                }
            }
            SendResponse(request.ListenerResponse, data);
        }

        private void OnSocialFollowing(RequestMessage request)
        {
            GetFollowingRequest Request = request.Body.Deserialize<GetFollowingRequest>();
            UserListResponse data = new UserListResponse()
            {
                users = new List<UserSocialProfile>(),
                nextCursor = 999609960360833025
            };
            if (Request == null)
            {
                SendResponse(request.ListenerResponse, new UserListResponse());
                return;
            }

            User user = GetUser(Request.user_id);
            if (user != null)
            {
                List<ulong> Following = user.Following;
                foreach (var userid in Following)
                {
                    User _user = GetUser(userid);
                    if (_user != null)
                    {
                        data.users.Add(_user.GetUserSocialProfile());
                    }
                }
            }

            //string json = "{\"users\":[{\"userId\":780287103840948224,\"name\":\"frie\",\"title\":503,\"picture\":39074,\"numPosts\":0,\"numFollowing\":1,\"numFollowers\":11,\"following\":true,\"followedBy\":false},{\"userId\":780293253999239168,\"name\":\"Goldberg\",\"title\":503,\"picture\":39038,\"numPosts\":1,\"numFollowing\":3,\"numFollowers\":4,\"following\":true,\"followedBy\":false},{\"userId\":853051650703765504,\"name\":\"The_Road_West\",\"title\":503,\"picture\":39028,\"numPosts\":0,\"numFollowing\":4,\"numFollowers\":3,\"following\":true,\"followedBy\":false},{\"userId\":853518086270230528,\"name\":\"Mojochy\",\"title\":503,\"picture\":39021,\"numPosts\":1,\"numFollowing\":10,\"numFollowers\":12,\"following\":true,\"followedBy\":true},{\"userId\":992243705568178176,\"name\":\"Ensorcel\",\"title\":504,\"picture\":39055,\"numPosts\":0,\"numFollowing\":3,\"numFollowers\":2,\"following\":true,\"followedBy\":false},{\"userId\":999609916379361280,\"name\":\"Quimz\",\"title\":31012,\"picture\":39061,\"numPosts\":0,\"numFollowing\":0,\"numFollowers\":1,\"following\":true,\"followedBy\":false},{\"userId\":999609924579225600,\"name\":\"Hansmeupinside\",\"title\":504,\"picture\":39016,\"numPosts\":0,\"numFollowing\":0,\"numFollowers\":5,\"following\":true,\"followedBy\":false},{\"userId\":999609924994461697,\"name\":\"Kalteira\",\"title\":504,\"picture\":39033,\"numPosts\":0,\"numFollowing\":0,\"numFollowers\":3,\"following\":true,\"followedBy\":false},{\"userId\":999609927624290304,\"name\":\"AsemoHimejima\",\"title\":504,\"picture\":39030,\"numPosts\":0,\"numFollowing\":0,\"numFollowers\":4,\"following\":true,\"followedBy\":false},{\"userId\":999609934238707713,\"name\":\"weapoofilth\",\"title\":504,\"picture\":39041,\"numPosts\":0,\"numFollowing\":0,\"numFollowers\":2,\"following\":true,\"followedBy\":false},{\"userId\":999609935287283713,\"name\":\"Blamdio\",\"title\":504,\"picture\":39031,\"numPosts\":0,\"numFollowing\":0,\"numFollowers\":3,\"following\":true,\"followedBy\":false},{\"userId\":999609938785333249,\"name\":\"theghoctmann\",\"title\":504,\"picture\":39243,\"numPosts\":0,\"numFollowing\":0,\"numFollowers\":3,\"following\":true,\"followedBy\":false},{\"userId\":999609941754900482,\"name\":\"magismaestro\",\"title\":504,\"picture\":39018,\"numPosts\":0,\"numFollowing\":0,\"numFollowers\":2,\"following\":true,\"followedBy\":false},{\"userId\":999609942019141633,\"name\":\"xCtitches\",\"title\":504,\"picture\":39013,\"numPosts\":0,\"numFollowing\":0,\"numFollowers\":2,\"following\":true,\"followedBy\":false},{\"userId\":999609943759777796,\"name\":\"Zaiosa\",\"title\":504,\"picture\":39072,\"numPosts\":0,\"numFollowing\":0,\"numFollowers\":2,\"following\":true,\"followedBy\":false},{\"userId\":999609944233734146,\"name\":\"Tsukume\",\"title\":504,\"picture\":39018,\"numPosts\":0,\"numFollowing\":0,\"numFollowers\":3,\"following\":true,\"followedBy\":false},{\"userId\":999609945731100677,\"name\":\"KorpanRaven\",\"title\":504,\"picture\":39054,\"numPosts\":0,\"numFollowing\":0,\"numFollowers\":4,\"following\":true,\"followedBy\":false},{\"userId\":999609951301136385,\"name\":\"Ebiphysis\",\"title\":504,\"picture\":39031,\"numPosts\":0,\"numFollowing\":0,\"numFollowers\":3,\"following\":true,\"followedBy\":false},{\"userId\":999609960323084289,\"name\":\"usneek\",\"title\":504,\"picture\":37004,\"numPosts\":0,\"numFollowing\":0,\"numFollowers\":2,\"following\":true,\"followedBy\":false},{\"userId\":999609960360833024,\"name\":\"DrCittens\",\"title\":504,\"picture\":39039,\"numPosts\":0,\"numFollowing\":0,\"numFollowers\":3,\"following\":true,\"followedBy\":false}],\"nextCursor\":999609960360833025}";
            //SendResponse(request.ListenerResponse, json);
            SendResponse(request.ListenerResponse, data);
        }

        private void OnSocialNewTimeLine(RequestMessage request)
        {
            GetTimelineResponse data = new GetTimelineResponse()
            {
                entries = new List<TimelineEntry>()
                {
                    new TimelineEntry()
                    {
                        createTime = 1612368989224,
                        userId = 1000,
                        contentId = 1357000011434598400,
                        text = "Este es el texto del Timeline",
                        description = "Esta es la descripcion del Timeline",
                        numLikes = 3567,
                        numReposts = 1263,
                        reposterId = 1001,
                        repostId = 5454,
                        media = new List<MediaEntry>()
                        {
                            new MediaEntry()
                            {
                                url = "http://10.31.0.2:25000/lolo",
                                type = 1
                            }
                        },
                        liked = true,
                        reposted = false,
                    },
                    new TimelineEntry()
                    {
                        createTime = 1612368989224,
                        userId = 1000,
                        contentId = 1357000011434598400,
                        text = "Este es el texto del Timeline",
                        description = "Esta es la descripcion del Timeline",
                        numLikes = 3567,
                        numReposts = 1263,
                        reposterId = 1001,
                        repostId = 5454,
                        media = new List<MediaEntry>()
                        {
                            new MediaEntry()
                            {
                                url = "http://10.31.0.2:25000/lolo",
                                type = 1
                            }
                        },
                        liked = true,
                        reposted = false,
                    },
                },
                users = new List<UserProfile>()
                {
                    new UserProfile()
                    {
                        name = "Hackerprod",
                        picture = 55453,
                        title = 503,
                        userId = 1000
                    },
                    new UserProfile()
                    {
                        name = "AtillaLifeson",
                        picture = 34502,
                        title = 503,
                        userId = 1002
                    },
                }
            };
            string json = "{\"entries\":[{\"createTime\":1612368989224,\"userId\":1268057724030169088,\"contentId\":1357000011434598400,\"text\":\"kk\",\"description\":\"\",\"numLikes\":0,\"numReposts\":0,\"media\":[{\"url\":\"https://replays.battlerite.net/4aa72c403ac011adb0ea673b9c5b711bf756187ebda97a36401ea167bb103dbd\",\"type\":1}],\"liked\":false,\"reposted\":false},{\"createTime\":1612368333113,\"userId\":1346863383999295488,\"contentId\":1356997259505577984,\"text\":\"good play\",\"description\":\"so good play\",\"numLikes\":0,\"numReposts\":0,\"media\":[{\"url\":\"https://replays.battlerite.net/b6e64b5f54dfd2ec521d860aca4b2883719441e7377bf90f2598d9ab5b18ec93\",\"type\":1}],\"liked\":false,\"reposted\":false},{\"createTime\":1612368228746,\"userId\":1275025110771843072,\"contentId\":1356996821758648320,\"text\":\"how i use othersides +33 instaheals to negate true dmg\",\"description\":\"\",\"numLikes\":0,\"numReposts\":0,\"media\":[{\"url\":\"https://replays.battlerite.net/b1ceddb036273e8fff3b7902f72a201800029577410b205587bfc44d379251b5\",\"type\":1}],\"liked\":false,\"reposted\":false},{\"createTime\":1612366185414,\"userId\":1268057724030169088,\"contentId\":1356988251403075584,\"text\":\"ss\",\"description\":\"\",\"numLikes\":0,\"numReposts\":0,\"media\":[{\"url\":\"https://replays.battlerite.net/5379a5f6c059b0887ffa8072cd3dc148b76237781e1e8c95ed1ef0fd61ede7a7\",\"type\":1}],\"liked\":false,\"reposted\":false},{\"createTime\":1612365847382,\"userId\":1355224846660939776,\"contentId\":1356986833594130432,\"text\":\"quest\",\"description\":\"quest\",\"numLikes\":0,\"numReposts\":0,\"media\":[{\"url\":\"https://replays.battlerite.net/d0ec5a7194568dc0512cd0d50607efecd228ab024604c72a9e54ed7ecd6b5f12\",\"type\":1}],\"liked\":false,\"reposted\":false},{\"createTime\":1612364980150,\"userId\":1356696508052942848,\"contentId\":1356983196159483904,\"text\":\"2v1 clutch\",\"description\":\"\",\"numLikes\":0,\"numReposts\":0,\"media\":[{\"url\":\"https://replays.battlerite.net/29098c4f198c50be620c2551a6f894cfed48f8c48270856bb2f35cc5aaa28d86\",\"type\":1}],\"liked\":false,\"reposted\":false},{\"createTime\":1612362317485,\"userId\":1124207903709003776,\"contentId\":1356972028132995072,\"text\":\"ÐÑÑÐ°ÐµÐ¼ÑÑ Ð¿Ð¾Ð´Ð½ÑÑÑ ÐÐ»Ð°ÑÐ¸Ð½Ñ 4 \",\"description\":\"ÐÑÐ¸ÐºÑÐ¸Ñ Ð¸ ÐÐ²Ð¸Ð· Ð² Ð´ÐµÐ»Ðµ\",\"numLikes\":0,\"numReposts\":0,\"media\":[{\"url\":\"https://replays.battlerite.net/f10fc3a61328cca239ddab181619317e1b1f01cf0be4175a4906c42f93d86416\",\"type\":1}],\"liked\":false,\"reposted\":false},{\"createTime\":1612352310923,\"userId\":931542029660491776,\"contentId\":1356930057569972224,\"text\":\"1vs2\",\"description\":\"\",\"numLikes\":0,\"numReposts\":0,\"media\":[{\"url\":\"https://replays.battlerite.net/b2fc527faae4f127a941dad466fc52ebb0763a7e63c27d481d3bd0a10b665645\",\"type\":1}],\"liked\":false,\"reposted\":false},{\"createTime\":1612347933998,\"userId\":1356186406782910464,\"contentId\":1356911699415932928,\"text\":\"111\",\"description\":\"\",\"numLikes\":0,\"numReposts\":0,\"media\":[{\"url\":\"https://replays.battlerite.net/1ba9ebfefa865e7b75ac3ac277718e5951478bff05fe1ce9d5afbd8645b635fc\",\"type\":1}],\"liked\":false,\"reposted\":false},{\"createTime\":1612344639888,\"userId\":877498826309644288,\"contentId\":1356897882917187584,\"text\":\"å¤©ç¼æ2\",\"description\":\"\",\"numLikes\":0,\"numReposts\":0,\"media\":[{\"url\":\"https://replays.battlerite.net/560f03090bec4d98a8379f87e0d535a8791ca805df3f49d9f4d5637248bfb12b\",\"type\":1}],\"liked\":false,\"reposted\":false},{\"createTime\":1612343022992,\"userId\":877498826309644288,\"contentId\":1356891101163827200,\"text\":\"å¤©ç¼æ\",\"description\":\"\",\"numLikes\":0,\"numReposts\":0,\"media\":[{\"url\":\"https://replays.battlerite.net/c9e0e1f4c421a933790c043994cc2c7d5189dd267de8f4e07d345797cd9b010a\",\"type\":1}],\"liked\":false,\"reposted\":false},{\"createTime\":1612340654263,\"userId\":814453739699191808,\"contentId\":1356881165994307584,\"text\":\"Muscle Memory Kicking In\",\"description\":\"\",\"numLikes\":0,\"numReposts\":0,\"media\":[{\"url\":\"https://replays.battlerite.net/e7f8d95f2ef294688f647af3f6795f8832acbb1e7e54a9822225368843f0e188\",\"type\":1}],\"liked\":false,\"reposted\":false}],\"users\":[{\"userId\":814453739699191808,\"name\":\"AtillaLifeson\",\"picture\":34502,\"title\":503},{\"userId\":877498826309644288,\"name\":\"TR4SHBIN\",\"picture\":37043,\"title\":31021},{\"userId\":931542029660491776,\"name\":\"Kallaran\",\"picture\":36046,\"title\":31008},{\"userId\":1124207903709003776,\"name\":\"Qwizer_Pro\",\"picture\":39054,\"title\":504},{\"userId\":1268057724030169088,\"name\":\"åãã¾ãã¦ã\",\"picture\":39019,\"title\":504},{\"userId\":1275025110771843072,\"name\":\"Poloma\",\"picture\":39020,\"title\":31011},{\"userId\":1346863383999295488,\"name\":\"TheMaster023\",\"picture\":39001,\"title\":504},{\"userId\":1355224846660939776,\"name\":\"Zeram\",\"picture\":39076,\"title\":504},{\"userId\":1356186406782910464,\"name\":\"tiyu_ler\",\"picture\":39003,\"title\":504},{\"userId\":1356696508052942848,\"name\":\"Freshdidgeridoo\",\"picture\":39014,\"title\":504}]}";
            //SendResponse(request.ListenerResponse, json);
            SendResponse(request.ListenerResponse, data);
        }

        private void OnSocialTrending(RequestMessage request)
        {
            GetTimelineResponse data = new GetTimelineResponse()
            {
                entries = new List<TimelineEntry>()
                {
                    new TimelineEntry()
                    {
                        createTime = 1487336949251,
                        userId = 1001,
                        contentId = 832577626047660032,
                        text = "30 sec to ulty",
                        description = "increase watching rate (x2)",
                        numLikes = 1104,
                        numReposts = 313,
                        reposterId = 1002,
                        repostId = 832653117375000576,
                        media = new List<MediaEntry>()
                        {
                            new MediaEntry()
                            {
                                url = "http://10.31.0.2:25000/lolo",
                                type = 1
                            }
                        },
                        liked = false,
                        reposted = false,
                    }
                },
                users = new List<UserProfile>()
                {
                    new UserProfile()
                    {
                        name = "Hackerprod",
                        picture = 55453,
                        title = 503,
                        userId = 1000
                    },
                    new UserProfile()
                    {
                        name = "AtillaLifeson",
                        picture = 34502,
                        title = 503,
                        userId = 1001
                    },
                }
            };
            //string json = "{\"entries\":[{\"createTime\":1516759350823,\"userId\":853518086270230528,\"contentId\":955984122650705920,\"text\":\"acheivementplease\",\"description\":\"\",\"numLikes\":0,\"numReposts\":0,\"media\":[{\"url\":\"https://replays.battlerite.net/39e166cbdbc45f15f6801d7c9952a063cdc99d7875274e38c9b256547b915d68\",\"type\":1}],\"liked\":false,\"reposted\":false},{\"createTime\":1510350790267,\"userId\":780293253999239168,\"contentId\":9291046714710015664,\"text\":\"eat this\",\"description\":\"\",\"numLikes\":0,\"numReposts\":0,\"media\":[{\"url\":\"https://replays.battlerite.net/7383bd5956a4a512d8221bc0c74ad4f2df6fde8b86c66d264bafb177a3e28ba9\",\"type\":1}],\"liked\":false,\"reposted\":false},{\"createTime\":1487299057772,\"userId\":779528393816432640,\"contentId\":832418697665724416,\"text\":\"Operation: Sonic the Hedgehog  #DuHatStrat\",\"description\":\"\",\"numLikes\":3567,\"numReposts\":1263,\"reposterId\":1002,\"repostId\":832653438784520192,\"media\":[{\"url\":\"https://replays.battlerite.net/aaf7f60826110481fa1ffbd85ce3b990d6386289220370108525ee7ec71457d0\",\"type\":1}],\"liked\":false,\"reposted\":false},{\"createTime\":1487336949251,\"userId\":1001,\"contentId\":832577626047660032,\"text\":\"30 sec to ulty\",\"description\":\"increase watching rate (x2)\",\"numLikes\":1104,\"numReposts\":313,\"reposterId\":1002,\"repostId\":832653117375000576,\"media\":[{\"url\":\"https://replays.battlerite.net/41ea6b9f2ce8c556e3c13e1605f4baaf3fce6268348f9f7934ad9b0e39b39875\",\"type\":1}],\"liked\":false,\"reposted\":false},{\"createTime\":1487305513857,\"userId\":11000,\"contentId\":832445776448864256,\"text\":\"Batata Quente Show\",\"description\":\"\",\"numLikes\":702,\"numReposts\":189,\"reposterId\":1002,\"repostId\":832652642437177344,\"media\":[{\"url\":\"https://replays.battlerite.net/3341e3bed3154bcfb22d05035f8e7c996ee8096dd2585cd3c891a25989f3d2c6\",\"type\":1}],\"liked\":false,\"reposted\":false},{\"createTime\":1487235119106,\"userId\":780739168669167616,\"contentId\":832150519463165952,\"text\":\"Simple Geometry\",\"description\":\"\",\"numLikes\":785,\"numReposts\":237,\"reposterId\":1002,\"repostId\":832234759823437824,\"media\":[{\"url\":\"https://replays.battlerite.net/3158cf4ad87a365d2a1efd4572d36ff7725527aa5c7f40f28842d273d90033f9\",\"type\":1}],\"liked\":false,\"reposted\":false},{\"createTime\":1486594368543,\"userId\":79,\"contentId\":829463016813768704,\"text\":\"1hp comeback vs croak\",\"description\":\"\",\"numLikes\":271,\"numReposts\":77,\"reposterId\":1002,\"repostId\":831815827970994176,\"media\":[{\"url\":\"https://replays.battlerite.net/df217c52a9b73f4f9b3c355f9163e829076416b957eb8ddd0a7091e47bc18e30\",\"type\":1}],\"liked\":false,\"reposted\":false},{\"createTime\":1486307495703,\"userId\":142,\"contentId\":828259784913469440,\"text\":\"Varesh damage\",\"description\":\"He dem bois\",\"numLikes\":440,\"numReposts\":126,\"reposterId\":1002,\"repostId\":829226920028811264,\"media\":[{\"url\":\"https://replays.battlerite.net/dc93bf5c9e760679a638e75186418e101693c50ae861ca83abd021bff3ece7b4\",\"type\":1}],\"liked\":false,\"reposted\":false},{\"createTime\":1485441932254,\"userId\":499,\"contentId\":824629348677062656,\"text\":\"When the yoloqueue is just right\",\"description\":\"\",\"numLikes\":901,\"numReposts\":295,\"reposterId\":1002,\"repostId\":824689048605044736,\"media\":[{\"url\":\"https://replays.battlerite.net/879d2287650d5704845c117e3a69264d963947addd86e4db7c0ab7765033a277\",\"type\":1}],\"liked\":false,\"reposted\":false},{\"createTime\":1480563750585,\"userId\":4248,\"contentId\":804168771790057472,\"text\":\"damo online 1v2 \",\"description\":\"\",\"numLikes\":2284,\"numReposts\":842,\"reposterId\":1002,\"repostId\":805217458108645376,\"media\":[{\"url\":\"https://replays.battlerite.net/e9c983b7cdd84aa1d00fffb6a513fac5a45961ed8e3e453f8af439e52605ab4d\",\"type\":1}],\"liked\":false,\"reposted\":false},{\"createTime\":1480788147067,\"userId\":627,\"contentId\":805109958852100096,\"text\":\"Special Delivery! [3s]\",\"description\":\"Good ulti with Croak in 3s.\",\"numLikes\":1444,\"numReposts\":397,\"reposterId\":1002,\"repostId\":805111167012970496,\"media\":[{\"url\":\"https://replays.battlerite.net/efd2daf23e4343ab052bee7537eefa3877e0c93d3b4a381298803582e632be61\",\"type\":1}],\"liked\":false,\"reposted\":false},{\"createTime\":1480357809392,\"userId\":796210373396148224,\"contentId\":803304991820484608,\"text\":\"Bakko ult vs Bakko ult\",\"description\":\"\",\"numLikes\":693,\"numReposts\":212,\"reposterId\":1002,\"repostId\":805078140308299776,\"media\":[{\"url\":\"https://replays.battlerite.net/7dcb84a183ea43e961a954550317a9e29db9d445f796454244d4c9a1396d9f6e\",\"type\":1}],\"liked\":false,\"reposted\":false}],\"users\":[{\"userId\":79,\"name\":\"ViktorY\",\"picture\":39034,\"title\":500},{\"userId\":142,\"name\":\"Kraetyz\",\"picture\":30043,\"title\":31025},{\"userId\":499,\"name\":\"Melanoleuca\",\"picture\":39504,\"title\":31019},{\"userId\":627,\"name\":\"Ikos\",\"picture\":39238,\"title\":500},{\"userId\":1001,\"name\":\"Bo4\",\"picture\":39026,\"title\":31013},{\"userId\":4248,\"name\":\"Damocles\",\"picture\":39507,\"title\":31021},{\"userId\":11000,\"name\":\"Aklo\",\"picture\":34001,\"title\":60130},{\"userId\":1002,\"name\":\"Stunlock\",\"picture\":38002,\"title\":503},{\"userId\":779528393816432640,\"name\":\"MrHuDat\",\"picture\":39273,\"title\":60095},{\"userId\":780293253999239168,\"name\":\"Goldberg\",\"picture\":39038,\"title\":503},{\"userId\":780739168669167616,\"name\":\"pip.\",\"picture\":37021,\"title\":503},{\"userId\":796210373396148224,\"name\":\"ThornBronze5\",\"picture\":39042,\"title\":60234},{\"userId\":853518086270230528,\"name\":\"Mojochy\",\"picture\":39021,\"title\":503}]}";
            //SendResponse(request.ListenerResponse, json);
            SendResponse(request.ListenerResponse, data);
        }

        private void OnSocialHomeTimeLine(RequestMessage request)
        {
            GetTimelineResponse data = new GetTimelineResponse()
            {
                entries = new List<TimelineEntry>()
                { 
                    new TimelineEntry()
                    {
                        createTime = 1487336949251,
                        userId = 1001,
                        contentId = 832577626047660032,
                        text = "30 sec to ulty",
                        description = "increase watching rate (x2)",
                        numLikes = 1104,
                        numReposts = 313,
                        reposterId = 1002,
                        repostId = 832653117375000576,
                        media = new List<MediaEntry>()
                        {
                            new MediaEntry()
                            {
                                url = "http://10.31.0.2:25000/lolo",
                                type = 1
                            }
                        },
                        liked = false,
                        reposted = false,
                    }
                },
                users = new List<UserProfile>()
                {
                    new UserProfile()
                    {
                        name = "Hackerprod",
                        picture = 55453,
                        title = 503,
                        userId = 1000
                    },
                    new UserProfile()
                    {
                        name = "AtillaLifeson",
                        picture = 34502,
                        title = 503,
                        userId = 1001
                    },
                }
            };
            //string json = "{\"entries\":[{\"createTime\":1516759350823,\"userId\":853518086270230528,\"contentId\":955984122650705920,\"text\":\"acheivementplease\",\"description\":\"\",\"numLikes\":0,\"numReposts\":0,\"media\":[{\"url\":\"https://replays.battlerite.net/39e166cbdbc45f15f6801d7c9952a063cdc99d7875274e38c9b256547b915d68\",\"type\":1}],\"liked\":false,\"reposted\":false},{\"createTime\":1510350790267,\"userId\":780293253999239168,\"contentId\":9291046714710015664,\"text\":\"eat this\",\"description\":\"\",\"numLikes\":0,\"numReposts\":0,\"media\":[{\"url\":\"https://replays.battlerite.net/7383bd5956a4a512d8221bc0c74ad4f2df6fde8b86c66d264bafb177a3e28ba9\",\"type\":1}],\"liked\":false,\"reposted\":false},{\"createTime\":1487299057772,\"userId\":779528393816432640,\"contentId\":832418697665724416,\"text\":\"Operation: Sonic the Hedgehog  #DuHatStrat\",\"description\":\"\",\"numLikes\":3567,\"numReposts\":1263,\"reposterId\":1002,\"repostId\":832653438784520192,\"media\":[{\"url\":\"https://replays.battlerite.net/aaf7f60826110481fa1ffbd85ce3b990d6386289220370108525ee7ec71457d0\",\"type\":1}],\"liked\":false,\"reposted\":false},{\"createTime\":1487336949251,\"userId\":1001,\"contentId\":832577626047660032,\"text\":\"30 sec to ulty\",\"description\":\"increase watching rate (x2)\",\"numLikes\":1104,\"numReposts\":313,\"reposterId\":1002,\"repostId\":832653117375000576,\"media\":[{\"url\":\"https://replays.battlerite.net/41ea6b9f2ce8c556e3c13e1605f4baaf3fce6268348f9f7934ad9b0e39b39875\",\"type\":1}],\"liked\":false,\"reposted\":false},{\"createTime\":1487305513857,\"userId\":11000,\"contentId\":832445776448864256,\"text\":\"Batata Quente Show\",\"description\":\"\",\"numLikes\":702,\"numReposts\":189,\"reposterId\":1002,\"repostId\":832652642437177344,\"media\":[{\"url\":\"https://replays.battlerite.net/3341e3bed3154bcfb22d05035f8e7c996ee8096dd2585cd3c891a25989f3d2c6\",\"type\":1}],\"liked\":false,\"reposted\":false},{\"createTime\":1487235119106,\"userId\":780739168669167616,\"contentId\":832150519463165952,\"text\":\"Simple Geometry\",\"description\":\"\",\"numLikes\":785,\"numReposts\":237,\"reposterId\":1002,\"repostId\":832234759823437824,\"media\":[{\"url\":\"https://replays.battlerite.net/3158cf4ad87a365d2a1efd4572d36ff7725527aa5c7f40f28842d273d90033f9\",\"type\":1}],\"liked\":false,\"reposted\":false},{\"createTime\":1486594368543,\"userId\":79,\"contentId\":829463016813768704,\"text\":\"1hp comeback vs croak\",\"description\":\"\",\"numLikes\":271,\"numReposts\":77,\"reposterId\":1002,\"repostId\":831815827970994176,\"media\":[{\"url\":\"https://replays.battlerite.net/df217c52a9b73f4f9b3c355f9163e829076416b957eb8ddd0a7091e47bc18e30\",\"type\":1}],\"liked\":false,\"reposted\":false},{\"createTime\":1486307495703,\"userId\":142,\"contentId\":828259784913469440,\"text\":\"Varesh damage\",\"description\":\"He dem bois\",\"numLikes\":440,\"numReposts\":126,\"reposterId\":1002,\"repostId\":829226920028811264,\"media\":[{\"url\":\"https://replays.battlerite.net/dc93bf5c9e760679a638e75186418e101693c50ae861ca83abd021bff3ece7b4\",\"type\":1}],\"liked\":false,\"reposted\":false},{\"createTime\":1485441932254,\"userId\":499,\"contentId\":824629348677062656,\"text\":\"When the yoloqueue is just right\",\"description\":\"\",\"numLikes\":901,\"numReposts\":295,\"reposterId\":1002,\"repostId\":824689048605044736,\"media\":[{\"url\":\"https://replays.battlerite.net/879d2287650d5704845c117e3a69264d963947addd86e4db7c0ab7765033a277\",\"type\":1}],\"liked\":false,\"reposted\":false},{\"createTime\":1480563750585,\"userId\":4248,\"contentId\":804168771790057472,\"text\":\"damo online 1v2 \",\"description\":\"\",\"numLikes\":2284,\"numReposts\":842,\"reposterId\":1002,\"repostId\":805217458108645376,\"media\":[{\"url\":\"https://replays.battlerite.net/e9c983b7cdd84aa1d00fffb6a513fac5a45961ed8e3e453f8af439e52605ab4d\",\"type\":1}],\"liked\":false,\"reposted\":false},{\"createTime\":1480788147067,\"userId\":627,\"contentId\":805109958852100096,\"text\":\"Special Delivery! [3s]\",\"description\":\"Good ulti with Croak in 3s.\",\"numLikes\":1444,\"numReposts\":397,\"reposterId\":1002,\"repostId\":805111167012970496,\"media\":[{\"url\":\"https://replays.battlerite.net/efd2daf23e4343ab052bee7537eefa3877e0c93d3b4a381298803582e632be61\",\"type\":1}],\"liked\":false,\"reposted\":false},{\"createTime\":1480357809392,\"userId\":796210373396148224,\"contentId\":803304991820484608,\"text\":\"Bakko ult vs Bakko ult\",\"description\":\"\",\"numLikes\":693,\"numReposts\":212,\"reposterId\":1002,\"repostId\":805078140308299776,\"media\":[{\"url\":\"https://replays.battlerite.net/7dcb84a183ea43e961a954550317a9e29db9d445f796454244d4c9a1396d9f6e\",\"type\":1}],\"liked\":false,\"reposted\":false}],\"users\":[{\"userId\":79,\"name\":\"ViktorY\",\"picture\":39034,\"title\":500},{\"userId\":142,\"name\":\"Kraetyz\",\"picture\":30043,\"title\":31025},{\"userId\":499,\"name\":\"Melanoleuca\",\"picture\":39504,\"title\":31019},{\"userId\":627,\"name\":\"Ikos\",\"picture\":39238,\"title\":500},{\"userId\":1001,\"name\":\"Bo4\",\"picture\":39026,\"title\":31013},{\"userId\":4248,\"name\":\"Damocles\",\"picture\":39507,\"title\":31021},{\"userId\":11000,\"name\":\"Aklo\",\"picture\":34001,\"title\":60130},{\"userId\":1002,\"name\":\"Stunlock\",\"picture\":38002,\"title\":503},{\"userId\":779528393816432640,\"name\":\"MrHuDat\",\"picture\":39273,\"title\":60095},{\"userId\":780293253999239168,\"name\":\"Goldberg\",\"picture\":39038,\"title\":503},{\"userId\":780739168669167616,\"name\":\"pip.\",\"picture\":37021,\"title\":503},{\"userId\":796210373396148224,\"name\":\"ThornBronze5\",\"picture\":39042,\"title\":60234},{\"userId\":853518086270230528,\"name\":\"Mojochy\",\"picture\":39021,\"title\":503}]}";
            //SendResponse(request.ListenerResponse, json);
            SendResponse(request.ListenerResponse, data);
        }

        private void OnAccountSchedulesHandler(RequestMessage request)
        {
            GetSchedulesResponse data = new GetSchedulesResponse()
            {
                schedules = new List<GetSchedulesResponse.PublicScheduleData>()
            };
            string json = "{\"schedules\":[{\"name\":\"battlePassSeason03Week01\",\"id\":5021},{\"name\":\"battlePassSeason03Week02\",\"id\":5022},{\"name\":\"battlePassSeason03Week03\",\"id\":5023},{\"name\":\"brawlActive\",\"id\":1003,\"startTimeInSeconds\":-651347901,\"endTimeInSeconds\":826447426},{\"name\":\"matchmakingInactive\",\"id\":998}]}";
            SendResponse(request.ListenerResponse, json);
            //SendResponse(request.ListenerResponse, data);
        }
    }
}
