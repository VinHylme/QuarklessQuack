using Quarkless.Interfacing;
using Quarkless.Interfacing.Objects;
using QuarklessContexts.Extensions;
using QuarklessContexts.Models.LookupModels;
using QuarklessContexts.Models.Sections;
using QuarklessRepositories.RedisRepository.LoggerStoring;
using QuarklessRepositories.RedisRepository.LookupCache;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QuarklessLogic.Logic.LookupLogic
{
	public class LookupLogic : CommonInterface, ILookupLogic
	{
		private readonly ILookupCache _lookupCache;
		public LookupLogic(ILookupCache lookupCache, ILoggerStore loggerStore)
			: base(loggerStore, Sections.LookupLogic.GetDescription())
		{
			_lookupCache = lookupCache;
		}

		public async Task AddObjectToLookup(SString accountId, SString instagramAccountId, SString objId, LookupModel lookup)
		{
			await RunCodeWithExceptionAsync(async () =>
			{
				await _lookupCache.AddObjectToLookup(accountId, instagramAccountId, objId, lookup);
			},nameof(AddObjectToLookup), accountId, instagramAccountId);
		}
		public async Task UpdateObjectToLookup(SString accountId, SString instagramAccountId, SString objId, 
			LookupModel oldLookup, LookupModel newLookup)
		{
			await RunCodeWithExceptionAsync(async () =>
			{
				await _lookupCache.UpdateObjectToLookup(accountId, instagramAccountId, objId, oldLookup, newLookup);
			},nameof(UpdateObjectToLookup), accountId, instagramAccountId);
		}
		public async Task<IEnumerable<LookupModel>> Get(SString accountId, SString instagramAccountId, SString objId)
		{
			return await RunCodeWithLoggerExceptionAsync(async () => await _lookupCache.Get(accountId, instagramAccountId, objId),nameof(Get), accountId, instagramAccountId);
		}
	}
}
