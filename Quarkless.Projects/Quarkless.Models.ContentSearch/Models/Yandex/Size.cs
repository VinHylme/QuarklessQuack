using Newtonsoft.Json;

namespace Quarkless.Models.ContentSearch.Models.Yandex
{
	public class Size
	{
		[JsonProperty("width")]
		public int Width { get; set; }

		[JsonProperty("height")]
		public int height { get; set; }
	}
}