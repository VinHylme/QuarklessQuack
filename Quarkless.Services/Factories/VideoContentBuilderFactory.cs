using System;
using System.Collections.Generic;
using System.Text;
using Quarkless.Queue.Jobs.JobOptions;
using Quarkless.Services.ContentBuilder.MediaBuilder;
using Quarkless.Services.Interfaces;
using QuarklessContexts.Models.Profiles;

namespace Quarkless.Services.Factories
{
	public class VideoContentBuilderFactory : ContentBuilderFactory
	{
		public override IContent CreateEngagement(UserStore userSession, IContentBuilderManager builder, ProfileModel profile, DateTime executeTime)
		{
			throw new NotImplementedException();
		}
		public override IContent CreatePost(UserStore userSession, IContentBuilderManager builder, ProfileModel profile, DateTime executeTime) => new VideoBuilder(userSession, builder, profile, executeTime);
	}
}
