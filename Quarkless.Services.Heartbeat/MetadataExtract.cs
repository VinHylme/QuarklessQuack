using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using QuarklessContexts.Extensions;
using QuarklessContexts.Models.Profiles;
using QuarklessContexts.Models.Proxies;
using QuarklessContexts.Models.ServicesModels.HeartbeatModels;
using QuarklessContexts.Models.ServicesModels.SearchModels;
using QuarklessLogic.ContentSearch.GoogleSearch;
using QuarklessLogic.ContentSearch.YandexSearch;
using QuarklessLogic.Handlers.ClientProvider;
using QuarklessLogic.Logic.InstagramAccountLogic;
using QuarklessLogic.Logic.ResponseLogic;
using QuarklessLogic.ServicesLogic.ContentSearch;
using QuarklessLogic.ServicesLogic.HeartbeatLogic;

namespace Quarkless.Services.Heartbeat
{
	public class MetadataExtract : IMetadataExtract
	{
		private readonly IHeartbeatLogic _heartbeatLogic;
		private readonly IAPIClientContext _context;
		private readonly IResponseResolver _responseResolver;
		private readonly IGoogleSearchLogic _googleSearchLogic;
		private readonly IYandexImageSearch _yandexImageSearch;
		private readonly IInstagramAccountLogic _instagramAccountLogic;
		private Assignment _assignment;
		public MetadataExtract(IAPIClientContext context, IHeartbeatLogic heartbeatLogic, IResponseResolver responseResolver,
			IGoogleSearchLogic googleSearchLogic, IYandexImageSearch yandexImageSearch, IInstagramAccountLogic accountLogic)
		{
			_context = context;
			_heartbeatLogic = heartbeatLogic;
			_responseResolver = responseResolver;
			_googleSearchLogic = googleSearchLogic;
			_yandexImageSearch = yandexImageSearch;
			_instagramAccountLogic = accountLogic;
		}

		public void Initialise(Assignment assignment) => _assignment = assignment;
		private ContentSearcherHandler CreateSearch(IAPIClientContainer context, ProxyModel proxy)
			=> new ContentSearcherHandler(context, _responseResolver, _googleSearchLogic, _yandexImageSearch, proxy);

		public async Task BuildBase(int limit = 2, int cutBy = 1, int takeTopicAmount = 1)
		{
			Console.WriteLine("Began - BuildBase");
			var watch = System.Diagnostics.Stopwatch.StartNew();
			try
			{
				var worker = _assignment.Worker;
				var apiClient = new APIClientContainer(_context, worker.InstagramAccount.AccountId, 
					worker.InstagramAccount.Id);

				var searcher = CreateSearch(apiClient, worker.ProxyModel);

				var topicTotal = new List<string>();

				var subtopics = _assignment.CustomerTopic.SubTopics.Select(a => a.TopicName);
				var related = _assignment.CustomerTopic.SubTopics.Select(s => s.RelatedTopics).SquashMe();

				topicTotal.AddRange(subtopics);
				topicTotal.AddRange(related);

				var topicSelect = topicTotal.Distinct().TakeAny(takeTopicAmount);

				var mediaByTopics = await searcher.SearchMediaDetailInstagram(topicSelect, limit);
				var recentMedias = await searcher.SearchMediaDetailInstagram(topicSelect, limit, true);

				if (mediaByTopics != null)
				{
					await _heartbeatLogic.RefreshMetaData(MetaDataType.FetchMediaByTopic, 
						_assignment.CustomerTopic.TopicFriendlyName);
					var meta = mediaByTopics.CutObjects(cutBy);

					meta.ToList().ForEach(async z => {
						z.Medias = z.Medias.Where(t => !t.HasLikedBefore && !t.HasSeen
						&& t.User.Username != _assignment.Customer.InstagramAccount.Username).ToList();

						await _heartbeatLogic.AddMetaData(MetaDataType.FetchMediaByTopic, _assignment.CustomerTopic.TopicFriendlyName,
							new __Meta__<Media>(z));
					});
				}
				if (recentMedias != null)
				{
					await _heartbeatLogic.RefreshMetaData(MetaDataType.FetchMediaByTopicRecent, 
						_assignment.CustomerTopic.TopicFriendlyName);

					var meta = recentMedias.CutObjects(cutBy);

					meta.ToList().ForEach(async z => {
						z.Medias = z.Medias.Where(t => !t.HasLikedBefore && !t.HasSeen 
						&& t.User.Username != _assignment.Customer.InstagramAccount.Username).ToList();

						await _heartbeatLogic.AddMetaData(MetaDataType.FetchMediaByTopicRecent, _assignment.CustomerTopic.TopicFriendlyName,
							new __Meta__<Media>(z));
					});
				}
			}
			catch (Exception ee)
			{
				Console.WriteLine(ee.Message);
			}
			watch.Stop();
			Console.WriteLine($"Ended - BuildBase : Took {TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds).TotalSeconds}s");
		}
		public async Task BuildUsersTargetListMedia(int limit = 1, int cutBy = 1)
		{
			Console.WriteLine("Began - BuildUsersTargetListMedia");
			var watch = System.Diagnostics.Stopwatch.StartNew();
			try
			{
				var worker = _assignment.Worker;
				var apiClient = new APIClientContainer(_context, worker.InstagramAccount.AccountId,
					worker.InstagramAccount.Id);

				var searcher = CreateSearch(apiClient, worker.ProxyModel);

				var user = _assignment.Customer.InstagramAccount;
				var userTargetList = _assignment.Customer.Profile.UserTargetList;
				if (userTargetList != null && userTargetList.Count > 0)
				{
					foreach (var theUser in userTargetList)
					{
						var fetchUsersMedia = await searcher.SearchUsersMediaDetailInstagram(theUser, limit);
						if (fetchUsersMedia == null) continue;
						await _heartbeatLogic.RefreshMetaData(MetaDataType.FetchMediaByUserTargetList, user.Id);
						foreach (var s in fetchUsersMedia.CutObjects(cutBy))
						{
							s.Medias = s.Medias.Where(t =>
								!t.HasLikedBefore &&
								!t.HasSeen).ToList();
							await _heartbeatLogic.AddMetaData(MetaDataType.FetchMediaByUserTargetList,
								_assignment.CustomerTopic.TopicFriendlyName,
								new __Meta__<Media>(s), user.Id);
						};

					}
				}
			}
			catch (Exception ee)
			{
				Console.WriteLine(ee.Message);
			}
			Console.WriteLine($"Ended - BuildUsersTargetListMedia : Took {TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds).TotalSeconds}s");
		}
		public async Task BuildLocationTargetListMedia(int limit = 1, int cutBy = 1)
		{
			Console.WriteLine("Began - BuildLocationTargetListMedia");
			var watch = System.Diagnostics.Stopwatch.StartNew();
			try
			{
				var worker = _assignment.Worker;
				var apiClient = new APIClientContainer(_context, worker.InstagramAccount.AccountId,
					worker.InstagramAccount.Id);

				var searcher = CreateSearch(apiClient, worker.ProxyModel);

				var user = _assignment.Customer.InstagramAccount;
				var profileLocationTargetList = _assignment.Customer.Profile.LocationTargetList;
				if (profileLocationTargetList != null && profileLocationTargetList.Count > 0)
				{
					foreach (var targetLocation in profileLocationTargetList)
					{
						var fetchUsersMedia = await searcher.SearchTopLocationMediaDetailInstagram(targetLocation, limit);
						await Task.Delay(TimeSpan.FromSeconds(1));
						var recentUserMedia =
							await searcher.SearchRecentLocationMediaDetailInstagram(targetLocation, limit);
						if (recentUserMedia != null && recentUserMedia.Medias.Count > 0)
						{
							fetchUsersMedia.Medias.AddRange(recentUserMedia.Medias);
						}
						if (fetchUsersMedia == null) continue;
						await _heartbeatLogic.RefreshMetaData(MetaDataType.FetchMediaByUserLocationTargetList, _assignment.CustomerTopic.TopicFriendlyName, user.Id);
						foreach (var s in fetchUsersMedia.CutObjects(cutBy))
						{
							s.Medias = s.Medias.Where(t =>
								!t.HasLikedBefore &&
								!t.HasSeen &&
								t.User.Username != _assignment.Customer.InstagramAccount.Username).ToList();
							await _heartbeatLogic.AddMetaData(MetaDataType.FetchMediaByUserLocationTargetList, _assignment.CustomerTopic.TopicFriendlyName,
								new __Meta__<Media>(s), user.Id);
						};
					}
				}
			}
			catch (Exception ee)
			{
				Console.WriteLine(ee.Message);
			}
			Console.WriteLine($"Ended - BuildLocationTargetListMedia : Took {TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds).TotalSeconds}s");
		}
		public async Task BuildUsersOwnMedias(int limit = 1, int cutBy = 1)
		{
			Console.WriteLine("Began - BuildUsersOwnMedias");
			var watch = System.Diagnostics.Stopwatch.StartNew();
			try
			{
				var worker = _assignment.Worker;
				var apiClient = new APIClientContainer(_context, worker.InstagramAccount.AccountId,
					worker.InstagramAccount.Id);
				var searcher = CreateSearch(apiClient, worker.ProxyModel);

				var user = _assignment.Customer.InstagramAccount;
				var fetchUsersMedia = await searcher.SearchUsersMediaDetailInstagram(user.Username, limit);
				if (fetchUsersMedia != null)
				{
					await _heartbeatLogic.RefreshMetaData(MetaDataType.FetchUserOwnProfile, _assignment.CustomerTopic.TopicFriendlyName, user.Id);
					foreach (var s in fetchUsersMedia.CutObjects(cutBy))
					{
						await _heartbeatLogic.AddMetaData(MetaDataType.FetchUserOwnProfile, _assignment.CustomerTopic.TopicFriendlyName,
							new __Meta__<Media>(s), user.Id);
					};
					var userFirst = fetchUsersMedia.Medias.FirstOrDefault();
					if (userFirst != null)
					{
						var details = await searcher.SearchInstagramFullUserDetail(userFirst.User.UserId);
						if (details.UserDetail.Pk != 0)
						{
							await _instagramAccountLogic.PartialUpdateInstagramAccount(user.AccountId, user.Id,
								new QuarklessContexts.Models.InstagramAccounts.InstagramAccountModel
								{
									UserId = details.UserDetail.Pk,
									FollowersCount = details.UserDetail.FollowerCount,
									FollowingCount = details.UserDetail.FollowingCount,
									TotalPostsCount = details.UserDetail.MediaCount,
									Email = details.UserDetail.PublicEmail,
									ProfilePicture =
										details.UserDetail.ProfilePicture ?? details.UserDetail.ProfilePicUrl,
									FullName = details.UserDetail.FullName,
									UserBiography = new QuarklessContexts.Models.InstagramAccounts.Biography
									{
										Text = details.UserDetail.BiographyWithEntities.Text,
										Hashtags = details.UserDetail.BiographyWithEntities.Entities
											.Select(_ => _.Hashtag.Name).ToList()
									},
									IsBusiness = details.UserDetail.IsBusiness,
									Location = new Location
									{
										Address = details.UserDetail.AddressStreet,
										City = details.UserDetail.CityName,
										Coordinates = new Coordinates
										{
											Latitude = details.UserDetail.Latitude,
											Longitude = details.UserDetail.Longitude
										},
										PostCode = details.UserDetail.ZipCode
									},
									PhoneNumber = details.UserDetail.PublicPhoneCountryCode +
												  (!string.IsNullOrEmpty(details.UserDetail.PublicPhoneNumber)
													  ? details.UserDetail.PublicPhoneNumber
													  : details.UserDetail.ContactPhoneNumber)
								});
						}
					}
				}
			}
			catch (Exception ee)
			{
				Console.WriteLine(ee.Message);
			}
			Console.WriteLine($"Ended - BuildUsersOwnMedias : Took {TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds).TotalSeconds}s");
		}
		public async Task BuildUsersFeed(int limit = 1, int cutBy = 1)
		{
			Console.WriteLine("Began - BuildUsersFeed");
			var watch = System.Diagnostics.Stopwatch.StartNew();
			try
			{
				var worker = _assignment.Worker;
				var apiClient = new APIClientContainer(_context, worker.InstagramAccount.AccountId,
					worker.InstagramAccount.Id);
				var searcher = CreateSearch(apiClient, worker.ProxyModel);

				var user = _assignment.Customer.InstagramAccount;

				searcher.ChangeUser(new APIClientContainer(_context, user.AccountId, user.Id));
				var fetchUsersMedia = await searcher.SearchUserFeedMediaDetailInstagram(limit: limit);
				searcher.ChangeUser(apiClient);

				if (fetchUsersMedia != null)
				{
					await _heartbeatLogic.RefreshMetaData(MetaDataType.FetchUsersFeed, _assignment.CustomerTopic.TopicFriendlyName, user.Id);
					foreach (var s in fetchUsersMedia.CutObjects(cutBy))
					{
						s.Medias = s.Medias.Where(_ =>
							!_.HasLikedBefore
							&& !_.HasSeen
							&& _.User.Username != user.Username).ToList();

						await _heartbeatLogic.AddMetaData(MetaDataType.FetchUsersFeed, _assignment.CustomerTopic.TopicFriendlyName,
							new __Meta__<Media>(s), user.Id);
					};
				}
			}
			catch (Exception ee)
			{
				Console.WriteLine(ee.Message);
			}
			Console.WriteLine($"Ended - BuildUsersFeed : Took {TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds).TotalSeconds}s");
		}
		public async Task BuildUserFollowList(int limit = 3, int cutBy = 1)
		{
			Console.WriteLine("Began - BuildUserUnfollowList");
			var watch = System.Diagnostics.Stopwatch.StartNew();
			try
			{
				var worker = _assignment.Worker;
				var apiClient = new APIClientContainer(_context, worker.InstagramAccount.AccountId,
					worker.InstagramAccount.Id);
				var searcher = CreateSearch(apiClient, worker.ProxyModel);

				var user = _assignment.Customer.InstagramAccount;
				var fetchUsersMedia = await searcher.GetUserFollowingList(user.Username, limit);
				if (fetchUsersMedia != null)
				{
					await _heartbeatLogic.RefreshMetaData(MetaDataType.FetchUsersFollowingList, _assignment.CustomerTopic.TopicFriendlyName, user.Id);
					foreach (var s in fetchUsersMedia.ToList().CutObject(cutBy))
					{
						await _heartbeatLogic.AddMetaData(MetaDataType.FetchUsersFollowingList, _assignment.CustomerTopic.TopicFriendlyName,
							new __Meta__<List<UserResponse<string>>>(s), user.Id);
					};
				}
			}
			catch (Exception ee)
			{
				Console.WriteLine(ee.Message);
			}
			Console.WriteLine($"Ended - BuildUserUnfollowList : Took {TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds).TotalSeconds}s");
		}
		public async Task BuildUserFollowerList(int limit = 4, int cutBy = 1)
		{
			Console.WriteLine("Began - BuildUserFollowerList");
			var watch = System.Diagnostics.Stopwatch.StartNew();
			try
			{
				var worker = _assignment.Worker;
				var apiClient = new APIClientContainer(_context, worker.InstagramAccount.AccountId,
					worker.InstagramAccount.Id);
				var searcher = CreateSearch(apiClient, worker.ProxyModel);

				var user = _assignment.Customer.InstagramAccount;
				var fetchUsersMedia = await searcher.GetUsersFollowersList(user.Username, limit);
				if (fetchUsersMedia != null)
				{
					await _heartbeatLogic.RefreshMetaData(MetaDataType.FetchUsersFollowerList, _assignment.CustomerTopic.TopicFriendlyName, user.Id);
					foreach (var s in fetchUsersMedia.ToList().CutObject(cutBy))
					{
						await _heartbeatLogic.AddMetaData(MetaDataType.FetchUsersFollowerList, _assignment.CustomerTopic.TopicFriendlyName,
							new __Meta__<List<UserResponse<string>>>(s), user.Id);
					};
				}
			}
			catch (Exception ee)
			{
				Console.WriteLine(ee.Message);
			}
			Console.WriteLine($"Ended - BuildUserFollowerList : Took {TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds).TotalSeconds}s");
		}
		public async Task BuildUsersFollowSuggestions(int limit = 1, int cutObjectBy = 1)
		{
			Console.WriteLine("Began - BuildUsersFollowSuggestions");
			var watch = System.Diagnostics.Stopwatch.StartNew();
			try
			{
				var worker = _assignment.Worker;
				var apiClient = new APIClientContainer(_context, worker.InstagramAccount.AccountId,
					worker.InstagramAccount.Id);
				var searcher = CreateSearch(apiClient, worker.ProxyModel);

				var user = _assignment.Customer.InstagramAccount;
				searcher.ChangeUser(new APIClientContainer(_context, user.AccountId, user.Id));

				var fetchUsersMedia = await searcher.GetSuggestedPeopleToFollow(limit);

				searcher.ChangeUser(apiClient);

				if (fetchUsersMedia != null)
				{
					await _heartbeatLogic.RefreshMetaData(MetaDataType.FetchUsersFollowSuggestions, _assignment.CustomerTopic.TopicFriendlyName, user.Id);
					foreach (var s in fetchUsersMedia.ToList().CutObject(cutObjectBy))
					{
						await _heartbeatLogic.AddMetaData(MetaDataType.FetchUsersFollowSuggestions, _assignment.CustomerTopic.TopicFriendlyName,
							new __Meta__<List<UserResponse<UserSuggestionDetails>>>(s), user.Id);
					};
				}
			}
			catch (Exception ee)
			{
				Console.WriteLine(ee.Message);
			}
			Console.WriteLine($"Ended - BuildUsersFollowSuggestions : Took {TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds).TotalSeconds}s");
		}
	}
}
