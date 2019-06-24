using Quarkless.Services.ActionBuilders.OtherActions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Quarkless.Services.Interfaces
{
	public interface IMediaFetched
	{
		FetchResponse FetchByTopic(int totalTopics = 15, int takeAmount = 2);
	}
}
