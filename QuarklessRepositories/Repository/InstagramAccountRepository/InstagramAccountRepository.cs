using InstagramApiSharp.Classes;
using MongoDB.Bson;
using MongoDB.Driver;
using Quarkless.Extensions;
using QuarklessContexts.Classes.Carriers;
using QuarklessContexts.Models.InstagramAccounts;
using QuarklessRepositories.RepositoryClientManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QuarklessRepositories.InstagramAccountRepository
{
	public class InstagramAccountRepository : IInstagramAccountRepository
	{
		private readonly IRepositoryContext _context;
		public InstagramAccountRepository(IRepositoryContext context)
		{
			_context = context;
		}

		public async Task<ResultCarrier<IEnumerable<InstagramAccountModel>>> GetInstagramAccountsOfUser(string userId, int type)
		{
			ResultCarrier<IEnumerable<InstagramAccountModel>> Result = new ResultCarrier<IEnumerable<InstagramAccountModel>>();
			try
			{
				var builder = Builders<InstagramAccountModel>.Filter;
				var filter = builder.Eq("AccountId",userId) & builder.Eq("Type",type);

				var accounts = await _context.InstagramAccounts.FindAsync(filter);
				Result.IsSuccesful = true;
				Result.Results = accounts.ToList();
				return Result;
			}
			catch (Exception ee)
			{
				Result.Info = new ErrorResponse
				{
					Exception = ee,
					Message = $"Failed to get instagram accounts for user: {userId}, error: {ee.Message}"
				};
				return Result;
			}
		}
		public async Task<ResultCarrier<InstagramAccountModel>> GetInstagramAccount(string accountId, string instagramAccountId)
		{
			ResultCarrier<InstagramAccountModel> Result = new ResultCarrier<InstagramAccountModel>();
			try
			{
				var builders = Builders<InstagramAccountModel>.Filter;

				var filter = builders.Eq("AccountId", accountId) & builders.Eq("_id",ObjectId.Parse(instagramAccountId));
				var accounts = await _context.InstagramAccounts.FindAsync(filter);
				Result.IsSuccesful = true;
				Result.Results = accounts.FirstOrDefault();
				return Result;
			}
			catch (Exception ee)
			{
				Result.Info = new ErrorResponse
				{
					Exception = ee,
					Message = $"Failed to get instagram accounts for user: {accountId} for account {instagramAccountId}, error: {ee.Message}"
				};
				return Result;
			}
		}
		public async Task<InstagramAccountModel> AddInstagramAccount(InstagramAccountModel instagramAccount)
		{
			try
			{
				var exists = await _context.InstagramAccounts.FindAsync(_=>_.Username == instagramAccount.Username);
				if (exists.FirstOrDefault() != null)
				{
					//account already exists
					return null;
				}
				await _context.InstagramAccounts.InsertOneAsync(instagramAccount);
				return instagramAccount;
			}
			catch(Exception ee)
			{
				return null;
			}
		}
		public async Task<long?> PartialUpdateInstagramAccount(string instagramAccountId, InstagramAccountModel instagramAccountModel)
		{
			try { 
				var filter = Builders<InstagramAccountModel>.Filter.Eq("_id", instagramAccountId);
				var updList = new List<UpdateDefinition<InstagramAccountModel>>();

				var updates = Builders<InstagramAccountModel>.Update;
				var IgnoreNullValues = instagramAccountModel.Recreate();

				foreach(var valuesToTake in IgnoreNullValues)
				{
					updList.Add(updates.Set(valuesToTake.Key, valuesToTake.Value));
				}

				var finalUpdateCommand = Builders<InstagramAccountModel>.Update.Combine(updList);
				var result = await _context.InstagramAccounts.UpdateOneAsync(filter, finalUpdateCommand);

				return result.ModifiedCount;
			}
			catch(Exception ee)
			{
				return null;
			}
		}

		public async Task<ResultCarrier<StateData>> GetInstagramAccountStateData(string accountId, string instagramAccountId)
		{
			ResultCarrier<StateData> Result = new ResultCarrier<StateData>();
			try
			{
				var builders = Builders<InstagramAccountModel>.Filter;
				var filter = builders.Eq("AccountId", accountId) & builders.Eq("_id", ObjectId.Parse(instagramAccountId));
				var state = await _context.InstagramAccounts.FindAsync(filter, new FindOptions<InstagramAccountModel> 
				{ 
					AllowPartialResults = true,
					Projection = Builders<InstagramAccountModel>.Projection.Include("State")
				});
				Result.IsSuccesful = true;
				Result.Results = state.ToList().Select(_=>_.State).SingleOrDefault();
				return Result;
			}
			catch (Exception ee)
			{
				Result.Info = new ErrorResponse
				{
					Exception = ee,
					Message = $"Failed to get instagram accounts for user: {accountId} for account {instagramAccountId}, error: {ee.Message}"
				};
				return Result;
			}
		}

		public async Task<IEnumerable<ShortInstagramAccountModel>> GetActiveAgentInstagramAccounts()
		{
			var builders = Builders<InstagramAccountModel>.Filter;
			var filter = builders.Eq(_=>_.AgentState, (int) AgentState.Running) & builders.Eq(_=>_.Type, 0);
			var res = await _context.InstagramAccounts.FindAsync(filter);
			return res.ToList().Select(r => new ShortInstagramAccountModel
			{
				AccountId = r.AccountId,
				AgentState = r.AgentState,
				LastPurgeCycle = r.LastPurgeCycle,
				FollowersCount = r.FollowersCount,
				FollowingCount = r.FollowingCount, 
				Id = r._id,
				TotalPostsCount = r.TotalPostsCount,
				Username = r.Username,
				DateAdded = r.DateAdded,
				SleepTimeRemaining = r.SleepTimeRemaining,
				Email = r.Email,
				PhoneNumber = r.PhoneNumber
			});
		}

		public async Task<IEnumerable<ShortInstagramAccountModel>> GetInstagramAccounts(int type)
		{
			var builders = Builders<InstagramAccountModel>.Filter;
			var filter = builders.Eq(_ => _.Type, type);
			var res = await _context.InstagramAccounts.FindAsync(filter);
			return res.ToList().Select(r => new ShortInstagramAccountModel
			{
				AccountId = r.AccountId,
				AgentState = r.AgentState,
				LastPurgeCycle = r.LastPurgeCycle,
				FollowersCount = r.FollowersCount,
				FollowingCount = r.FollowingCount,
				Id = r._id,
				TotalPostsCount = r.TotalPostsCount,
				Username = r.Username,
				DateAdded = r.DateAdded,
				SleepTimeRemaining = r.SleepTimeRemaining,
				Email = r.Email,
				PhoneNumber = r.PhoneNumber
			});
		}

		//public async Task<ResultBase<InstagramClientAccount>> GetClientAccount(string accountId, string instagramAccountId)
		//{
		//	ResultBase<InstagramClientAccount> resultBase = new ResultBase<InstagramClientAccount>();
		//	PipelineDefinition<InstagramClientAccount, InstagramClientAccount> pipeline = new BsonDocument[]
		//	{
		//		new BsonDocument("$match", new BsonDocument()
		//				.Add("_id", new BsonObjectId(ObjectId.Parse(instagramAccountId)))),
		//		new BsonDocument("$match", new BsonDocument()
		//				.Add("AccountId", accountId)),
		//		new BsonDocument("$lookup", new BsonDocument()
		//				.Add("from", "Profiles")
		//				.Add("localField", "_id")
		//				.Add("foreignField", "InstagramAccountId")
		//				.Add("as", "Profile")),
		//		new BsonDocument("$unwind", new BsonDocument()
		//				.Add("path", "$Profile")),
		//		new BsonDocument("$lookup", new BsonDocument()
		//				.Add("from", "Proxies")
		//				.Add("localField", "_id")
		//				.Add("foreignField", "AssignedTo.InstaId")
		//				.Add("as", "Proxy")),
		//		new BsonDocument("$unwind", new BsonDocument()
		//				.Add("path", "$Proxy")),
		//		new BsonDocument("$project", new BsonDocument()
		//				.Add("_id", 0.0)
		//				.Add("InstagramAccount", new BsonDocument()
		//						.Add("_id", "$_id")
		//						.Add("AccountId", "$AccountId")
		//						.Add("State", "$State")
		//						.Add("Username", "$Username")
		//						.Add("Password", "$Password")
		//						.Add("FollowersCount", "$FollowersCount")
		//						.Add("FollowingCount", "$FollowingCount")
		//						.Add("TotalPostsCount", "$TotalPostsCount")
		//						.Add("TotalLikes", "$TotalLikes")
		//						.Add("Device", "$Device"))
		//				.Add("Profile", "$Profile")
		//				.Add("Proxy", "$Proxy"))
		//	};

		//	var results = await _context.InstagramClientAccount.AggregateAsync(pipeline);
		//	if (results != null)
		//	{
		//		resultBase.Results = results.ToList().FirstOrDefault();
		//		return resultBase;
		//	}
		//	return resultBase;
		//}
	}
}
