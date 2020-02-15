using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Quarkless.Logic.Actions.AccountContainer.Extensions;
using Quarkless.Models.Actions;
using Quarkless.Models.Actions.Enums;
using Quarkless.Models.Actions.Factory.Action_Options;
using Quarkless.Models.Actions.Interfaces;
using Quarkless.Models.Common.Enums;
using Quarkless.Models.Common.Extensions;
using Quarkless.Models.Common.Objects;

namespace Quarkless.Logic.Actions.AccountContainer
{
	public class ActionsContainerManager
	{
		private readonly List<Chance<CommitContainer>> _actions;
		private readonly ConcurrentQueue<ActionWorkerModel> _working;
		private readonly ConcurrentQueue<ActionWorkerModel> _failedWorks;
		private readonly Queue<EventActionModel> _finishedActions;

		public ActionsContainerManager()
		{
			_failedWorks = new ConcurrentQueue<ActionWorkerModel>();
			_working = new ConcurrentQueue<ActionWorkerModel>();
			_actions = new List<Chance<CommitContainer>>();
			_finishedActions = new Queue<EventActionModel>();
		}

		public List<CommitContainer> RegisteredActions
			=> _actions.Select(s => s.Object).ToList();

		public void RegisterAction(IActionCommit action, DateTimeOffset execTime, double chance)
		{
			if (_actions.Any(_ => _.Object.ActionType == action.GetActionType()))
			{
				throw new Exception("Action already exists");
			}
			_actions.Add(new Chance<CommitContainer>
			{
				Object = new CommitContainer(action, execTime),
				Probability = chance
			});
		}
		public void AddWork(CommitContainer action)
		{
			if (_working.Count >= 100) return;
			var actionFromList = _actions.Where(x => x.Object.Remaining == null).ToList().Find(_ => _.Object == action);
			if (actionFromList != null)
			{
				_working.Enqueue(new ActionWorkerModel
				{
					ActionContainer = actionFromList.Object,
					ActionState = ActionState.NotStarted
				});
			}
		}

		public async Task RunAction()
		{
			try
			{
				_working.TryDequeue(out var nextAct);
				if (nextAct == null) return;

				nextAct.ActionState = ActionState.Began;

				var runningRes = await nextAct.ActionContainer.Action.PushAsync(nextAct.ActionContainer.ExecTime);
				if (runningRes == null)
				{
					nextAct.ActionState = ActionState.Failed;
					_failedWorks.Enqueue(nextAct);
					return;
				}

				if (runningRes.IsSuccessful)
				{
					nextAct.ActionState = ActionState.Finished;
					_finishedActions.Enqueue(runningRes.Results);
				}
				else
				{
					nextAct.ActionState = ActionState.Failed;
					nextAct.Response = runningRes.Info;
					_failedWorks.Enqueue(nextAct);
				}
			}
			catch (Exception ee)
			{
				Console.Write(ee.Message);
			}
		}
		public bool HasMetTimeLimit()
		{
			var dateNow = DateTime.UtcNow;
			foreach (var act in _actions)
			{
				if (act.Object.Remaining == null) continue;
				if (dateNow <= act.Object.Remaining.Value) continue;
				act.Object.Remaining = null;
				return true;
			}
			return false;
		}
		public CommitContainer GetRandomAction()
		{
			var rollAction = SecureRandom.ProbabilityRoll(_actions);
			return rollAction;
		}

		public EventActionModel GetFinishedAction()
		{
			return _finishedActions.Count > 0 ? _finishedActions.Dequeue() : null;
		}

		public XRange FindActionLimit(CommitContainer actionContainer)
		{
			return actionContainer.Action.GetActionType() switch
			{
				ActionType.CreatePost => PostActionOptions.TimeFrameSeconds,
				ActionType.CreateCommentMedia => CommentingActionOptions.TimeFrameSeconds,
				ActionType.FollowUser => FollowActionOptions.TimeFrameSeconds,
				ActionType.LikePost => LikeActionOptions.TimeFrameSeconds,
				ActionType.LikeComment => LikeCommentActionOptions.TimeFrameSeconds,
				ActionType.SendDirectMessage => SendDirectMessageActionOptions.TimeFrameSeconds,
				ActionType.WatchStory => WatchStoryOptions.TimeFrameSeconds,
				ActionType.ReactStory => ReactStoryOptions.TimeFrameSeconds,
				_ => throw new NotImplementedException()
			};
		}
		public XRange FindActionLimit(ActionType actionName)
		{
			return actionName switch
			{
				ActionType.CreatePost => PostActionOptions.TimeFrameSeconds,
				ActionType.CreateCommentMedia => CommentingActionOptions.TimeFrameSeconds,
				ActionType.FollowUser => FollowActionOptions.TimeFrameSeconds,
				ActionType.LikePost => LikeActionOptions.TimeFrameSeconds,
				ActionType.LikeComment => LikeCommentActionOptions.TimeFrameSeconds,
				ActionType.SendDirectMessage => SendDirectMessageActionOptions.TimeFrameSeconds,
				ActionType.WatchStory => WatchStoryOptions.TimeFrameSeconds,
				ActionType.ReactStory => ReactStoryOptions.TimeFrameSeconds,
				_ => throw new NotImplementedException()
			};
		}
		public void UpdateActionState(CommitContainer action, ActionState actionState)
		{
			var find = _working.SingleOrDefault(_ => _.ActionContainer == action);
			if (find != null)
				find.ActionState = actionState;
		}
		public void UpdateExecutionTime(ActionType actionType, DateTime newDate)
		{
			var find = _actions.Find(_ => _.Object.ActionType == actionType);
			if (find != null)
				find.Object.ExecTime = newDate;

			var findCurrent = _working.SingleOrDefault(_ => _.ActionContainer.ActionType == actionType);
			if (findCurrent != null)
				findCurrent.ActionContainer.ExecTime = newDate;
		}
		public void UpdateExecutionTime(CommitContainer action, DateTime newDate)
		{
			var find = _actions.Find(_ => _.Object == action)?.Object;
			if (find != null)
				find.ExecTime = newDate;

			var findCurrent = _working.SingleOrDefault(_ => _.ActionContainer == action);
			if (findCurrent != null)
				findCurrent.ActionContainer.ExecTime = newDate;
		}
	}
}
