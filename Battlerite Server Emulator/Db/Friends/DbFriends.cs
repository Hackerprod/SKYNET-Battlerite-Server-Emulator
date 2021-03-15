using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.IO;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using System.Threading.Tasks;

namespace SKYNET.Db
{
    public class DbFriends
    {
        //ILog ilog = LogManager.GetLogger("FRIENDS");
        public event EventHandler<Friend> OnNewAccount;
        public event EventHandler<Friend> OnAccountDeleted;
        public event EventHandler<Friend> OnAccountBaned;
        public event EventHandler<Friend> OnAccountUnbanned;

        private readonly MongoDbCollection<Friend> DBFriends;
        DbManager dbManager;
        public DbFriends(DbManager dbManager)
        {
            this.DBFriends = new MongoDbCollection<Friend>(dbManager, "SKYNET_friends");
            this.dbManager = dbManager;
        }



        public bool Create(ulong steamId)
        {
            Friend friend = new Friend()
            {
                SteamId = steamId
            };

            if (steamId == 0)
            {
                return false;
            }
            if (this.DBFriends.Collection.CountDocuments((Friend usr) => usr.SteamId == steamId, null, default(CancellationToken)) != 0L)
            {
                return false;
            }
            this.DBFriends.Collection.InsertOne(friend, null, default(CancellationToken));
            return true;
        }


        public Friend GetBySteamId(ulong steamId)
        {
            return this.DBFriends.Collection.Find((Friend usr) => usr.SteamId == steamId, null).FirstOrDefault(default(CancellationToken));
        }
        public List<uint> GetFriends(ulong steamId)
        {
            Friend user = this.DBFriends.Collection.Find((Friend usr) => usr.SteamId == steamId, null).FirstOrDefault(default(CancellationToken));
            return user == null ? new List<uint>() : user.friends;
        }
        public bool IsFriend(ulong steamId, ulong requestedfriend)
        {
            Friend user = this.DBFriends.Collection.Find((Friend usr) => usr.SteamId == steamId, null).FirstOrDefault(default(CancellationToken));
            return user == null ? false : user.friends.Contains(requestedfriend.GetAccountId());
        }

        public Friend GetByAccountId(uint accountId)
        {
            return this.DBFriends.Collection.Find((Friend usr) => usr.SteamId.GetAccountId() == accountId, null).FirstOrDefault(default(CancellationToken));
        }

        public bool TryGetFriends(ulong steamId, out Friend friend)
        {
            friend = GetBySteamId(steamId);
            return friend != null;
        }

        public void AddNewFriend(ulong steamId, ulong targetFriend)
        {
            if (!ExistInDB(steamId)) Create(steamId);

            if (TryGetFriends(steamId, out Friend friend))
            {
                friend.friends.Add(targetFriend.GetAccountId());
                this.DBFriends.Collection.FindOneAndUpdate((Friend f) => f.SteamId == steamId, this.DBFriends.Ub.Set<List<uint>>((Friend f) => f.friends, friend.friends), null, default(CancellationToken));
            }
        }

        private bool ExistInDB(ulong steamId)
        {
            return (this.DBFriends.Collection.CountDocuments((Friend usr) => usr.SteamId == steamId, null, default(CancellationToken)) != 0L);
        }

        public void ExportJson(string outputFileName)
        {
            using (var streamWriter = new StreamWriter(outputFileName))
            {
                DBFriends.Collection.Find(new BsonDocument())
                    .ForEachAsync((document) =>
                    {
                        using (var stringWriter = new StringWriter())
                        using (var jsonWriter = new JsonWriter(stringWriter))
                        {
                            var context = BsonSerializationContext.CreateRoot(jsonWriter);
                            DBFriends.Collection.DocumentSerializer.Serialize(context, document);
                            var line = stringWriter.ToString();
                            streamWriter.WriteLineAsync(line);
                        }
                    });
            }
        }
        public bool ImportJson(string inputFileName)
        {
            try
            {
                using (var streamReader = new StreamReader(inputFileName))
                {
                    string line;
                    while ((line = streamReader.ReadLine()) != null)
                    {
                        using (var jsonReader = new JsonReader(line))
                        {
                            var context = BsonDeserializationContext.CreateRoot(jsonReader);
                            var document = DBFriends.Collection.DocumentSerializer.Deserialize(context);
                            DBFriends.Collection.InsertOneAsync(document);
                        }
                    }
                }
            }
            catch
            {
                return false;
            }
            return true;
        }

        public void Test()
        {
            dbManager.ExportDatabase();
        }
        public static async Task WriteCollectionToFile(IMongoDatabase database, string collectionName, string fileName)
        {
            var collection = database.GetCollection<RawBsonDocument>(collectionName);

            // Make sure the file is empty before we start writing to it
            File.WriteAllText(fileName, string.Empty);

            using (var cursor = await collection.FindAsync(new BsonDocument()))
            {
                while (await cursor.MoveNextAsync())
                {
                    var batch = cursor.Current;
                    foreach (var document in batch)
                    {
                        File.AppendAllLines(fileName, new[] { document.ToString() });
                    }
                }
            }
        }
        public static async Task LoadCollectionFromFile(IMongoDatabase database, string collectionName, string fileName)
        {
            using (FileStream fs = File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (BufferedStream bs = new BufferedStream(fs))
            using (StreamReader sr = new StreamReader(bs))
            {
                var collection = database.GetCollection<BsonDocument>(collectionName);

                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    await collection.InsertOneAsync(BsonDocument.Parse(line));
                }
            }
        }

        public bool AreFriends(ulong sourceSteamId, ulong targetSteamId)
        {
            return GetFriends(sourceSteamId).Contains(targetSteamId.GetAccountId());
        }
    }
}
