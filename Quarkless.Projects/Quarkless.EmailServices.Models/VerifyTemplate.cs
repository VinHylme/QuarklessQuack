namespace Quarkless.EmailServices.Models
{
	public struct VerifyTemplate
	{
		public bool Verify { get; set; }
		public string VerifyUrlRegexTemplate { get; set; }
		public string VerifyCodeTemplate { get; set; }
		public string VerifyCodeRegex { get; set; }
	}
}