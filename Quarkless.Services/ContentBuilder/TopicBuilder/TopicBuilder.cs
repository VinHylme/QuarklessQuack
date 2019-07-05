using InstagramApiSharp;
using Quarkless.Services.Extensions;
using QuarklessContexts.Extensions;
using QuarklessContexts.Models.ServicesModels.DatabaseModels;
using QuarklessContexts.Models.Timeline;
using QuarklessLogic.Handlers.ClientProvider;
using QuarklessLogic.Handlers.ReportHandler;
using QuarklessLogic.Logic.HashtagLogic;
using QuarklessLogic.ServicesLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
namespace Quarkless.Services.ContentBuilder.TopicBuilder
{
	public class TopicBuilder : ITopicBuilder
	{
		//for each topic from the profile generate multiple sub topics 
		//instagram hashtags
		///TO DO
		///Add Google suggested keywords for a given topic

		private readonly ITopicServicesLogic _topicServicesLogic;
		private readonly IReportHandler _reportHandler;
		private readonly IAPIClientContext _context;
		private readonly IHashtagLogic _hashtagLogic;
		private UserStoreDetails tempuser;
		private readonly Random _random;
		private IAPIClientContainer _aPIClientContainer {get; set;}
		public TopicBuilder(ITopicServicesLogic topicServicesLogic, IHashtagLogic hashtagLogic, IReportHandler reportHandler,
			IAPIClientContext aPIClient)
		{
			_topicServicesLogic = topicServicesLogic;
			_reportHandler = reportHandler;
			_context = aPIClient;
			_hashtagLogic = hashtagLogic;
			_random = new Random();
			_reportHandler.SetupReportHandler("topicBuilder");
		}
		public void Init(UserStoreDetails userStore)
		{
			tempuser = userStore;
		}
		public async Task<TopicsModel> Build(string topic, int takeSuggested = 15, int takeHowMany = -1)
		{
			try
			{
				if(takeSuggested < 0) return null;
				//search for similar hashtags
				var dbRes = await _topicServicesLogic.GetTopicByName(topic);
				if(dbRes!=null && dbRes.SubTopics.Count > 3)
				{
					return dbRes;
				}
				_aPIClientContainer = new APIClientContainer(_context, tempuser.OAccountId, tempuser.OInstagramAccountUser);
				if(_aPIClientContainer==null) return null;
				var hashtagsRes = await _aPIClientContainer.Hashtag.SearchHashtagAsync(topic);
				if (hashtagsRes.Succeeded)
				{
					var hashtags = hashtagsRes.Value;
					TopicsModel topic_model = new TopicsModel
					{
						TopicName = topic,
						SubTopics = hashtags.Take(takeSuggested).Select(s => {
							if (s != null)
							{
								var res = _aPIClientContainer.Hashtag
								.GetHashtagsSectionsAsync(s.Name, PaginationParameters.MaxPagesToLoad(1))
								.GetAwaiter().GetResult();
								if (res.Succeeded)
								{
									return new SubTopics
									{
										Topic = s.Name,
										RelatedTopics = res.Value.RelatedHashtags.Select(a => a.Name).ToList()
									};
								}
							}
							return null;
						}).ToList()
					};		
					var resUpdate = await _topicServicesLogic.AddOrUpdateTopic(topic_model);
					if (resUpdate)
					{
						//warrior, warriors, warrior2, artwarrior
						return topic_model;
					}
					_reportHandler.MakeReport("Failed to insert or update topic");
					return topic_model;
				}
				_reportHandler.MakeReport(hashtagsRes.Info);
				return null;
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee);
				return null;
			}
		}

		public async Task<IEnumerable<string>> BuildHashtags(string topic, int limit = 1, int pickRate = 20)
		{
			var res = (await _hashtagLogic.GetHashtagsByTopic(topic,limit)).ToList();
			int min = 0;
			int max = res.Count()-1;	
			List<string> hashtags = new List<string>();
			while (hashtags.Count < pickRate) { 
				var chosenHashtags = res.Select(sh=>sh.Hashtags).ElementAtOrDefault(SecureRandom.Next(min,max))
					.Where(_=>_.Count(count=>count==' ')<=1).Select(s=>$"#{s}");
				hashtags.AddRange(chosenHashtags.Where(s=>s.Length>=3 && s.Length<=20));
			}
			return hashtags;
		}
	}
}
