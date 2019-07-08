using System;
using System.Collections.Generic;
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
		CreatePostTypeImage,
		CreatePostTypeVideo,
		CreatePostTypeCarousel,
		CreateStory,
		CreateCommentMedia,
		CreateCommentReply,
		CreateBiography,
		FollowHashtag,
		UnFollowHashtag,
		FollowUser,
		UnFollowUser,
		LikePost,
		UnlikePost,
		LikeComment,
		UnlikeComment,
		MaintainAccount
	}
}
