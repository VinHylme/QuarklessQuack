using InstagramApiSharp;
using MoreLinq;
using QuarklessContexts.Extensions;
using QuarklessContexts.Models.Profiles;
using QuarklessContexts.Models.ServicesModels.DatabaseModels;
using QuarklessContexts.Models.Timeline;
using QuarklessContexts.Models.Topics;
using QuarklessLogic.Handlers.ClientProvider;
using QuarklessLogic.Handlers.ReportHandler;
using QuarklessLogic.Logic.HashtagLogic;
using QuarklessLogic.Logic.ProfileLogic;
using QuarklessLogic.ServicesLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using QuarklessLogic.ServicesLogic.CorpusLogic;
using SubTopics = QuarklessContexts.Models.Profiles.SubTopics;

namespace Quarkless.Services.ContentBuilder.TopicBuilder
{
	public class TopicBuilder : ITopicBuilder
	{
		//for each topic from the profile generate multiple sub topics 
		//instagram hashtags
		///TO DO
		///Add Google suggested keywords for a given topic

		private readonly ITopicServicesLogic _topicServicesLogic;
		private readonly IProfileLogic _profileLogic;
		private readonly IReportHandler _reportHandler;
		private readonly IAPIClientContext _context;
		private readonly IHashtagLogic _hashtagLogic;
		private readonly IMediaCorpusLogic _mediaCorpusLogic;
		private UserStoreDetails tempuser;
		private IAPIClientContainer _aPIClientContainer {get; set;}
		public TopicBuilder(ITopicServicesLogic topicServicesLogic, IProfileLogic profileLogic, IHashtagLogic hashtagLogic, IReportHandler reportHandler,
			IAPIClientContext aPIClient, IMediaCorpusLogic mediaCorpusLogic, ICommentCorpusLogic commentCorpusLogic)
		{
			_profileLogic = profileLogic;
			_topicServicesLogic = topicServicesLogic;
			_reportHandler = reportHandler;
			_context = aPIClient;
			_hashtagLogic = hashtagLogic;
			_mediaCorpusLogic = mediaCorpusLogic;
			_reportHandler.SetupReportHandler("topicBuilder");
		}
		public void Init(UserStoreDetails userStore)
		{
			tempuser = userStore;
		}
		public async Task BuildTopics(IEnumerable<TopicCategories> topicCategories)
		{
			_aPIClientContainer = new APIClientContainer(_context, tempuser.OAccountId, tempuser.OInstagramAccountUser);
			if (_aPIClientContainer == null) return;
			foreach(var topic in topicCategories)
			{
				var subtopics = new List<SubTopics>();
				foreach(var subCategory in topic.SubCategories)
				{
					try {
						var topics = new SubTopics {TopicName = subCategory, RelatedTopics = new List<string>()};
						var makeSearchableCategory = Regex.Replace(subCategory, @"[^\w\d]", "");
						var hashtagResults = await _aPIClientContainer.Hashtag.SearchHashtagAsync(makeSearchableCategory);
						if (hashtagResults.Succeeded)
						{
							topics.RelatedTopics.AddRange(hashtagResults.Value.Where(s=>s.NonViolating).Select(c=>c.Name));
							foreach(var hashtag in hashtagResults.Value.Take(20))
							{
								var sectionRes = await _aPIClientContainer.Hashtag.GetHashtagsSectionsAsync(hashtag.Name, PaginationParameters.MaxPagesToLoad(1));
								if (!sectionRes.Succeeded) continue;
								topics.RelatedTopics.AddRange(sectionRes.Value.RelatedHashtags.Select(sx=>sx.Name));
								await Task.Delay(700);
							}
						}
						await _topicServicesLogic.AddRelated(topics);
						subtopics.Add(new SubTopics
						{
							TopicName = topics.TopicName,
							RelatedTopics =  topics.RelatedTopics
						});
					}
					catch(Exception e)
					{
						Console.WriteLine(e.Message);
					} 
				}

				await _topicServicesLogic.AddOrUpdateTopic(new TopicsModel
				{
					TopicName = topic.CategoryName,
					SubTopics = subtopics.Select(p=>new QuarklessContexts.Models.ServicesModels.DatabaseModels.SubTopics
					{
						Topic = p.TopicName,
						RelatedTopics = p.RelatedTopics
					}).ToList()
				});
			}
		}
		public async Task<Topics> BuildTopics(ProfileModel profile, int takeSuggested = 15)
		{
			try
			{
				if (takeSuggested < 0) return null;
				
				_aPIClientContainer = new APIClientContainer(_context, tempuser.OAccountId, tempuser.OInstagramAccountUser);
				if (_aPIClientContainer == null) return null;
				var hashtagsRes = await _aPIClientContainer.Hashtag.SearchHashtagAsync(profile.Topics.TopicFriendlyName);

				if (hashtagsRes.Succeeded)
				{
					var related = new List<string>();
					if (profile.Topics.SubTopics != null) { 
						foreach(var srt in profile.Topics.SubTopics)
						{
							var res = await _aPIClientContainer.Hashtag.GetHashtagsSectionsAsync(srt.TopicName.Replace(" ",""),PaginationParameters.MaxPagesToLoad(1));
							if(res.Succeeded && res.Value.RelatedHashtags.Count > 3)
							{
								srt.RelatedTopics = res.Value.RelatedHashtags.Select(t=>t.Name).ToList();
							}
							else
							{
								var resSearch = await _aPIClientContainer.Hashtag.SearchHashtagAsync(srt.TopicName.Replace(" ", ""));
								if (resSearch.Succeeded)
								{
									srt.RelatedTopics = resSearch.Value.Select(t=>t.Name).ToList();
								}
							}
						}
					}

					var hashtags = hashtagsRes.Value;
					var subTopics = new List<QuarklessContexts.Models.Profiles.SubTopics>();
					if (profile.AutoGenerateTopics) { 
						subTopics = hashtags.Take(takeSuggested).Select(s => {
							if (s == null) return null;
							var res = _aPIClientContainer.Hashtag
								.GetHashtagsSectionsAsync(s.Name, PaginationParameters.MaxPagesToLoad(1))
								.GetAwaiter().GetResult();
							if (!res.Succeeded) return null;
							if (res.Value.RelatedHashtags.Count > 0) { 
								return new QuarklessContexts.Models.Profiles.SubTopics
								{
									TopicName = s.Name,
									RelatedTopics = res.Value.RelatedHashtags.Select(a => a.Name).ToList()
								};
							}
							else
							{
								return null;
							}
						}).Where(s=>s!=null).ToList();
					}

					if (profile.Topics.SubTopics != null && profile.Topics.SubTopics.Count > 0)
					{
						subTopics.AddRange(profile.Topics.SubTopics);
					}
					subTopics = subTopics.Distinct().ToList();
					var newtopics = new Topics
					{
						TopicType = profile.Topics.TopicType,
						SubTopics = subTopics,
						TopicFriendlyName = profile.Topics.TopicFriendlyName
					};
					var updateProfile = await _profileLogic.PartialUpdateProfile(profile._id,new ProfileModel
					{
						Topics = newtopics
					});

					return newtopics;
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
									return new QuarklessContexts.Models.ServicesModels.DatabaseModels.SubTopics
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
		public async Task<IEnumerable<string>> BuildHashtags(string topic, string subcategory, string language, int limit = 1, int pickRate = 20)
		{
			var res = (await _hashtagLogic.GetHashtagsByTopicAndLanguage(topic.OnlyWords(), language.ToUpper().OnlyWords(), language.MapLanguages().OnlyWords(),limit)).Shuffle().ToList();
			var clean = new Regex(@"[^\w\d]");
			if (res.Count <= 0) return null;
			var hashtags = new List<string>();
			while (hashtags.Count < pickRate) { 
				var chosenHashtags = new List<string>();

				foreach(var hashtagres in res)
				{
					if (string.IsNullOrEmpty(hashtagres.Language)) continue;
					var hlang = clean.Replace(hashtagres.Language.ToLower(),"");
					var langpicked = clean.Replace(language.MapLanguages().ToLower(),"");

					if(hlang == langpicked)
						chosenHashtags.AddRange(hashtagres.Hashtags);
				}

				if (chosenHashtags.Count <= 0) continue;
				var chosenHashtagsFiltered = chosenHashtags.Where(space => space.Count(oc => oc == ' ') <= 1);
				var hashtagsFiltered = chosenHashtagsFiltered as string[] ?? chosenHashtagsFiltered.ToArray();
				if (!hashtagsFiltered.Any()) return null;
				hashtags.AddRange(hashtagsFiltered.Where(s => s.Length >= 3 && s.Length <= 30));
			}
			return hashtags;

			///code here is for brining back hashtags similar to the subcategory
			//if (subcategory != null) { 
			//	subcategory = Regex.Replace(subcategory, @"\w*.?com", "");
			//	hashtags.ForEach(s=>s.ToLower());
			//	var orderedBySimilarity = hashtags.Distinct().Select(s => new { SimilarityScore = s.Similarity(subcategory), Text = s })
			//		.OrderBy(s => s.SimilarityScore).Select(t=>t.Text);

			//	var filt = orderedBySimilarity.Select(r => r.Replace("\t","").Replace("\n","").Replace("\r","").Replace(".","").Replace(" ",""))
			//		.Where(x=>!string.IsNullOrEmpty(x)).Take(100);
			//	return filt;
			//}
			//else
			//{
			//	return hashtags;
			//}
		}

		public async Task AddTopicCategories(IEnumerable<TopicCategories> topicCategories) => await _topicServicesLogic.AddTopicCategories(topicCategories);

		public Task<IEnumerable<TopicCategories>> GetAllTopicCategories()
		{
			return _topicServicesLogic.GetAllTopicCategories();
		}
		public async Task<SubTopics> GetAllRelatedTopics(string topic)
		{
			return await _topicServicesLogic.GetAllRelatedTopics(topic);
		}

		public async Task<IEnumerable<TopicsModel>> GetTopics()
		{
			return await _topicServicesLogic.GetTopics();
		}
		public async Task Update(string selected, string subItem)
		{
			await _mediaCorpusLogic.UpdateTopicName(selected, subItem);
		}
	}
}
