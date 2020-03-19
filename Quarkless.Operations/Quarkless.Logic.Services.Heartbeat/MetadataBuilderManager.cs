using System;
using System.Threading.Tasks;
using Quarkless.Base.ContentSearch.Models.Interfaces;
using Quarkless.Base.InstagramAccounts.Models.Interfaces;
using Quarkless.Base.Topic.Models.Interfaces;
using Quarkless.Models.Common.Enums;
using Quarkless.Models.Heartbeat.Interfaces;
using Quarkless.Models.Services.Heartbeat;
using Quarkless.Models.WorkerManager.Interfaces;

namespace Quarkless.Logic.Services.Heartbeat
{
	public class MetadataBuilderManager
	{
		private readonly FullUserDetail _customer;
		private readonly MetadataExtract _metadataExtract;
		public MetadataBuilderManager(FullUserDetail customer,
			IHeartbeatLogic heartbeatLogic, IWorkerManager workerManager,
			ISearchProvider searchProvider, IInstagramAccountLogic accountLogic,
			ITopicLookupLogic topicLookup)
		{
			_customer = customer;
			_metadataExtract = new MetadataExtract(heartbeatLogic,
				accountLogic,topicLookup, workerManager, searchProvider, _customer);
		}

		/// <summary>
		/// Will begin work by fetching popular medias based on the users topic
		/// </summary>
		/// <returns></returns>
		public async Task BaseExtract()
		{
			Console.WriteLine("Began Base Extract for {0}", _customer.InstagramAccount.Username);
			await _metadataExtract.BuildBase(2, takeTopicAmount:2);

			var liker = Task.Run(async () =>
				await _metadataExtract.BuildUserFromLikers(takeMediaAmount: 12, takeUserMediaAmount: 400));
			var commenter = Task.Run(async () =>
				await _metadataExtract.BuildUsersFromCommenters(takeMediaAmount: 12, takeUserMediaAmount: 400));

			var stories = Task.Run(async () => await _metadataExtract.BuildUsersStoryFromTopics());

			await Task.WhenAll(liker, commenter);

			var mediaLiker = _metadataExtract.BuildMediaFromUsersLikers(takeMediaAmount: 12, takeUserMediaAmount: 50);
			var mediaCommenter = _metadataExtract.BuildMediaFromUsersCommenters(takeMediaAmount: 7, takeUserMediaAmount: 50);
			await Task.WhenAll(mediaLiker, mediaCommenter);

			var commentMediaLiker = _metadataExtract.BuildCommentsFromSpecifiedSource(MetaDataType.FetchMediaByLikers,
				MetaDataType.FetchCommentsViaPostsLiked, limit: 2, takeMediaAmount: 7, takeUserAmount: 400);

			var commentMediaCommenter = _metadataExtract.BuildCommentsFromSpecifiedSource(MetaDataType.FetchMediaByCommenters,
				MetaDataType.FetchCommentsViaPostCommented, limit: 2, takeMediaAmount: 7, takeUserAmount: 400);

			await Task.WhenAll(commentMediaLiker, commentMediaCommenter, stories);
			Console.WriteLine("Ended Base Extract for {0}", _customer.InstagramAccount.Username);
		}

		/// <summary>
		/// Will get images from outside based on the users profile
		/// </summary>
		/// <returns></returns>
		public async Task ExternalExtract()
		{
			Console.WriteLine("Began External Extract for {0}", _customer.InstagramAccount.Username);

			if (!_customer.Profile.AdditionalConfigurations.EnableAutoPosting)
			{
				Console.WriteLine("Ended External Extract for {0} as user has disabled this feature",
					_customer.InstagramAccount.Username);
				return;
			}

			var googleImages = Task.Run(async () => await _metadataExtract.BuildGoogleImages(25));
			var yandexQuery = Task.Run(async () => await _metadataExtract.BuildYandexImagesQuery(4));
			var yandexImages = Task.Run(async () => await _metadataExtract.BuildYandexImages());

			await Task.WhenAll(googleImages, yandexImages, yandexQuery);
			Console.WriteLine("Ended External Extract for {0}", _customer.InstagramAccount.Username);
		}

		/// <summary>
		/// Will extract all users target listening based on profile (e.g. location and users)
		/// </summary>
		/// <returns></returns>
		public async Task TargetListingExtract()
		{
			Console.WriteLine("Began Target Extract for {0}", _customer.InstagramAccount.Username);
			var userTargetList = await Task.Run(async () => await _metadataExtract.BuildUsersTargetListMedia(1))
				.ContinueWith(async x =>
				{
					await _metadataExtract.BuildCommentsFromSpecifiedSource(MetaDataType.FetchMediaByUserTargetList, 
						MetaDataType.FetchCommentsViaUserTargetList, 2, takeMediaAmount: 7, takeUserAmount: 200);
				});

			var locTargetList = await Task.Run(async () => await _metadataExtract.BuildLocationTargetListMedia(1))
				.ContinueWith(async s =>
				{
					await _metadataExtract.BuildCommentsFromSpecifiedSource(MetaDataType.FetchMediaByUserLocationTargetList, 
						MetaDataType.FetchCommentsViaLocationTargetList, 2, takeMediaAmount: 7, takeUserAmount: 200);
				});

			await Task.WhenAll(userTargetList, locTargetList);
			Console.WriteLine("Ended Target Extract for {0}", _customer.InstagramAccount.Username);
		}

		/// <summary>
		/// Will extract and update current users instagram profile
		/// </summary>
		/// <returns></returns>
		public async Task UserInformationExtract()
		{
			Console.WriteLine("Began User Info Extract for {0}", _customer.InstagramAccount.Username);
			var profileRefresh = Task.Run(async () => await _metadataExtract.BuildUsersOwnMedias())
				.ContinueWith(async x => { await _metadataExtract.BuildUsersRecentComments(); });

			var followingList = Task.Run(async () => await _metadataExtract.BuildUserFollowList());

			var followerList = Task.Run(async () => await _metadataExtract.BuildUserFollowerList());

			var storyFeed = Task.Run(async () =>
			{
				if(_customer.Profile.AdditionalConfigurations.EnableAutoWatchStories 
					|| _customer.Profile.AdditionalConfigurations.EnableAutoReactStories)
					await _metadataExtract.BuildUsersStoryFeed();
			});

			var feedRefresh = await Task.Run(async () => await _metadataExtract.BuildUsersFeed())
				.ContinueWith(async x =>
				{
					await _metadataExtract.BuildUsersFollowSuggestions(2);
					await _metadataExtract.BuildUsersInbox();
					await _metadataExtract.BuildCommentsFromSpecifiedSource(MetaDataType.FetchUsersFeed,
						MetaDataType.FetchCommentsViaUserFeed, takeMediaAmount: 7);
				});

			await Task.WhenAll(feedRefresh);
			
			await Task.WhenAll(profileRefresh, followerList, followingList, storyFeed);
			Console.WriteLine("Ended User Info Extract for {0}", _customer.InstagramAccount.Username);
		}
	}
}
