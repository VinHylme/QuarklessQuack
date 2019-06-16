using Amazon.CognitoIdentityProvider.Model;
using Amazon.Extensions.CognitoAuthentication;
using Quarkless.Models;
using Quarkless.Models.Auth;
using QuarklessContexts.Classes.Carriers;
using System.Threading.Tasks;

namespace Quarkless.Auth
{
	public interface IAuthHandler
	{
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
