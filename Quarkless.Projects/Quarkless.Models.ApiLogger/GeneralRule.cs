using System;

namespace Quarkless.Models.ApiLogger
{
	[Serializable]
	public class GeneralRule
	{
		public string Endpoint { get; set; }
		public string Period { get; set; }
		public int Limit { get; set; }
	}
}