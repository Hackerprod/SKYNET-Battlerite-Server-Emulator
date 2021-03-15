using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.IO;
using MongoDB.Bson;
using System.Threading;

namespace SKYNET.Db
{
    public class DbManager
    {
        public IMongoDatabase Database
        {
            get;
        }

        public DbProvider DbProvider
        {
            get;
        }
        
        public DbUsers Users
        {
            get;
        }
        public DbTeams Teams
        {
            get;
        }
        public DbFriends Friends
        {
            get;
        }
        
        public DbNotifications Notifications
        {
            get;
        }

        public DbManager(DbProvider dbProvider)
        {
            DbProvider = dbProvider;
            Database = dbProvider.Client.GetDatabase("SKYNET_Battlerite");

            SetIgnoreConvention();

            Users = new DbUsers(this);
            Notifications = new DbNotifications(this);
            Friends = new DbFriends(this);
            Teams = new DbTeams(this);
            
        }

        private void SetIgnoreConvention()
        {
            ConventionPack conventionPack = new ConventionPack
            {
                new MongoIngoreConvention(),
                new IgnoreExtraElementsConvention(ignoreExtraElements: true),
                new UnsignedConventions()
            };
            conventionPack.AddClassMapConvention("IgnoreExtraElements", delegate (BsonClassMap map)
            {
                map.SetIgnoreExtraElements(ignoreExtraElements: true);
            });
            ConventionRegistry.Register("Ignores", conventionPack, (Type t) => true);

        }
        public async Task WriteCollectionToFile(string collectionName, string fileName)
        {
            var collection = Database.GetCollection<RawBsonDocument>(collectionName);
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
        public async Task LoadCollectionFromFile(string collectionName, string fileName)
        {
            using (FileStream fs = File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (BufferedStream bs = new BufferedStream(fs))
            using (StreamReader sr = new StreamReader(bs))
            {
                var collection = Database.GetCollection<BsonDocument>(collectionName);

                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    await collection.InsertOneAsync(BsonDocument.Parse(line));
                }
            }
        }

        public bool ExportDatabase()
        {
            try
            {
                var collections = Database.ListCollections().ToList();
                for (int i = 0; i < collections.Count; i++)
                {
                    string collection = collections[i].GetElement(0).Value.ToString();
                    string filepath = Path.Combine("Data", "Exported Database", collection + ".json");

                    Task.Factory.StartNew(() => WriteCollectionToFile(collection, filepath));
                }
                return true;
                //(new Thread(() => 
                //{
                //})).Start();
            }
            catch (Exception)
            {
                return false;
            }
        }
        public bool ImportDatabase()
        {
            bool result = false;
            try
            {
                string path = Path.Combine("Data", "Exported Database");
                string[] array = Directory.GetFiles(path);

                if (array.Length == 0) result = false;

                for (int i = 0; i < array.Length; i++)
                {
                    string file = array[i];
                    string collection = Path.GetFileNameWithoutExtension(file);
                    Task.Factory.StartNew(() => LoadCollectionFromFile(collection, file));
                }

                result = true;
            }
            catch (Exception)
            {
                result = false;
            }
            return result;
        }


    }
}
