using Quarkless.Services.Interfaces;
using QuarklessContexts.Models.Profiles;
using QuarklessContexts.Models.Timeline;
using System;

namespace Quarkless.Services.Factories.Content
{
	public abstract class ContentBuilderFactory
	{
		public abstract IContent CreatePost(UserStore userSession, IContentManager builder, ProfileModel profile, DateTime executeTime);
		//For things like commenting, direct messaging
		public abstract IContent CreateEngagement(UserStore userSession, IContentManager builder, ProfileModel profile, DateTime executeTime);
	}
}
