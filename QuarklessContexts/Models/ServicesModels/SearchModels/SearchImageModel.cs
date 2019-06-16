using System;
using System.Collections.Generic;
using System.Text;

namespace QuarklessContexts.Models.ServicesModels.SearchModels
{
	public class SearchImageModel
	{
		public string prefix { get; set; }
		public string prefix_keywords { get; set; }
		public string keywords { get; set; }
		public string suffix_keywords { get; set; }
		public int limit { get; set; }
		public bool print_urls { get; set; }
		public string color { get; set; }
		public string type { get; set; }
		public bool no_download { get; set; }
		public string related_images { get; set; }
		public string format { get; set; }
		public string color_type { get; set; }
		public string usage_rights { get; set; }
		public string size { get; set; }
		public string exact_size { get; set; }
		public string proxy { get; set; }
	}
}
