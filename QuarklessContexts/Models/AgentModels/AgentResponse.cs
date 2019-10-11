using System.Net;

namespace QuarklessContexts.Models.AgentModels
{
	public class AgentResponse
	{
		public HttpStatusCode HttpStatus { get; set; }
		public string Message { get; set; }
	}

}
