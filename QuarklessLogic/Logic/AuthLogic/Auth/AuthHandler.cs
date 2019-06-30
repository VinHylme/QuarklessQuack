using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using Amazon.Extensions.CognitoAuthentication;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json.Linq;
using QuarklessContexts.Classes.Carriers;
using QuarklessContexts.Contexts.AccountContext;
using QuarklessContexts.Models.UserAuth.Auth;
using QuarklessLogic.Handlers.ReportHandler;
using QuarklessLogic.Logic.AuthLogic.Auth.Manager;
using System;
using System.Collections.Generic;
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
		public AuthHandler(IAuthAccessHandler accessHandler, IAmazonCognitoIdentityProvider provider, IReportHandler reportHandler, 
			CognitoUserPool cognitoUser, UserManager<AccountUser> userManager, SignInManager<AccountUser> signInManager)
		{
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
			return (await _userManager.CreateAsync(accountUser,password)).Succeeded;
		}
		public async Task<bool> UpdateUser(AccountUser accountUser)
		{
			return (await _userManager.UpdateAsync(accountUser)).Succeeded;
		}
		public async Task<AccountUser> GetUserByUsername(string username)
		{
			return await _userManager.FindByNameAsync(username);
		}
		public AccountUser GetUserByUsernameOffline(string username)
		{
			return null;
		}
		#endregion
		public async Task<ResultBase<GetUserResponse>> GetUser(string accessToken)
		{
			var Responseresult = new ResultBase<GetUserResponse>();
			var req = new GetUserRequest
			{
				AccessToken = accessToken
			};

			try
			{
				var userResp = await _cognito.GetUserAsync(req);
				if(userResp.HttpStatusCode == System.Net.HttpStatusCode.OK)
				{
					Responseresult.StatusCode = userResp.HttpStatusCode;
					Responseresult.Results = userResp;
					return Responseresult;
				}
				else
				{
					Responseresult.StatusCode = userResp.HttpStatusCode;
					return Responseresult;
				}
			}
			catch(Exception ee)
			{
				Responseresult.StatusCode = System.Net.HttpStatusCode.ExpectationFailed;
				Responseresult.Message = ee.Message;
				return Responseresult;
			}
		}
		public ResultBase<CognitoUser> GetUserById(string userId)
		{
			ResultBase<CognitoUser> responseResults= new ResultBase<CognitoUser>();
			try
			{
				var results =  _cognitoUser.GetUser(userId);
				responseResults.Results = results;
				return responseResults;
			}
			catch(Exception ee)
			{
				responseResults.Message = ee.Message;
				responseResults.StatusCode = System.Net.HttpStatusCode.ExpectationFailed;
				_reportHandler.MakeReport($"Failed to get user: {userId}, error: {ee.Message}");
				return responseResults;
			}
		}
		public async Task<ResultBase<InitiateAuthResponse>> RefreshLogin(string refreshToken, string userName)
		{
			ResultBase<InitiateAuthResponse> resultBase = new ResultBase<InitiateAuthResponse>();
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
					resultBase.StatusCode = results.HttpStatusCode;
					resultBase.Results = results;
					return resultBase;
				}
				results.HttpStatusCode = results.HttpStatusCode;
				return resultBase;
			}
			catch(Exception ee)
			{
				_reportHandler.MakeReport($"Failed to refresh token for user: {userName}, error: {ee.Message}");
				resultBase.Message = ee.Message;
				resultBase.StatusCode = System.Net.HttpStatusCode.ExpectationFailed;
				return resultBase;
			}
			
		}

		public async Task<ResultBase<AdminInitiateAuthResponse>> Login(LoginRequest loginRequest)
		{
			var authReq = new AdminInitiateAuthRequest
			{
				UserPoolId = _cognitoUser.PoolID,
				ClientId = _cognitoUser.ClientID,
				AuthFlow = AuthFlowType.ADMIN_NO_SRP_AUTH,
			};
			ResultBase<AdminInitiateAuthResponse> result = new ResultBase<AdminInitiateAuthResponse>();
			authReq.AuthParameters.Add("USERNAME", loginRequest.Username);
			authReq.AuthParameters.Add("PASSWORD", loginRequest.Password);		
			authReq.AuthParameters.Add("SECRET_HASH", _accessHandler.GetHash(loginRequest.Username,_cognitoUser.ClientID));
			try
			{
				var authResp = await _cognito.AdminInitiateAuthAsync(authReq);
				result.Results = authResp;
				result.StatusCode = authResp.HttpStatusCode;
				return result;
			}
			catch (Exception ee)
			{
				result.StatusCode = System.Net.HttpStatusCode.ExpectationFailed;
				result.Message = ee.Message;
				_reportHandler.MakeReport($"Could Not login user: {loginRequest.Username}, error: {ee.Message}");
				return result;
			}
		}
		public async Task<ResultBase<AdminAddUserToGroupResponse>> AddUserToGroup(string groupName, string username)
		{
			ResultBase<AdminAddUserToGroupResponse> responseResults = new ResultBase<AdminAddUserToGroupResponse>();

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
					responseResults.StatusCode = results.HttpStatusCode;
					return responseResults;
				}
				else
				{
					responseResults.StatusCode = results.HttpStatusCode;
					responseResults.Message = "Failed to add user to group";
					return responseResults;
				}

			}
			catch (Exception ee)
			{
				responseResults.Message = ee.Message;
				responseResults.StatusCode = System.Net.HttpStatusCode.ExpectationFailed;
				_reportHandler.MakeReport($"Could Not add user to group : {username}, error: {ee.Message}");
				return responseResults;
			}
		}
		public async Task<ResultBase<ConfirmSignUpResponse>> ConfirmSignUp(EmailConfirmationModel emailConfirmationModel)
		{
			ResultBase<ConfirmSignUpResponse> Responseresult = new ResultBase<ConfirmSignUpResponse>();

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
					Responseresult.StatusCode = results.HttpStatusCode;
					return Responseresult;
				}
				return null;
			}
			catch(Exception ee) 
			{
				Responseresult.StatusCode = System.Net.HttpStatusCode.ExpectationFailed;
				Responseresult.Message = ee.Message;
				_reportHandler.MakeReport($"Could Not confirm user: {emailConfirmationModel.Username}, error: {ee.Message}");

				return Responseresult;
			}
		}
		public async Task<ResultBase<RespondToAuthChallengeResponse>> SetNewPassword(NewPasswordRequest Newrequest)
		{
			ResultBase<RespondToAuthChallengeResponse> Responseresult = new ResultBase<RespondToAuthChallengeResponse>();

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
					Responseresult.StatusCode = resCha.HttpStatusCode;
					return Responseresult;
				}
				else
				{
					Responseresult.StatusCode = resCha.HttpStatusCode;
					return Responseresult;
				}
			}
			catch(Exception ee)
			{
				Responseresult.StatusCode = System.Net.HttpStatusCode.ExpectationFailed;
				Responseresult.Message = ee.Message;
				_reportHandler.MakeReport($"Could Not set new password for user: {Newrequest.Username}, error: {ee.Message}");
				return Responseresult;
			}
		}
		public async Task<ResultBase<SignUpResponse>> Register(RegisterAccountModel registerAccount)
		{
			ResultBase<SignUpResponse> Responseresult = new ResultBase<SignUpResponse>();
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
				Responseresult.StatusCode = response.HttpStatusCode;
				Responseresult.Results = response;
				return Responseresult;
			}
			else
			{
				Responseresult.StatusCode = response.HttpStatusCode;		
				return Responseresult;
			}
			}
			catch(Exception ee)
			{
				Responseresult.StatusCode = System.Net.HttpStatusCode.ExpectationFailed;
				Responseresult.Message = ee.Message;
				_reportHandler.MakeReport($"Could Not register user: {registerAccount.Username}, error: {ee.Message}");
				return Responseresult;
			}

		}


	}
}
