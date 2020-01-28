using System.ComponentModel;

namespace Quarkless.Models.Actions.Enums
{
	public enum EngageActionType
	{
		[Description("createpost")]
		CreatePost,
		[Description("createstory")]
		CreateStory,
		[Description("comment")]
		CreateCommentMedia,
		[Description("followhashtag")]
		FollowHashtag,
		[Description("unfollowhashtag")]
		UnFollowHashtag,
		[Description("followuser")]
		FollowUser,
		[Description("unfollowuser")]
		UnFollowUser,
		[Description("likemedia")]
		LikePost,
		[Description("unlikemedia")]
		UnlikePost,
		[Description("likecomment")]
		LikeComment,
		[Description("unlikecomment")]
		UnlikeComment,
		[Description("senddirectmessage")]
		SendDirectMessage
	}
}