using System.Threading.Tasks;
using InstagramApiSharp.Classes;
using InstagramApiSharp.Classes.Models;

namespace Quarkless.Base.InstagramCollections
{
	public interface ICollectionsLogic
	{
		Task<IResult<InstaCollections>> GetCollections(int limit);
		Task<IResult<InstaCollectionItem>> GetCollection(long collectionId, int limit);
		Task<IResult<InstaCollectionItem>> AddItemsCollection(long collectionId, params string[] mediaIds);
		Task<IResult<InstaCollectionItem>> CreateCollection(string collectionName);
		Task<IResult<bool>> DeleteCollection(long collectionId);
		Task<IResult<InstaCollectionItem>> CreateCollection(long collectionId, string collectionName, string photoCoverId = null);
	}
}