using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using Quarkless.Models.Common.Extensions;
using Quarkless.Models.Profile;
using Quarkless.Models.Profile.Interfaces;
using Quarkless.Models.SearchResponse;
using Quarkless.Repository.MongoContext;

namespace Quarkless.Repository.Profile
{
	public class ProfileRepository : IProfileRepository
	{
		private readonly IMongoCollection<ProfileModel> _ctx;
		private const string COLLECTION_NAME = "Profiles";

		public ProfileRepository(IMongoClientContext context)
			=> _ctx = context.AccountDatabase.GetCollection<ProfileModel>(COLLECTION_NAME);

		public async Task<bool> AddMediaUrl(string profileId, string mediaUrl)
		{
			try
			{
				var filter = Builders<ProfileModel>.Filter.Eq("_id", profileId);
				var getProfile = (await _ctx.FindAsync(filter)).FirstOrDefault();
				if (getProfile == null) return false;
				var updates = Builders<ProfileModel>.Update;
				var toAdd = new GroupImagesAlike
				{
					TopicGroup = getProfile.ProfileTopic.Category,
					Url = mediaUrl
				};
				var newPush = updates.Push(e => e.Theme.ImagesLike, toAdd);
				var res = await _ctx.UpdateOneAsync(filter, newPush);
				return res.IsAcknowledged;
			}
			catch
			{
				return false;
			}
		}

		public async Task<bool> RemoveProfile(string instagramAccountId)
		{
			try
			{
				var res = await _ctx.DeleteOneAsync(
					new FilterDefinitionBuilder<ProfileModel>().Eq(_ => _.InstagramAccountId, instagramAccountId));
				return res.IsAcknowledged;
			}
			catch
			{
				return false;
			}
		}

		public async Task<ProfileModel> AddProfile(ProfileModel profile)
		{
			try
			{
				await _ctx.InsertOneAsync(profile);
				return profile;
			}
			catch (Exception err)
			{
				Console.WriteLine(err);
				return null;
			}
		}
		public async Task<ProfileModel> GetProfile(string accountId, string instagramAccountId)
		{
			try
			{
				var builders = Builders<ProfileModel>.Filter;
				var filter = builders.Eq("Account_Id", accountId) & builders.Eq("InstagramAccountId", ObjectId.Parse(instagramAccountId));
				var profiles = await _ctx.FindAsync(filter);
				return profiles.SingleOrDefault();
			}
			catch (Exception ee)
			{
				Console.WriteLine(ee.Message);
				return null;
			}
		}
		public async Task<ProfileModel> GetProfile(string profileId)
		{
			try
			{
				var builders = Builders<ProfileModel>.Filter;
				var filter = builders.Eq("_id", ObjectId.Parse(profileId));
				var profiles = await _ctx.FindAsync(filter);
				return profiles.SingleOrDefault();
			}
			catch (Exception ee)
			{
				Console.WriteLine(ee.Message);
				return null;
			}
		}
		public async Task<IEnumerable<ProfileModel>> GetProfiles(string accountId)
		{
			try
			{
				var filter = Builders<ProfileModel>.Filter.Eq("Account_Id", accountId);
				var profiles = await _ctx.FindAsync(filter);
				return profiles?.ToList();
			}
			catch (Exception ee)
			{
				Console.WriteLine(ee.Message);
				return null;
			}
		}
		public async Task<long?> PartialUpdateProfile(string profileId, ProfileModel profile)
		{
			try
			{
				var updList = new List<UpdateDefinition<ProfileModel>>();
				var filter = Builders<ProfileModel>.Filter.Eq("_id", profileId);
				var updates = Builders<ProfileModel>.Update;
				var ignoreNullValues = profile.Recreate();

				foreach (var valuesToTake in ignoreNullValues)
				{
					updList.Add(updates.Set(valuesToTake.Key, valuesToTake.Value));
				}

				var finalUpdateCommand = Builders<ProfileModel>.Update.Combine(updList);
				var result = await _ctx.UpdateOneAsync(filter, finalUpdateCommand);

				return result.ModifiedCount;
			}
			catch (Exception err)
			{
				Console.WriteLine(err);
				return null;
			}
		}
	}
}
