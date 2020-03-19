namespace Quarkless.Base.Account.Models
{
	public class StripeCredentials
	{
		public string PublishableKey { get; set; }
		public string SecretKey { get; set; }
		public string WebHookKey { get; set; }
	}
	public class AccountOptions
	{
		public StripeCredentials StripeKey { get; set; }
	}
}
