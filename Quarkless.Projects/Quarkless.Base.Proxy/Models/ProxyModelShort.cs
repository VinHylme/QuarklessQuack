using Quarkless.Base.Proxy.Models.Enums;

namespace Quarkless.Base.Proxy.Models
{
	public class ProxyModelShort
	{
		public string HostAddress { get; set; }
		public int Port { get; set; }
		public bool NeedServerAuth { get; set; }
		public string Username { get; set; }
		public string Password { get; set; }
		public ProxyType ProxyType { get; set; }
	}
}