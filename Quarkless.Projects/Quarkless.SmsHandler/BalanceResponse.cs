using Newtonsoft.Json;

namespace Quarkless.SmsHandler.Models
{
	public class BalanceResponse
	{
		[JsonProperty("response")]
		public string Response { get; set; }

		[JsonProperty("balance")]
		public int Balance { get; set; }

		[JsonProperty("zbalance")]
		public int ZBalance { get; set; }
	}
}