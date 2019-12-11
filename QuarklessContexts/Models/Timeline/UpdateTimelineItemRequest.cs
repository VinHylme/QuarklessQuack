using InstagramApiSharp.Classes.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace QuarklessContexts.Models.Timeline
{
	public class UpdateTimelineItemRequest
	{
		[JsonProperty("id")]
		public string Id { get; set; }

		[JsonProperty("caption")]
		public string Caption { get; set; }

		[JsonProperty("time")]
		public DateTime Time { get; set; }

		[JsonProperty("hashtags")]
		public List<string> Hashtags { get; set; }

		[JsonProperty("location")]
		public InstaLocationShort Location { get; set; }

		[JsonProperty("credit")]
		public string Credit { get; set; }

		[JsonProperty("type")]
		public int Type { get; set; }
	}
}
