using System;

namespace Quarkless.Models.Auth.Aws
{
	[Serializable]
	public class AWSAccess
	{
		public string AccessKey { get; set; }
		public string SecretKey { get; set; }
		public string Region { get; set; }
	}
}
