using QuarklessLogic.Handlers.AgentServices;
using QuarklessLogic.Handlers.ClientProvider;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace QuarklessLogic.Logic.AgentLogic
{
	public class AgentLogic : IAgentLogic
	{
		private readonly IAPIClientContainer _client;
		
		public AgentLogic(IAPIClientContainer client)
		{
			_client = client;
		}

		public async Task<bool> Begin(string user, string instagramAccountId)
		{
			//_client.Create(user,instagramAccountId);
			
			return false;
		}
		public async Task<bool> StartScrape(string user, string instagramAccountId)
		{
			//_client.Create(user, instagramAccountId);

			return false;
		}
		public async Task<bool> Stop(string user, string instagramAccountId)
		{
			return false;
		}


	}
}
