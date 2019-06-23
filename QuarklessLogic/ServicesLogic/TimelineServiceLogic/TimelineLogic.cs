using Quarkless.Queue.Jobs.Interfaces;
using QuarklessContexts.Models.Timeline;
using QuarklessLogic.Handlers.RequestBuilder.RequestBuilder;
using System;


namespace QuarklessLogic.ServicesLogic.TimelineServiceLogic.TimelineLogic
{
	public class TimelineLogic : ITimelineLogic
	{
		private readonly ITaskService _taskService;
		private readonly IRequestBuilder _requestBuilder;
		public TimelineLogic(ITaskService taskService, IRequestBuilder requestBuilder)
		{
			_taskService = taskService;
			_requestBuilder = requestBuilder;
		}

		public bool AddToTimeline(RestModel restBody, DateTimeOffset executeTime)
		{
			restBody.RequestHeaders.AddRange(
				_requestBuilder.DefaultHeaders(
				restBody.User.OInstagramAccountUser,
				restBody.User.OAccessToken));

			_taskService.LongRunningTask(restBody, executeTime);
			return true;
		}
	}
}
