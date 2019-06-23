using Quarkless.Services.Interfaces;
using QuarklessContexts.Models.Profiles;
using QuarklessContexts.Models.Timeline;
using System;
using System.Collections.Generic;
using System.Text;

namespace Quarkless.Services.Factories
{
	public abstract class MediaFetcherFactory
	{
		public abstract IMediaFetched Commit(IContentManager builder, ProfileModel profile);

	}
}
