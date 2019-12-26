using System;
using QuarklessContexts.Models.Profiles;
using QuarklessRepositories.ProfileRepository;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MoreLinq;
using QuarklessContexts.Models.InstagramAccounts;
using QuarklessContexts.Models.Topics;
using QuarklessLogic.Handlers.EventHandlers;

namespace QuarklessLogic.Logic.ProfileLogic
{
	public class ProfileLogic : IProfileLogic, IEventSubscriber<InstagramAccountModel>
	{
		private readonly IProfileRepository _profileRepository;
		private readonly IEventPublisher _eventPublisher;
		public ProfileLogic(IProfileRepository profileRepository, IEventPublisher eventPublisher)
		{
			_profileRepository = profileRepository;
			_eventPublisher = eventPublisher;
		}

		public Task<bool> AddMediaUrl(string profileId, string mediaUrl)
		{
			return _profileRepository.AddMediaUrl(profileId, mediaUrl);
		}

		public async Task<ProfileModel> AddProfile(ProfileModel profile, bool assignProxy = false)
		{
			var results =  await _profileRepository.AddProfile(profile);
			if (results == null) return null;

			if(assignProxy)
				await _eventPublisher.PublishAsync(results);
			
			return results;
		}

		public async Task<ProfileModel> GetProfile(string accountId, string instagramAccountId)
		{
			return await _profileRepository.GetProfile(accountId,instagramAccountId);
		}
		public async Task<IEnumerable<ProfileModel>> GetProfiles(string accountId)
		{
			return await _profileRepository.GetProfiles(accountId);
		}
		public async Task<ProfileModel> GetProfile(string profileId) 
			=> await _profileRepository.GetProfile(profileId);

		public async Task<bool> AddProfileTopics(ProfileTopicAddRequest profileTopics)
		{
			try
			{
				if (!profileTopics.Topics.Any()) return false;
				var currentProfileVersion = await GetProfile(profileTopics.ProfileId);

				var distinctTopics = profileTopics.Topics
					.Where(_=>!currentProfileVersion.ProfileTopic.Topics.Any(x=>x.Name.Equals(_.Name)))
					.ToList();

				if (!distinctTopics.Any()) return false;
				profileTopics.Topics = distinctTopics;
				await _eventPublisher.PublishAsync(profileTopics);
				return true;
			}
			catch (Exception err)
			{
				Console.WriteLine(err.Message);
				return false;
			}
		}
		public async Task<long?> PartialUpdateProfile(string profileId, ProfileModel profile)
		{
			return await _profileRepository.PartialUpdateProfile(profileId, profile);
		}

		public async Task Handle(InstagramAccountModel @event)
		{
			await AddProfile(new ProfileModel
			{
				Account_Id = @event.AccountId,
				InstagramAccountId = @event._id,
				ProfileTopic = new Topic
				{
					Category = null,
					Topics = new List<CTopic>()
				},
				Description = "Add a description about this profile",
				Name = "My Profile 1",
				AdditionalConfigurations = new AdditionalConfigurations
				{
					ImageType = 0,
					IsTumblry = false,
					SearchTypes = new List<int> { 0, 1, 2 },
					EnableAutoPosting = true,
					AutoGenerateCaption = true,
					AllowRepost = true
				},
				AutoGenerateTopics = false,
				Language = "English",
				Theme = new Themes
				{
					Name = "My Cool Theme",
					Percentage = 20,
					Colors = new List<Color>(),
					ImagesLike = new List<GroupImagesAlike>()
				},
				LocationTargetList = new List<Location>(),
				UserTargetList = new List<string>(),
				UserLocation = new Location()
			}, true);
		}
	}
}
