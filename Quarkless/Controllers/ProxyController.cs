using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QuarklessContexts.Contexts;
using QuarklessContexts.Models.Proxies;
using QuarklessContexts.Models.UserAuth.AuthTypes;
using QuarklessLogic.Logic.ProxyLogic;

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
		public IActionResult AddProxy([FromBody]ProxyModel proxy)
		{
			if(_userContext.IsAdmin && proxy!=null)
			{
				var res = _proxyLogic.AddProxy(proxy);
				if (res)
				{
					return Ok("Added");
				}
			}
			return BadRequest("Failed to Add");
		}
		[HttpPost]
		[Route("api/admin/proxies/")]
		public IActionResult AddProxies([FromBody] List<ProxyModel> proxy)
		{
			if (_userContext.IsAdmin && proxy != null)
			{
				var res = _proxyLogic.AddProxies(proxy);
				if (res)
				{
					return Ok("Added");
				}
			}
			return BadRequest("Failed to Add");
		}
		[HttpGet]
		[Route("api/admin/proxies")]
		public async Task<IActionResult> GetAllAssignedProxies() {

			if (_userContext.IsAdmin)
			{
				var res = await _proxyLogic.GetAllAssignedProxies();
				if (res!=null)
				{
					return Ok(res);
				}
			}
			return BadRequest("Failed to get proxies");
		}
		[HttpGet]
		[Route("api/admin/proxies/{accountId}/{instagramAccountId}")]
		public async Task<IActionResult> GetAllAssignedProxiesTo(string accountId, string instagramAccountId)
		{
			if (_userContext.IsAdmin && !string.IsNullOrEmpty(accountId) && !string.IsNullOrEmpty(instagramAccountId))
			{
				var res = await _proxyLogic.GetProxyAssignedTo(accountId,instagramAccountId);
				if (res != null)
				{
					return Ok(res);
				}
			}
			return BadRequest("Failed to get proxies");
		}

		[HttpPost]
		[Route("api/admin/proxies/assign")]
		public async Task<IActionResult> AssignProxy(AssignedTo assigned)
		{
			if (_userContext.IsAdmin && assigned!=null)
			{
				var res = await _proxyLogic.AssignProxy(assigned);
				if (res)
				{
					return Ok($"Assigned proxy to {assigned.Account_Id}");
				}
			}
			return BadRequest("Failed to get proxies");
		}
		[HttpPost]
		[Route("api/admin/proxies/test/{ip}/{port}")]
		public async Task<IActionResult> TestProxy(string ip, int port)
		{
			if (!_userContext.IsAdmin || !string.IsNullOrEmpty(ip) || port > 0) return BadRequest("Failed");
			var res = await _proxyLogic.TestProxy(new ProxyModel
			{
				Address = ip,
				Port = port
			});
			if (res)
			{
				return Ok($"Proxy works fine");
			}
			return BadRequest("Failed");
		}
	}
}