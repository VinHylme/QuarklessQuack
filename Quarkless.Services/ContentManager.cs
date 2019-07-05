using Quarkless.Services.ContentBuilder.TopicBuilder;
using Quarkless.Services.Extensions;
using Quarkless.Services.Interfaces;
using QuarklessContexts.Extensions;
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
			return _textGeneration.MarkovTextGenerator(@"C:\Users\yousef.alaw\source\repos\QuarklessQuark\Requires\Datas\normalised_data\{0}.csv",
				type,topic,lang,size,limit) ;
		}
		public string GenerateMediaInfo(TopicsModel topicSelect, string language)
		{
			var hash = GetHashTags(topicSelect.TopicName, 300, 30).GetAwaiter().GetResult().ToList();
			hash = hash.TakeAny(15).ToList();

			var totaltopics = topicSelect.SubTopics.Select(a=>a.Topic).ToList();
			totaltopics.AddRange(topicSelect.SubTopics.Select(a=>a.RelatedTopics).SquashMe());
			totaltopics.Add(topicSelect.TopicName);
			hash.AddRange(totaltopics.TakeAny(30).Select(s => $"#{s}"));
			var hashtags = hash.Take(29).JoinEvery(Environment.NewLine, 3);
			var caption_ = GenerateText(topicSelect.TopicName.ToLower(), language.ToUpper(), 1, SecureRandom.Next(1,3), SecureRandom.Next(2,6));
			return caption_ + Environment.NewLine + hashtags;
		}
		public string GenerateComment(string topic, string language)
		{
			return GenerateText(topic.ToLower(), language.ToUpper(),0,1,SecureRandom.Next(1,3));
		}

		public Task<TopicsModel> GetTopic(string topic, int takeSuggested = -1, int limit = -1)
		{
			return _topicBuilder.Build(topic,takeSuggested,limit);
		}
	}
}
