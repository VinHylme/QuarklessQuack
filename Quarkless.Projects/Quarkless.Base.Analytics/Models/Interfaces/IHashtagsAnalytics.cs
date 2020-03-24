using System.Threading.Tasks;
using Quarkless.Models.Common.Models.Carriers;

namespace Quarkless.Base.Analytics.Models.Interfaces
{
	public interface IHashtagsAnalytics
	{
		Task<ResultCarrier<HashtagDetailResponse>> GetHashtagAnalysis(
			string tagName, int mediaLimit = 0);
	}
}
