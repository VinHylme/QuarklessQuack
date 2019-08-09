using QuarklessContexts.Models.Profiles;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QuarklessLogic.Logic.ProfileLogic
{
	public interface IProfileLogic
	{
		Task<ProfileModel> AddProfile(ProfileModel profile);
		Task<IEnumerable<ProfileModel>> GetProfiles(string accountId);
		Task<ProfileModel> GetProfile(string accountId, string instagramAccountId);
		Task<long?> PartialUpdateProfile(string profileId, ProfileModel profile);
		Task<bool> AddMediaUrl(string profileId, string mediaUrl);
	}
}