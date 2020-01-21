using System;
using MongoDB.Bson.Serialization.Attributes;
using Quarkless.Models.ReportHandler.Enums;

namespace Quarkless.Models.ReportHandler
{

	public class LoggerModel
	{
		[BsonId]
		[BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
		public string _id { get; set; }
		public string Message { get; set; }
		public DateTime Date { get; set; }
		public string AccountId { get; set; }
		public string InstagramUsername { get; set; }
		public string Type { get; set; }
		public string Id { get; set; }
		public SeverityLevel SeverityLevel { get; set; }
		public string Section { get; set; }
		public string Function { get; set; }
		public string InstagramAccountId { get; set; }
	}
}