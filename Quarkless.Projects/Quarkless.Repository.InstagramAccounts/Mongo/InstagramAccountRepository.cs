using MongoDB.Driver;
using Quarkless.Models.Common.Models.Carriers;
using Quarkless.Models.InstagramAccounts;
using Quarkless.Repository.MongoContext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InstagramApiSharp.Classes;
using MongoDB.Bson;
using Quarkless.Models.Common.Extensions;
using Quarkless.Models.InstagramAccounts.Enums;
using Quarkless.Models.InstagramAccounts.Interfaces;

namespace Quarkless.Repository.InstagramAccounts.Mongo
{
	public class InstagramAccountRepository : IInstagramAccountRepository
	{
		private readonly IMongoCollection<InstagramAccountModel> _ctx;
		private const string COLLECTION_NAME = "InstagramAccounts";
		public InstagramAccountRepository(IMongoClientContext context)
			=> _ctx = context.AccountDatabase.GetCollection<InstagramAccountModel>(COLLECTION_NAME);

		public async Task<ResultCarrier<IEnumerable<InstagramAccountModel>>> GetInstagramAccountsOfUser(string userId, int type)
		{
			var result = new ResultCarrier<IEnumerable<InstagramAccountModel>>();
			try
			{
				var builder = Builders<InstagramAccountModel>.Filter;
				var filter = builder.Eq("AccountId", userId) & builder.Eq("Type", type);

				var accounts = await _ctx.FindAsync(filter);
				result.IsSuccessful = true;
				result.Results = accounts.ToList();
				return result;
			}
			catch (Exception ee)
			{
				result.Info = new ErrorResponse
				{
					Exception = ee,
					Message = $"Failed to get instagram accounts for user: {userId}, error: {ee.Message}"
				};
				return result;
			}
		}
		public async Task<ResultCarrier<InstagramAccountModel>> GetInstagramAccount(string accountId, string instagramAccountId)
		{
			var result = new ResultCarrier<InstagramAccountModel>();
			try
			{
				var builders = Builders<InstagramAccountModel>.Filter;

				var filter = builders.Eq("AccountId", accountId) & builders.Eq("_id", ObjectId.Parse(instagramAccountId));
				var accounts = await _ctx.FindAsync(filter);
				result.IsSuccessful = true;
				result.Results = accounts.FirstOrDefault();
				return result;
			}
			catch (Exception ee)
			{
				result.Info = new ErrorResponse
				{
					Exception = ee,
					Message = $"Failed to get instagram accounts for user: {accountId} for account {instagramAccountId}, error: {ee.Message}"
				};
				return result;
			}
		}
		public async Task<InstagramAccountModel> AddInstagramAccount(InstagramAccountModel instagramAccount)
		{
			try
			{
				var exists = await _ctx.FindAsync(_ => _.Username == instagramAccount.Username);
				if (exists.FirstOrDefault() != null)
				{
					//account already exists
					return null;
				}
				await _ctx.InsertOneAsync(instagramAccount);
				return instagramAccount;
			}
			catch
			{
				return null;
			}
		}
		public async Task<long?> PartialUpdateInstagramAccount(string instagramAccountId, InstagramAccountModel instagramAccountModel)
		{
			try
			{
				var filter = Builders<InstagramAccountModel>.Filter.Eq("_id", instagramAccountId);

				var updates = Builders<InstagramAccountModel>.Update;
				var ignoreNullValues = instagramAccountModel.Recreate();

				var updList = ignoreNullValues.Select(valuesToTake => updates.Set(valuesToTake.Key, valuesToTake.Value)).ToList();

				var finalUpdateCommand = Builders<InstagramAccountModel>.Update.Combine(updList);
				var result = await _ctx.UpdateOneAsync(filter, finalUpdateCommand);

				return result.ModifiedCount;
			}
			catch (Exception err)
			{
				Console.WriteLine(err);
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
				var state = await _ctx.FindAsync(filter, new FindOptions<InstagramAccountModel>
				{
					AllowPartialResults = true,
					Projection = Builders<InstagramAccountModel>.Projection.Include("State")
				});
				Result.IsSuccessful = true;
				Result.Results = state.ToList().Select(_ => _.State).SingleOrDefault();
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
		public async Task EmptyChallengeInfo(string instagramAccountId)
		{
			var filter = Builders<InstagramAccountModel>.Filter.Eq("_id", instagramAccountId);
			var updateDef = Builders<InstagramAccountModel>.Update.Set(o => o.ChallengeInfo, null);
			await _ctx.UpdateOneAsync(filter, updateDef);
		}
		public async Task<IEnumerable<ShortInstagramAccountModel>> GetActiveAgentInstagramAccounts(int actionExType = -1)
		{
			IAsyncCursor<InstagramAccountModel> results;
			if (actionExType == 1)
				results = await _ctx.FindAsync(_ => (_.AgentState == (int)AgentState.Running
							|| _.AgentState == (int)AgentState.Sleeping) && _.Type == 0);
			else
			{
				results = await _ctx.FindAsync(_ => _.AgentState == (int)AgentState.Running && _.Type == 0);
			}
			return results.ToList().Select(r => new ShortInstagramAccountModel
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
				PhoneNumber = r.PhoneNumber,
				FullName = r.FullName,
				ProfilePicture = r.ProfilePicture,
				UserBiography = r.UserBiography,
				UserLimits = r.UserLimits,
				IsBusiness = r.IsBusiness,
				Location = r.Location,
				Type = r.Type,
				ChallengeInfo = r.ChallengeInfo,
				UserId = r.UserId
			});
		}
		public async Task<IEnumerable<ShortInstagramAccountModel>> GetInstagramAccounts(int type)
		{
			var builders = Builders<InstagramAccountModel>.Filter;
			var filter = builders.Eq(_ => _.Type, type);
			var res = await _ctx.FindAsync(filter);
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
				PhoneNumber = r.PhoneNumber,
				FullName = r.FullName,
				ProfilePicture = r.ProfilePicture,
				UserBiography = r.UserBiography,
				UserLimits = r.UserLimits,
				IsBusiness = r.IsBusiness,
				Location = r.Location,
				Type = r.Type,
				ChallengeInfo = r.ChallengeInfo,
				UserId = r.UserId
			});
		}
		public async Task<IEnumerable<InstagramAccountModel>> GetInstagramAccountsFull(int type)
		{
			var builders = Builders<InstagramAccountModel>.Filter;
			var filter = builders.Eq(_ => _.Type, type);
			var res = await _ctx.FindAsync(filter);
			return res.ToList();
		}
	}
}
