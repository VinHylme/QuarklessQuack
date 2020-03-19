using System.Collections.Generic;
using System.Threading.Tasks;
using Quarkless.Base.Lookup.Models;
using Quarkless.Base.Lookup.Models.Interfaces;
using Quarkless.Models.Common.Objects;

namespace Quarkless.Base.Lookup.Logic
{
	public class LookupLogic : ILookupLogic
	{
		private readonly ILookupCache _lookupCache;
		public LookupLogic(ILookupCache lookupCache)
		{
			_lookupCache = lookupCache;
		}

		public async Task AddObjectToLookup(SString accountId, SString instagramAccountId, LookupModel lookup)
		{
			await _lookupCache.AddObjectToLookup(accountId, instagramAccountId, lookup);
		}

		public async Task UpdateObjectToLookup(SString accountId, SString instagramAccountId,
			LookupModel oldLookup, LookupModel newLookup)
		{
			await _lookupCache.UpdateObjectToLookup(accountId, instagramAccountId, oldLookup, newLookup);
		}

		public async Task<IEnumerable<LookupModel>> Get(SString accountId, SString instagramAccountId)
		{
			return await _lookupCache.Get(accountId, instagramAccountId);
		}
	}
}