using System.Collections.Generic;

namespace Quarkless.Base.InstagramAccounts.Models
{
	public class Biography
	{
		public string Text { get; set; }
		public IList<string> Hashtags { get; set; }
	}
}