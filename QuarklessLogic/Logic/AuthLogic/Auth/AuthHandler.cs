using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using Amazon.Extensions.CognitoAuthentication;
using AspNetCore.Identity.MongoDbCore.Models;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json.Linq;
using QuarklessContexts.Classes.Carriers;
using QuarklessContexts.Contexts.AccountContext;
using QuarklessContexts.Models.UserAuth.Auth;
using QuarklessLogic.Handlers.ReportHandler;
using QuarklessLogic.Logic.AuthLogic.Auth.Manager;
using QuarklessRepositories.RedisRepository.AccountCache;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;

namespace QuarklessLogic.Logic.AuthLogic.Auth
{
	public class AuthHandler : IAuthHandler
	{
		private readonly IAmazonCognitoIdentityProvider _cognito;
		private readonly UserManager<AccountUser> _userManager;
		private readonly SignInManager<AccountUser> _signInManager;
		private readonly CognitoUserPool _cognitoUser;
		private readonly IAuthAccessHandler _accessHandler;
		private readonly IReportHandler _reportHandler;
		private readonly IAccountCache _accountCache;
		public AuthHandler(IAuthAccessHandler accessHandler, IAccountCache accountCache, IAmazonCognitoIdentityProvider provider, IReportHandler reportHandler, 
			CognitoUserPool cognitoUser, UserManager<AccountUser> userManager, SignInManager<AccountUser> signInManager)
		{
			_accountCache = accountCache;
			_userManager = userManager;
			_signInManager = signInManager;
			_accessHandler = accessHandler;
			_cognitoUser = cognitoUser;
			_cognito = provider;
			_reportHandler = reportHandler;
			_reportHandler.SetupReportHandler("Auth");
		}

		public CognitoUserPool UserPool {  get { return _cognitoUser;} }
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
			if(responseFromDb!=null)
			{
				await _accountCache.SetAccount(responseFromDb);
				return responseFromDb;
			}

			return null;
		}
		public AccountUser GetUserByUsernameOffline(string username)
		{
			return null;
		}
		#endregion
		public async Task<ResultCarrier<GetUserResponse>> GetUser(string accessToken)
		{
			var Responseresult = new ResultCarrier<GetUserResponse>();
			var req = new GetUserRequest
			{
				AccessToken = accessToken
			};

			try
			{
				var userResp = await _cognito.GetUserAsync(req);
				if(userResp.HttpStatusCode == System.Net.HttpStatusCode.OK)
				{
					Responseresult.Results = userResp;
					Responseresult.IsSuccesful = true;
					return Responseresult;
				}
				else
				{
					return Responseresult;
				}
			}
			catch(Exception ee)
			{
				Responseresult.Info = new ErrorResponse
				{
					StatusCode = System.Net.HttpStatusCode.ExpectationFailed,
					Message = ee.Message
				};
				return Responseresult;
			}
		}
		public ResultCarrier<CognitoUser> GetUserById(string userId)
		{
			ResultCarrier<CognitoUser> responseResults= new ResultCarrier<CognitoUser>();
			try
			{
				var results =  _cognitoUser.GetUser(userId);
				responseResults.Results = results;
				responseResults.IsSuccesful = true;
				return responseResults;
			}
			catch(Exception ee)
			{
				responseResults.Info = new ErrorResponse
				{
					StatusCode = System.Net.HttpStatusCode.ExpectationFailed,
					Message = ee.Message
				};
				_reportHandler.MakeReport($"Failed to get user: {userId}, error: {ee.Message}");
				return responseResults;
			}
		}
		public async Task<ResultCarrier<InitiateAuthResponse>> RefreshLogin(string refreshToken, string userName)
		{
			ResultCarrier<InitiateAuthResponse> resultBase = new ResultCarrier<InitiateAuthResponse>();
			InitiateAuthRequest initiateAuthRequest = new InitiateAuthRequest()
			{
				ClientId = _cognitoUser.ClientID,
				AuthFlow = AuthFlowType.REFRESH_TOKEN,
				AuthParameters = new Dictionary<string, string>
				{
					{ "REFRESH_TOKEN",refreshToken },
					{ "SECRET_HASH", _accessHandler.GetHash(userName, _cognitoUser.ClientID)  }
				},
				
			};
			try { 
				var results = await _cognito.InitiateAuthAsync(initiateAuthRequest);
				if(results.HttpStatusCode == System.Net.HttpStatusCode.OK)
				{
					resultBase.Results = results;
					resultBase.IsSuccesful = true;
					var userdb = await GetUserByUsername(userName);
					if (userdb != null)
					{
						JwtSecurityTokenHandler hand = new JwtSecurityTokenHandler();
						userdb.IsUserConfirmed = true;

						var tokenClaims = hand.ReadJwtToken(results.AuthenticationResult.IdToken);
						userdb.Claims = tokenClaims.Claims.Select(x => new MongoClaim
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
							Value = refreshToken
						},
						new Token
						{
							LoginProvider = "aws",
							Name = "expires_in",
							Value = results.AuthenticationResult.ExpiresIn.ToString()
						},
						new Token
						{
							LoginProvider = "aws",
							Name = "access_token",
							Value = results.AuthenticationResult.AccessToken
						},
						new Token
						{
							LoginProvider = "aws",
							Name = "id_token",
							Value = results.AuthenticationResult.IdToken
						}
					};

						var aresu = await UpdateUser(userdb);
						if (!aresu)
						{
							//something went wrong with the db
						}
					}
					return resultBase;
				}
				results.HttpStatusCode = results.HttpStatusCode;

				return resultBase;
			}
			catch(Exception ee)
			{
				_reportHandler.MakeReport($"Failed to refresh token for user: {userName}, error: {ee.Message}");
				resultBase.Info = new ErrorResponse
				{
					StatusCode = System.Net.HttpStatusCode.ExpectationFailed,
					Message = ee.Message
				};
				return resultBase;
			}
			
		}

		public async Task<ResultCarrier<AdminInitiateAuthResponse>> Login(LoginRequest loginRequest)
		{
			var authReq = new AdminInitiateAuthRequest
			{
				UserPoolId = _cognitoUser.PoolID,
				ClientId = _cognitoUser.ClientID,
				AuthFlow = AuthFlowType.ADMIN_NO_SRP_AUTH,
			};
			ResultCarrier<AdminInitiateAuthResponse> result = new ResultCarrier<AdminInitiateAuthResponse>();
			authReq.AuthParameters.Add("USERNAME", loginRequest.Username);
			authReq.AuthParameters.Add("PASSWORD", loginRequest.Password);		
			authReq.AuthParameters.Add("SECRET_HASH", _accessHandler.GetHash(loginRequest.Username,_cognitoUser.ClientID));
			try
			{
				var authResp = await _cognito.AdminInitiateAuthAsync(authReq);
				result.Results = authResp;
				result.IsSuccesful = true;
				return result;
			}
			catch (Exception ee)
			{
				result.Info = new ErrorResponse
				{
					StatusCode = System.Net.HttpStatusCode.ExpectationFailed,
					Message = ee.Message
				};	
				_reportHandler.MakeReport($"Could Not login user: {loginRequest.Username}, error: {ee.Message}");
				return result;
			}
		}
		public async Task<ResultCarrier<AdminAddUserToGroupResponse>> AddUserToGroup(string groupName, string username)
		{
			ResultCarrier<AdminAddUserToGroupResponse> responseResults = new ResultCarrier<AdminAddUserToGroupResponse>();

			try
			{
				var groupRequest = new AdminAddUserToGroupRequest
				{
					GroupName = groupName,
					Username = username,
					UserPoolId = _cognitoUser.PoolID
				};
				var results = await _cognito.AdminAddUserToGroupAsync(groupRequest);
				if(results.HttpStatusCode == System.Net.HttpStatusCode.OK)
				{
					responseResults.Results = results;
					responseResults.IsSuccesful = true;
					return responseResults;
				}
				else
				{
					responseResults.Info = new ErrorResponse
					{
						StatusCode = results.HttpStatusCode					
					};
					return responseResults;
				}

			}
			catch (Exception ee)
			{
				responseResults.Info = new ErrorResponse
				{
					StatusCode = System.Net.HttpStatusCode.ExpectationFailed,
					Message = ee.Message
				};
				_reportHandler.MakeReport($"Could Not add user to group : {username}, error: {ee.Message}");
				return responseResults;
			}
		}
		public async Task<ResultCarrier<ConfirmSignUpResponse>> ConfirmSignUp(EmailConfirmationModel emailConfirmationModel)
		{
			ResultCarrier<ConfirmSignUpResponse> Responseresult = new ResultCarrier<ConfirmSignUpResponse>();

			var confirmRequest = new ConfirmSignUpRequest
			{
				ClientId = _cognitoUser.ClientID,
				ConfirmationCode = emailConfirmationModel.ConfirmationCode,
				SecretHash = _accessHandler.GetHash(emailConfirmationModel.Username, _cognitoUser.ClientID),
				Username = emailConfirmationModel.Username,
				ForceAliasCreation = emailConfirmationModel.CreateAlias
			};
			try
			{
				var results = await _cognito.ConfirmSignUpAsync(confirmRequest);
				if(results.HttpStatusCode == System.Net.HttpStatusCode.OK) {
					Responseresult.Results = results;
					Responseresult.IsSuccesful = true;

					return Responseresult;
				}
				return null;
			}
			catch(Exception ee) 
			{
				Responseresult.Info = new ErrorResponse
				{
					StatusCode = System.Net.HttpStatusCode.ExpectationFailed,
					Message = ee.Message
				};
				_reportHandler.MakeReport($"Could Not confirm user: {emailConfirmationModel.Username}, error: {ee.Message}");

				return Responseresult;
			}
		}
		public async Task<ResultCarrier<RespondToAuthChallengeResponse>> SetNewPassword(NewPasswordRequest Newrequest)
		{
			ResultCarrier<RespondToAuthChallengeResponse> Responseresult = new ResultCarrier<RespondToAuthChallengeResponse>();

			Dictionary<String, String> challengeResponses = new Dictionary<string, string>();
			challengeResponses.Add("USERNAME", Newrequest.Username);
			challengeResponses.Add("SECRET_HASH", _accessHandler.GetHash(Newrequest.Username, _cognitoUser.ClientID));
			challengeResponses.Add("NEW_PASSWORD", Newrequest.NewPassword);
			if (Newrequest.ChallengeParams.ContainsKey("userAttributes"))
			{
				var userAttributes = JObject.Parse(Newrequest.ChallengeParams["userAttributes"]);
				//challengeResponses.Add(userAttributes[])
			}
			try { 
				var resCha = await _cognito.RespondToAuthChallengeAsync(new RespondToAuthChallengeRequest()
				{
					ClientId = _cognitoUser.ClientID,
					ChallengeName = Newrequest.ChallengeNameType,
					Session = Newrequest.Session,
					ChallengeResponses = challengeResponses
				});
				if (resCha.HttpStatusCode == System.Net.HttpStatusCode.OK)
				{
					Responseresult.Results = resCha;
					Responseresult.IsSuccesful = true;

					return Responseresult;
				}
				else
				{
					return Responseresult;
				}
			}
			catch(Exception ee)
			{
				Responseresult.Info = new ErrorResponse
				{
					StatusCode = System.Net.HttpStatusCode.ExpectationFailed,
					Message = ee.Message
				};
				_reportHandler.MakeReport($"Could Not set new password for user: {Newrequest.Username}, error: {ee.Message}");
				return Responseresult;
			}
		}
		public async Task<ResultCarrier<SignUpResponse>> Register(RegisterAccountModel registerAccount)
		{
			ResultCarrier<SignUpResponse> Responseresult = new ResultCarrier<SignUpResponse>();
			try { 
			var registerAccountRequest = new SignUpRequest
			{
				ClientId = _cognitoUser.ClientID,
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
				SecretHash = _accessHandler.GetHash(registerAccount.Username, _cognitoUser.ClientID)
			};

			var response = await _cognito.SignUpAsync(registerAccountRequest);
			if (response.HttpStatusCode == System.Net.HttpStatusCode.OK)
			{
				Responseresult.Results = response;
				Responseresult.IsSuccesful = true;
				return Responseresult;
			}
			else
			{
				return Responseresult;
			}
			}
			catch(Exception ee)
			{
				Responseresult.Info = new ErrorResponse
				{
					StatusCode = System.Net.HttpStatusCode.ExpectationFailed,
					Message = ee.Message
				};
				_reportHandler.MakeReport($"Could Not register user: {registerAccount.Username}, error: {ee.Message}");
				return Responseresult;
			}

		}


	}
}
