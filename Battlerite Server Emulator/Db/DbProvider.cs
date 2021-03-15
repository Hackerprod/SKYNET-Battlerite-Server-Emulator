using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace SKYNET.Db
{
	public class DbProvider
	{
        public DbProvider(string host, int port)
        {
            Host = host;
            Port = port;
        }
        public string Host
        {
            get;
            set;
        }

        public int Port
        {
            get;
            set;
        }
        public bool Ready
		{
			get;
			private set;
		}

		public MongoClient Client
		{
			get;
			private set;
		}

		public bool Initialize()
		{
			Ready = InitializeDatabase();
			return Ready;
		}

		private bool InitializeDatabase()
		{
			try
			{
				List<MongoServerAddress> list = new List<MongoServerAddress>();
                list.Add(new MongoServerAddress(Host, Port));
                MongoUrlBuilder mongoUrlBuilder = new MongoUrlBuilder
                {
                    ConnectionMode = ConnectionMode.Standalone,
                    Servers = list,
                    DatabaseName = "SKYNET_Battlerite",
                    ReadPreference = ReadPreference.Primary,
					ConnectTimeout = TimeSpan.FromSeconds(3.0),
					SocketTimeout = TimeSpan.FromSeconds(30),
					ServerSelectionTimeout = TimeSpan.FromSeconds(3.0)
				};
				Client = new MongoClient(mongoUrlBuilder.ToMongoUrl());
				Client.GetDatabase(mongoUrlBuilder.DatabaseName).ListCollections();
                return true;
			}
			catch (Exception)
			{
				return false;
			}
		}
	}
}
