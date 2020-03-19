using System;

namespace Quarkless.Base.Auth.Models.Aws
{
	[Serializable]
	public class XAWSOptions 
	{
		public string ProfileLocation { get; set; }
		public string Profile { get; set; }
	}
}