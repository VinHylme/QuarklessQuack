namespace Quarkless.EmailServices.Models
{
	public interface ISearchRequest
	{
		string SubjectName { get; set; }
		bool StopOnFirst { get; set; }
		LoginNotices LoginNotices { get; set; }
		VerifyTemplate VerifyTemplate { get; set; }
	}
}
