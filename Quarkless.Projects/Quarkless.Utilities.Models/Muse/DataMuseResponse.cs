using Newtonsoft.Json;

namespace Quarkless.Utilities.Models.Muse
{

	public class WordResponse
	{
		[JsonProperty("word")]
		public string Word { get; set; }
		[JsonProperty("score")]
		public int Score { get; set; }
		[JsonProperty("tags")]
		public string[] Tags { get; set; }
	}

}
