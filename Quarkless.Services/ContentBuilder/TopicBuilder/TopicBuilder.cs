using InstagramApiSharp;
using MoreLinq;
using Quarkless.Services.Extensions;
using QuarklessContexts.Extensions;
using QuarklessContexts.Models.Profiles;
using QuarklessContexts.Models.ServicesModels.DatabaseModels;
using QuarklessContexts.Models.Timeline;
using QuarklessLogic.Handlers.ClientProvider;
using QuarklessLogic.Handlers.ReportHandler;
using QuarklessLogic.Logic.HashtagLogic;
using QuarklessLogic.Logic.ProfileLogic;
using QuarklessLogic.ServicesLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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
		private readonly IProfileLogic _profileLogic;
		private readonly IReportHandler _reportHandler;
		private readonly IAPIClientContext _context;
		private readonly IHashtagLogic _hashtagLogic;
		private UserStoreDetails tempuser;
		private readonly Random _random;
		private IAPIClientContainer _aPIClientContainer {get; set;}
		public TopicBuilder(ITopicServicesLogic topicServicesLogic,IProfileLogic profileLogic, IHashtagLogic hashtagLogic, IReportHandler reportHandler,
			IAPIClientContext aPIClient)
		{
			_profileLogic = profileLogic;
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
					List<string> related = new List<string>();
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
								var res_search = await _aPIClientContainer.Hashtag.SearchHashtagAsync(srt.TopicName.Replace(" ", ""));
								if (res_search.Succeeded)
								{
									srt.RelatedTopics = res_search.Value.Select(t=>t.Name).ToList();
								}
							}
						}
					}

					var hashtags = hashtagsRes.Value;
					List<QuarklessContexts.Models.Profiles.SubTopics> subTopics = new List<QuarklessContexts.Models.Profiles.SubTopics>();
					if (profile.AutoGenerateTopics) { 
						subTopics = hashtags.Take(takeSuggested).Select(s => {
							if (s != null)
							{
								var res = _aPIClientContainer.Hashtag
								.GetHashtagsSectionsAsync(s.Name, PaginationParameters.MaxPagesToLoad(1))
								.GetAwaiter().GetResult();
								if (res.Succeeded)
								{
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
								}
							}
							return null;
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
			var res = (await _hashtagLogic.GetHashtagsByTopicAndLanguage(topic, language.ToUpper(), language.MapLanguages(),limit)).Shuffle().ToList();
			Regex clean = new Regex(@"[^\w\d]");
			if(res!=null && res.Count > 0) { 
				List<string> hashtags = new List<string>();
				while (hashtags.Count < pickRate) { 
					List<string> chosenHashtags = new List<string>();

					foreach(var hashtagres in res)
					{
						if(!string.IsNullOrEmpty(hashtagres.Language)){
							var hlang = clean.Replace(hashtagres.Language.ToLower(),"");
							var langpicked = clean.Replace(language.MapLanguages().ToLower(),"");

							if(hlang == langpicked)
								chosenHashtags.AddRange(hashtagres.Hashtags);
						}
					}

					if (chosenHashtags.Count > 0) { 
						var chosenHashtags_filtered = chosenHashtags.Where(space => space.Count(oc => oc == ' ') <= 1);
						if (chosenHashtags_filtered.Count() <=0) return null;
						hashtags.AddRange(chosenHashtags_filtered.Where(s => s.Length >= 3 && s.Length <= 20));
					}
				}
				if (subcategory != null) { 
					subcategory = Regex.Replace(subcategory, @"\w*.?com", "");
					hashtags.ForEach(s=>s.ToLower());
					var orderedBySimilarity = hashtags.Distinct().Select(s => new { SimilarityScore = s.Similarity(subcategory), Text = s })
						.OrderBy(s => s.SimilarityScore).Select(t=>t.Text);

					var filt = orderedBySimilarity.Select(r => r.Replace("\t","").Replace("\n","").Replace("\r","").Replace(".","").Replace(" ",""))
						.Where(x=>!string.IsNullOrEmpty(x)).Take(80).Shuffle();
					return filt;
				}
				else
				{
					return hashtags;
				}
			}
			return null;
		}
	}
}
