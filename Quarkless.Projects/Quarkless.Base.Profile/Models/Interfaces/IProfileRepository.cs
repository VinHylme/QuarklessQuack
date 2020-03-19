using System.Collections.Generic;
using System.Threading.Tasks;

namespace Quarkless.Base.Profile.Models.Interfaces
{
	public interface IProfileRepository
	{
		Task<ProfileModel> AddProfile(ProfileModel profile);
		Task<IEnumerable<ProfileModel>> GetProfiles(string accountId);
		Task<ProfileModel> GetProfile(string accountId, string instagramAccountId);
		Task<ProfileModel> GetProfile(string profileId);
		Task<long?> PartialUpdateProfile(string profileId, ProfileModel profile);
		Task<bool> AddMediaUrl(string profileId, string mediaUrl);
		Task<bool> RemoveProfile(string instagramAccountId);
	}
}
