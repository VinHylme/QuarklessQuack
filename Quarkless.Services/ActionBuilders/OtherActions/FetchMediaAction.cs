using Quarkless.Services.Interfaces;
using QuarklessContexts.Extensions;
using QuarklessContexts.Models.Profiles;
using QuarklessContexts.Models.Timeline;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Quarkless.Services.ActionBuilders.OtherActions
{
	public class FetchMediaAction : IMediaFetched
	{
		private readonly ProfileModel _profile;
		private readonly IContentManager _builder;
		public FetchMediaAction(IContentManager contentBuild, ProfileModel profile)
		{
			_profile = profile;
			_builder = contentBuild;
		}
		public object FetchByTopic()
		{
			var topicSearch = _builder.GetTopics(_profile.TopicList, 15).GetAwaiter().GetResult();
			var topicSelect = topicSearch.ElementAt(SecureRandom.Next(topicSearch.Count));

			List<string> pickedSubsTopics = topicSelect.SubTopics.TakeAny(1).ToList();
			pickedSubsTopics.Add(topicSelect.TopicName);
			return _builder.SearchMediaDetailInstagram(pickedSubsTopics, 1);
		}
	}
}
