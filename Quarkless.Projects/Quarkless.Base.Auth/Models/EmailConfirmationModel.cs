namespace Quarkless.Base.Auth.Models
{
	public class SignupConfirmationModel
	{
		public string Username { get; set; }
		public string ConfirmationCode { get; set; }
		public bool CreateAlias { get; set; }
	}
}
