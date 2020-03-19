using System.Net;

namespace Quarkless.Base.Agent.Models
{
	public class AgentResponse
	{
		public HttpStatusCode HttpStatus { get; set; }
		public string Message { get; set; }
	}
}
