using System;

namespace Quarkless.Models.Auth.Aws
{
	[Serializable]
	public class XAWSOptions 
	{
		public string ProfileLocation { get; set; }
		public string Profile { get; set; }
	}
}