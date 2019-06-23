using System;
using System.Collections.Generic;
using System.Text;
using Quarkless.Services.ActionBuilders.OtherActions;
using Quarkless.Services.Interfaces;
using QuarklessContexts.Models.Profiles;
using QuarklessContexts.Models.Timeline;

namespace Quarkless.Services.Factories.Actions
{
	public class FetchMediaActionFactory : MediaFetcherFactory
	{
		public override IMediaFetched Commit(IContentManager builder, ProfileModel profile)
			=> new FetchMediaAction(builder,profile);
	}
}
