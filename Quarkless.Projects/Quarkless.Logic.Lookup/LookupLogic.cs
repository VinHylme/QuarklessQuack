using System.Collections.Generic;
using System.Threading.Tasks;
using Quarkless.Models.Common.Objects;
using Quarkless.Models.Lookup;
using Quarkless.Models.Lookup.Interfaces;

namespace Quarkless.Logic.Lookup
{
	public class LookupLogic : ILookupLogic
	{
		private readonly ILookupCache _lookupCache;
		public LookupLogic(ILookupCache lookupCache)
		{
			_lookupCache = lookupCache;
		}

		public async Task AddObjectToLookup(SString accountId, SString instagramAccountId, SString objId, LookupModel lookup)
		{
			await _lookupCache.AddObjectToLookup(accountId, instagramAccountId, objId, lookup);
		}

		public async Task UpdateObjectToLookup(SString accountId, SString instagramAccountId, SString objId,
			LookupModel oldLookup, LookupModel newLookup)
		{
			await _lookupCache.UpdateObjectToLookup(accountId, instagramAccountId, objId, oldLookup, newLookup);
		}

		public async Task<IEnumerable<LookupModel>> Get(SString accountId, SString instagramAccountId, SString objId)
		{
			return await _lookupCache.Get(accountId, instagramAccountId, objId);
		}
	}
}