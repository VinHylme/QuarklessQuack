using Amazon.CognitoIdentityProvider;
using System.Collections.Generic;

namespace QuarklessContexts.Models.UserAuth.Auth
{
	public class NewPasswordRequest
	{
		public string Username { get; set; }
		public string NewPassword {get; set; }
		public ChallengeNameType ChallengeNameType {get; set; }
		public string Session { get; set; } 
		public Dictionary<string,string> ChallengeParams { get; set; }
	}
}
