namespace Quarkless.Models.ResponseResolver.Models
{
	public class ProxyTestResponse
	{
		public bool IsSuccessful { get; set; }
		public bool ProxyIsFromUser { get; set; }
		public bool AttemptedToReAssign { get; set; }
		public string Reason { get; set; }
	}
}