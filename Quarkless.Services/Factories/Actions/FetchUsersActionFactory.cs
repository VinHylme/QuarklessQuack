using System;
using System.Collections.Generic;
using System.Text;
using Quarkless.Services.ActionBuilders.OtherActions;
using Quarkless.Services.Interfaces;
using QuarklessContexts.Models.Profiles;

namespace Quarkless.Services.Factories.Actions
{
	public class FetchUsersActionFactory : FetcherFactory
	{
		public override IFetched Commit(IContentManager builder, ProfileModel profile) => new FetchUserAction(builder,profile);
	}
}
