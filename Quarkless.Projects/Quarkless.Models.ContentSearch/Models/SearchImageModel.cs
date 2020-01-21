using Newtonsoft.Json;

namespace Quarkless.Models.ContentSearch.Models
{
	public class SearchImageModel
	{
		[JsonProperty("prefix")]
		public string Prefix { get; set; }

		[JsonProperty("prefix_keywords")]
		public string PrefixKeywords { get; set; }

		[JsonProperty("keywords")]
		public string Keywords { get; set; }

		[JsonProperty("suffix_keywords")]
		public string SuffixKeywords { get; set; }

		[JsonProperty("limit")]
		public int Limit { get; set; }

		[JsonProperty("print_urls")]
		public bool PrintUrls { get; set; }

		[JsonProperty("color")]
		public string Color { get; set; }

		[JsonProperty("type")]
		public string Type { get; set; }

		[JsonProperty("no_download")]
		public bool NoDownload { get; set; }

		[JsonProperty("related_images")]
		public bool RelatedImages { get; set; }

		[JsonProperty("similar_images")]
		public string SimilarImages { get; set; }

		[JsonProperty("format")]
		public string Format { get; set; }

		[JsonProperty("color_type")]
		public string ColorType { get; set; }

		[JsonProperty("usage_rights")]
		public string UsageRights { get; set; }

		[JsonProperty("size")]
		public string Size { get; set; }

		[JsonProperty("exact_size")]
		public string ExactSize { get; set; }

		[JsonProperty("proxy")]
		public string Proxy { get; set; }

		[JsonProperty("offset")]
		public int Offset { get; set; }
	}

}
