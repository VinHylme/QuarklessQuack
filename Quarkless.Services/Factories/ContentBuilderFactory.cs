using Quarkless.Queue.Jobs.JobOptions;
using Quarkless.Services.Interfaces;
using Quarkless.Services.RequestBuilder;
using QuarklessContexts.Models.ContentBuilderModels;
using QuarklessContexts.Models.Profiles;
using QuarklessContexts.Models.Requests;
using QuarklessLogic.Handlers.ClientProvider;
using System;
using System.Collections.Generic;
using System.Text;

namespace Quarkless.Services.Factories
{
	public abstract class ContentBuilderFactory
	{
		public abstract IContent CreatePost(UserStore userSession, IContentBuilderManager builder, ProfileModel profile, DateTime executeTime);
		//For things like commenting, direct messaging
		public abstract IContent CreateEngagement(UserStore userSession, IContentBuilderManager builder, ProfileModel profile, DateTime executeTime);
	}
}
