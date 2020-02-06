using Amazon.CognitoIdentityProvider.Model;

namespace Quarkless.Models.Auth
{
	public class RegisterAccountResponse
	{
		public SignUpResponse SignUpResponse { get; set; }
		public RegisterAccountModel RegisteredAccount { get; set; }
	}
}