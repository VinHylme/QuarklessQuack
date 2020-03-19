namespace Quarkless.Base.Topic.Models
{
	public class AddTopicResponse
	{
		public string Id { get; set; }
		public bool Exists { get; set; }
		public int SubTopicsAmount { get; set; }
	}
}