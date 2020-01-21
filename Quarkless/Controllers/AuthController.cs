using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Amazon.CognitoIdentityProvider;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Quarkless.Models.Auth;
using Quarkless.Models.Auth.AccountContext;
using Quarkless.Models.Auth.Enums;
using Quarkless.Models.Auth.Interfaces;

namespace Quarkless.Controllers
{
	[HashtagAuthorize(AuthTypes.Admin)]
    public class AuthController : ControllerBase
    {
		private readonly IAuthHandler _authHandler;
		public AuthController(IAuthHandler authHandler)
		{
			_authHandler = authHandler;
		}

		[EnableCors("HashtagGrowCORSPolicy")]
		[AllowAnonymous]
		[HttpPost]
		[Route("api/auth/loginaccount")]
		public async Task<IActionResult> Login([FromBody]LoginRequest loginRequest)
		{
			var results = await _authHandler.Login(loginRequest);
			if (results == null) return BadRequest("Invalid Request");
			if (!results.IsSuccessful && results.Info.Message.Contains("not confirmed"))
			{
				return Unauthorized(new
				{
					Status = (int) HttpStatusCode.Unauthorized
				});
			}
			if (results.Results == null) return NotFound(results);
			if (results.Results.ChallengeName == ChallengeNameType.NEW_PASSWORD_REQUIRED)
			{
				return Ok(new ChangePasswordResponse()
				{
					ChallengeNameType = results.Results.ChallengeName,
					ChallengeParams = results.Results.ChallengeParameters,
					Session = results.Results.Session,
					Username = loginRequest.Username
				});
			}
			if (!results.IsSuccessful || results.Results.HttpStatusCode != System.Net.HttpStatusCode.OK)
				return BadRequest(results.Results.AuthenticationResult);

			var responseConcatenate = new LoginResponse { 
				IdToken = results.Results.AuthenticationResult.IdToken,
				AccessToken = results.Results.AuthenticationResult.AccessToken,
				Username = loginRequest.Username, 
				ExpiresIn = results.Results.AuthenticationResult.ExpiresIn, 
				RefreshToken = results.Results.AuthenticationResult.RefreshToken 
			};

			if(!await _authHandler.UpdateUserState(responseConcatenate))
				return BadRequest(new { Message = "Something went wrong, please try logging in again", AuthResults = results.Info });

			return Ok(responseConcatenate);
		}

		[AllowAnonymous]
		[HttpPost]
		[Route("api/auth/refreshState")]
		public async Task<IActionResult> RefreshLogin([FromBody] RefreshTokenRequest refreshToken)
		{
			try
			{
				if (string.IsNullOrEmpty(refreshToken.Username) || string.IsNullOrEmpty(refreshToken.refreshToken))
				{
					return BadRequest("Invalid Parameter");
				}
				var results = await _authHandler.RefreshLogin(refreshToken.refreshToken, refreshToken.Username);
				if (!results.IsSuccessful)
					return NotFound(results.Info);

				return Ok(results.Results);
			}
			catch(Exception ee)
			{
				return BadRequest(ee.Message);
			}
		}
		
		[AllowAnonymous]
		[HttpPut]
		[Route("api/auth/resendConfirmation/{userName}")]
		public async Task<IActionResult> ResendConfirmation([FromRoute] string userName)
		{
			var results = await _authHandler.ResendConfirmationCode(userName);
			if (results.IsSuccessful)
			{
				return Ok(results.Results);
			}
			return BadRequest(results.Info);
		}

		[AllowAnonymous]
		[HttpPost]
		[Route("api/auth/confirmTrialEmail")]
		
		public async Task<IActionResult> ConfirmEmail([FromBody]SignupConfirmationModel signupConfirmationModel)
		{
			var results = await _authHandler.ConfirmSignUp(signupConfirmationModel);
			if (results == null || !results.IsSuccessful) return BadRequest("Failed to confirm user");
			var addUserToGroup = await _authHandler.AddUserToGroup(AuthTypes.TrialUsers.ToString(), signupConfirmationModel.Username);
			if (addUserToGroup.Results != null) { 

				return Ok(results);
			}
			return BadRequest("something went wrong on our side");
		}
		/// <summary>
		/// TODO: Need to add other props such as fingerprint stuff, unqiue identifier, Ips, etc
		/// </summary>
		/// <param name="registerAccountModel"></param>
		/// <returns></returns>
		[AllowAnonymous]
		[HttpPost]
		[Route("api/auth/registeraccount")]
		public async Task<IActionResult> Register([FromBody]RegisterAccountModel registerAccountModel)
		{
			try { 
				var results = await _authHandler.Register(registerAccountModel);
				if (results.Results == null) return BadRequest(results);
				var accountUser = new AccountUser()
				{
					Email = registerAccountModel.Email,
					UserName = registerAccountModel.Username,
					Sub = results.Results.UserSub,
					IsUserConfirmed = results.Results.UserConfirmed,
					Roles = new List<string> { { AuthTypes.TrialUsers.ToString() } }
				};

				await _authHandler.CreateAccount(accountUser, registerAccountModel.Password);
				if (results.Results.UserConfirmed)
				{
					var userIs = await _authHandler.GetUserByUsername(registerAccountModel.Username);
					await _authHandler.SignIn(userIs);
					await Login(new LoginRequest { Username = registerAccountModel.Username, Password = registerAccountModel.Password });
					return Ok(results);
				}
				else
				{
					return NotFound("User not confirmed");
				}
			}
			catch(Exception ee)
			{
				return BadRequest($"Something went wrong: {ee.Message}");
			}
		}
		
		[AllowAnonymous]
		[HttpPost]
		[Route("api/auth/challange/newpassword")]
		public async Task<IActionResult> ChallangeNewPassword([FromBody]NewPasswordRequest newPasswordRequest)
		{
			var results = await _authHandler.SetNewPassword(newPasswordRequest);
			if(results.Results!=null && results.IsSuccessful)
				return Ok(results.Results.AuthenticationResult);

			return BadRequest($"Request Invalid: {results.Info.StatusCode}");
		}
    }
}