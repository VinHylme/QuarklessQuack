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

			if(string.IsNullOrEmpty(options.AccountDatabase) || string.IsNullOrEmpty(options.ConnectionString))
				throw new Exception("Please provide the database name");

			var client = new MongoClient(options.ConnectionString);

			CreatorDatabase = client.GetDatabase(options.AccountCreatorDatabase);
			AccountDatabase = client.GetDatabase(options.AccountDatabase);
			ControlDatabase = client.GetDatabase(options.ControlDatabase);
			ContentDatabase = client.GetDatabase(options.ContentDatabase);
			SchedulerDatabase = client.GetDatabase(options.SchedulerDatabase);
		}

		public IMongoDatabase CreatorDatabase { get; }
		public IMongoDatabase AccountDatabase { get; }
		public IMongoDatabase ControlDatabase { get; }
		public IMongoDatabase ContentDatabase { get; }
		public IMongoDatabase SchedulerDatabase { get; }
	}
}
