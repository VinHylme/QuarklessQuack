using Newtonsoft.Json;

namespace Quarkless.Models.ContentSearch.Models.Yandex
{
	public class Thumb
	{
		[JsonProperty("url")]
		public string Url { get; set; }

		[JsonProperty("size")]
		public Size Size { get; set; }

		[JsonProperty("microImg")]
		public string MicroImg { get; set; }
	}
}