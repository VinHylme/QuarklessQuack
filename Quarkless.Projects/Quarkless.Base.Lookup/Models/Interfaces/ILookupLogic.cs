using System.Collections.Generic;
using System.Threading.Tasks;
using Quarkless.Models.Common.Objects;

namespace Quarkless.Base.Lookup.Models.Interfaces
{
	public interface ILookupLogic
	{
		Task AddObjectToLookup(SString accountId, SString instagramAccountId, LookupModel lookup);
		Task UpdateObjectToLookup(SString accountId, SString instagramAccountId, LookupModel oldLookup, LookupModel newLookup);
		Task<IEnumerable<LookupModel>> Get(SString accountId, SString instagramAccountId);
	}
}
