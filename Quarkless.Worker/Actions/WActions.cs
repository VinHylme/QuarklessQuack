using Quarkless.Worker.Actions.ActionFactories;
using QuarklessLogic.Handlers.ClientProvider;
using QuarklessLogic.Handlers.ReportHandler;
using QuarklessLogic.Handlers.Util;
using QuarklessRepositories.Repository.ServicesRepositories;
using System;
using System.Collections.Generic;
using System.Text;

namespace Quarkless.Worker.Actions
{
	public class WActions
	{
		private readonly Dictionary<ActionType, WorkActionsFactory> _factories;
		public WActions()
		{
			_factories = new Dictionary<ActionType, WorkActionsFactory>
			{
				{ ActionType.FetchMedia, new MediaActionFactory() },
				{ ActionType.FetchComments, new CommentActionFactory() },
				{ ActionType.FetchCaptions, new CaptionActionFactory() },
				{ ActionType.FetchBiography, new UserBiographyFactory() }
			};
		}

		public static WActions InitializeFactories => new WActions();
		public IWActions Execute(ActionType actionType, IAPIClientContainer context, IReportHandler reportHandler, string topic, int limit, params IServiceRepository[] serviceRepositories) => _factories[actionType].Create(context, reportHandler, topic, limit,serviceRepositories);

	}
}
