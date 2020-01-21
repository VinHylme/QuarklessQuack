using Newtonsoft.Json;

namespace Quarkless.Repository.MongoContext.Models
{
	public class MongoOptions
	{
		[JsonProperty("AccountCreator")]
		public string AccountCreatorDatabase { get; set; }

		[JsonProperty("Accounts")]
		public string AccountDatabase { get; set; }

		[JsonProperty("Control")]
		public string ControlDatabase { get; set; }

		[JsonProperty("Content")]
		public string ContentDatabase { get; set; }

		[JsonProperty("Scheduler")]
		public string SchedulerDatabase { get; set; }

		[JsonProperty("ConnectionString")]
		public string ConnectionString { get; set; }
	}
}
