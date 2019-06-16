using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QuarklessModels.Models.InstagramAccounts
{
	public class InstagramClientAccount
	{
		public InstagramAccountModel InstagramAccount { get; set; }
		public Proxies.ProxyModel Proxy { get; set; }
		public Profiles.ProfileModel Profile { get; set; }
		public InstaClient InstaClient { get; set; }

	}
}
