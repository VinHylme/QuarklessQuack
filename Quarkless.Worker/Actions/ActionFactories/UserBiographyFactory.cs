using System;
using System.Collections.Generic;
using System.Text;
using QuarklessLogic.Handlers.ClientProvider;
using QuarklessLogic.Handlers.ReportHandler;
using QuarklessLogic.Handlers.Util;
using QuarklessRepositories.Repository.ServicesRepositories;

namespace Quarkless.Worker.Actions.ActionFactories
{
	public class UserBiographyFactory : WorkActionsFactory
	{
		public override IWActions Create(IAPIClientContainer context, IReportHandler reportHandler, string topic, int limit, params IServiceRepository[] serviceRepository) 
			=> new UserBiographyAction(context, reportHandler, topic, limit, serviceRepository);

	}
}
