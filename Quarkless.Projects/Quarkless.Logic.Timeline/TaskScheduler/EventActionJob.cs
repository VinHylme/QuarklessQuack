using System;
using Hangfire;
using Quarkless.Models.Actions;
using Quarkless.Models.Actions.Interfaces;
using Quarkless.Models.Common.Enums;
using Quarkless.Models.Common.Models;
using Quarkless.Models.Timeline;
using Quarkless.Models.Timeline.Enums;
using Quarkless.Models.Timeline.Interfaces;
using Quarkless.Models.Timeline.Interfaces.TaskScheduler;
using Quarkless.Models.Timeline.TaskScheduler;

namespace Quarkless.Logic.Timeline.TaskScheduler
{
	[ProlongExpirationTimeAttribute(ExpiryInHours = 278)]
	[AutomaticRetry(Attempts = 0)]
	public class EventActionJob : IJob<EventActionOptions>
	{
		private readonly IActionExecuteFactory _actionExecuteFactory;
		private readonly IActionExecuteLogsRepository _actionExecuteLogsRepository;
		public EventActionJob(IActionExecuteFactory executeFactory, IActionExecuteLogsRepository actionExecuteLogs)
		{
			_actionExecuteFactory = executeFactory;
			_actionExecuteLogsRepository = actionExecuteLogs;
		}

		public void Perform(EventActionOptions jobOptions)
		{
			if (jobOptions == null)
				return;
			
			if (jobOptions.ExecutionTime > DateTimeOffset.UtcNow.AddSeconds(15))
			{
				//push to another queue for re-attempting (if user choices to do so)
				return;
			}

			var result = _actionExecuteFactory.Create((ActionType) jobOptions.ActionType, new UserStore
			{
				AccountId = jobOptions.User.AccountId,
				InstagramAccountUsername = jobOptions.User.InstagramAccountUsername,
				InstagramAccountUser = jobOptions.User.InstagramAccountUser
			}).ExecuteAsync(new EventExecuteBody(jobOptions.DataObject.Body, jobOptions.DataObject.BodyType)
			{
				ActionType = (ActionType)jobOptions.ActionType
			}).Result;

			if (result.IsSuccessful) return;

			_actionExecuteLogsRepository.AddActionLog(new ActionExecuteLog
			{
				AccountId = jobOptions.User.AccountId,
				InstagramAccountId = jobOptions.User.InstagramAccountUser,
				InstagramAccountUsername = jobOptions.User.InstagramAccountUsername,
				ActionType = (ActionType) jobOptions.ActionType,
				Status = ActionExecuteStatus.Failed,
				DateAdded = DateTime.UtcNow,
				Error = result.Info
			}).GetAwaiter().GetResult();

			throw new InvalidOperationException($"{((ActionType)jobOptions.ActionType).ToString()} Has failed for user {jobOptions.User.AccountId}/{jobOptions.User.InstagramAccountUsername}, Reason: {result.Info.Message}");
		}
	}
}
