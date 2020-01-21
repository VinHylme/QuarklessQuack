using System.Collections.Generic;
using System.Threading.Tasks;

namespace Quarkless.Models.InstagramAccounts.Interfaces
{
	public interface IInstagramAccountRedis
	{
		Task<IEnumerable<ShortInstagramAccountModel>> GetWorkerAccounts();
		Task<ShortInstagramAccountModel> GetInstagramAccountDetail(string userId, string instaId);
		Task<IEnumerable<ShortInstagramAccountModel>> GetInstagramAccountActiveDetail();
		Task SetInstagramAccountDetail(string userId, string instaId, ShortInstagramAccountModel value);
		Task<bool> AccountExists(string userId, string instaId);
	}
}
