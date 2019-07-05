using System.Collections.Generic;
using System.Threading.Tasks;
using QuarklessContexts.Models.InstagramAccounts;

namespace QuarklessRepositories.RedisRepository.InstagramAccountRedis
{
	public interface IInstagramAccountRedis
	{
		Task<ShortInstagramAccountModel> GetInstagramAccountDetail(string userId, string instaId);
		Task<IEnumerable<ShortInstagramAccountModel>> GetInstagramAccountActiveDetail();
		Task SetInstagramAccountDetail(string userId, string instaId, ShortInstagramAccountModel value);
	}
}