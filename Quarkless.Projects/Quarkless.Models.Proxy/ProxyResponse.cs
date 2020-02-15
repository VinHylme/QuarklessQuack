namespace Quarkless.Models.Proxy
{
	public class ProxyResponse
	{
		public string InstagramId { get; set; }
		public string ProxyId { get; set; }
		public string AccountId { get; set; }
		public string HostAddress { get; set; }
		public int Port { get; set; }
		public string Username { get; set; }
		public string Password { get; set; }
		public bool FromUser { get; set; }
		public Location Location { get; set; }
		public int ProxyType { get; set; }
	}
}