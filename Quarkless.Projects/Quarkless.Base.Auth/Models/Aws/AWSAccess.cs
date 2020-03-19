using System;

namespace Quarkless.Base.Auth.Models.Aws
{
	[Serializable]
	public class AWSAccess
	{
		public string AccessKey { get; set; }
		public string SecretKey { get; set; }
		public string Region { get; set; }
	}
}
