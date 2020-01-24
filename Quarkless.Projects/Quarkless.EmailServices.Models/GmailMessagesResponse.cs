using Quarkless.EmailServices.Models.Enums;

namespace Quarkless.EmailServices.Models
{
	public class GmailMessagesResponse
	{
		public string Body { get; set; }
		public BodyType BodyType { get; set; }

		public GmailMessagesResponse(string body, BodyType bodyType)
		{
			this.Body = body;
			this.BodyType = bodyType;
		}
	}
}
