using Quarkless.Services.Interfaces;
using QuarklessContexts.Models.Profiles;
using QuarklessContexts.Models.Timeline;
using System;
using System.Collections.Generic;
using System.Text;

namespace Quarkless.Services.Factories
{
	public abstract class FetcherFactory
	{
		public abstract IFetched Commit(IContentManager builder, ProfileModel profile);

	}
}
