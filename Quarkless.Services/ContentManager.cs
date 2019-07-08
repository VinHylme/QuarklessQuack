using Quarkless.Services.ContentBuilder.TopicBuilder;
using Quarkless.Services.Extensions;
using Quarkless.Services.Interfaces;
using QuarklessContexts.Extensions;
using QuarklessContexts.Models.Profiles;
using QuarklessContexts.Models.ServicesModels.DatabaseModels;
using QuarklessLogic.Handlers.TextGeneration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Quarkless.Services
{
	public class ContentManager : IContentManager
	{
		private readonly ITopicBuilder _topicBuilder; 
		private readonly ITextGeneration _textGeneration;
		public ContentManager(ITopicBuilder topicBuilder, ITextGeneration textGeneration)
		{
			_topicBuilder = topicBuilder;
			_textGeneration = textGeneration;
		}
		public async Task<List<TopicsModel>> GetTopics(List<string> topics, int takeSuggested = -1, int limit = -1)
		{
			List<TopicsModel> totalFound = new List<TopicsModel>();
			foreach(var topic in topics) { 
				var res = await _topicBuilder.Build(topic, takeSuggested, limit);
				if(res!=null)
					totalFound.Add(res);
			}

			return totalFound;
		}
		public async Task<IEnumerable<string>> GetHashTags (string topic, int limit, int pickAmount)
		{
			return await _topicBuilder.BuildHashtags(topic,limit,pickAmount);
		}
		public string GenerateText(string topic,string lang, int type, int limit, int size)
		{
			//return _textGeneration.MarkovTextGenerator(@"C:\Users\yousef.alaw\source\repos\QuarklessQuark\Requires\Datas\normalised_data\{0}.csv",
			//	type,topic,lang,size,limit) ;
			return _textGeneration.MarkovIt(type,topic,lang,size,limit).GetAwaiter().GetResult();
		}
		public string GenerateMediaInfo(Topics topicSelect, string language, string credit = null)
		{
			var hash = GetHashTags(topicSelect.TopicFriendlyName, 300, 30).GetAwaiter().GetResult().ToList();
			hash = hash.TakeAny(15).ToList();

			var totaltopics = topicSelect.SubTopics.Select(a=>a.TopicName).ToList();
			totaltopics.AddRange(topicSelect.SubTopics.Select(a=>a.RelatedTopics).SquashMe());
			totaltopics.Add(topicSelect.TopicFriendlyName);
			hash.AddRange(totaltopics.TakeAny(30).Select(s => $"#{s}"));
			var hashtags = hash.Take(29).JoinEvery(Environment.NewLine, 3);
			var caption_ = GenerateText(topicSelect.TopicFriendlyName.ToLower(), language.ToUpper(), 1, SecureRandom.Next(1,3), SecureRandom.Next(2,6)).Split(',')[0];
			string creditLine = string.Empty;
			if (credit != null)
				creditLine = $"credit: @{credit}";

			return caption_ + Environment.NewLine + creditLine + Environment.NewLine + hashtags;
		}
		public string GenerateComment(string topic, string language)
		{
			var comment = GenerateText(topic.ToLower(), language.ToUpper(), 0 , SecureRandom.Next(1,3) ,SecureRandom.Next(3,10)).Split(',')[0];
			return comment;
		}

		public Task<Topics> GetTopic(ProfileModel profile, int takeSuggested = -1)
		{
			return _topicBuilder.BuildTopics(profile,takeSuggested);
		}
	}
}
