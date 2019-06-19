using System;
using Quarkless.Services.ContentBuilder.MediaBuilder;
using Quarkless.Services.Factories.Content;
using Quarkless.Services.Interfaces;
using QuarklessContexts.Models.Profiles;
using QuarklessContexts.Models.Timeline;

namespace Quarkless.Services.Factories
{
	public class VideoContentBuilderFactory : ContentBuilderFactory
	{
		public override IContent CreateEngagement(UserStore userSession, IContentManager builder, ProfileModel profile, DateTime executeTime)
		{
			throw new NotImplementedException();
		}
		public override IContent CreatePost(UserStore userSession, IContentManager builder, ProfileModel profile, DateTime executeTime) => new VideoBuilder(userSession, builder, profile, executeTime);
	}
}
