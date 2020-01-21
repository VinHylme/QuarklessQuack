using InstagramApiSharp.Classes.Models;

namespace Quarkless.Models.InstagramSearch
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
