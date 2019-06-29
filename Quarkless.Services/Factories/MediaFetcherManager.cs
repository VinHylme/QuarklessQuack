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
		Media,
		Users
	}
	public class MediaFetcherManager
	{
		private readonly Dictionary<FetchType, FetcherFactory> _factories;

		public MediaFetcherManager()
		{
			_factories = new Dictionary<FetchType, FetcherFactory>
			{
				{ FetchType.Media, new  FetchMediaActionFactory() },
				{ FetchType.Users, new FetchUsersActionFactory() }
			};
		}

		public static MediaFetcherManager Begin => new MediaFetcherManager();

		public IFetched Commit(FetchType fetchType, IContentManager actionBuilderManager,
			ProfileModel profile) => _factories[fetchType].Commit(actionBuilderManager, profile);
	}
}

