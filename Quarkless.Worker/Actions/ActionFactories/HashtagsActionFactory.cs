using QuarklessLogic.Handlers.ClientProvider;
using QuarklessLogic.Handlers.ReportHandler;
using QuarklessLogic.Handlers.Util;
using QuarklessRepositories.Repository.ServicesRepositories;
using System;
using System.Collections.Generic;
using System.Text;

namespace Quarkless.Worker.Actions.ActionFactories
{
	class HashtagsActionFactory : WorkActionsFactory
	{
		public override IWActions Create(IAPIClientContainer context, IReportHandler reportHandler, string topic, int limit, params IServiceRepository[] serviceRepositories) 
			=> new HashtagsAction(context, reportHandler, topic, limit, serviceRepositories);
	}
}
