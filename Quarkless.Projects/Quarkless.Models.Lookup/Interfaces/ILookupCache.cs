using System.Collections.Generic;
using System.Threading.Tasks;
using Quarkless.Models.Common.Objects;

namespace Quarkless.Models.Lookup.Interfaces
{
	public interface ILookupCache
	{
		Task AddObjectToLookup(string accountId, string instagramAccountId, LookupModel lookup);
		Task UpdateObjectToLookup(string accountId, string instagramAccountId, LookupModel oldLookup, LookupModel newLookup);
		Task<IEnumerable<LookupModel>> Get(string accountId, string instagramAccountId);
	}
}
