using Newtonsoft.Json;

namespace Quarkless.Base.ContentSearch.Models.Models.Yandex
{
	public class SerpItem
	{
		[JsonProperty("reqid")]
		public string ReqID { get; set; }

		[JsonProperty("freshness")]
		public string Freshness { get; set; }

		[JsonProperty("preview")]
		public Preview[] Preview { get; set; }

		[JsonProperty("dups")]
		public Dup[] Dups { get; set; }

		[JsonProperty("thumb")]
		public Thumb Thumb { get; set; }

		[JsonProperty("snippet", NullValueHandling = NullValueHandling.Ignore)]
		public Snippet Snippet { get; set; }

		[JsonProperty("detail_url", NullValueHandling = NullValueHandling.Ignore)]
		public string Detail_Url { get; set; }

		[JsonProperty("img_href", NullValueHandling = NullValueHandling.Ignore)]
		public string Img_href { get; set; }

		[JsonProperty("useProxy", NullValueHandling = NullValueHandling.Ignore)]
		public bool UseProxy { get; set; }

		[JsonProperty("pos", NullValueHandling = NullValueHandling.Ignore)]
		public int Pos { get; set; }

		[JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
		public string ID { get; set; }

		[JsonProperty("rimId", NullValueHandling = NullValueHandling.Ignore)]
		public string RimID { get; set; }

		[JsonProperty("docid", NullValueHandling = NullValueHandling.Ignore)]
		public string DocID { get; set; }

		[JsonProperty("greenUrlCounterPath", NullValueHandling = NullValueHandling.Ignore)]
		public string GreenUrlCounterPath { get; set; }

		[JsonProperty("counterPath", NullValueHandling = NullValueHandling.Ignore)]
		public string CounterPath { get; set; }
	}
}
