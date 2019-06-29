using Quarkless.Services.ActionBuilders.OtherActions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Quarkless.Services.Interfaces
{
	public interface IFetched
	{
		IFetchResponse FetchByTopic(int totalTopics = 15, int takeAmount = 2);
		IFetchResponse FetchUsers(int limit, UserFetchType userFetchType);
	}
}
