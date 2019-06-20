using Quarkless.Services.Interfaces;
using QuarklessContexts.Enums;
using QuarklessContexts.Extensions;
using QuarklessContexts.Models.Profiles;
using QuarklessContexts.Models.Timeline;
using QuarklessLogic.Handlers.RequestBuilder.Consts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Quarkless.Services.ActionBuilders.EngageActions
{
	public class LikeMediaAction : IActionCommit
	{
		private readonly UserStore _userStore;
		private readonly IContentManager _builder;
		private readonly ProfileModel _profile;
		private readonly DateTime _executeTime;
		public LikeMediaAction(UserStore userSession, IContentManager builder, ProfileModel profile, DateTime executeTime)
		{
			_userStore = userSession;
			_builder = builder;
			_profile = profile;
			_executeTime = executeTime;
		}
		public void Operate()
		{
			var topicSearch = _builder.GetTopics(_userStore, _profile.TopicList, 15).GetAwaiter().GetResult();
			var topicSelect = topicSearch.ElementAt(SecureRandom.Next(topicSearch.Count));
			var searchMedias = _builder.GetMediaDetailInstagram(_userStore, topicSelect.SubTopics.TakeAny(3).ToList(),1);

			var nominatedMedia = searchMedias.ElementAt(SecureRandom.Next(searchMedias.Count()-1));

			RestModel restModel = new RestModel
			{
				User = _userStore,
				BaseUrl = string.Format(UrlConstants.LikeMedia, nominatedMedia.MediaId),
				RequestType = RequestType.POST,
				JsonBody = null
			};
			_builder.AddToTimeline(restModel, _executeTime);
		}
	}
}
