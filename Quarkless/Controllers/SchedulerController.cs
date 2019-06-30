using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Quarkless.Services;
using QuarklessContexts.Contexts;
using QuarklessContexts.Models.UserAuth.AuthTypes;
using System.Threading.Tasks;

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
		private readonly IAgentManager _agentManager;
		public SchedulerController(IUserContext userContext, IAgentManager agentManager)
		{
			_userContext = userContext;
			_agentManager = agentManager;
		}

		[HttpPost]
		[Route("api/scheduler/agent/start/{instagramId}")]
		public async Task<IActionResult> StartAgent(string instagramId)
		{
			if (!string.IsNullOrEmpty(_userContext.CurrentUser))
			{
				var accessToken = HttpContext.Request.Headers["Authorization"];
				if(string.IsNullOrEmpty(accessToken)) return BadRequest("Not Authorized");

				return Ok(await _agentManager.StartAgent(_userContext.CurrentUser, instagramId, accessToken));
			}
			return BadRequest("failed to start");
		}
		[HttpPost]
		[Route("api/scheduler/agent/stop/{instagramId}")]
		public IActionResult StopAgent(string instagramId)
		{
			if (!string.IsNullOrEmpty(_userContext.CurrentUser))
			{
				return Ok(_agentManager.StopAgent(instagramId));
			}
			return BadRequest("failed to stop");
		}
	}
}