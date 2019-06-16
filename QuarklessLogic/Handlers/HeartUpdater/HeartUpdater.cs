using MongoDB.Driver;
using QuarklessLogic.Handlers.HeartUpdater.Models;

namespace QuarklessLogic.Handlers.HeartUpdater
{
	public class HeartUpdater : IHeartUpdater
	{
		IMongoDatabase _Client {get; set; }
		private const string DatabaseName = "QueueUpdater";
		private const string CollectionName = "Updater";

		public HeartUpdater(MongoClient mongoClient)
		{
			_Client = mongoClient.GetDatabase(DatabaseName);
		}
		public async void AddToQueue(RequestUpdateModel requestUpdate)
		{
			await _Client.GetCollection<RequestUpdateModel>(CollectionName).InsertOneAsync(requestUpdate);
		}
	}
}
