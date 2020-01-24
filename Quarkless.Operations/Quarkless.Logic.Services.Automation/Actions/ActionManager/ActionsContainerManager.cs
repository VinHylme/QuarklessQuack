using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Quarkless.Models.Common.Enums;
using Quarkless.Models.Common.Extensions;
using Quarkless.Models.Common.Objects;
using Quarkless.Models.Services.Automation.Interfaces;
using Quarkless.Models.Services.Automation.Models.ActionOptions;
using Quarkless.Models.Timeline;

namespace Quarkless.Logic.Services.Automation.Actions.ActionManager
{
	public class ActionsContainerManager
	{
		private readonly List<Chance<CommitContainer>> _actions;
		private readonly ConcurrentQueue<ActionWorkerModel> _working;
		private readonly ConcurrentQueue<ActionWorkerModel> _failedWorks;
		private readonly Queue<IEnumerable<TimelineEventModel>> _finishedActions;

		public ActionsContainerManager()
		{
			_failedWorks = new ConcurrentQueue<ActionWorkerModel>();
			_working = new ConcurrentQueue<ActionWorkerModel>();
			_actions = new List<Chance<CommitContainer>>();
			_finishedActions = new Queue<IEnumerable<TimelineEventModel>>();
		}
		public void AddAction(IActionCommit action, IActionOptions options, double chance)
		{
			if(_actions.Any(_=>_.Object.ActionType == action.GetActionType().Value))
			{
				throw new Exception("Action already exists");
			}

			_actions.Add(new Chance<CommitContainer>
			{
				Object = new CommitContainer(action,options),
				Probability = chance
			});
		}
		public void AddWork(CommitContainer action)
		{
			if(_working.Count >= 100) return;
			var actionFromList = _actions.Where(x=>x.Object.Remaining==null).ToList().Find(_=>_.Object == action);
			if (actionFromList != null)
			{
				_working.Enqueue(new ActionWorkerModel
				{
					ActionContainer = actionFromList.Object,
					ActionState = ActionState.NotStarted
				});
			}
		}
		public void RunAction()
		{
			try {

				_working.TryDequeue(out var actionToTake);
				if (actionToTake == null) return;

				actionToTake.ActionState = ActionState.Began;
				var runningRes = actionToTake.ActionContainer.Action.Push(actionToTake.ActionContainer.Options);
				if (runningRes != null && runningRes.IsSuccessful)
				{
					actionToTake.ActionState = ActionState.Finished;
					_finishedActions.Enqueue(runningRes.Results);
				}
				else
				{
					actionToTake.ActionState = ActionState.Failed;
					actionToTake.Response = runningRes.Info;
					_failedWorks.Enqueue(actionToTake);
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
			foreach(var action in _actions)
			{
				if (action.Object.Remaining == null) continue;
				if (dateNow <= action.Object.Remaining.Value) continue;
				action.Object.Remaining = null;
				return true;
			}
			return false;
		}
		public CommitContainer GetRandomAction()
		{
			var rollAction = SecureRandom.ProbabilityRoll(_actions);
			return rollAction;
		}
		public void TriggerAction(ActionType action, DateTime time)
		{
			var first = _actions.FirstOrDefault(ac => ac.Object.ActionType == action);

			if (first != null)
			{
				var lm = first.Object.Remaining = time;
			}
		}
		public IEnumerable<TimelineEventModel> GetFinishedActions()
		{
			return _finishedActions.Count>0 ? _finishedActions.Dequeue() : null;
		}		
		public XRange FindActionLimit(CommitContainer actionContainer)
		{
			var actionOptionFrame = actionContainer.Options.GetType().GetProperties().FirstOrDefault(_ => _.Name.Equals("TimeFrameSeconds"));
			var actionFrameValue = (XRange)actionOptionFrame.GetValue(actionContainer.Options);
			return actionFrameValue;
		}
		public XRange? FindActionLimit(ActionType actionName)
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
			var find = _working.SingleOrDefault(_ => _.ActionContainer == action);
			if (find != null)
				find.ActionState = actionState;
		}
		public void UpdateExecutionTime(ActionType actionType, DateTime newDate)
		{
			var find = _actions.Find(_=>_.Object.ActionType == actionType);
			if(find!=null)
				find.Object.Options.ExecutionTime = newDate;

			var findc = _working.SingleOrDefault(_ => _.ActionContainer.ActionType == actionType);
			if (findc != null)
				findc.ActionContainer.Options.ExecutionTime = newDate;
		}
		public void UpdateExecutionTime(CommitContainer action, DateTime newDate)
		{
			var find = _actions.Find(_=>_.Object==action)?.Object;
			if (find != null)
				find.Options.ExecutionTime = newDate;

			var findc = _working.SingleOrDefault(_ => _.ActionContainer == action);
			if (findc != null)
				findc.ActionContainer.Options.ExecutionTime = newDate;
		}
		public void ChangeOptionOfAction(ActionType actionType, IActionOptions newOptions)
		{
			var find = _actions.Find(_ => _.Object.ActionType == actionType);
			if (find != null)
				find.Object.Options = newOptions;
		}
		public void UpdateStrategy(ActionType actionType, IStrategySettings newStrategySettings)
		{
			var find = _actions.Find(_ => _.Object.ActionType == actionType);
			find?.Object.Action.IncludeStrategy(newStrategySettings);
		}
		public void UpdateUser(ActionType actionType, UserStoreDetails newUserDetails)
		{
			var find = _actions.Find(_ => _.Object.ActionType == actionType);
			find?.Object.Action.IncludeUser(newUserDetails);
		}
	}
}
