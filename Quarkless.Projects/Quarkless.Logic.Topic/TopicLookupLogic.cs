using Quarkless.Models.Topic.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InstagramApiSharp;
using InstagramApiSharp.Classes.Models;
using MongoDB.Bson;
using Quarkless.Events.Interfaces;
using Quarkless.Models.Common.Extensions;
using Quarkless.Models.ContentSearch.Interfaces;
using Quarkless.Models.Profile;
using Quarkless.Models.Profile.Interfaces;
using Quarkless.Models.Topic;
using Quarkless.Models.WorkerManager.Interfaces;
using MoreLinq.Extensions;
using Quarkless.Models.Common.Enums;
using Quarkless.Models.Common.Models;
using Quarkless.Models.Topic.Extensions;

namespace Quarkless.Logic.Topic
{
	public class TopicLookupLogic : ITopicLookupLogic, IEventSubscriber<ProfileTopicAddRequest>
	{
		private readonly IProfileLogic _profileLogic;
		private readonly ITopicLookupRepository _topicLookupRepository;
		private readonly IGoogleSearchLogic _googleSearchLogic;
		private readonly IWorkerManager _workerManager;
		private readonly ISearchingCache _searchingCache;
		public TopicLookupLogic(ITopicLookupRepository repository, IWorkerManager workerManager,
			IGoogleSearchLogic googleSearchLogic, IProfileLogic profileLogic, ISearchingCache searchingCache)
		{
			_workerManager = workerManager;
			_topicLookupRepository = repository;
			_googleSearchLogic = googleSearchLogic;
			_profileLogic = profileLogic;
			_searchingCache = searchingCache;
		}

		public async Task<List<InstaMedia>> GetMediasFromTopic(string topic, int limit)
		{
			var medias = new List<InstaMedia>();
			if (string.IsNullOrEmpty(topic))
				return medias;

			var cacheResults = await _searchingCache.GetSearchData<InstaMedia>(topic);
			if (cacheResults.Any())
				return cacheResults;

			await _workerManager.PerformTaskOnWorkers(async workers =>
			{
				await workers.PerformAction(async worker =>
				{
					var results = await worker.Client.Hashtag.GetTopHashtagMediaListAsync(topic,
						PaginationParameters.MaxPagesToLoad(limit));

					if (results.Succeeded && results.Value.Medias.Count > 0)
					{
						medias.AddRange(results.Value.Medias.OrderByDescending(_ => _.LikesCount).Take(80));
					}
				});
			});

			if (medias.Any())
				await _searchingCache.AddSearchData(topic, medias);
			return medias;
		}

		public async Task<List<HashtagResponse>> BuildRelatedTopics(CTopic topic, bool saveToDb,
			bool includeGoogleSuggest = true, int instagramTopicTakeAmount = 25)
		{
			var responseResults = new List<HashtagResponse>();

			var cacheResults = await _searchingCache.GetSearchData<HashtagResponse>(topic.Name);
			if (cacheResults.Any())
				return cacheResults;

			await _workerManager.PerformTaskOnWorkers(async workers =>
			{
				await workers.PerformAction(async worker =>
				{
					var results = await worker.Client.Hashtag.SearchHashtagAsync(topic
						.Name.RemoveLargeSpacesInText(1, ""));

					if (results.Succeeded && results.Value.Count <= 0)
						return;

					var rankOrdered = results.Value
						.Where(str=>str.Name.IsUsingLatinCharacters())
						.OrderByDescending(_ => _.MediaCount)
						.Take(instagramTopicTakeAmount)
						.Select(_=> new HashtagResponse
						{
							Name = _.Name.RemoveFirstHashtagCharacter(),
							Rarity = (HashtagRarity) _.GetRarity(),
							IsMediaDescriptor = true
						})
						.ToList();

					responseResults.AddRange(rankOrdered);

					if (saveToDb)
					{
						await AddTopics(rankOrdered.Select(_ => new CTopic
						{
							Name = _.Name,
							ParentTopicId = topic._id
						}).ToList());
					}

					if (includeGoogleSuggest)
					{
						var googleSuggest = await _googleSearchLogic
							.GetSuggestions(topic
								.Name.RemoveLargeSpacesInText(1, ""));

						if (!googleSuggest.Any())
							return;

						responseResults.AddRange(googleSuggest.Select(_=> new HashtagResponse
						{
							Name = topic.Name+_,
							Rarity = HashtagRarity.ExternalSuggestion,
							IsMediaDescriptor = true
						}));

						if (saveToDb)
						{
							await AddTopics(googleSuggest.Select(_ =>
							{
								var name = _.RemoveLargeSpacesInText(1, "");
								return new CTopic
								{
									Name = topic.Name + " " + name,
									ParentTopicId = topic._id
								};
							}).ToList());
						}
					}
				});
			});

			if (responseResults.Any())
				await _searchingCache.AddSearchData(topic.Name, responseResults);

			return responseResults;
		}
		public async Task<TopicResponse> AddTopic(CTopic topic, bool includeGoogleSuggest = true, bool saveTopicsSuggested = false)
		{
			if (topic == null || string.IsNullOrEmpty(topic.Name)
							  || string.IsNullOrWhiteSpace(topic.Name)
							  || topic.Name.Length <= 2)
				throw new Exception("ProfileCategoryTopicId cannot be empty");

			var response = await _topicLookupRepository.AddTopic(topic);
			topic._id = response.Id;
			var results = new TopicResponse
			{
				Topic = topic
			};

			if (response.Exists && response.SubTopicsAmount > 0)
				return results;

			var related = await BuildRelatedTopics(topic, saveTopicsSuggested, includeGoogleSuggest);
			results.RelatedTopicsFound = related.Select(_=>_.Name);
			return results;
		}
		
		public async Task<CTopic> GetHighestParent(CTopic topic)
		{
			if (topic._id == null) throw new Exception("Id Cannot be null");
			if (topic.ParentTopicId == BsonObjectId.Empty.AsObjectId.ToString()) return topic;

			var results = await GetTopicById(topic.ParentTopicId);

			if (results == null) return null;

			return await GetHighestParent(results);
		}

		public async Task<List<CTopic>> GetHighestParents(CTopic topic)
		{
			if (topic._id == null) throw new Exception("Id Cannot be null");
			var topicResults = new Stack<CTopic>();
			topicResults.Push(topic);

			if (topic.ParentTopicId == BsonObjectId.Empty.AsObjectId.ToString())
				return topicResults.ToList();

			const int maxIteration = 100;
			var current = 0;

			do
			{
				var results = await GetTopicById(topic.ParentTopicId);
				if (results == null) break;

				topicResults.Push(results);

				if (results.ParentTopicId == BsonObjectId.Empty.AsObjectId.ToString())
					break;

				topic = results;
				current++;

			} while (current < maxIteration);

			return topicResults.ToList();
		}

		public async Task ResetTopics()
		{
			var categories = await GetCategories();
			var totalTopics = await GetAllTopics();

			var totalCategorySubTopics = new List<CTopic>();
			foreach (var category in categories)
			{
				totalCategorySubTopics.AddRange(await GetTopicsByParentId(category._id));
			}

			var deleteList = totalTopics.Where(s=>ObjectId.Parse(s.ParentTopicId) != ObjectId.Empty)
				.Where(_ => !totalCategorySubTopics
					.Any(s => s.Name.Equals(_.Name))).ToList();

			var deleteResults = await DeleteAll(deleteList.Select(x => x._id).ToArray());
		}

		public async Task<List<string>> AddTopics(List<CTopic> topics)
		{
			return await _topicLookupRepository.AddTopics(topics);
		}

		public async Task<CTopic> GetTopicById(string id)
			=> await _topicLookupRepository.GetTopicById(id);

		public async Task<List<CTopic>> GetTopicsByParentId(string parentId, bool buildIfNotExists = false)
		{
			var results =  await _topicLookupRepository.GetTopicsByParentId(parentId);
			if (results.Any())
				return results;
			
			if (!buildIfNotExists)
				return results;

			var topicDetail = await _topicLookupRepository.GetTopicById(parentId);
			if (topicDetail == null)
				return results;
			
			await BuildRelatedTopics(topicDetail, true, false);
			return await _topicLookupRepository.GetTopicsByParentId(parentId);
		}

		public async Task<List<CTopic>> GetTopicByNameLike(string name)
			=> await _topicLookupRepository.GetTopicsNameLike(name);
		public async Task<List<CTopic>> GetTopicByName(string name)
			=> await _topicLookupRepository.GetTopicsName(name);
		public async Task<List<CTopic>> GetCategories()
			=> await _topicLookupRepository.GetCategories();
		public async Task<List<CTopic>> GetAllTopics()
			=> await _topicLookupRepository.GetAllTopics();

		public async Task<long> DeleteAll(params string[] topicIds)
			=> await _topicLookupRepository.DeleteAll(topicIds);

		public async Task<long> DeleteAll()
			=> await _topicLookupRepository.DeleteAll();

		public async Task Handle(ProfileTopicAddRequest @event)
		{
			var profileTopicsUpdate = new List<CTopic>();
			foreach (var topic in @event.Topics)
			{
				topic.Name = topic.Name.RemoveNonWordsFromText()
					.RemoveLargeSpacesInText(1, "")
					.RemoveHashtagsFromText()
					.RemoveCurrencyFromText();

				if(string.IsNullOrEmpty(topic.Name) || topic.Name.Length <=2)
					continue;

				var results = await AddTopic(topic, includeGoogleSuggest:false, saveTopicsSuggested:true);
				if (string.IsNullOrEmpty(results.Topic._id)) continue;
				topic._id = results.Topic._id;
				profileTopicsUpdate.Add(topic);
			}

			var profile = await _profileLogic.GetProfile(@event.ProfileId);

			//profileTopicsUpdate.AddRange(profile.ProfileTopic.Topics);
			var updatedTopic = new Models.Profile.Topic
			{
				Category = profile.ProfileTopic.Category,
				Topics = DistinctByExtension.DistinctBy(profileTopicsUpdate, _ => _.Name).ToList()
			};

			await _profileLogic.PartialUpdateProfile(@event.ProfileId, new ProfileModel
			{
				ProfileTopic = updatedTopic
			});
		}
	}
}
