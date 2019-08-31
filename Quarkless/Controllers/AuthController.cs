using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;
using Amazon.CognitoIdentityProvider;
using AspNetCore.Identity.MongoDbCore.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Nancy;
using QuarklessContexts.Contexts.AccountContext;
using QuarklessContexts.Models.UserAuth.Auth;
using QuarklessContexts.Models.UserAuth.AuthTypes;
using QuarklessLogic.Logic.AuthLogic.Auth;

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
			if (!results.IsSuccesful && results.Info.Message.Contains("not confirmed"))
			{
				return Unauthorized(new
				{
					Status = (int) HttpStatusCode.Checkpoint
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

			if (!results.IsSuccesful || results.Results.HttpStatusCode != System.Net.HttpStatusCode.OK)
				return Ok(results.Results.AuthenticationResult);
			var responseConcatenate = new { 
				results.Results.AuthenticationResult.IdToken,
				results.Results.AuthenticationResult.AccessToken,
				loginRequest.Username, 
				results.Results.AuthenticationResult.ExpiresIn, 
				results.Results.AuthenticationResult.RefreshToken 
			};
			var userdb = await _authHandler.GetUserByUsername(loginRequest.Username);
			if (userdb == null) return Ok(responseConcatenate);
			var hand = new JwtSecurityTokenHandler();
			userdb.IsUserConfirmed = true;
			var tokenClaims = hand.ReadJwtToken(responseConcatenate.IdToken);
			userdb.Claims = tokenClaims.Claims.Select(x=>new MongoClaim 
			{
				Issuer = x.Issuer,
				Type = x.Type,
				Value = x.Value
			}).ToList();

			userdb.Tokens = new List<Token>
			{
				new Token
				{
					LoginProvider = "aws",
					Name = "refresh_token",
					Value = responseConcatenate.RefreshToken
				},
				new Token
				{
					LoginProvider = "aws",
					Name = "expires_in",
					Value = responseConcatenate.ExpiresIn.ToString()
				},
				new Token
				{
					LoginProvider = "aws",
					Name = "access_token",
					Value = responseConcatenate.AccessToken
				},
				new Token
				{
					LoginProvider = "aws",
					Name = "id_token",
					Value = responseConcatenate.IdToken
				}
			};
			userdb.Roles =  tokenClaims.Claims.Where(_=>_.Type.Contains("groups")).Select(s=>s.Value).ToList();
			var aresu = await _authHandler.UpdateUser(userdb);
			if (!aresu)
			{
				//something went wrong with the db
			}
			return Ok(responseConcatenate);

		}

		[AllowAnonymous]
		[HttpPost]
		[Route("api/auth/refreshState")]
		public async Task<IActionResult> RefreshLogin([FromBody] RefreshTokenRequest refreshToken)
		{
			try
			{
				var results = await _authHandler.RefreshLogin(refreshToken.refreshToken, refreshToken.Username);
				return Ok(results);
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
			if (results.IsSuccesful)
			{
				return Ok(results.Results);
			}
			return BadRequest(results.Info);
		}

		[AllowAnonymous]
		[HttpPost]
		[Route("api/auth/confirmTrialEmail")]
		
		public async Task<IActionResult> ConfirmEmail([FromBody]EmailConfirmationModel emailConfirmationModel)
		{
			var results = await _authHandler.ConfirmSignUp(emailConfirmationModel);
			if (results == null || !results.IsSuccesful) return BadRequest("Failed to confirm user");
			var addUserToGroup = await _authHandler.AddUserToGroup(AuthTypes.TrialUsers.ToString(), emailConfirmationModel.Username);
			if (addUserToGroup.Results != null) { 

				return Ok(results);
			}
			return BadRequest("something went wrong on our side");
		}
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
					Roles = new List<string> { { AuthTypes.TrialUsers.ToString() } },
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

				}
				return Ok(results);
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
			if(results.Results!=null && results.IsSuccesful)
				return Ok(results.Results.AuthenticationResult);

			return BadRequest($"Request Invalid: {results.Info.StatusCode}");
		}
    }
}