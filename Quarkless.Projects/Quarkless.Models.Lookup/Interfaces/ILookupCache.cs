using System.Collections.Generic;
using System.Threading.Tasks;

namespace Quarkless.Models.Lookup.Interfaces
{
	public interface ILookupCache
	{
		Task AddObjectToLookup(string accountId, string instagramAccountId, string objId, LookupModel lookup);
		Task UpdateObjectToLookup(string accountId, string instagramAccountId, string objId, LookupModel oldLookup, LookupModel newLookup);
		Task<IEnumerable<LookupModel>> Get(string accountId, string instagramAccountId, string objId);
	}
}
