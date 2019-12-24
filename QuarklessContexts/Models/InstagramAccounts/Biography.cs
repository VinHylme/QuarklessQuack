using System.Collections.Generic;

namespace QuarklessContexts.Models.InstagramAccounts
{
	public class Biography
	{
		public string Text { get; set; }
		public IList<string> Hashtags { get; set; }
	}
}