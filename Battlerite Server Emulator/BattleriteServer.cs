using MongoDB.Driver;
using Newtonsoft.Json;
using SKYNET.Db;
using SKYNET.Dispatchers;
using SKYNET.Managements;
using StunShared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using WebSocketSharp.Server;

namespace SKYNET
{
    public class BattleriteServer
    {
        public static ulong SteamID;
        public static MsgDispatcher MsgDispatcher;
        internal static ILog ilog;
        private DbProvider dbProvider;

        public static ShopDocument ShopDocument { get; set; }
        public static Client_StackableData StackableData { get; set; }
        public static LobbySystem Lobbies { get; set; }
        public static DbManager DbManager { get; set; }
        public IMongoDatabase Database { get; set; }

        internal FileHandler fileHandler;

        public static BattleriteServer Instance { get; set; }
        public static ConnectionsManager Connections { get; internal set; }

        public BattleriteServer(ILog _ilog)
        {
            ilog = _ilog;
            Instance = this;
            MsgDispatcher = new MsgDispatcher();
            SteamID = 76561197960266728;

            Lobbies = new LobbySystem();
        }

        public void Initialize()
        {
           
            MsgDispatcher.AddHandler(new Account());
            MsgDispatcher.AddHandler(new Authentication());
            MsgDispatcher.AddHandler(new Chat());
            MsgDispatcher.AddHandler(new Matches());
            MsgDispatcher.AddHandler(new MatchSession());
            MsgDispatcher.AddHandler(new RankingSeasons());
            MsgDispatcher.AddHandler(new Social());
            MsgDispatcher.AddHandler(new Storage());
            MsgDispatcher.AddHandler(new Twitch());
            MsgDispatcher.AddHandler(new MatchLobby());
            MsgDispatcher.AddHandler(new Leaderboards());
            MsgDispatcher.AddHandler(new Matchmaking());
            

            try
            {
                Socket sockets = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                sockets.Connect("127.0.0.1", 25002);
            }
            catch
            {
                ilog.Warn(("Error sending ping request to MongoDB on " + "127.0.0.1" + ":" + 25002));
            }

            dbProvider = new DbProvider("127.0.0.1", 25002);
            ilog.Info((object)("Testing database connection " + "127.0.0.1" + ":" + 25002));

            if (!dbProvider.Initialize())
            {
                ilog.Error("Error connecting to database host.");
                ilog.Info("Please verify database status and try again.");
                return;
            }
            ilog.Info("Database connection tested successfully.");

            DbManager = new DbManager(dbProvider);
            this.Database = DbManager.Database;

            fileHandler = new FileHandler();

            Connections = new ConnectionsManager();
            Connections.Start();

            //ShopDocument
            string shopDocument = Path.Combine("Data", "Files", "JSON", "ShopDocument.def.json");
            if (File.Exists(shopDocument))
            {
                string JSON = File.ReadAllText(shopDocument);
                ShopDocument =  JsonConvert.DeserializeObject<ShopDocument>(JSON);
            }

            //StackableData
            string stackableData = Path.Combine("Data", "Files", "JSON", "Stackables.def.json");
            if (File.Exists(stackableData))
            {
                string JSON = File.ReadAllText(stackableData);
                StackableData = JsonConvert.DeserializeObject<Client_StackableData>(JSON);
            }


        }
        public bool Stop()
        {
            //wApi.Stop();
            //fileServer.Stop();
            return true;
        }
    }
}
