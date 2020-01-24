using Quarkless.Logic.Services.Automation.Actions.EngageActions;
using Quarkless.Models.Common.Enums;
using Quarkless.Models.Services.Automation.Interfaces;

namespace Quarkless.Logic.Services.Automation.Actions.ActionManager
{
	public static class ActionsManagerExtension
	{
		public static ActionType? GetActionType(this IActionCommit Action)
		{
			var typeofaction = Action.GetType().Name;
			switch (typeofaction)
			{
				case nameof(CreateCommentAction):
					return ActionType.CreateCommentMedia;
				case nameof(CreatePost):
					return ActionType.CreatePost;
				case nameof(FollowUserAction):
					return ActionType.FollowUser;
				case nameof(LikeMediaAction):
					return ActionType.LikePost;
				case nameof(UnFollowUserAction):
					return ActionType.UnFollowUser;
				case nameof(LikeCommentAction):
					return ActionType.LikeComment;
				case nameof(DirectMessagingAction):
					return ActionType.SendDirectMessage;
			}
			return null;
		}
	}
}