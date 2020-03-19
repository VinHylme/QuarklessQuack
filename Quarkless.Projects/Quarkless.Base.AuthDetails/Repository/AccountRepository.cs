using System;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using Quarkless.Base.Auth.Common.Models;
using Quarkless.Base.Auth.Common.Models.AccountContext;
using Quarkless.Base.Auth.Common.Models.Interfaces;
using Quarkless.Repository.MongoContext;

namespace Quarkless.Base.AuthDetails.Repository
{
	public class AccountRepository : IAccountRepository
	{
		private readonly IMongoCollection<AccountUser> _ctx;
		private const string COLLECTION_NAME = "accountUsers";
		public AccountRepository(IMongoClientContext context)
			=> _ctx = context.AccountDatabase.GetCollection<AccountUser>(COLLECTION_NAME);

		public async Task<AccountUser> GetAccountByUsername(string username)
		{
			var filter = new FilterDefinitionBuilder<AccountUser>().Eq(_=>_.UserName, username);
			try
			{
				var results = await _ctx.FindAsync(filter);
				return results.FirstOrDefault();
			}
			catch(Exception err)
			{
				Console.WriteLine(err);
				return null;
			}
		}
		public async Task<AccountUser> GetAccountById(string id)
		{
			var filter = new FilterDefinitionBuilder<AccountUser>().Eq("_id", id);
			try
			{
				var results = await _ctx.FindAsync(filter);
				return results.FirstOrDefault();
			}
			catch (Exception err)
			{
				Console.WriteLine(err);
				return null;
			}
		}
		public async Task<bool> UpdateAddAccountUserInformation(string id, UserInformationDetail detail)
		{
			try
			{
				if (string.IsNullOrEmpty(detail._id))
				{
					detail._id = ObjectId.GenerateNewId(DateTime.UtcNow).ToString();
					var updatePush = Builders<AccountUser>.Update.Push(_ => _.Details, detail);
					var resultsAdd = await _ctx
						.UpdateOneAsync(new FilterDefinitionBuilder<AccountUser>().Eq(_=>_.UserName, id), updatePush);
					return resultsAdd.IsAcknowledged;
				}

				var filter = Builders<AccountUser>.Filter.And(Builders<AccountUser>.Filter.Eq(_=>_.UserName, id),
					Builders<AccountUser>.Filter.ElemMatch(x => x.Details, f => f._id == detail._id));

				var update = Builders<AccountUser>.Update.Set(model => model.Details[-1], detail);

				var results = await _ctx
					.UpdateOneAsync(filter, update, new UpdateOptions
					{
						IsUpsert = true
					});

				return results.IsAcknowledged;
			}
			catch(Exception err)
			{
				Console.WriteLine(err);
				return false;
			}
		}
	}
}
