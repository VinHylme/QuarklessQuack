using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using Quarkless.Extensions;
using QuarklessContexts.Models.Profiles;
using QuarklessRepositories.RepositoryClientManager;

namespace QuarklessRepositories.ProfileRepository
{
	public class ProfileRepository : IProfileRepository
	{
		private readonly IRepositoryContext _context;
		public ProfileRepository(IRepositoryContext context)
		{
			_context = context;
		}
		public async Task<ProfileModel> AddProfile(ProfileModel profile)
		{
			try
			{
				await _context.Profiles.InsertOneAsync(profile);
				return profile;
			}
			catch (Exception ee)
			{
				return null;
			}
		}

		public async Task<ProfileModel> GetProfile(string accountId, string instagramAccountId)
		{
			try
			{
				var builders = Builders<ProfileModel>.Filter;
				var filter = builders.Eq("Account_Id", accountId) & builders.Eq("InstagramAccountId",ObjectId.Parse(instagramAccountId));
				var profiles = await _context.Profiles.FindAsync(filter);
				return profiles.SingleOrDefault();
			}
			catch (Exception ee)
			{
				return null;
			}
		}

		public async Task<IEnumerable<ProfileModel>> GetProfiles(string accountId)
		{
			try
			{
				var filter = Builders<ProfileModel>.Filter.Eq("Account_Id", accountId);
				var profiles = await _context.Profiles.FindAsync(filter);
				if (profiles != null)
				{
					return profiles.ToList();
				}
				return null;
			}
			catch(Exception ee)
			{
				return null;
			}
		}

		public async Task<long?> PartialUpdateProfile(string profileId, ProfileModel profile)
		{
			try
			{
				var updList = new List<UpdateDefinition<ProfileModel>>();
				var filter = Builders<ProfileModel>.Filter.Eq("_id",profileId);
				var updates = Builders<ProfileModel>.Update;
				var IgnoreNullValues = profile.Recreate();

				foreach (var valuesToTake in IgnoreNullValues)
				{
					updList.Add(updates.Set(valuesToTake.Key, valuesToTake.Value));
				}

				var finalUpdateCommand = Builders<ProfileModel>.Update.Combine(updList);
				var result = await _context.Profiles.UpdateOneAsync(filter, finalUpdateCommand);

				return result.ModifiedCount;
			}
			catch (Exception ee)
			{
				return null;
			}
		}
	}
}
