using System;
using System.Collections.Generic;
using System.Text;

namespace QuarklessContexts.Models.ServicesModels.SearchModels
{
	public class MediaDetail
	{
		public string Topic { get; set; }
		public List<string> MediaUrl { get; set; } = new List<string>();
		public int LikesCount { get; set; }
		public string MediaId { get; set; }
	}
}
