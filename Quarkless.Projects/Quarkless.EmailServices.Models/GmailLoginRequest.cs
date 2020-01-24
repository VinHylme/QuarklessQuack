namespace Quarkless.EmailServices.Models
{
	public class GmailLoginRequest
	{
		public string UserAgent { get; set; }
		public string Email { get; set; }
		public string Password { get; set; }

		public GmailLoginRequest(string email, string password)
		{
			Email = email;
			Password = password;
		}

		public GmailLoginRequest(string email, string password, string userAgent)
		{
			Email = email;
			Password = password;
			UserAgent = userAgent;
		}
	}
}