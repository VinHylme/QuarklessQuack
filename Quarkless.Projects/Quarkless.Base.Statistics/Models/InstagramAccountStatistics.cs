using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Quarkless.Base.Statistics.Models
{
	public class InstagramAccountStatistics
	{
		[BsonRepresentation(BsonType.ObjectId)]
		[BsonId]
		public string _id { get; set; }
		public string AccountId { get; set; }

		[BsonRepresentation(BsonType.ObjectId)]
		public string InstagramAccountId { get; set; }
		public ActionStatistics ActionStatistics { get; set; }
		public UserStatistics UserStatistics { get; set; }
	}
}