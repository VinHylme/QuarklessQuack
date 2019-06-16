using Auth_.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Auth_.Controllers
{
	public class AuthController : ControllerBase
	{
		private readonly UserManager<UserApplication> _userManager;
		private readonly SignInManager<UserApplication> _signInManager;

		public AuthController(UserManager<UserApplication> userManager, SignInManager<UserApplication> signInManager)
		{
			_userManager = userManager;
			_signInManager = signInManager;
		}

		public class LoginRequest
		{
			public string Username { get; set; }
			public string Password { get; set; }
		}
		public class RegisterViewModel
		{
			public string Email { get; set; }
			public string Username { get; set; }
			public string Password { get; set; }
		}
		[HttpPost]
		[Route("api/auth/login")]
		public async Task<IActionResult> Login([FromBody]LoginRequest loginRequest)
		{
			try { 
			var user = await _userManager.FindByNameAsync(loginRequest.Username);
			if (user != null)
			{
				var results = await _signInManager.PasswordSignInAsync(loginRequest.Username,loginRequest.Password, true,false);
				if (results.Succeeded)
				{
						var userOf = User;
						var context = HttpContext;
						var lemon = _signInManager.Context;
				}
			}
			}
			catch(Exception ee)
			{
				return BadRequest(ee);
			}
			return Ok("Failure");
		}
		[HttpPost]
		[Route("api/auth/register")]
		public async Task<IActionResult> Register([FromBody]RegisterViewModel model)
		{
			if (ModelState.IsValid)
			{
				var user = new UserApplication { UserName = model.Email, Email = model.Email };
				var result = await _userManager.CreateAsync(user, model.Password);
				if (result.Succeeded)
				{
					try { 
					await _userManager.AddClaimsAsync(user, new List<Claim>(){ new Claim("user",model.Username) });

					// For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=532713
					// Send an email with this link
					//var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
					//var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: HttpContext.Request.Scheme);
					//await _emailSender.SendEmailAsync(model.Email, "Confirm your account",
					//    "Please confirm your account by clicking this link: <a href=\"" + callbackUrl + "\">link</a>");

						await _signInManager.SignInAsync(user, isPersistent: false);
					}
					catch(Exception ee)
					{

					}		
					return Ok(result);
				}
			}
			return Ok("eerrorrr");
		}
	}
}
