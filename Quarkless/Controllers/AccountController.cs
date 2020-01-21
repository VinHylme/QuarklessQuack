using Microsoft.AspNetCore.Mvc;
using Quarkless.Models.Account;
using Quarkless.Models.Account.Interfaces;
using Quarkless.Models.Auth.Interfaces;

namespace Quarkless.Controllers
{
	public class AccountController: ControllerBase
	{
		private readonly IAccountLogic _accountLogic;
		private readonly IUserContext _userContext;
		public AccountController(IUserContext userContext, IAccountLogic accountLogic)
		{
			_userContext = userContext;
			_accountLogic = accountLogic;
		}

		[HttpPost]
		[Route("api/account/session")]
		public IActionResult CreateSubscriptionSession(ChargeRequest chargeRequest)
		{
			if (string.IsNullOrEmpty(_userContext.CurrentUser))
				return BadRequest("Invalid User");
			var session = _accountLogic.CreateSubscriptionSession(chargeRequest);
			if (session == null)
				return NotFound("Something went wrong");
			return Ok(session);
		}
	}
}
