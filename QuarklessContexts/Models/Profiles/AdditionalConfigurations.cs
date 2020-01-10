using System.Collections.Generic;

namespace QuarklessContexts.Models.Profiles
{
	public class AdditionalConfigurations
	{
		public bool IsTumblry { get; set; }
		public List<string> Sites { get; set; }
		public int ImageType { get; set; }
		public string PostSize { get; set; }
		public List<int> SearchTypes {get; set; }
		public bool EnableAutoPosting { get; set; }
		public bool EnableAutoDirectMessaging { get; set; }
		public bool AllowRepost { get; set; }
		public bool AutoGenerateCaption { get; set; }
		public bool FocusLocalMore { get; set; }

		public AdditionalConfigurations()
		{
			Sites = new List<string>();
			SearchTypes = new List<int>();
		}
	}
}