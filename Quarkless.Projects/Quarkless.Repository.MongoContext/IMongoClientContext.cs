using MongoDB.Driver;

namespace Quarkless.Repository.MongoContext
{
	public interface IMongoClientContext
	{
		IMongoDatabase CreatorDatabase { get; }
		IMongoDatabase AccountDatabase { get; }
		IMongoDatabase ControlDatabase { get; }
		IMongoDatabase ContentDatabase { get; }
		IMongoDatabase StatisticsDatabase { get; }
	}
}