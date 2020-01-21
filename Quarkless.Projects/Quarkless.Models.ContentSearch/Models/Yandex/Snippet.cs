using Newtonsoft.Json;

namespace Quarkless.Models.ContentSearch.Models.Yandex
{
	public class Snippet
	{
		[JsonProperty("title")]
		public string Title { get; set; }

		[JsonProperty("hasTitle")]
		public bool HasTitle { get; set; }

		[JsonProperty("text")]
		public string Text { get; set; }

		[JsonProperty("url")]
		public string Url { get; set; }

		[JsonProperty("domain")]
		public string Domain { get; set; }

		[JsonProperty("redirUrl")]
		public string RedirUrl { get; set; }
	}
}