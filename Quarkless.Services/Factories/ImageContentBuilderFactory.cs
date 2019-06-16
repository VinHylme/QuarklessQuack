using System;
using System.Collections.Generic;
using System.Text;
using Quarkless.Queue.Jobs.JobOptions;
using Quarkless.Services.ContentBuilder.MediaBuilder;
using Quarkless.Services.Interfaces;
using Quarkless.Services.RequestBuilder;
using QuarklessContexts.Models.ContentBuilderModels;
using QuarklessContexts.Models.Profiles;
using QuarklessContexts.Models.Requests;
using QuarklessLogic.Handlers.ClientProvider;

namespace Quarkless.Services.Factories
{
	public class ImageContentBuilderFactory : ContentBuilderFactory
	{
		public override IContent CreateEngagement(UserStore userSession, IContentBuilderManager builder, ProfileModel profile, DateTime executeTime) { throw new NotImplementedException(); }

		public override IContent CreatePost(UserStore userSession, IContentBuilderManager builder, ProfileModel profile, DateTime executeTime) => new ImageBuilder(userSession, builder, profile, executeTime);
	}
}
