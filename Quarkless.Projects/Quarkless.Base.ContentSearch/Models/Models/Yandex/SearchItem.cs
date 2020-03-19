using Newtonsoft.Json;

namespace Quarkless.Base.ContentSearch.Models.Models.Yandex
{
	public class SearchItem
	{
		[JsonProperty("serpitem", NullValueHandling = NullValueHandling.Ignore)]
		public SerpItem SerpItem { get; set; }
	}
}