using Newtonsoft.Json;

namespace Quarkless.SmsHandler.Models
{
	public class Status
	{
		[JsonIgnore]
		public int MessageCount { get; set; }
		[JsonIgnore]
		public bool IsNewNumber { get; set; }
		[JsonProperty("country")]
		public int Country { get; set; }

		[JsonProperty("sum")]
		public int Sum { get; set; }

		[JsonProperty("service")]
		public string Service { get; set; }

		[JsonProperty("number")]
		public string Number { get; set; }

		[JsonProperty("response")]
		public string Response { get; set; }

		[JsonProperty("tzid")]
		public int Tzid { get; set; }

		[JsonProperty("time")]
		public int Time { get; set; }

		[JsonProperty("msg")]
		public string Message { get; set; } = null;

		[JsonProperty("form")]
		public string Form { get; set; }
	}
}