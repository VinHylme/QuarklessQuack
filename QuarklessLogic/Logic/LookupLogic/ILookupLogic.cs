using System.Collections.Generic;
using System.Threading.Tasks;
using Quarkless.Interfacing.Objects;
using QuarklessContexts.Models.LookupModels;

namespace QuarklessLogic.Logic.LookupLogic
{
	public interface ILookupLogic
	{
		Task AddObjectToLookup(SString accountId, SString instagramAccountId, SString objId, LookupModel lookup);
		Task UpdateObjectToLookup(SString accountId, SString instagramAccountId, SString objId, LookupModel oldLookup, LookupModel newLookup);
		Task<IEnumerable<LookupModel>> Get(SString accountId, SString instagramAccountId, SString objId);
	}
}