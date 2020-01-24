using Newtonsoft.Json;

namespace Quarkless.SmsHandler.Models
{
	public class StatusMulti
	{
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
		public Msg[] Message { get; set; }

		[JsonProperty("form")]
		public string Form { get; set; }
	}
}