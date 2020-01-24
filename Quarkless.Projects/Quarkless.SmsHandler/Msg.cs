using Newtonsoft.Json;

namespace Quarkless.SmsHandler.Models
{
	public class Msg
	{
		[JsonProperty("service")]
		public string Service { get; set; }
		[JsonProperty("msg")]
		public string Message { get; set; }
	}
}