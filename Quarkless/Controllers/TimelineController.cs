using Microsoft.AspNetCore.Mvc;
using Quarkless.Auth.AuthTypes;
using Quarkless.Queue.Jobs.Interfaces;
using Quarkless.Queue.Jobs.JobOptions;
using QuarklessContexts.Contexts;
using QuarklessLogic.Handlers.RequestBuilder.RequestBuilder;
using System;
using System.Linq;

namespace Quarkless.Controllers
{
	[ApiController]
	[HashtagAuthorize(AuthTypes.EnterpriseUsers)]
	[HashtagAuthorize(AuthTypes.TrialUsers)]
	[HashtagAuthorize(AuthTypes.BasicUsers)]
	[HashtagAuthorize(AuthTypes.PremiumUsers)]
	[HashtagAuthorize(AuthTypes.Admin)]
	public class TimelineController : ControllerBase
	{
		private readonly IUserContext _userContext;
		private readonly IRequestBuilder _requestBuilder;
		private readonly ITaskService _taskService;
		public TimelineController(IUserContext userContext)
		{
			_userContext = userContext;
		}

		[HttpPost]
		[Route("timeline/{instagramId}")]
		public IActionResult AddEvent([FromRoute] string instagramId, [FromBody] LongRunningJobOptions eventItem)
		{

			if (!string.IsNullOrEmpty(_userContext.CurrentUser))
			{
				var accessToken = HttpContext.Request.Headers["Authorization"];
				_userContext.FocusInstaAccount = instagramId;
				var headers = _requestBuilder.DefaultHeaders(instagramId,accessToken); //token will expire after 1 hour, either increase token time or find another solution
				try {
					eventItem.Rest.RequestHeaders.ToList().AddRange(headers);
					_taskService.LongRunningTask(eventItem.Rest, eventItem.ExecutionTime);
					return Ok("Added to queue");
				}
				catch(Exception ee) {
					return BadRequest(ee.Message);
				}
			}

			return BadRequest("something went wrong");
		}
	}
}
