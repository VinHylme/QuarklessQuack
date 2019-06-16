using System;
using System.Collections.Generic;
using System.Text;
using QuarklessLogic.Handlers.ClientProvider;
using QuarklessLogic.Handlers.ReportHandler;
using QuarklessLogic.Handlers.Util;
using QuarklessRepositories.Repository.ServicesRepositories;

namespace Quarkless.Worker.Actions.ActionFactories
{
	public class MediaActionFactory : WorkActionsFactory
	{
		public override IWActions Create(IAPIClientContainer context, IReportHandler reportHandler, string topic, int limit, params IServiceRepository[] serviceRepositories) => 
			new MediaAction(context, reportHandler, topic, limit, serviceRepositories);
	}
}
