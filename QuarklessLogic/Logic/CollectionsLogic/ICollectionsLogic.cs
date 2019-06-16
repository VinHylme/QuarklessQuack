using InstagramApiSharp.Classes;
using InstagramApiSharp.Classes.Models;
using System.Threading.Tasks;

namespace QuarklessLogic.Logic.CollectionsLogic
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