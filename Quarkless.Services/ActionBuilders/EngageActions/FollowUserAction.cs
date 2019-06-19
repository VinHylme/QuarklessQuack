using Newtonsoft.Json;
using Quarkless.Services.Interfaces;
using QuarklessContexts.Enums;
using QuarklessContexts.Extensions;
using QuarklessContexts.Models.Profiles;
using QuarklessContexts.Models.Timeline;
using QuarklessLogic.Handlers.RequestBuilder.Consts;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Quarkless.Services.ActionBuilders.EngageActions
{
	public class FollowUserAction : IActionCommit
	{
		private readonly UserStore _userStore;
		private readonly IContentManager _builder;
		private readonly ProfileModel _profile;
		private readonly DateTime _executeTime;
		public FollowUserAction(UserStore userSession, IContentManager builder, ProfileModel profile, DateTime executeTime)
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
			var searchPeople = _builder.SearchInstagramUsersByTopic(_userStore, topicSelect.SubTopics.TakeAny(1).FirstOrDefault(), 1);
			var filterPersonToFollow = searchPeople.Where(_ => !_.IsPrivate);
			if (filterPersonToFollow == null) { return; }
			var nominatedPerson = filterPersonToFollow.ElementAtOrDefault(SecureRandom.Next(filterPersonToFollow.Count()));

			RestModel restModel =  new RestModel
			{
				User = _userStore,
				BaseUrl = string.Format(UrlConstants.FollowUser, nominatedPerson.UserId),
				RequestType = RequestType.POST,
				JsonBody = null
			};
			_builder.AddToTimeline(restModel, _executeTime);
		}
	}
}
