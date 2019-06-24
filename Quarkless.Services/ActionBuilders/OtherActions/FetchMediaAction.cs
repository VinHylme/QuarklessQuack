using Quarkless.Services.Interfaces;
using QuarklessContexts.Extensions;
using QuarklessContexts.Models.Profiles;
using QuarklessContexts.Models.ServicesModels.DatabaseModels;
using QuarklessContexts.Models.Timeline;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Quarkless.Services.ActionBuilders.OtherActions
{
	public class FetchResponse
	{
		public TopicsModel SelectedTopic { get; set; }
		public object FetchedItems { get; set; }
		public FetchResponse(TopicsModel topic, object fetchedItems)
		{
			this.SelectedTopic = topic;
			this.FetchedItems = fetchedItems;
		}
	}
	public class FetchMediaAction : IMediaFetched
	{
		private readonly ProfileModel _profile;
		private readonly IContentManager _builder;
		public FetchMediaAction(IContentManager contentBuild, ProfileModel profile)
		{
			_profile = profile;
			_builder = contentBuild;
		}
		public FetchResponse FetchByTopic(int totalTopics = 15, int takeAmount = 2)
		{
			var topicSearch = _builder.GetTopics(_profile.TopicList, totalTopics).GetAwaiter().GetResult();
			var topicSelect = topicSearch.ElementAt(SecureRandom.Next(topicSearch.Count));

			List<string> pickedSubsTopics = topicSelect.SubTopics.TakeAny(takeAmount).ToList();
			pickedSubsTopics.Add(topicSelect.TopicName);
			return new FetchResponse(topicSelect, _builder.SearchMediaDetailInstagram(pickedSubsTopics,1));
		}
	}
}
