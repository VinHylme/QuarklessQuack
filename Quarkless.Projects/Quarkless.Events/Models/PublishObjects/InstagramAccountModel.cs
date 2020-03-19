using Quarkless.Models.Common.Models;

namespace Quarkless.Events.Models.PublishObjects
{
	public class InstagramAccountModel
	{
		public string _id { get; set; }
		public string AccountId { get; set; }
		public long? UserId { get; set; }
		public string Username { get; set; }
		public string FullName { get; set; }
		public string ProfilePicture { get; set; }
		public Location Location { get; set; }
		public string Password { get; set; }
		public string Email { get; set; }
		public string EmailPassword { get; set; }
		public string PhoneNumber { get; set; }
		public long? FollowersCount { get; set; }
		public long? FollowingCount { get; set; }
		public long? TotalPostsCount { get; set; }
		public int? Type { get; set; } //0 = normal account, 1 = learner account
		public int? AgentState { get; set; }
	}
}
