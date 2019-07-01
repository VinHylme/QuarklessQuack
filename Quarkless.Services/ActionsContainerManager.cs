using Quarkless.Services.ActionBuilders.EngageActions;
using Quarkless.Services.Interfaces;
using Quarkless.Services.StrategyBuilders;
using QuarklessContexts.Extensions;
using QuarklessContexts.Models;
using QuarklessContexts.Models.Timeline;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Quarkless.Services
{
	public class CommitContainer
	{
		public ActionType ActionType;
		public IActionCommit Action;
		public IActionOptions Options;
		public CommitContainer(IActionCommit actionCommit, IActionOptions actionOptions)
		{
			Action = actionCommit;
			Options = actionOptions;
			ActionType = GetActionType(Action).Value;
		}
		public static ActionType? GetActionType(IActionCommit Action)
		{
			var typeofaction = Action.GetType().Name;
			switch (typeofaction)
			{
				case nameof(CreateCommentAction):
					return ActionType.CreateCommentMedia;
				case nameof(CreateImagePost):
					return ActionType.CreatePostTypeImage;
				case nameof(CreateVideoPost):
					return ActionType.CreatePostTypeVideo;
				case nameof(FollowUserAction):
					return ActionType.FollowUser;
				case nameof(LikeMediaAction):
					return ActionType.LikePost;
				case nameof(UnFollowUserAction):
					return ActionType.UnFollowUser;
			}
			return null;
		}
	}
	public class ActionsContainerManager
	{
		private List<Chance<CommitContainer>> actions;
		public ActionsContainerManager()
		{
			actions = new List<Chance<CommitContainer>>();
		}
		public void Add(IActionCommit action, IActionOptions options, double chance)
		{
			if(actions.Any(_=>_.Object.ActionType == CommitContainer.GetActionType(action).Value))
			{
				throw new Exception("Action already exists");
			}
			actions.Add(new Chance<CommitContainer>
			{
				Object = new CommitContainer(action,options),
				Probability = chance
			});
		}
		public IEnumerable<TimelineEventModel> RunAction(ActionType action)
		{
			var torun = actions.Where(_=>_.Object.ActionType == action).SingleOrDefault();
			if(torun!=null)
				return torun.Object.Action.Push(torun.Object.Options);
			return null;
		}
		public IEnumerable<TimelineEventModel> RunAction(CommitContainer action)
		{
			return action.Action.Push(action.Options);
		}
		public CommitContainer GetRandomAction()
		{
			var rollAction = SecureRandom.ProbabilityRoll(actions);
			return rollAction;
		}
		public Range FindActionLimit(CommitContainer actionContainer)
		{
			var actionOptionFrame = actionContainer.Options.GetType().GetProperties().Where(_ => _.Name.Equals("TimeFrameSeconds")).FirstOrDefault();
			var actionFrameValue = (Range)actionOptionFrame.GetValue(actionContainer.Options);
			return actionFrameValue;
		}
		public IEnumerable<TimelineEventModel> RunRandomAction()
		{
			var rollAction = SecureRandom.ProbabilityRoll(actions);
			return rollAction.Action.Push(rollAction.Options);
		}
		public void UpdateExecutionTime(ActionType actionType, DateTime newDate)
		{
			var find = actions.Where(_=>_.Object.ActionType == actionType).SingleOrDefault();
			if(find!=null)
				find.Object.Options.ExecutionTime = newDate;
		}
		public void UpdateExecutionTime(CommitContainer action, DateTime newDate)
		{
			var find = actions.Find(_=>_.Object==action)?.Object;
			if (find != null)
				find.Options.ExecutionTime = newDate;
		}

		public void ChangeOptionOfAction(ActionType actionType, IActionOptions newOptions)
		{
			var find = actions.Where(_ => _.Object.ActionType == actionType).SingleOrDefault();
			if (find != null)
				find.Object.Options = newOptions;
		}
		public void UpdateStrategy(ActionType actionType, IStrategySettings newStrategySettings)
		{
			var find = actions.Where(_ => _.Object.ActionType == actionType).SingleOrDefault();
			if (find != null)
				find.Object.Action.IncludeStrategy(newStrategySettings);
		}
		public void UpdateUser(ActionType actionType, UserStoreDetails newUserDetails)
		{
			var find = actions.Where(_ => _.Object.ActionType == actionType).SingleOrDefault();
			if (find != null)
				find.Object.Action.IncludeUser(newUserDetails);
		}
	}
}
