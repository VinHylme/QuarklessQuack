using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Quarkless.Logic.Proxy;
using Quarkless.Models.Auth.Enums;
using Quarkless.Models.Auth.Interfaces;
using Quarkless.Models.Proxy;
using Quarkless.Models.Proxy.Enums;

namespace Quarkless.Controllers
{
    [ApiController]
	[HashtagAuthorize(AuthTypes.Admin)]
	public class ProxyController : ControllerBase
    {
		private readonly IProxyLogic _proxyLogic;
		private readonly IUserContext _userContext;

		public ProxyController(IProxyLogic proxyLogic, IUserContext userContext)
		{
			_proxyLogic = proxyLogic;
			_userContext = userContext;
		}

		[HttpPost]
		[Route("api/admin/proxy/")]
		public async Task<IActionResult> AddProxy([FromBody]ProxyModel proxy)
		{
			if (!_userContext.IsAdmin || proxy == null) return BadRequest("Failed to Add");
			var res = await _proxyLogic.AssignProxy(proxy);
			if (res)
			{
				return Ok("Added");
			}
			return BadRequest("Failed to Add");
		}
		[HttpGet]
		[Route("api/admin/proxies")]
		public async Task<IActionResult> GetAllAssignedProxies() {
			if (!_userContext.IsAdmin) return BadRequest("Failed to get proxies");
			var res = await _proxyLogic.GetAllProxyAssigned(ProxyType.Http);
			if (res!=null)
			{
				return Ok(res);
			}
			return BadRequest("Failed to get proxies");
		}
		[HttpGet]
		[Route("api/admin/proxies/{accountId}/{instagramAccountId}")]
		public async Task<IActionResult> GetAllAssignedProxiesTo(string accountId, string instagramAccountId)
		{
			if (_userContext.IsAdmin && !string.IsNullOrEmpty(accountId) && !string.IsNullOrEmpty(instagramAccountId))
			{
				var res = await _proxyLogic.GetProxyAssigned(accountId,instagramAccountId);
				if (res != null)
				{
					return Ok(res);
				}
			}
			return BadRequest("Failed to get proxies");
		}
    }
}