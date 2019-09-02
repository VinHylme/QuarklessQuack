using Quarkless.Services.ActionBuilders.EngageActions;
using Quarkless.Services.Interfaces;
using Quarkless.Services.Interfaces.Actions;
using Quarkless.Services.StrategyBuilders;
using QuarklessContexts.Classes.Carriers;
using QuarklessContexts.Enums;
using QuarklessContexts.Extensions;
using QuarklessContexts.Models;
using QuarklessContexts.Models.Timeline;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;

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
	public class CommitContainer
	{
		public ActionType ActionType;
		public IActionCommit Action;
		public IActionOptions Options;
		public DateTime? Remaining { get; set; } = null;
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
			var action_from_list = actions.Where(x=>x.Object.Remaining==null).ToList().Find(_=>_.Object == action);
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
					else
					{
						outToTake.ActionState = ActionState.Failed;
						outToTake.Response = runningRes.Info;
						failedWorks.Enqueue(outToTake);
					}
				}
			}
			catch(Exception ee)
			{
				Console.Write(ee.Message);
			}
		}
		public bool HasMetTimeLimit()
		{
			var dateNow = DateTime.UtcNow;
			foreach(var actio in actions)
			{
				if (actio.Object.Remaining == null) continue;
				if (dateNow <= actio.Object.Remaining.Value) continue;
				actio.Object.Remaining = null;
				return true;
			}
			return false;
		}
		public CommitContainer GetRandomAction()
		{
			var rollAction = SecureRandom.ProbabilityRoll(actions);
			return rollAction;
		}
		public void TriggerAction(ActionType action, DateTime time)
		{
			var lm = actions.FirstOrDefault(ac => ac.Object.ActionType == action).Object.Remaining = time;
		}
		public IEnumerable<TimelineEventModel> GetFinishedActions()
		{
			return finishedActions.Count>0 ? finishedActions.Dequeue() : null;
		}		
		public Range FindActionLimit(CommitContainer actionContainer)
		{
			var actionOptionFrame = actionContainer.Options.GetType().GetProperties().Where(_ => _.Name.Equals("TimeFrameSeconds")).FirstOrDefault();
			var actionFrameValue = (Range)actionOptionFrame.GetValue(actionContainer.Options);
			return actionFrameValue;
		}
		public Range? FindActionLimit(ActionType actionName)
		{
			switch (actionName)
			{
				case ActionType.CreatePost:
					return PostActionOptions.TimeFrameSeconds;
				case ActionType.CreateCommentMedia:
					return CommentingActionOptions.TimeFrameSeconds;
				case ActionType.FollowUser:
					return FollowActionOptions.TimeFrameSeconds;
				case ActionType.LikePost:
					return LikeActionOptions.TimeFrameSeconds;
				case ActionType.LikeComment:
					return LikeCommentActionOptions.TimeFrameSeconds;
				case ActionType.SendDirectMessage:
					return SendDirectMessageActionOptions.TimeFrameSeconds;
			}
			return null;
		}
		public void UpdateActionState(CommitContainer action, ActionState actionState)
		{
			var find = working.SingleOrDefault(_ => _.ActionContainer == action);
			if (find != null)
				find.ActionState = actionState;
		}
		public void UpdateExecutionTime(ActionType actionType, DateTime newDate)
		{
			var find = actions.Find(_=>_.Object.ActionType == actionType);
			if(find!=null)
				find.Object.Options.ExecutionTime = newDate;

			var findc = working.SingleOrDefault(_ => _.ActionContainer.ActionType == actionType);
			if (findc != null)
				findc.ActionContainer.Options.ExecutionTime = newDate;
		}
		public void UpdateExecutionTime(CommitContainer action, DateTime newDate)
		{
			var find = actions.Find(_=>_.Object==action)?.Object;
			if (find != null)
				find.Options.ExecutionTime = newDate;

			var findc = working.SingleOrDefault(_ => _.ActionContainer == action);
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
			find?.Object.Action.IncludeStrategy(newStrategySettings);
		}
		public void UpdateUser(ActionType actionType, UserStoreDetails newUserDetails)
		{
			var find = actions.Find(_ => _.Object.ActionType == actionType);
			find?.Object.Action.IncludeUser(newUserDetails);
		}
	}
}
