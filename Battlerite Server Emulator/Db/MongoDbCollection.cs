using MongoDB.Driver;
using System.Runtime.CompilerServices;

namespace SKYNET.Db
{
	public class MongoDbCollection<T> where T : class
	{

		public IMongoCollection<T> Collection
		{
			get;
		}

		public FilterDefinitionBuilder<T> Filter
		{
			get;
		}

		public UpdateDefinitionBuilder<T> Ub
		{
			get;
		}

		public MongoDbCollection(DbManager dba, string collection)
		{
            Collection = dba.Database.GetCollection<T>(collection);
            Filter = new FilterDefinitionBuilder<T>();
            Ub = new UpdateDefinitionBuilder<T>();
		}

		public MongoDbCollection(IMongoDatabase dba, string collection)
		{
            Collection = dba.GetCollection<T>(collection);
            Filter = new FilterDefinitionBuilder<T>();
            Ub = new UpdateDefinitionBuilder<T>();
		}

		public void CreateIndex(IndexKeysDefinition<T> def)
		{
			CreateIndexModel<T> model = new CreateIndexModel<T>(def);
			Collection.Indexes.CreateOne(model);
		}
	}
}
