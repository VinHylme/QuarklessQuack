using Quarkless.Models.Auth.Interfaces;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using Amazon.Extensions.CognitoAuthentication;
using AspNetCore.Identity.MongoDbCore.Models;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json.Linq;
using Quarkless.Models.Auth;
using Quarkless.Models.Auth.AccountContext;
using Quarkless.Models.Common.Models.Carriers;

namespace Quarkless.Logic.Auth
{
	public class AuthHandler : IAuthHandler
	{
		private readonly IAmazonCognitoIdentityProvider _cognito;
		private readonly UserManager<AccountUser> _userManager;
		private readonly SignInManager<AccountUser> _signInManager;
		private readonly IAuthAccessHandler _accessHandler;
		private readonly IAccountCache _accountCache;
		public AuthHandler(IAuthAccessHandler accessHandler, IAmazonCognitoIdentityProvider provider,
			CognitoUserPool cognitoUser, UserManager<AccountUser> userManager, 
			SignInManager<AccountUser> signInManager, IAccountCache accountCache)
		{
			_accountCache = accountCache;
			_userManager = userManager;
			_signInManager = signInManager;
			_accessHandler = accessHandler;
			_cognito = provider;
			UserPool = cognitoUser;
		}

		public CognitoUserPool UserPool { get; }

		#region Database
		public async Task<bool> SignIn(AccountUser user, bool isPersistent = false)
		{
			try 
			{
				await _signInManager.SignInAsync(user,isPersistent);
				return true;
			}
			catch
			{
				return false;
			}
		}
		public async Task<bool> CreateAccount(AccountUser accountUser, string password)
		{
			var createResp = await _userManager.CreateAsync(accountUser,password);
			if (createResp.Succeeded)
				await _accountCache.SetAccount(accountUser);
			
			return createResp.Succeeded;
		}
		public async Task<bool> UpdateUser(AccountUser accountUser)
		{
			var updateResp = await _userManager.UpdateAsync(accountUser);
			if(updateResp.Succeeded)
				await _accountCache.SetAccount(accountUser);

			return updateResp.Succeeded;
		}
		public async Task<AccountUser> GetUserByUsername(string username)
		{
			var getResp = await _accountCache.GetAccount(username);
			if (getResp != null)
			{
				return getResp;
			}
			var responseFromDb = await _userManager.FindByNameAsync(username);
			if (responseFromDb == null) return null;
			await _accountCache.SetAccount(responseFromDb);
			return responseFromDb;

		}
		public AccountUser GetUserByUsernameOffline(string username)
		{
			return null;
		}
		#endregion
		public async Task<ResultCarrier<GetUserResponse>> GetUser(string accessToken)
		{
			var responseResult = new ResultCarrier<GetUserResponse>();
			var req = new GetUserRequest
			{
				AccessToken = accessToken
			};

			try
			{
				var userResp = await _cognito.GetUserAsync(req);
				if (userResp.HttpStatusCode != HttpStatusCode.OK) return responseResult;
				responseResult.Results = userResp;
				responseResult.IsSuccessful = true;
				return responseResult;

			}
			catch(Exception ee)
			{
				responseResult.Info = new ErrorResponse
				{
					StatusCode = HttpStatusCode.ExpectationFailed,
					Message = ee.Message
				};
				return responseResult;
			}
		}
		public async Task<ResultCarrier<CodeDeliveryDetailsType>> ResendConfirmationCode(string userName)
		{
			var results = new ResultCarrier<CodeDeliveryDetailsType>();
			var resendRequest = new ResendConfirmationCodeRequest
			{
				ClientId = UserPool.ClientID,
				SecretHash = _accessHandler.GetHash(userName,UserPool.ClientID),
				Username = userName
			};
			try
			{
				var resendResponse = await _cognito.ResendConfirmationCodeAsync(resendRequest);
				if (resendResponse.HttpStatusCode == HttpStatusCode.OK)
				{
					results.IsSuccessful = true;
					results.Results = resendResponse.CodeDeliveryDetails;
				}
				else
				{
					results.IsSuccessful = false;
					results.Info = new ErrorResponse
					{
						Message = resendResponse.ResponseMetadata.RequestId
					};
				}
			}
			catch (Exception ee)
			{
				results.IsSuccessful = false;
				results.Info = new ErrorResponse
				{
					Exception = ee,
					Message = ee.Message
				};
			}
			return results;
		}
		public ResultCarrier<CognitoUser> GetUserById(string userId)
		{
			var responseResults = new ResultCarrier<CognitoUser>();
			try
			{
				var results =  UserPool.GetUser(userId);
				responseResults.Results = results;
				responseResults.IsSuccessful = true;
				return responseResults;
			}
			catch(Exception ee)
			{
				responseResults.Info = new ErrorResponse
				{
					StatusCode = System.Net.HttpStatusCode.ExpectationFailed,
					Message = ee.Message
				};
				return responseResults;
			}
		}
		public async Task<ResultCarrier<LoginResponse>> RefreshLogin(string refreshToken, string userName)
		{
			var resultBase = new ResultCarrier<LoginResponse>();
			var initiateAuthRequest = new InitiateAuthRequest()
			{
				ClientId = UserPool.ClientID,
				AuthFlow = AuthFlowType.REFRESH_TOKEN,
				AuthParameters = new Dictionary<string, string>
				{
					{ "REFRESH_TOKEN",refreshToken },
					{ "SECRET_HASH", _accessHandler.GetHash(userName, UserPool.ClientID)  }
				},
				
			};
			try { 
				var results = await _cognito.InitiateAuthAsync(initiateAuthRequest);
				if(results.HttpStatusCode == HttpStatusCode.OK)
				{
					var loginResp = new LoginResponse
					{
						Username = userName,
						IdToken = results.AuthenticationResult.IdToken,
						RefreshToken = refreshToken,
						AccessToken = results.AuthenticationResult.AccessToken,
						ExpiresIn = results.AuthenticationResult.ExpiresIn
					};

					resultBase.Results = loginResp;
					resultBase.IsSuccessful = true;
					if (await UpdateUserState(loginResp)) return resultBase;
					resultBase.Info = new ErrorResponse
					{
						Message = "Failed to update state during refresh"
					};
					return resultBase;
				}
				results.HttpStatusCode = results.HttpStatusCode;

				return resultBase;
			}
			catch(Exception ee)
			{
				resultBase.Info = new ErrorResponse
				{
					StatusCode = HttpStatusCode.ExpectationFailed,
					Message = ee.Message
				};
				return resultBase;
			}
			
		}
		public async Task<ResultCarrier<AdminInitiateAuthResponse>> Login(LoginRequest loginRequest)
		{
			var authReq = new AdminInitiateAuthRequest
			{
				UserPoolId = UserPool.PoolID,
				ClientId = UserPool.ClientID,
				AuthFlow = AuthFlowType.ADMIN_NO_SRP_AUTH,
			};
			var result = new ResultCarrier<AdminInitiateAuthResponse>();
			authReq.AuthParameters.Add("USERNAME", loginRequest.Username);
			authReq.AuthParameters.Add("PASSWORD", loginRequest.Password);		
			authReq.AuthParameters.Add("SECRET_HASH", _accessHandler.GetHash(loginRequest.Username,UserPool.ClientID));
			try
			{
				var authResp = await _cognito.AdminInitiateAuthAsync(authReq);
				result.Results = authResp;
				result.IsSuccessful = true;
				return result;
			}
			catch (Exception ee)
			{
				result.Info = new ErrorResponse
				{
					StatusCode = HttpStatusCode.ExpectationFailed,
					Message = ee.Message
				};	
				return result;
			}
		}
		public async Task<ResultCarrier<AdminAddUserToGroupResponse>> AddUserToGroup(string groupName, string username)
		{
			var responseResults = new ResultCarrier<AdminAddUserToGroupResponse>();

			try
			{
				var groupRequest = new AdminAddUserToGroupRequest
				{
					GroupName = groupName,
					Username = username,
					UserPoolId = UserPool.PoolID
				};
				var results = await _cognito.AdminAddUserToGroupAsync(groupRequest);
				if(results.HttpStatusCode == HttpStatusCode.OK)
				{
					responseResults.Results = results;
					responseResults.IsSuccessful = true;
					return responseResults;
				}

				responseResults.Info = new ErrorResponse
				{
					StatusCode = results.HttpStatusCode					
				};
				return responseResults;

			}
			catch (Exception ee)
			{
				responseResults.Info = new ErrorResponse
				{
					StatusCode = HttpStatusCode.ExpectationFailed,
					Message = ee.Message
				};
				return responseResults;
			}
		}
		public async Task<ResultCarrier<ConfirmSignUpResponse>> ConfirmSignUp(SignupConfirmationModel signupConfirmationModel)
		{
			var responseResult = new ResultCarrier<ConfirmSignUpResponse>();

			var confirmRequest = new ConfirmSignUpRequest
			{
				ClientId = UserPool.ClientID,
				ConfirmationCode = signupConfirmationModel.ConfirmationCode,
				SecretHash = _accessHandler.GetHash(signupConfirmationModel.Username, UserPool.ClientID),
				Username = signupConfirmationModel.Username,
				ForceAliasCreation = signupConfirmationModel.CreateAlias
			};
			try
			{
				var results = await _cognito.ConfirmSignUpAsync(confirmRequest);
				if (results.HttpStatusCode != System.Net.HttpStatusCode.OK) return null;
				responseResult.Results = results;
				responseResult.IsSuccessful = true;

				return responseResult;
			}
			catch(Exception ee)
			{
				responseResult.IsSuccessful = false;
				responseResult.Info = new ErrorResponse
				{
					StatusCode = HttpStatusCode.ExpectationFailed,
					Message = ee.Message
				};
				return responseResult;
			}
		}
		public async Task<ResultCarrier<RespondToAuthChallengeResponse>> SetNewPassword(NewPasswordRequest Newrequest)
		{
			var responseResults = new ResultCarrier<RespondToAuthChallengeResponse>();

			var challengeResponses = new Dictionary<string, string>();
			challengeResponses.Add("USERNAME", Newrequest.Username);
			challengeResponses.Add("SECRET_HASH", _accessHandler.GetHash(Newrequest.Username, UserPool.ClientID));
			challengeResponses.Add("NEW_PASSWORD", Newrequest.NewPassword);
			if (Newrequest.ChallengeParams.ContainsKey("userAttributes"))
			{
				var userAttributes = JObject.Parse(Newrequest.ChallengeParams["userAttributes"]);
				//challengeResponses.Add(userAttributes[])
			}
			try { 
				var resCha = await _cognito.RespondToAuthChallengeAsync(new RespondToAuthChallengeRequest()
				{
					ClientId = UserPool.ClientID,
					ChallengeName = Newrequest.ChallengeNameType,
					Session = Newrequest.Session,
					ChallengeResponses = challengeResponses
				});
				if (resCha.HttpStatusCode != HttpStatusCode.OK) return responseResults;
				responseResults.Results = resCha;
				responseResults.IsSuccessful = true;

				return responseResults;

			}
			catch(Exception ee)
			{
				responseResults.Info = new ErrorResponse
				{
					StatusCode = System.Net.HttpStatusCode.ExpectationFailed,
					Message = ee.Message
				};
				return responseResults;
			}
		}
		public async Task<ResultCarrier<RegisterAccountResponse>> Register(RegisterAccountModel registerAccount)
		{
			var responseResults = new ResultCarrier<RegisterAccountResponse>();
			if (registerAccount == null)
			{
				responseResults.IsSuccessful = false;
				responseResults.Info = new ErrorResponse
				{
					Message = "model is empty"
				};
				return responseResults;
			}
			if (string.IsNullOrEmpty(registerAccount.Name)
				|| string.IsNullOrEmpty(registerAccount.Username)
				|| string.IsNullOrEmpty(registerAccount.Email)
				|| string.IsNullOrEmpty(registerAccount.Password)
				|| string.IsNullOrEmpty(registerAccount.PhoneNumber))
			{
				responseResults.IsSuccessful = false;
				responseResults.Info = new ErrorResponse
				{
					Message = "value cannot be empty"
				};
				return responseResults;
			}
			try { 
				var registerAccountRequest = new SignUpRequest
				{
					ClientId = UserPool.ClientID,
					Username = registerAccount.Username,
					Password = registerAccount.Password,
					UserAttributes = new List<AttributeType>()
					{
						new AttributeType
						{
							Name = "email",
							Value = registerAccount.Email
						},
						new AttributeType
						{
							Name = "name",
							Value = registerAccount.Name
						}
					},
					SecretHash = _accessHandler.GetHash(registerAccount.Username, UserPool.ClientID)
				};

				var response = await _cognito.SignUpAsync(registerAccountRequest);
				if (response.HttpStatusCode != HttpStatusCode.OK) return responseResults;
				responseResults.Results = new RegisterAccountResponse
				{
					RegisteredAccount = registerAccount,
					SignUpResponse = response
				};
				responseResults.IsSuccessful = true;
				return responseResults;

			}
			catch(Exception ee)
			{
				responseResults.Info = new ErrorResponse
				{
					StatusCode = HttpStatusCode.ExpectationFailed,
					Message = ee.Message
				};
				return responseResults;
			}
		}
		public async Task<bool> UpdateUserState(LoginResponse login)
		{
			var user = await GetUserByUsername(login.Username);
			if (user == null) return false;
			var hand = new JwtSecurityTokenHandler();
			var tokenClaims = hand.ReadJwtToken(login.IdToken);

			user.IsUserConfirmed = true;
			user.LastLoggedIn = DateTime.UtcNow;
			user.Claims = tokenClaims.Claims.Select(x => new MongoClaim
			{
				Issuer = x.Issuer,
				Type = x.Type,
				Value = x.Value
			}).ToList();

			user.Tokens = new List<Token>
			{
				new Token
				{
					LoginProvider = "aws",
					Name = "refresh_token",
					Value = login.RefreshToken
				},
				new Token
				{
					LoginProvider = "aws",
					Name = "expires_in",
					Value = login.ExpiresIn.ToString()
				},
				new Token
				{
					LoginProvider = "aws",
					Name = "access_token",
					Value = login.AccessToken
				},
				new Token
				{
					LoginProvider = "aws",
					Name = "id_token",
					Value = login.IdToken
				}
			};
			user.Roles = tokenClaims.Claims.Where(_ => _.Type.Contains("groups")).Select(s => s.Value).ToList();
			return await UpdateUser(user);
		}
	}
}
