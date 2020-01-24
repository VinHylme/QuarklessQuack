using Newtonsoft.Json;

namespace Quarkless.SmsHandler.Models
{
	public class GetNumberResponse
	{
		[JsonProperty("response")]
		public int Response { get; set; }

		[JsonProperty("tzid")]
		public int Tzid { get; set; }
	}
}