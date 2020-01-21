namespace Quarkless.Models.Auth
{
	public class LoginResponse
	{
		public string IdToken { get; set; }
		public string AccessToken { get; set; }
		public string RefreshToken { get; set; }
		public string Username { get; set; }
		public int ExpiresIn { get; set; }
	}
}
