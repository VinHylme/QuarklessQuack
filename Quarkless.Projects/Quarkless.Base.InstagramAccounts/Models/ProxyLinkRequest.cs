namespace Quarkless.Base.InstagramAccounts.Models
{
	public class ProxyLinkRequest
	{
		public int proxyType { get; set; }
		public string HostAddress { get; set; }
		public string Port { get; set; }
		public string Username { get; set; }
		public string Password { get; set; }
	}
}