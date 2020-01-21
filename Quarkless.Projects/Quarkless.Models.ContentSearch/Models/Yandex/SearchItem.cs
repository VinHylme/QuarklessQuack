using Newtonsoft.Json;

namespace Quarkless.Models.ContentSearch.Models.Yandex
{
	public class SearchItem
	{
		[JsonProperty("serpitem", NullValueHandling = NullValueHandling.Ignore)]
		public SerpItem SerpItem { get; set; }
	}
}