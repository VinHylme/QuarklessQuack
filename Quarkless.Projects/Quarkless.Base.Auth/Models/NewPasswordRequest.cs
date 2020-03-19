using System.Collections.Generic;

namespace Quarkless.Base.Auth.Models
{
	public class NewPasswordRequest
	{
		public string Username { get; set; }
		public string NewPassword {get; set; }
		public string Session { get; set; } 
		public Dictionary<string,string> ChallengeParams { get; set; }
	}
}
