using Amazon.CognitoIdentityProvider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Quarkless.Models
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
