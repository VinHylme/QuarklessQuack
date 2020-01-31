namespace Quarkless.Models.Proxy
{
	public class ProxyRequestOptions
	{
		public string Url { get; set; }

		public ProxyRequestOptions(string url)
			=> Url = url;
	}
}