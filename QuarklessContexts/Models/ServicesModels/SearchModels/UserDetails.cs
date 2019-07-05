using InstagramApiSharp.Classes.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace QuarklessContexts.Models.ServicesModels.SearchModels
{
	public class UserSuggestionDetails
	{
		public bool? IsNewSuggestions { get; set; }
		public float Value { get; set; }
		public string Caption { get; set; }
		public string FollowText { get; set; }
		public string Algorithm { get; set; }
		public InstaUserInfo UserInfo { get; set; }

	}
}
