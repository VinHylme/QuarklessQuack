using System;
using System.Collections.Generic;
using System.Linq;
using Quarkless.Models.Actions;
using Quarkless.Models.Actions.Enums;
using Quarkless.Models.Actions.Interfaces;
using Quarkless.Models.Common.Models;
using Quarkless.Models.Timeline.Interfaces.TaskScheduler;
using Quarkless.Models.Timeline.TaskScheduler;

namespace Quarkless.Logic.Timeline.TaskScheduler
{
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
				OAccountId = jobOptions.User.OAccountId,
				OInstagramAccountUsername = jobOptions.User.OInstagramAccountUsername,
				OInstagramAccountUser = jobOptions.User.OInstagramAccountUser
			}).ExecuteAsync(new EventExecuteBody(jobOptions.DataObject.Body, jobOptions.DataObject.BodyType)).Result;

			if (!result.IsSuccessful)
			{
				throw new Exception("Action Failed");
			}
		}
	}
}
