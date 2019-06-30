using Amazon.CognitoIdentityProvider.Model;
using Amazon.Extensions.CognitoAuthentication;
using QuarklessContexts.Classes.Carriers;
using QuarklessContexts.Contexts.AccountContext;
using QuarklessContexts.Models.UserAuth.Auth;
using System.Threading.Tasks;

namespace QuarklessLogic.Logic.AuthLogic.Auth
{
	public interface IAuthHandler
	{
		Task<bool> SignIn(AccountUser user, bool isPersistent = false);
		Task<bool> CreateAccount(AccountUser accountUser, string password);
		Task<AccountUser> GetUserByUsername(string username);
		Task<bool> UpdateUser(AccountUser accountUser);
		Task<ResultBase<AdminInitiateAuthResponse>> Login(LoginRequest loginRequest);
		Task<ResultBase<RespondToAuthChallengeResponse>> SetNewPassword(NewPasswordRequest Newrequest);
		Task<ResultBase<SignUpResponse>> Register(RegisterAccountModel registerAccount);
		Task<ResultBase<GetUserResponse>> GetUser(string accessToken);
		Task<ResultBase<ConfirmSignUpResponse>> ConfirmSignUp(EmailConfirmationModel emailConfirmationModel);
		Task<ResultBase<AdminAddUserToGroupResponse>> AddUserToGroup(string groupName, string username);
		ResultBase<CognitoUser> GetUserById(string userId);
		Task<ResultBase<InitiateAuthResponse>> RefreshLogin(string refreshToken, string userName);
	}
}
