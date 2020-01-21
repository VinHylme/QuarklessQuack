using System.Net;

namespace Quarkless.Models.Agent
{
	public class AgentResponse
	{
		public HttpStatusCode HttpStatus { get; set; }
		public string Message { get; set; }
	}
}
