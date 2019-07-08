using Quarkless.Services.ActionBuilders.EngageActions;
using Quarkless.Services.Interfaces;
using Quarkless.Services.Interfaces.Actions;
using Quarkless.Services.StrategyBuilders;
using QuarklessContexts.Classes.Carriers;
using QuarklessContexts.Extensions;
using QuarklessContexts.Models;
using QuarklessContexts.Models.Timeline;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Quarkless.Services
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
	public class CommitContainer
	{
		public ActionType ActionType;
		public IActionCommit Action;
		public IActionOptions Options;
		public CommitContainer(IActionCommit actionCommit, IActionOptions actionOptions)
		{
			Action = actionCommit;
			Options = actionOptions;
			ActionType = Action.GetActionType().Value;
		}
	}
	public enum ActionState
	{
		NotStarted,
		Began,
		Finished,
		Failed
	}

	public class ActionWorkerModel
	{
		public CommitContainer ActionContainer { get; set; }
		public ActionState ActionState { get; set; }
		public ErrorResponse Response { get; set; }
	}

	public class ActionsContainerManager
	{
		private List<Chance<CommitContainer>> actions;
		private ConcurrentQueue<ActionWorkerModel> working;
		private ConcurrentQueue<ActionWorkerModel> failedWorks;
		private Queue<IEnumerable<TimelineEventModel>> finishedActions;
		public ActionsContainerManager()
		{
			failedWorks = new ConcurrentQueue<ActionWorkerModel>();
			working = new ConcurrentQueue<ActionWorkerModel>();
			actions = new List<Chance<CommitContainer>>();
			finishedActions = new Queue<IEnumerable<TimelineEventModel>>();
		}
		public void AddAction(IActionCommit action, IActionOptions options, double chance)
		{
			if(actions.Any(_=>_.Object.ActionType == action.GetActionType().Value))
			{
				throw new Exception("Action already exists");
			}
			actions.Add(new Chance<CommitContainer>
			{
				Object = new CommitContainer(action,options),
				Probability = chance
			});
		}
		public void AddWork(CommitContainer action)
		{
			if(working.Count >= 100) return;
			var action_from_list = actions.Find(_=>_.Object == action);
			if (action_from_list != null)
			{
				working.Enqueue(new ActionWorkerModel
				{
					ActionContainer = action_from_list.Object,
					ActionState = ActionState.NotStarted
				});
			}
		}
		public void RunAction()
		{
			try { 
				ActionWorkerModel outToTake = null;
				working.TryDequeue(out outToTake);
				if(outToTake!=null)
				{
					outToTake.ActionState = ActionState.Began;
					var runningRes = outToTake.ActionContainer.Action.Push(outToTake.ActionContainer.Options);
					if (runningRes != null && runningRes.IsSuccesful)
					{
						outToTake.ActionState = ActionState.Finished;
						finishedActions.Enqueue(runningRes.Results);
					}
					outToTake.ActionState = ActionState.Failed;
					outToTake.Response = runningRes.Info;
					failedWorks.Enqueue(outToTake);
				}
			}
			catch(Exception ee)
			{
				Console.Write(ee.Message);
			}
		}
		public CommitContainer GetRandomAction()
		{
			var rollAction = SecureRandom.ProbabilityRoll(actions);
			return rollAction;
		}

		public IEnumerable<TimelineEventModel> GetFinishedActions()
		{
			if(finishedActions.Count>0)
				return finishedActions.Dequeue();
			return null;
		}		
		public Range FindActionLimit(CommitContainer actionContainer)
		{
			var actionOptionFrame = actionContainer.Options.GetType().GetProperties().Where(_ => _.Name.Equals("TimeFrameSeconds")).FirstOrDefault();
			var actionFrameValue = (Range)actionOptionFrame.GetValue(actionContainer.Options);
			return actionFrameValue;
		}
		public Range? FindActionLimit(string actionName)
		{
			switch (actionName)
			{
				case ActionNames.CreatePhoto:
					return ImageActionOptions.TimeFrameSeconds;
				case ActionNames.Comment:
					return CommentingActionOptions.TimeFrameSeconds;
				case ActionNames.CreateVideo:
					return VideoActionOptions.TimeFrameSeconds;
				case ActionNames.FollowUser:
					return FollowActionOptions.TimeFrameSeconds;
				case ActionNames.LikeMedia: 
					return LikeActionOptions.TimeFrameSeconds;
			}
			return null;
		}
		public void UpdateActionState(CommitContainer action, ActionState actionState)
		{
			var find = working.Where(_ => _.ActionContainer == action).SingleOrDefault();
			if (find != null)
				find.ActionState = actionState;
		}
		public void UpdateExecutionTime(ActionType actionType, DateTime newDate)
		{
			var find = actions.Find(_=>_.Object.ActionType == actionType);
			if(find!=null)
				find.Object.Options.ExecutionTime = newDate;

			var findc = working.Where(_ => _.ActionContainer.ActionType == actionType).SingleOrDefault();
			if (findc != null)
				findc.ActionContainer.Options.ExecutionTime = newDate;
		}
		public void UpdateExecutionTime(CommitContainer action, DateTime newDate)
		{
			var find = actions.Find(_=>_.Object==action)?.Object;
			if (find != null)
				find.Options.ExecutionTime = newDate;

			var findc = working.Where(_ => _.ActionContainer == action).SingleOrDefault();
			if (findc != null)
				findc.ActionContainer.Options.ExecutionTime = newDate;
		}
		public void ChangeOptionOfAction(ActionType actionType, IActionOptions newOptions)
		{
			var find = actions.Find(_ => _.Object.ActionType == actionType);
			if (find != null)
				find.Object.Options = newOptions;
		}
		public void UpdateStrategy(ActionType actionType, IStrategySettings newStrategySettings)
		{
			var find = actions.Find(_ => _.Object.ActionType == actionType);
			if (find != null)
				find.Object.Action.IncludeStrategy(newStrategySettings);
		}
		public void UpdateUser(ActionType actionType, UserStoreDetails newUserDetails)
		{
			var find = actions.Find(_ => _.Object.ActionType == actionType);
			if (find != null)
				find.Object.Action.IncludeUser(newUserDetails);
		}
	}
}
