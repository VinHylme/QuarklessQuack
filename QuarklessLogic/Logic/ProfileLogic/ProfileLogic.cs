using QuarklessContexts.Models.Profiles;
using QuarklessRepositories.ProfileRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QuarklessLogic.Logic.ProfileLogic
{
	public class ProfileLogic : IProfileLogic
	{
		private readonly IProfileRepository _profileRepository;
		public ProfileLogic(IProfileRepository profileRepository)
		{
			_profileRepository = profileRepository;
		}

		public Task<bool> AddMediaUrl(string profileId, string mediaUrl)
		{
			return _profileRepository.AddMediaUrl(profileId, mediaUrl);
		}

		public Task<ProfileModel> AddProfile(ProfileModel profile)
		{
			return _profileRepository.AddProfile(profile);
		}

		public async Task<ProfileModel> GetProfile(string accountId, string instagramAccountId)
		{
			return await _profileRepository.GetProfile(accountId,instagramAccountId);
		}

		public async Task<IEnumerable<ProfileModel>> GetProfiles(string accountId)
		{
			return await _profileRepository.GetProfiles(accountId);
		}

		public async Task<long?> PartialUpdateProfile(string profileId, ProfileModel profile)
		{
			return await _profileRepository.PartialUpdateProfile(profileId, profile);
		}
	}
}
