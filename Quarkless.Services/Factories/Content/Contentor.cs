using Quarkless.Services.Factories.Content;
using Quarkless.Services.Interfaces;
using QuarklessContexts.Models.Profiles;
using QuarklessContexts.Models.Timeline;
using System;
using System.Collections.Generic;

namespace Quarkless.Services.Factories
{
	public class Contentor
	{
		private readonly Dictionary<ContentType, ContentBuilderFactory> _factories;

		public Contentor()
		{
			_factories = new Dictionary<ContentType, ContentBuilderFactory>
			{
				{ ContentType.Image, new ImageContentBuilderFactory() },
				{ ContentType.Video, new VideoContentBuilderFactory() }
			};
		}

		public static Contentor Begin => new Contentor();
		public IContent ExecutePost(ContentType contentType, UserStore userSession, IContentManager contentManager, 
			ProfileModel profile, DateTime executionTime) 
			=> _factories[contentType].CreatePost(userSession,contentManager, profile, executionTime);

		public IContent ExecuteEngagment(ContentType contentType, UserStore userSession, IContentManager contentManager, 
			ProfileModel profile, DateTime executionTime) 
			=> _factories[contentType].CreateEngagement(userSession,contentManager, profile,executionTime);

	}
}
