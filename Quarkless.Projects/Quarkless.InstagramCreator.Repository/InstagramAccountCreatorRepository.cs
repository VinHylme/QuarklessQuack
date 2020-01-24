using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using Quarkless.InstagramCreator.Models;
using Quarkless.Repository.MongoContext;

namespace Quarkless.InstagramCreator.Repository
{
	public class InstagramAccountCreatorRepository : IInstagramAccountCreatorRepository
	{
		private readonly IMongoCollection<InstagramAccount> _ctx;
		private const string COLLECTION_NAME = "InstagramAccounts";
		public InstagramAccountCreatorRepository(IMongoClientContext context)
		{
			_ctx = context.CreatorDatabase.GetCollection<InstagramAccount>(COLLECTION_NAME);
		}

		public async Task AddAccount(InstagramAccount account)
		{
			try
			{
				await _ctx.InsertOneAsync(account);
			}
			catch (Exception err)
			{
				Console.WriteLine(err.Message);
			}
		}
		public async Task<List<InstagramAccount>> GetAllInstagramAccounts()
		{
			return (await _ctx.FindAsync(_ => true))?.ToList();
		}
		public async Task<List<InstagramAccount>> GetAllNonVerifiedInstagramAccounts()
		{
			var filter = Builders<InstagramAccount>.Filter.Eq(_ => _.NeedsVerifying, true);
			return (await _ctx.FindAsync(filter)).ToList();
		}
		public async Task<InstagramAccount> GetInstagramAccount(string id)
		{
			try
			{
				var builders = Builders<InstagramAccount>.Filter;
				var filter = builders.Eq("_id", new ObjectId(id));
				var results = await _ctx.FindAsync(filter);
				return results.First();
			}
			catch (Exception err)
			{
				Console.WriteLine(err.Message);
				return null;
			}
		}
	}
}
