using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MoreLinq;
using QuarklessContexts.Extensions;
using QuarklessContexts.Models.Profiles;
using QuarklessContexts.Models.Topics;
using QuarklessLogic.ContentSearch.GoogleSearch;
using QuarklessLogic.Handlers.EventHandlers;
using QuarklessLogic.Handlers.WorkerManagerService;
using QuarklessLogic.Logic.ProfileLogic;
using QuarklessRepositories.Repository.TopicLookupRepository;

namespace QuarklessLogic.Logic.TopicLookupLogic
{
	public class TopicLookupLogic : ITopicLookupLogic, IEventSubscriber<ProfileTopicAddRequest>
	{
		private readonly IProfileLogic _profileLogic;
		private readonly ITopicLookupRepository _topicLookupRepository;
		private readonly IGoogleSearchLogic _googleSearchLogic;
		private readonly IWorkerManager _workerManager;
		public TopicLookupLogic(ITopicLookupRepository repository, IWorkerManager workerManager, 
			IGoogleSearchLogic googleSearchLogic, IProfileLogic profileLogic)
		{
			_workerManager = workerManager;
			_topicLookupRepository = repository;
			_googleSearchLogic = googleSearchLogic;
			_profileLogic = profileLogic;
		}

		public async Task<string> AddTopic(CTopic topic)
		{
			if (topic == null || string.IsNullOrEmpty(topic.Name) 
			                  || string.IsNullOrWhiteSpace(topic.Name) 
			                  || topic.Name.Length<=2) 
				throw new Exception("Topic cannot be empty");

			var response = await _topicLookupRepository.AddTopic(topic);

			if (response.Exists)
				return response.Id;

			var topicCreatedId = response.Id;

			const int instagramTopicTakeAmount = 7;
			await _workerManager.PerformTaskOnWorkers(async workers =>
			{
				await workers.PerformAction(async worker =>
					{
						var results = await worker.Client.Hashtag.SearchHashtagAsync(topic
							.Name.RemoveLargeSpacesInText(1, ""));

						if (results.Succeeded && results.Value.Count <= 0)
							return;
						else
						{
							var rankOrdered = results.Value
								.OrderByDescending(_ => _.MediaCount)
								.Take(instagramTopicTakeAmount);
							
							await AddTopics(rankOrdered.Select(_ => new CTopic
							{
								Name = _.Name, 
								ParentTopicId = topicCreatedId
							}).ToList());

							var googleSuggest = await _googleSearchLogic
								.GetSuggestions(topic
								.Name.RemoveLargeSpacesInText(1, ""));

							if (!googleSuggest.Any())
								return;

							await AddTopics(googleSuggest.Select(_ =>
							{
								var name = _.RemoveLargeSpacesInText(1,"");
								return new CTopic
								{
									Name = name,
									ParentTopicId = topicCreatedId
								};
							}).ToList());
						}
					});
			});
			return topicCreatedId;
		}

		public async Task<List<string>> AddTopics(List<CTopic> topics)
		{
			return await _topicLookupRepository.AddTopics(topics);
		}
		public async Task<CTopic> GetTopicById(string id) 
			=> await _topicLookupRepository.GetTopicById(id);

		public async Task<List<CTopic>> GetTopicByParentId(string parentId) 
			=> await _topicLookupRepository.GetTopicsByParentId(parentId);

		public async Task<List<CTopic>> GetTopicByNameLike(string name)
			=> await _topicLookupRepository.GetTopicsNameLike(name);
		public async Task<List<CTopic>> GetTopicByName(string name)
			=> await _topicLookupRepository.GetTopicsName(name);

		public async Task<List<CTopic>> GetCategories()
			=> await _topicLookupRepository.GetCategories();

		public async Task<long> DeleteAll()
			=> await _topicLookupRepository.DeleteAll();

		public async Task Handle(ProfileTopicAddRequest @event)
		{
			var profileTopicsUpdate = new List<CTopic>();
			foreach (var topic in @event.Topics)
			{
				var id = await AddTopic(topic);
				if (string.IsNullOrEmpty(id)) continue;
				topic._id = id;
				profileTopicsUpdate.Add(topic);
			}

			var profile = await _profileLogic.GetProfile(@event.ProfileId);
			profileTopicsUpdate.AddRange(profile.ProfileTopic.Topics);
			var updatedTopic = new Topic
			{
				Category = profile.ProfileTopic.Category,
				Topics = profileTopicsUpdate.DistinctBy(_=>_.Name).ToList()
			};

			await _profileLogic.PartialUpdateProfile(@event.ProfileId, new ProfileModel
			{
				ProfileTopic = updatedTopic
			});
		}
	}
}
