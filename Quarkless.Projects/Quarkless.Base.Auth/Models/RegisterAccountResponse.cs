using Amazon.CognitoIdentityProvider.Model;

namespace Quarkless.Base.Auth.Models
{
	public class RegisterAccountResponse
	{
		public SignUpResponse SignUpResponse { get; set; }
		public RegisterAccountModel RegisteredAccount { get; set; }
	}
}