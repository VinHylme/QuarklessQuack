using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Quarkless.Services
{
	public enum ContentType
	{
		Image,
		Collection,
		Video,
		Story,
		Carousel,
		Comment,
		Bio,
		DirectMessage,
		ITV
	}
	public enum ActionType
	{
		[Description("createpost")]
		CreatePost,
		//[Description("createphoto")]
		//CreatePostTypeImage,
		//[Description("createvideo")]
		//CreatePostTypeVideo,
		//[Description("createcarousel")]
		//CreatePostTypeCarousel,
		[Description("createstory")]
		CreateStory,
		[Description("comment")]
		CreateCommentMedia,
		[Description("commentreply")]
		CreateCommentReply,
		[Description("createbio")]
		CreateBiography,
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
		[Description("actionchecker")]
		MaintainAccount
	}
}
