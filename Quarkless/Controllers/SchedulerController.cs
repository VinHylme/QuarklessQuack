﻿using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Quarkless.Base.Agent.Models.Interfaces;
using Quarkless.Base.Auth.Common.Models.Enums;
using Quarkless.Base.Auth.Common.Models.Interfaces;

namespace Quarkless.Controllers
{
	[ApiController]
	[HashtagAuthorize(AuthTypes.EnterpriseUsers)]
	[HashtagAuthorize(AuthTypes.TrialUsers)]
	[HashtagAuthorize(AuthTypes.BasicUsers)]
	[HashtagAuthorize(AuthTypes.PremiumUsers)]
	[HashtagAuthorize(AuthTypes.Admin)]
	public class SchedulerController : ControllerBase
    {
		private readonly IUserContext _userContext;
		private readonly IAgentLogic _agentManager;
		public SchedulerController(IUserContext userContext, IAgentLogic agentManager)
		{
			_userContext = userContext;
			_agentManager = agentManager;
		}

		[HttpPost]
		[Route("api/scheduler/agent/start/{instagramId}")]
		public async Task<IActionResult> StartAgent(string instagramId)
		{
			if (string.IsNullOrEmpty(_userContext.CurrentUser)) return BadRequest("failed to start");
			var accessToken = HttpContext.Request.Headers["Authorization"];
			if(string.IsNullOrEmpty(accessToken)) return BadRequest("Not Authorized");

			return Ok(await _agentManager.Start(_userContext.CurrentUser, instagramId));
		}
		[HttpPost]
		[Route("api/scheduler/agent/stop/{instagramId}")]
		public async Task<IActionResult> StopAgent(string instagramId)
		{
			if (!string.IsNullOrEmpty(_userContext.CurrentUser))
			{
				return Ok(await _agentManager.Stop(_userContext.CurrentUser,instagramId));
			}
			return BadRequest("failed to stop");
		}
	}
}