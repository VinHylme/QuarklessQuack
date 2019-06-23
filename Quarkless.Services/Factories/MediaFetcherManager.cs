using Quarkless.Services.Factories.Actions;
using Quarkless.Services.Interfaces;
using QuarklessContexts.Models.Profiles;
using QuarklessContexts.Models.Timeline;
using System;
using System.Collections.Generic;
using System.Text;

namespace Quarkless.Services.Factories
{
	public enum FetchType
	{
		Media
	}
	public class MediaFetcherManager
	{
		private readonly Dictionary<FetchType, MediaFetcherFactory> _factories;

		public MediaFetcherManager()
		{
			_factories = new Dictionary<FetchType, MediaFetcherFactory>
			{
				{ FetchType.Media, new  FetchMediaActionFactory() }
			};
		}

		public static MediaFetcherManager Begin => new MediaFetcherManager();

		public IMediaFetched Commit(FetchType fetchType, IContentManager actionBuilderManager,
			ProfileModel profile) => _factories[fetchType].Commit(actionBuilderManager, profile);
	}
}

