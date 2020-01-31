namespace Quarkless.Models.InstagramAccounts
{
	public class InstagramAccountPublishEventModel
	{
		public InstagramAccountModel InstagramAccount { get; set; }
		public string IpAddress { get; set; }
		public string LocationLatLon { get; set; }
	}
}