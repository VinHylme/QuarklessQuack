using System;
using Quarkless.Logic.Actions.Action_Instances;
using Quarkless.Models.Actions.Interfaces;
using Quarkless.Models.Common.Enums;

namespace Quarkless.Logic.Actions.AccountContainer.Extensions
{
	public static class ActionsManagerExtension
	{
		public static ActionType GetActionType(this IActionCommit action)
		{
			return action.GetType().Name switch
			{
				nameof(CreateCommentAction) => ActionType.CreateCommentMedia,
				nameof(CreatePostAction) => ActionType.CreatePost,
				nameof(FollowUserAction) => ActionType.FollowUser,
				nameof(LikeMediaAction) => ActionType.LikePost,
				nameof(UnFollowUserAction) => ActionType.UnFollowUser,
				nameof(LikeCommentAction) => ActionType.LikeComment,
				nameof(DirectMessagingAction) => ActionType.SendDirectMessage,
				nameof(WatchStoryAction) => ActionType.WatchStory,
				nameof(ReactStoryAction) => ActionType.ReactStory,

				_ => throw new NotImplementedException()
			};
		}
	}
}