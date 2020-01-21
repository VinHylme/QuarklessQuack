using System.Threading.Tasks;
using Amazon.CognitoIdentityProvider.Model;
using Amazon.Extensions.CognitoAuthentication;
using Quarkless.Models.Auth.AccountContext;
using Quarkless.Models.Common.Models.Carriers;

namespace Quarkless.Models.Auth.Interfaces
{
	public interface IAuthHandler
	{
		Task<bool> SignIn(AccountUser user, bool isPersistent = false);
		Task<bool> CreateAccount(AccountUser accountUser, string password);
		Task<AccountUser> GetUserByUsername(string username);
		Task<bool> UpdateUser(AccountUser accountUser);
		Task<ResultCarrier<AdminInitiateAuthResponse>> Login(LoginRequest loginRequest);
		Task<ResultCarrier<RespondToAuthChallengeResponse>> SetNewPassword(NewPasswordRequest Newrequest);
		Task<ResultCarrier<SignUpResponse>> Register(RegisterAccountModel registerAccount);
		Task<ResultCarrier<GetUserResponse>> GetUser(string accessToken);
		Task<ResultCarrier<CodeDeliveryDetailsType>> ResendConfirmationCode(string userName);
		Task<ResultCarrier<ConfirmSignUpResponse>> ConfirmSignUp(SignupConfirmationModel signupConfirmationModel);
		Task<ResultCarrier<AdminAddUserToGroupResponse>> AddUserToGroup(string groupName, string username);
		ResultCarrier<CognitoUser> GetUserById(string userId);
		Task<ResultCarrier<LoginResponse>> RefreshLogin(string refreshToken, string userName);
		Task<bool> UpdateUserState(LoginResponse login);
	}
}
