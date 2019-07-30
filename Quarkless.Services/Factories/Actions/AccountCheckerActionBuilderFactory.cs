using System;
using System.Collections.Generic;
using System.Text;
using Quarkless.Services.ActionBuilders.MaintainActions;
using Quarkless.Services.Interfaces;
using QuarklessContexts.Models.Profiles;
using QuarklessLogic.ServicesLogic.HeartbeatLogic;

namespace Quarkless.Services.Factories.Actions
{
	public class AccountCheckerActionBuilderFactory : ActionBuilderFactory
	{
		public override IActionCommit Commit(IContentManager builder, IHeartbeatLogic heartbeatLogic)
			=> new AccountCheckerAction(builder,heartbeatLogic);
	}
}
