namespace Quarkless.Models.Topic
{
	public class AddTopicResponse
	{
		public string Id { get; set; }
		public bool Exists { get; set; }
		public int SubTopicsAmount { get; set; }
	}
}