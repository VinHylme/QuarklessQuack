using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace QuarklessContexts.Models.AgentModels
{
	public class AgentResponse
	{
		public HttpStatusCode HttpStatus { get; set; }
		public string Message { get; set; }
	}

}
