using System.ComponentModel;

namespace Quarkless.Base.Actions.Models.Enums
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