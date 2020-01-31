﻿using System.Collections.Generic;
using System.Threading.Tasks;

namespace Quarkless.Models.Profile.Interfaces
{
	public interface IProfileLogic
	{
		Task<ProfileModel> AddProfile(ProfileModel profile, bool assignProxy = false, string ipAddress = null, string location = null);
		Task<IEnumerable<ProfileModel>> GetProfiles(string accountId);
		Task<ProfileModel> GetProfile(string accountId, string instagramAccountId);
		Task<ProfileModel> GetProfile(string profileId);
		Task<long?> PartialUpdateProfile(string profileId, ProfileModel profile);
		Task<bool> AddMediaUrl(string profileId, string mediaUrl);
		Task<bool> AddProfileTopics(ProfileTopicAddRequest profileTopics);
	}
}
