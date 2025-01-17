﻿using Quarkless.Base.InstagramAccounts.Models;
using Quarkless.Base.Profile.Models;
using Quarkless.Base.Proxy.Models;

namespace Quarkless.Run.Services.Heartbeat.Models
{
	public class FullUserDetail
	{
		public ShortInstagramAccountModel InstagramAccount { get; set; }
		public ProfileModel Profile { get; set; }
		public ProxyModel ProxyUsing { get; set; }
	}
}