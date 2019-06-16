using QuarklessContexts.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace QuarklessLogic.Handlers.ClientProvider
{
	public abstract class APIClientContextFactory
	{
		public abstract Task<ContextContainer> Create(string userId, string InstaId);
	}
}
