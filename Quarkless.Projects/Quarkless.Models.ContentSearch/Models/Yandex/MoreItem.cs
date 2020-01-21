using Newtonsoft.Json;

namespace Quarkless.Models.ContentSearch.Models.Yandex
{
	public struct MoreItem
	{
		[JsonProperty("url")]
		public string Url { get; set; }
		[JsonProperty("direction")]
		public string Direction { get; set; }
		[JsonProperty("visible")]
		public bool Visible { get; set; }
	}
}