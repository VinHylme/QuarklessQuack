using System.Collections.Generic;
using System.Threading.Tasks;
using Quarkless.Models.Common.Objects;

namespace Quarkless.Models.Lookup.Interfaces
{
	public interface ILookupLogic
	{
		Task AddObjectToLookup(SString accountId, SString instagramAccountId, SString objId, LookupModel lookup);
		Task UpdateObjectToLookup(SString accountId, SString instagramAccountId, SString objId, LookupModel oldLookup, LookupModel newLookup);
		Task<IEnumerable<LookupModel>> Get(SString accountId, SString instagramAccountId, SString objId);
	}
}
