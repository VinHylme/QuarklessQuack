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
		Follow,
		UnFollow,
		LikePost,
		UnlikePost,
		LikeComment,
		UnlikeComment
	}
	public enum ActionInstances
	{
		ChangeToRegularAccount,
		ChangeToBusinessAccount,
		Logout,
		Login,
		Like,
		Unlike,
		Follow,
		Unfollow,
		Create,
		Delete,
		None
	}
}
