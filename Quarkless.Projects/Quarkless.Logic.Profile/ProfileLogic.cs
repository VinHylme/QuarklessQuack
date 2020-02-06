using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Quarkless.Events.Interfaces;
using Quarkless.Models.Common.Models;
using Quarkless.Models.InstagramAccounts;
using Quarkless.Models.Profile;
using Quarkless.Models.Profile.Interfaces;
using Quarkless.Models.SearchResponse;
using Quarkless.Models.Topic;

namespace Quarkless.Logic.Profile
{
	public class ProfileLogic : IProfileLogic, IEventSubscriber<InstagramAccountPublishEventModel>, 
		IEventSubscriber<InstagramAccountDeletePublishEvent>
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

		public async Task<ProfileModel> AddProfile(ProfileModel profile,
			bool assignProxy = false, string ipAddress = null, Location location = null)
		{
			var results = await _profileRepository.AddProfile(profile);
			if (results == null) return null;

			if (assignProxy)
				await _eventPublisher.PublishAsync(new ProfilePublishEventModel
				{
					Profile = results,
					IpAddress = ipAddress,
					Location = location
				});

			return results;
		}

		public async Task<ProfileModel> GetProfile(string accountId, string instagramAccountId)
		{
			return await _profileRepository.GetProfile(accountId, instagramAccountId);
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
				//var currentProfileVersion = await GetProfile(profileTopics.ProfileId);
				//profileTopics.Topics = currentProfileVersion.ProfileTopic.Topics.Union(profileTopics.Topics);
				await _eventPublisher.PublishAsync(profileTopics);
				return true;
			}
			catch (Exception err)
			{
				Console.WriteLine(err.Message);
				return false;
			}
		}

		public async Task<bool> RemoveProfile(string instagramAccountId)
			=> await _profileRepository.RemoveProfile(instagramAccountId);

		public async Task<long?> PartialUpdateProfile(string profileId, ProfileModel profile)
		{
			return await _profileRepository.PartialUpdateProfile(profileId, profile);
		}

		public async Task Handle(InstagramAccountPublishEventModel @event)
		{
			await AddProfile(new ProfileModel
			{
				Account_Id = @event.InstagramAccount.AccountId,
				InstagramAccountId = @event.InstagramAccount._id,
				ProfileTopic = new Topic
				{
					Category = new CTopic()
					{
						Name = "Art",
						_id = "c9b7c5bb41bc9001b3c0bed5",
						ParentTopicId = "000000000000000000000000"
					},
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
			}, true, @event.IpAddress, @event.Location);
		}

		public async Task Handle(InstagramAccountDeletePublishEvent @event)
		{
			var res = await RemoveProfile(@event.InstagramAccountId);
			if (res)
			{
				await _eventPublisher.PublishAsync(new ProfileDeletedEventModel()
					{InstagramAccountId = @event.InstagramAccountId});
			}
		}
	}
}
