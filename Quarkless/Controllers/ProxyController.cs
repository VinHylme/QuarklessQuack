using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Quarkless.Logic.Proxy;
using Quarkless.Models.Auth.Enums;
using Quarkless.Models.Auth.Interfaces;
using Quarkless.Models.Proxy;
using Quarkless.Models.Proxy.Enums;
using Quarkless.Models.Proxy.Interfaces;

namespace Quarkless.Controllers
{
    [ApiController]
	[HashtagAuthorize(AuthTypes.Admin)]
	public class ProxyController : ControllerBase
    {
		private readonly IProxyLogic _proxyLogic;
		private readonly IProxyRequest _proxyRequest;
		private readonly IUserContext _userContext;

		public ProxyController(IProxyLogic proxyLogic, IProxyRequest proxyRequest, IUserContext userContext)
		{
			_proxyLogic = proxyLogic;
			_proxyRequest = proxyRequest;
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
		[Route("api/proxies/{instagramAccountId}")]
		public async Task<IActionResult> GetAllAssignedProxiesTo(string instagramAccountId)
		{
			if (string.IsNullOrEmpty(_userContext.CurrentUser) || string.IsNullOrEmpty(instagramAccountId))
				return BadRequest("Failed to get proxies");

			var res = await _proxyLogic.GetProxyAssignedShort(_userContext.CurrentUser,instagramAccountId);
			if (res != null)
			{
				return Ok(res);
			}
			return BadRequest("Failed to get proxies");
		}

		[HttpPost]
		[Route("api/proxies/update")]
		public async Task<IActionResult> Update(ProxyResponse proxy)
		{
			if (string.IsNullOrEmpty(_userContext.CurrentUser))
				return BadRequest("Failed to reassign proxy");

			if (!proxy.FromUser)
				return BadRequest("You cannot do this, please provide your own proxies");

			if (string.IsNullOrEmpty(proxy.AccountId) || string.IsNullOrEmpty(proxy.InstagramId))
				return BadRequest("Something went wrong, please refresh the page and try again");

			var userProxy = await _proxyLogic.GetProxyAssigned(proxy.AccountId, proxy.InstagramId);
			if (userProxy != null)
			{
				await _proxyLogic.DeleteProxyAssigned(userProxy._id);
			}

			var res = await _proxyLogic.AssignProxy(new ProxyModel
			{
				AccountId = proxy.AccountId,
				InstagramId = proxy.InstagramId,
				AssignedDate = DateTime.UtcNow,
				HostAddress = proxy.HostAddress,
				Port = proxy.Port,
				Username = proxy.Username,
				Password = proxy.Password,
				ProxyType = (ProxyType) proxy.ProxyType,
				FromUser = true
			});

			if (res)
				return Ok("Added new proxy");

			return BadRequest("Failed to add proxy");
		}

		[HttpPost]
		[Route("api/proxies/reassign")]
		public async Task<IActionResult> ReAssignProxy(ProxyResponse proxy)
		{
			if (string.IsNullOrEmpty(_userContext.CurrentUser))
				return BadRequest("Failed to reassign proxy");

			if (proxy.FromUser)
				return BadRequest("You cannot reassign your own proxies");

			if (string.IsNullOrEmpty(proxy.Location.LocationQuery))
				return BadRequest("Please specify the location");

			if (string.IsNullOrEmpty(proxy.AccountId) || string.IsNullOrEmpty(proxy.InstagramId))
				return BadRequest("Something went wrong, please refresh the page and try again");

			var response = await _proxyRequest.AssignProxy(proxy.AccountId, proxy.InstagramId, proxy.Location.LocationQuery);
			if (response == null)
				return BadRequest("Something went wrong, please try a different location");

			return Ok(new ProxyResponse
			{
				AccountId = response.AccountId,
				InstagramId = response.InstagramId,
				Location = response.Location,
				ProxyType = (int) response.ProxyType,
				FromUser = false
			});
		}

		[HttpPost]
		[Route("api/proxies/test")]
		public async Task<IActionResult> TestProxyConnectivity(ProxyResponse proxy)
		{
			if (string.IsNullOrEmpty(_userContext.CurrentUser))
				return BadRequest("Failed to test proxy");

			if (!proxy.FromUser)
			{
				var proxyM = await _proxyLogic.GetProxyAssigned(proxy.AccountId, proxy.InstagramId);
				if (proxyM != null)
				{
					proxy.HostAddress = proxyM.HostAddress;
					proxy.Port = proxyM.Port;
					proxy.ProxyType = (int) proxyM.ProxyType;
				}
			}

			var testConnectivity = await ProxyLogic.TestConnectivity(new ProxyModelShort
			{
				HostAddress = proxy.HostAddress,
				Port = proxy.Port,
				ProxyType = (ProxyType)proxy.ProxyType,
				Username = proxy.Username,
				Password = proxy.Password
			});

			return Ok(!string.IsNullOrEmpty(testConnectivity));
		}
	}
}