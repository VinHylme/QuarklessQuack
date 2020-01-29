using System.ComponentModel;

namespace Quarkless.Models.Common.Enums
{
	public enum ActionType
	{
		[Description("none")]
		None = -1,
		[Description("createpost")]
		CreatePost = 1,
		[Description("createstory")]
		CreateStory = 2,
		[Description("comment")]
		CreateCommentMedia = 3,
		[Description("commentreply")]
		CreateCommentReply = 4,
		[Description("createbio")]
		CreateBiography = 5,
		[Description("followhashtag")]
		FollowHashtag = 6,
		[Description("unfollowhashtag")]
		UnFollowHashtag = 7,
		[Description("followuser")]
		FollowUser = 8,
		[Description("unfollowuser")]
		UnFollowUser = 9,
		[Description("likemedia")]
		LikePost = 10,
		[Description("unlikemedia")]
		UnlikePost = 11,
		[Description("likecomment")]
		LikeComment = 12,
		[Description("unlikecomment")]
		UnlikeComment = 13,
		[Description("actionchecker")]
		MaintainAccount = 14,
		[Description("GetRecentActivityFeed")]
		RecentActivityFeed = 15,
		[Description("refreshlogin")]
		RefreshLogin = 16,
		[Description("changedProfilePicture")]
		ChangeProfilePicture = 17,
		[Description("getinbox")]
		GetInbox = 18,
		[Description("getthread")]
		GetThread = 19,
		[Description("senddirectmessage")]
		SendDirectMessage = 20,
		[Description("sendmessagetext")]
		SendDirectMessageText = 21,
		[Description("sendmessagelink")]
		SendDirectMessageLink = 22,
		[Description("sendmessagephoto")]
		SendDirectMessagePhoto = 23,
		[Description("sendmessagevideo")]
		SendDirectMessageVideo = 24,
		[Description("sendmessageaudio")]
		SendDirectMessageAudio = 25,
		[Description("sendmessageprofile")]
		SendDirectMessageProfile = 26,
		[Description("sharemessagemedia")]
		SendDirectMessageMedia = 27
	}
}
