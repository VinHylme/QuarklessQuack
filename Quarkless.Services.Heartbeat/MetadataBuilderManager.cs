using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using QuarklessContexts.Models.Proxies;
using QuarklessContexts.Models.ServicesModels.HeartbeatModels;
using QuarklessLogic.ContentSearch.GoogleSearch;
using QuarklessLogic.ContentSearch.YandexSearch;
using QuarklessLogic.Handlers.ClientProvider;
using QuarklessLogic.Logic.InstagramAccountLogic;
using QuarklessLogic.Logic.ResponseLogic;
using QuarklessLogic.ServicesLogic.HeartbeatLogic;

namespace Quarkless.Services.Heartbeat
{
	public class MetadataBuilderManager
	{
		private readonly Assignment _assignment;
		private readonly MetadataExtract _metadataExtract;
		public MetadataBuilderManager(Assignment assignment, 
			IAPIClientContext context, IHeartbeatLogic heartbeatLogic,
			IResponseResolver responseResolver,
			IGoogleSearchLogic googleSearchLogic, IYandexImageSearch yandexImageSearch,
			IInstagramAccountLogic accountLogic)
		{
			_assignment = assignment;

			_metadataExtract = new MetadataExtract(context, heartbeatLogic, responseResolver, googleSearchLogic,
				yandexImageSearch, accountLogic, assignment);
			//_metadataExtract.Initialise(assignment);
		}

		/// <summary>
		/// Will begin work by fetching popular medias based on the users topic
		/// </summary>
		/// <returns></returns>
		public async Task BaseExtract()
		{
			Console.WriteLine("Began Base Extract for {0}", _assignment.Customer.InstagramAccount.Username);
			await _metadataExtract.BuildBase(3);

			var liker = Task.Run(async () =>
				await _metadataExtract.BuildUserFromLikers(takeMediaAmount: 12, takeUserMediaAmount: 400));
			var commenter = Task.Run(async () =>
				await _metadataExtract.BuildUsersFromCommenters(takeMediaAmount: 12, takeUserMediaAmount: 400));
			Task.WaitAll(liker, commenter);

			var mediaLiker = _metadataExtract.BuildMediaFromUsersLikers(takeMediaAmount: 12, takeUserMediaAmount: 50);
			var mediaCommenter = _metadataExtract.BuildMediaFromUsersCommenters(takeMediaAmount: 12, takeUserMediaAmount: 50);
			Task.WaitAll(mediaLiker, mediaCommenter);

			var commentMediaLiker = _metadataExtract.BuildCommentsFromSpecifiedSource(MetaDataType.FetchMediaByLikers,
				MetaDataType.FetchCommentsViaPostsLiked, limit: 2, takeMediaAmount: 12, takeuserAmount: 400);

			var commentMediaCommenter = _metadataExtract.BuildCommentsFromSpecifiedSource(MetaDataType.FetchMediaByCommenters,
				MetaDataType.FetchCommentsViaPostCommented, limit: 2, takeMediaAmount: 12, takeuserAmount: 400);

			Task.WaitAll(commentMediaLiker, commentMediaCommenter);
			Console.WriteLine("Ended Base Extract for {0}", _assignment.Customer.InstagramAccount.Username);
		}

		/// <summary>
		/// Will get images from outside based on the users profile
		/// </summary>
		/// <returns></returns>
		public async Task ExternalExtract()
		{
			Console.WriteLine("Began External Extract for {0}", _assignment.Customer.InstagramAccount.Username);
			var googleImages = Task.Run(async () => await _metadataExtract.BuildGoogleImages(20));
			var yandexImages = Task.Run(async () => await _metadataExtract.BuildYandexImages(takeTopicAmount: 1));
			Task.WaitAll(googleImages, yandexImages);
			Console.WriteLine("Ended External Extract for {0}", _assignment.Customer.InstagramAccount.Username);
		}

		/// <summary>
		/// Will extract all users target listening based on profile (e.g. location and users)
		/// </summary>
		/// <returns></returns>
		public async Task TargetListingExtract()
		{
			Console.WriteLine("Began Target Extract for {0}", _assignment.Customer.InstagramAccount.Username);
			var userTargetList = await Task.Run(async () => await _metadataExtract.BuildUsersTargetListMedia(1))
				.ContinueWith(async x =>
				{
					await _metadataExtract.BuildCommentsFromSpecifiedSource(MetaDataType.FetchMediaByUserTargetList, MetaDataType.FetchCommentsViaUserTargetList, true, 2, takeMediaAmount: 14, takeuserAmount: 200);
				});

			var locTargetList = await Task.Run(async () => await _metadataExtract.BuildLocationTargetListMedia(1))
				.ContinueWith(async s =>
				{
					await _metadataExtract.BuildCommentsFromSpecifiedSource(MetaDataType.FetchMediaByUserLocationTargetList, MetaDataType.FetchCommentsViaLocationTargetList, true, 2, takeMediaAmount: 14, takeuserAmount: 200);
				});

			Task.WaitAll(userTargetList, locTargetList);
			Console.WriteLine("Ended Target Extract for {0}", _assignment.Customer.InstagramAccount.Username);
		}

		/// <summary>
		/// Will extract and update current users instagram profile
		/// </summary>
		/// <returns></returns>
		public async Task UserInformationExtract()
		{
			Console.WriteLine("Began User Info Extract for {0}", _assignment.Customer.InstagramAccount.Username);
			var profileRefresh = Task.Run(async () => await _metadataExtract.BuildUsersOwnMedias(3))
				.ContinueWith(async x => { await _metadataExtract.BuildUsersRecentComments(); });

			var followingList = Task.Run(async () => await _metadataExtract.BuildUserFollowList());

			var followerList = Task.Run(async () => await _metadataExtract.BuildUserFollowerList());

			var feedRefresh = await Task.Run(async () => await _metadataExtract.BuildUsersFeed()).ContinueWith(async x =>
			{
				await _metadataExtract.BuildUsersFollowSuggestions(2);
				await _metadataExtract.BuildUsersInbox();
				await _metadataExtract.BuildCommentsFromSpecifiedSource(MetaDataType.FetchUsersFeed, MetaDataType.FetchCommentsViaUserFeed, true);
			});
			Task.WaitAll(profileRefresh, followerList, followingList, feedRefresh);
			Console.WriteLine("Ended User Info Extract for {0}", _assignment.Customer.InstagramAccount.Username);
		}
	}
}
