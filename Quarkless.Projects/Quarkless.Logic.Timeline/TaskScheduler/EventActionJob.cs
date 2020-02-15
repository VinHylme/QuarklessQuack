using System;
using Hangfire;
using Quarkless.Models.Actions;
using Quarkless.Models.Actions.Interfaces;
using Quarkless.Models.Common.Enums;
using Quarkless.Models.Common.Models;
using Quarkless.Models.Common.Interfaces;
using Quarkless.Models.Timeline.Interfaces.TaskScheduler;
using Quarkless.Models.Timeline.TaskScheduler;

namespace Quarkless.Logic.Timeline.TaskScheduler
{
	[AutomaticRetry(Attempts = 0)]
	public class EventActionJob : IJob<EventActionOptions>
	{
		private readonly IActionExecuteFactory _actionExecuteFactory;
		
		public EventActionJob(IActionExecuteFactory executeFactory)
		{
			_actionExecuteFactory = executeFactory;
		}

		public void Perform(EventActionOptions jobOptions)
		{
			if (jobOptions == null)
				return;

			var result = _actionExecuteFactory.Create((ActionType) jobOptions.ActionType, new UserStore
				{
					AccountId = jobOptions.User.AccountId,
					InstagramAccountUsername = jobOptions.User.InstagramAccountUsername,
					InstagramAccountUser = jobOptions.User.InstagramAccountUser
				}).ExecuteAsync(new EventExecuteBody(jobOptions.DataObject.Body, jobOptions.DataObject.BodyType)
				{
					ActionType = (ActionType)jobOptions.ActionType
				})
				.Result;

			if (!result.IsSuccessful)
			{
				throw new InvalidOperationException($"{((ActionType)jobOptions.ActionType).ToString()} Has failed for user {jobOptions.User.AccountId}/{jobOptions.User.InstagramAccountUsername}");
			}
		}
	}
}
