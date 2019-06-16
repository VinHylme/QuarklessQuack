using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Quarkless.Auth.AuthTypes;
using Quarkless.Queue.Jobs.Interfaces;
using Quarkless.Services;
using QuarklessContexts.Contexts;
using QuarklessLogic.Logic.AgentLogic;

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
		private readonly IAgentLogic _agentLogic;
		private readonly IAgentServicer _agentServicer;
		private readonly IAgentManager _agentManager;
		public SchedulerController(IUserContext userContext, IAgentLogic agentLogic, IAgentServicer agentServicer, IAgentManager agentManager)
		{
			_userContext = userContext;
			_agentLogic = agentLogic;
			_agentServicer = agentServicer;
			_agentManager = agentManager;
		}


		[HttpPost]
		[Route("api/scheduler/agent/start/{instagramId}")]
		public async Task<IActionResult> StartJobAgent(string instagramId)
		{
			if (!string.IsNullOrEmpty(_userContext.CurrentUser))
			{
				var accessToken = HttpContext.Request.Headers["Authorization"];
				if(string.IsNullOrEmpty(accessToken)) return BadRequest("Not Authorized");
				await _agentManager.StartAgent(_userContext.CurrentUser,instagramId, accessToken);
				return Ok("started");
			}
			return BadRequest("wot");
		}
		[HttpPut]
		[Route("api/scheduler/beginAgent/{instagramId}")]
		public async Task<IActionResult> BeginAgent(string instagramId)
		{
			if (!string.IsNullOrEmpty(_userContext.CurrentUser))
			{
				var res = await _agentLogic.Begin(_userContext.CurrentUser,instagramId);
				if(res)
					return Ok("Started");
				return BadRequest("Could Not start");
			}

			return BadRequest("Invalid User");
		}

		[HttpPut]
		[Route("api/scheduler/stopAgent/{instagramId}")]
		public async Task<IActionResult> StopAgent(string instagramId)
		{
			if (!string.IsNullOrEmpty(_userContext.CurrentUser))
			{
				var res = await _agentLogic.Stop(_userContext.CurrentUser,instagramId);
				if(res)
					return Ok("Stopped");
				return BadRequest("Could not stop");
			}

			return BadRequest("Invalid User");
		}

		[HashtagAuthorize(AuthTypes.Admin)]
		[HttpGet]
		[Route("api/scheduler/admin/startScrape/{instagramId}")]
		public async Task<IActionResult> StartScrape(string instagramId)
		{
			if (!string.IsNullOrEmpty(_userContext.CurrentUser))
			{
				var accessToken = HttpContext.Request.Headers["Authorization"];
				_userContext.FocusInstaAccount = instagramId;
				var res = await _agentServicer.Start(_userContext.CurrentUser,instagramId,accessToken);
				if (res)
					return Ok("Started");
				return BadRequest("Could Not start");
			}
			return BadRequest("Invalid User");

		}
	}
}