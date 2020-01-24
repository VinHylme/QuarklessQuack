namespace Quarkless.EmailServices.Models
{
	public class InstagramSearchRequest : ISearchRequest
	{
		public string SubjectName { get; set; }
		public bool StopOnFirst { get; set; }
		public LoginNotices LoginNotices { get; set; }
		public VerifyTemplate VerifyTemplate { get; set; }
	}
}