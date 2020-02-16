using System;
using MongoDB.Driver;
using Quarkless.Repository.MongoContext.Models;

namespace Quarkless.Repository.MongoContext
{
	public class MongoClientContext : IMongoClientContext
	{
		public MongoClientContext(MongoOptions options)
		{
			if(options == null)
				throw new Exception("Options Cannot be null");

			if(string.IsNullOrEmpty(options.ConnectionString) || string.IsNullOrEmpty(options.AccountDatabase))
				throw new Exception("Please provide the database name");

			var client = new MongoClient(options.ConnectionString);

			AccountDatabase = client.GetDatabase(options.AccountDatabase);

			if(!string.IsNullOrEmpty(options.AccountCreatorDatabase))
				CreatorDatabase = client.GetDatabase(options.AccountCreatorDatabase);

			if(!string.IsNullOrEmpty(options.ControlDatabase))
				ControlDatabase = client.GetDatabase(options.ControlDatabase);

			if(!string.IsNullOrEmpty(options.ContentDatabase))
				ContentDatabase = client.GetDatabase(options.ContentDatabase);

			if(!string.IsNullOrEmpty(options.StatisticsDatabase))
				StatisticsDatabase = client.GetDatabase(options.StatisticsDatabase);
		}

		public IMongoDatabase CreatorDatabase { get; }
		public IMongoDatabase AccountDatabase { get; }
		public IMongoDatabase ControlDatabase { get; }
		public IMongoDatabase ContentDatabase { get; }
		public IMongoDatabase StatisticsDatabase { get; }
	}
}
