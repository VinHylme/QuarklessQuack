using System;
using System.Collections.Generic;
using System.Text;

namespace QuarklessContexts.Models.InstagramAccounts
{
	public class ShortInstagramAccountModel
	{
		public string Id { get; set; }
		public string AccountId { get; set; }
		public string Username { get; set; }
		public bool? AgentState { get; set; }
		public int? FollowersCount { get; set; }
		public int? FollowingCount { get; set; }
		public int? TotalPostsCount { get; set; }
	}
}
