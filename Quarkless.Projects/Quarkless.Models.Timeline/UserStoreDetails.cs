using System.Collections.Generic;
using Newtonsoft.Json;
using Quarkless.Models.InstagramAccounts;
using Quarkless.Models.Library;
using Quarkless.Models.Profile;

namespace Quarkless.Models.Timeline
{
	public class UserStoreDetails
	{
		[JsonIgnore]
		public ProfileModel Profile { get; set; }

		[JsonIgnore]
		public ShortInstagramAccountModel ShortInstagram { get; set; }

		[JsonIgnore]
		public IEnumerable<MessagesLib> MessagesTemplates { get; set; }

		public string AccountId { get; set; }
		public string InstagramAccountUser { get; set; }
		public string InstagramAccountUsername { get; set; }

		public void AddUpdateUser(string accountId, string instagramAccountId, string instagramAccountUsername)
		{
			AccountId = accountId;
			InstagramAccountUser = instagramAccountId;
			InstagramAccountUsername = instagramAccountUsername;
		}
	}
}
