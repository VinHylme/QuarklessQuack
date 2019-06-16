using System.Collections.Generic;
using System.Threading.Tasks;

namespace Quarkless.Services
{
	public interface ISearchServices
	{
		Task<IEnumerable<string>> SearchTag(string query, IEnumerable<long> exclude = null, string rankToken = null);
	}
}