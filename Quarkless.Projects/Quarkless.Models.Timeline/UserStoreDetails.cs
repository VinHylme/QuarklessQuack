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
		public IEnumerable<MessagesLib> MessagesTemplates { get; set;  }

		public string OAccountId { get; set; }
		public string OAccessToken { get; set; }
		public string ORefreshToken { get; set; }
		public string OInstagramAccountUser { get; set; }
		public string OInstagramAccountUsername { get; set; }

		public void AddUpdateUser(string accountId, string instagramAccountId, string accessToken)
		{
			this.OAccountId = accountId;
			this.OInstagramAccountUser = instagramAccountId;
			this.OAccessToken = accessToken;
		}
	}
}
