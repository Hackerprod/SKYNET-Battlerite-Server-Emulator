using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace SKYNET
{
    public class GameEnvironment
    {
        public GameEnvironment Instance;

        [Server("Default", RequestType.Default, true)]
        public List<string> DefaultURL;

        [Server("Authentication", RequestType.Authentication, true)]
        public List<string> AuthenticationURL;

        [Server("Matchmaking", RequestType.Matchmaking, true)]
        public List<string> MatchmakingURL;

        [Server("MatchSession", RequestType.MatchSession, true)]
        public List<string> MatchSessionURL;

        [Server("Account", RequestType.Account, true)]
        public List<string> AccountURL;

        [Server("Chat", RequestType.Chat, true)]
        public List<string> ChatURL;

        [Server("ChatDomain", RequestType.ChatDomain, true)]
        public List<string> ChatDomain;

        [Server("CustomerSupport", RequestType.CustomerSupport, true)]
        public List<string> CustomerSupportURL;

        [Server("LandingPage", RequestType.LandingPage, false)]
        public List<string> LandingPageDataURL;

        [Server("Shop", RequestType.Shop, false)]
        public List<string> ShopDocumentURL;

        [Server("Settings", RequestType.Settings, false)]
        public List<string> SettingsURL;

        [Server("Social", RequestType.Social, true)]
        public List<string> SocialURL;

        [Server("AccountVanity", RequestType.AccountVanity, true)]
        public List<string> AccountVanityURL;

        [Server("Seasons", RequestType.Seasons, true)]
        public List<string> SeasonsURL;

        [Server("Twitch", RequestType.Twitch, true)]
        public List<string> TwitchURL;

        [Server("LandingPageMetaData", RequestType.LandingPageMetaData, true)]
        public List<string> LandingPageMetaDataURL;

        [Server("LandingPageNews", RequestType.LandingPageNews, true)]
        public List<string> LandingPageNewsURL;

        [Server("Images", RequestType.Images, true)]
        public List<string> ImageURL;

        [Server("Leaderboards", RequestType.Leaderboards, true)]
        public List<string> LeaderboardsURL;

        [Server("Regions", RequestType.Regions, true)]
        public List<string> RegionsURL;

        [Server("Volgo", RequestType.Volgo, false)]
        public List<string> VolgoURL;

        [Server("ItemProperties", RequestType.ItemProperties, true)]
        public List<string> ItemPropertiesURL;

        [Server("NexonPatchData", RequestType.NexonPatchData, false)]
        public List<string> NexonPatchDataUrl;

        [Server("NexonGameCode", RequestType.NexonGameCode, false)]
        public List<string> NexonGameCode;

        [Server("Storage", RequestType.Storage, true)]
        public List<string> StorageUrl;

        [Server("ServerInfo", RequestType.ServerInfo, false)]
        public List<string> ServerInfo;

        [Server("MatchLobby", RequestType.MatchLobby, true)]
        public List<string> MatchLobbyURL;

        private Dictionary<RequestType, List<string>> _Servers;

        public static Dictionary<string, GameEnvironment> _Environments = new Dictionary<string, GameEnvironment>();

        public GameEnvironment()
        {
            Instance = this;
        }
        public bool TryParseEnvironmentServers(List<object> servers)
        {
            int num = 0;
            foreach (FieldInfo fieldInfo in this.GetType().GetFields())
            {
                object[] customAttributes = fieldInfo.GetCustomAttributes(typeof(ServerAttribute), true);

                if (customAttributes != null && customAttributes.Length != 0)
                {
                    ServerAttribute serverAttribute = customAttributes[0] as ServerAttribute;

                    string configurationName = serverAttribute.ConfigurationName;
                    RequestType requestType = serverAttribute.RequestType;

                    foreach (var item in servers)
                    {
                        Dictionary<string, object> list = (Dictionary<string, object>)item;
                        foreach (var item2 in list)
                        {
                            if (item2.Key == configurationName)
                            {
                                List<string> list_ = GetListString((List<object>)item2.Value);
                                fieldInfo.SetValue(this, list_);
                            }
                        }
                        
                    }
                }
            }
            return true;
        }

        private static List<string> GetListString(List<object> value)
        {
            List<string> list_ = new List<string>();
            foreach (var item in value)
            {
                list_.Add(item.ToString());
            }
            return list_;
        }

        public void ReadFromFile(string fileName)
        {
            Dictionary<string, GameEnvironment> dictionary = new Dictionary<string, GameEnvironment>();
            if (File.Exists(fileName))
            {
                Dictionary<string, object> dictionary2 = SJSON.Load(fileName);

                string key = "Environments";
                if (dictionary2.ContainsKey(key))
                {
                    foreach (object obj in (dictionary2[key] as List<object>))
                    {
                        Dictionary<string, object> dictionary3 = (Dictionary<string, object>)obj;
                        if (dictionary3.ContainsKey("Name") && dictionary3.ContainsKey("Servers"))
                        {
                            
                            string text = dictionary3["Name"].ToString();
                            GameEnvironment value;
                            TryParseEnvironmentServers(dictionary3["Servers"] as List<object>);
                        }
                    }
                    if (dictionary.Count <= 0)
                    {
                        //Log.Error("Error in Environment configuration file! No Environments found!", Responsible.khct, LogFilter.Unknown, LogIgnoreMask.None, null, true);
                    }
                }
                else
                {
                    //Log.Error("Error in Environment configuration file! Root object \"Environments\" not found!", Responsible.khct, LogFilter.Environment, LogIgnoreMask.None, null, true);
                }
            }
        }
    }
    public enum RequestType
    {
        Default,
        Authentication,
        Matchmaking,
        Account,
        MatchSession,
        Chat,
        ChatDomain,
        CustomerSupport,
        LandingPage,
        Settings,
        Social,
        Shop,
        AccountVanity,
        Seasons,
        Quest,
        Twitch,
        LandingPageMetaData,
        LandingPageNews,
        Images,
        Leaderboards,
        Regions,
        Volgo,
        ItemProperties,
        NexonGameCode,
        NexonPatchData,
        Storage,
        ServerInfo,
        MatchLobby
    }

}