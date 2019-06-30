namespace QuarklessContexts.Models.UserAuth.Auth
{
	public class EmailConfirmationModel
	{
		public string Username { get; set; }
		public string ConfirmationCode { get; set; }
		public bool CreateAlias { get; set; }
	}
}
