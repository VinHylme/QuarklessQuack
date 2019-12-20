using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InstagramApiSharp.Classes.Models;
using QuarklessContexts.Extensions;
using QuarklessContexts.Models.Profiles;
using QuarklessContexts.Models.ResponseModels;
using QuarklessContexts.Models.ServicesModels.HeartbeatModels;
using QuarklessContexts.Models.ServicesModels.SearchModels;
using QuarklessContexts.Models.Topics;
using QuarklessLogic.ContentSearch.GoogleSearch;
using QuarklessLogic.ContentSearch.YandexSearch;
using QuarklessLogic.Handlers.ClientProvider;
using QuarklessLogic.Logic.InstagramAccountLogic;
using QuarklessLogic.Logic.ResponseLogic;
using QuarklessLogic.Logic.TopicLookupLogic;
using QuarklessLogic.ServicesLogic.ContentSearch;
using QuarklessLogic.ServicesLogic.HeartbeatLogic;

namespace Quarkless.Services.Heartbeat
{
	public class MetadataExtract //: IMetadataExtract
	{
		private readonly IHeartbeatLogic _heartbeatLogic;
		private readonly IAPIClientContext _context;
		private readonly IResponseResolver _responseResolver;
		private readonly IGoogleSearchLogic _googleSearchLogic;
		private readonly IYandexImageSearch _yandexImageSearch;
		private readonly IInstagramAccountLogic _instagramAccountLogic;
		private readonly ITopicLookupLogic _topicLookup;
		private Assignment _assignment;
		private APIClientContainer _apiClient;

		public MetadataExtract(IAPIClientContext context, IHeartbeatLogic heartbeatLogic,
			IResponseResolver responseResolver,
			IGoogleSearchLogic googleSearchLogic, IYandexImageSearch yandexImageSearch,
			IInstagramAccountLogic accountLogic, ITopicLookupLogic topicLookup, Assignment assignment)
		{
			_context = context;
			_heartbeatLogic = heartbeatLogic;
			_responseResolver = responseResolver;
			_googleSearchLogic = googleSearchLogic;
			_yandexImageSearch = yandexImageSearch;
			_instagramAccountLogic = accountLogic;
			_topicLookup = topicLookup;
			_assignment = assignment;
		}

		//public void Initialise(Assignment assignment) => _assignment = assignment;
		private ContentSearcherHandler CreateSearch()
		{
			var worker = _assignment.Worker;
			_apiClient = new APIClientContainer(_context, worker.InstagramAccount.AccountId,
				worker.InstagramAccount.Id);

			return new ContentSearcherHandler(_apiClient, _responseResolver, _googleSearchLogic, _yandexImageSearch, 
				worker.ProxyModel);
		}

		#region Instagram Stuff
		public async Task BuildBase(int limit = 2, int cutBy = 1, int takeTopicAmount = 1)
		{
			Console.WriteLine("Began - BuildBase");
			var watch = System.Diagnostics.Stopwatch.StartNew();
			try
			{
				var searcher = CreateSearch();

				var topicTotal = new List<CTopic>();

				_assignment.CustomerTopic.Topics.ForEach(topic =>
				{
					topicTotal.Add(topic);
					var relatedTopic = _topicLookup.GetTopicByParentId(topic._id).Result;
					if(relatedTopic.Any())
						topicTotal.AddRange(relatedTopic);
				});

				var topicSelect = topicTotal.Distinct().TakeAny(takeTopicAmount).ToList();

				var mediaByTopics = await searcher.SearchMediaDetailInstagram(topicSelect, limit);
				var recentMedias = await searcher.SearchMediaDetailInstagram(topicSelect, limit, true);

				if (mediaByTopics != null)
				{
					await _heartbeatLogic.RefreshMetaData(MetaDataType.FetchMediaByTopic,
						_assignment.CustomerTopic.Category._id);
					var meta = mediaByTopics.CutObjects(cutBy);

					meta.ToList().ForEach(async z =>
					{
						z.Medias = z.Medias.Where(t => !t.HasLikedBefore && !t.HasSeen
							&&
							t.User.Username != _assignment.Customer
							 .InstagramAccount.Username).ToList();

						await _heartbeatLogic.AddMetaData(MetaDataType.FetchMediaByTopic,
							_assignment.CustomerTopic.Category._id,
							new __Meta__<Media>(z));
					});
				}

				if (recentMedias != null)
				{
					await _heartbeatLogic.RefreshMetaData(MetaDataType.FetchMediaByTopicRecent,
						_assignment.CustomerTopic.Category._id);

					var meta = recentMedias.CutObjects(cutBy);

					meta.ToList().ForEach(async z =>
					{
						z.Medias = z.Medias.Where(t => !t.HasLikedBefore && !t.HasSeen
							 &&
							 t.User.Username != _assignment.Customer
							     .InstagramAccount.Username).ToList();

						await _heartbeatLogic.AddMetaData(MetaDataType.FetchMediaByTopicRecent,
							_assignment.CustomerTopic.Category._id,
							new __Meta__<Media>(z));
					});
				}
			}
			catch (Exception ee)
			{
				Console.WriteLine(ee.Message);
			}

			watch.Stop();
			Console.WriteLine(
				$"Ended - BuildBase : Took {TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds).TotalSeconds}s");
		}
		public async Task BuildUsersTargetListMedia(int limit = 1, int cutBy = 1)
		{
			Console.WriteLine("Began - BuildUsersTargetListMedia");
			var watch = System.Diagnostics.Stopwatch.StartNew();
			try
			{
				var searcher = CreateSearch();

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
								_assignment.CustomerTopic.Category._id,
								new __Meta__<Media>(s), user.Id);
						}

						;

					}
				}
			}
			catch (Exception ee)
			{
				Console.WriteLine(ee.Message);
			}

			Console.WriteLine(
				$"Ended - BuildUsersTargetListMedia : Took {TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds).TotalSeconds}s");
		}
		public async Task BuildLocationTargetListMedia(int limit = 1, int cutBy = 1)
		{
			Console.WriteLine("Began - BuildLocationTargetListMedia");
			var watch = System.Diagnostics.Stopwatch.StartNew();
			try
			{
				var searcher = CreateSearch();

				var user = _assignment.Customer.InstagramAccount;
				var profileLocationTargetList = _assignment.Customer.Profile.LocationTargetList;
				if (profileLocationTargetList != null && profileLocationTargetList.Count > 0)
				{
					foreach (var targetLocation in profileLocationTargetList)
					{
						var fetchUsersMedia =
							await searcher.SearchTopLocationMediaDetailInstagram(targetLocation, limit);
						await Task.Delay(TimeSpan.FromSeconds(1));
						var recentUserMedia =
							await searcher.SearchRecentLocationMediaDetailInstagram(targetLocation, limit);
						if (recentUserMedia != null && recentUserMedia.Medias.Count > 0)
						{
							fetchUsersMedia.Medias.AddRange(recentUserMedia.Medias);
						}

						if (fetchUsersMedia == null) continue;
						await _heartbeatLogic.RefreshMetaData(MetaDataType.FetchMediaByUserLocationTargetList,
							_assignment.CustomerTopic.Category._id, user.Id);
						foreach (var s in fetchUsersMedia.CutObjects(cutBy))
						{
							s.Medias = s.Medias.Where(t =>
								!t.HasLikedBefore &&
								!t.HasSeen &&
								t.User.Username != _assignment.Customer.InstagramAccount.Username).ToList();
							await _heartbeatLogic.AddMetaData(MetaDataType.FetchMediaByUserLocationTargetList,
								_assignment.CustomerTopic.Category._id,
								new __Meta__<Media>(s), user.Id);
						}

						;
					}
				}
			}
			catch (Exception ee)
			{
				Console.WriteLine(ee.Message);
			}

			Console.WriteLine(
				$"Ended - BuildLocationTargetListMedia : Took {TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds).TotalSeconds}s");
		}
		public async Task BuildUsersOwnMedias(int limit = 1, int cutBy = 1)
		{
			Console.WriteLine("Began - BuildUsersOwnMedias");
			var watch = System.Diagnostics.Stopwatch.StartNew();
			try
			{
				var searcher = CreateSearch();

				var user = _assignment.Customer.InstagramAccount;
				var fetchUsersMedia = await searcher.SearchUsersMediaDetailInstagram(user.Username, limit);
				if (fetchUsersMedia != null)
				{
					await _heartbeatLogic.RefreshMetaData(MetaDataType.FetchUserOwnProfile,
						_assignment.CustomerTopic.Category._id, user.Id);
					foreach (var s in fetchUsersMedia.CutObjects(cutBy))
					{
						await _heartbeatLogic.AddMetaData(MetaDataType.FetchUserOwnProfile,
							_assignment.CustomerTopic.Category._id,
							new __Meta__<Media>(s), user.Id);
					}

					;
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

			Console.WriteLine(
				$"Ended - BuildUsersOwnMedias : Took {TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds).TotalSeconds}s");
		}
		public async Task BuildUsersFeed(int limit = 1, int cutBy = 1)
		{
			Console.WriteLine("Began - BuildUsersFeed");
			var watch = System.Diagnostics.Stopwatch.StartNew();
			try
			{
				var searcher = CreateSearch();

				var user = _assignment.Customer.InstagramAccount;

				searcher.ChangeUser(new APIClientContainer(_context, user.AccountId, user.Id));
				var fetchUsersMedia = await searcher.SearchUserFeedMediaDetailInstagram(limit: limit);
				searcher.ChangeUser(_apiClient);

				if (fetchUsersMedia != null)
				{
					await _heartbeatLogic.RefreshMetaData(MetaDataType.FetchUsersFeed,
						_assignment.CustomerTopic.Category._id, user.Id);
					foreach (var s in fetchUsersMedia.CutObjects(cutBy))
					{
						s.Medias = s.Medias.Where(_ =>
							!_.HasLikedBefore
							&& !_.HasSeen
							&& _.User.Username != user.Username).ToList();

						await _heartbeatLogic.AddMetaData(MetaDataType.FetchUsersFeed,
							_assignment.CustomerTopic.Category._id,
							new __Meta__<Media>(s), user.Id);
					}
				}
			}
			catch (Exception ee)
			{
				Console.WriteLine(ee.Message);
			}

			Console.WriteLine(
				$"Ended - BuildUsersFeed : Took {TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds).TotalSeconds}s");
		}
		public async Task BuildUserFollowList(int limit = 3, int cutBy = 1)
		{
			Console.WriteLine("Began - BuildUserUnfollowList");
			var watch = System.Diagnostics.Stopwatch.StartNew();
			try
			{
				var searcher = CreateSearch();

				var user = _assignment.Customer.InstagramAccount;
				var fetchUsersMedia = await searcher.GetUserFollowingList(user.Username, limit);
				if (fetchUsersMedia != null)
				{
					await _heartbeatLogic.RefreshMetaData(MetaDataType.FetchUsersFollowingList,
						_assignment.CustomerTopic.Category._id, user.Id);
					foreach (var s in fetchUsersMedia.ToList().CutObject(cutBy))
					{
						await _heartbeatLogic.AddMetaData(MetaDataType.FetchUsersFollowingList,
							_assignment.CustomerTopic.Category._id,
							new __Meta__<List<UserResponse<string>>>(s), user.Id);
					}
				}
			}
			catch (Exception ee)
			{
				Console.WriteLine(ee.Message);
			}

			Console.WriteLine(
				$"Ended - BuildUserUnfollowList : Took {TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds).TotalSeconds}s");
		}
		public async Task BuildUserFollowerList(int limit = 4, int cutBy = 1)
		{
			Console.WriteLine("Began - BuildUserFollowerList");
			var watch = System.Diagnostics.Stopwatch.StartNew();
			try
			{
				var searcher = CreateSearch();

				var user = _assignment.Customer.InstagramAccount;
				var fetchUsersMedia = await searcher.GetUsersFollowersList(user.Username, limit);
				if (fetchUsersMedia != null)
				{
					await _heartbeatLogic.RefreshMetaData(MetaDataType.FetchUsersFollowerList,
						_assignment.CustomerTopic.Category._id, user.Id);
					foreach (var s in fetchUsersMedia.ToList().CutObject(cutBy))
					{
						await _heartbeatLogic.AddMetaData(MetaDataType.FetchUsersFollowerList,
							_assignment.CustomerTopic.Category._id,
							new __Meta__<List<UserResponse<string>>>(s), user.Id);
					}
				}
			}
			catch (Exception ee)
			{
				Console.WriteLine(ee.Message);
			}

			Console.WriteLine(
				$"Ended - BuildUserFollowerList : Took {TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds).TotalSeconds}s");
		}
		public async Task BuildUsersFollowSuggestions(int limit = 1, int cutObjectBy = 1)
		{
			Console.WriteLine("Began - BuildUsersFollowSuggestions");
			var watch = System.Diagnostics.Stopwatch.StartNew();
			try
			{
				var searcher = CreateSearch();

				var user = _assignment.Customer.InstagramAccount;
				searcher.ChangeUser(new APIClientContainer(_context, user.AccountId, user.Id));

				var fetchUsersMedia = await searcher.GetSuggestedPeopleToFollow(limit);

				searcher.ChangeUser(_apiClient);

				if (fetchUsersMedia != null)
				{
					await _heartbeatLogic.RefreshMetaData(MetaDataType.FetchUsersFollowSuggestions,
						_assignment.CustomerTopic.Category._id, user.Id);
					foreach (var s in fetchUsersMedia.ToList().CutObject(cutObjectBy))
					{
						await _heartbeatLogic.AddMetaData(MetaDataType.FetchUsersFollowSuggestions,
							_assignment.CustomerTopic.Category._id,
							new __Meta__<List<UserResponse<UserSuggestionDetails>>>(s), user.Id);
					}
				}
			}
			catch (Exception ee)
			{
				Console.WriteLine(ee.Message);
			}

			Console.WriteLine(
				$"Ended - BuildUsersFollowSuggestions : Took {TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds).TotalSeconds}s");
		}
		public async Task BuildUsersInbox(int limit = 1)
		{
			Console.WriteLine("Began - BuildUsersInbox");
			var watch = System.Diagnostics.Stopwatch.StartNew();
			try
			{
				var searcher = CreateSearch();
				var user = _assignment.Customer.InstagramAccount;

				searcher.ChangeUser(new APIClientContainer(_context, user.AccountId, user.Id));
				var fetchUserInbox = await searcher.SearchUserInbox(limit);

				searcher.ChangeUser(_apiClient);

				if (fetchUserInbox != null)
				{
					await _heartbeatLogic.RefreshMetaData(MetaDataType.FetchUserDirectInbox, _assignment.CustomerTopic.Category._id, user.Id);

					await _heartbeatLogic.AddMetaData(MetaDataType.FetchUserDirectInbox,
						_assignment.CustomerTopic.Category._id, new __Meta__<InstaDirectInboxContainer>(fetchUserInbox),
						user.Id);
				}
			}
			catch (Exception ee)
			{
				Console.WriteLine(ee.Message);
			}
			Console.WriteLine($"Ended - BuildUsersInbox : Took {TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds).TotalSeconds}s");
		}
		public async Task BuildUsersRecentComments(int howManyMedias = 10, int limit = 1)
		{
			Console.WriteLine("Began - BuildUsersRecentComments");
			var watch = System.Diagnostics.Stopwatch.StartNew();

			try
			{
				var searcher = CreateSearch();

				var user = _assignment.Customer.InstagramAccount;

				await _heartbeatLogic.RefreshMetaData(MetaDataType.UsersRecentComments, _assignment.CustomerTopic.Category._id, user.Id);

				var mediasForUser = (await _heartbeatLogic.GetMetaData<Media>
						(MetaDataType.FetchUserOwnProfile, _assignment.CustomerTopic.Category._id, user.Id))
					.Select(x => x.ObjectItem.Medias)
					.SquashMe()
					.OrderByDescending(x => x.TakenAt)
					.Take(howManyMedias);

				foreach (var media in mediasForUser)
				{
					var comments = await searcher.SearchInstagramMediaCommenters(media.MediaId, limit);

					await _heartbeatLogic.AddMetaData(MetaDataType.UsersRecentComments,
						_assignment.CustomerTopic.Category._id,
						new __Meta__<List<UserResponse<InstaComment>>>(comments), user.Id);
				}
			}
			catch (Exception ee)
			{
				Console.WriteLine(ee.Message);
			}
			Console.WriteLine($"Ended - BuildUsersRecentComments : Took {TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds).TotalSeconds}s");
		}
		public async Task BuildUserFromLikers(int cutBy = 1, double sleepTime = 0.05,
			int takeMediaAmount = 7, int takeUserMediaAmount = 100)
		{
			Console.WriteLine("Began - BuildUserFromLikers");
			var watch = System.Diagnostics.Stopwatch.StartNew();
			try
			{
				var searcher = CreateSearch();

				var results = (await _heartbeatLogic.GetMetaData<Media>(MetaDataType.FetchMediaByTopic, 
					_assignment.CustomerTopic.Category._id)).ToList();

				var recentResults = await _heartbeatLogic.GetMetaData<Media>(MetaDataType.FetchMediaByTopicRecent, 
					_assignment.CustomerTopic.Category._id);
				var meta_S = recentResults as __Meta__<Media>[] ?? recentResults.ToArray();
				if (meta_S.Any())
					results.AddRange(meta_S);

				await _heartbeatLogic.RefreshMetaData(MetaDataType.FetchUsersViaPostLiked, _assignment.CustomerTopic.Category._id);
				foreach (var v in results.TakeAny(takeMediaAmount))
				{
					if (v == null) continue;
					foreach (var _ in v.ObjectItem.Medias.Where(t => t.MediaFrom == MediaFrom.Instagram))
					{
						var suggested = await searcher.SearchInstagramMediaLikers(_.MediaId);
						if (suggested == null) return;
						if (suggested.Count < 1) return;
						var separateSuggested = suggested.TakeAny(takeUserMediaAmount).ToList().CutObject(cutBy);
						separateSuggested.ForEach(async s =>
						{
							var filtered = s.Where(x => x.Username != _assignment.Customer.InstagramAccount.Username).ToList();
							await _heartbeatLogic.AddMetaData(MetaDataType.FetchUsersViaPostLiked, 
								_assignment.CustomerTopic.Category._id,
								new __Meta__<List<UserResponse<string>>>(filtered));
						});
						await Task.Delay(TimeSpan.FromSeconds(sleepTime));
					}
				}
			}
			catch (Exception ee)
			{
				Console.WriteLine(ee.Message);
			}
			watch.Stop();
			Console.WriteLine($"Ended - BuildUserFromLikers : Took {TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds).TotalSeconds}s");
		}
		public async Task BuildMediaFromUsersLikers(int limit = 1, int cutBy = 1, double sleepTime = 0.05,
			int takeMediaAmount = 7, int takeUserMediaAmount = 10)
		{
			Console.WriteLine("Began - BuildMediaFromUsersLikers");
			var watch = System.Diagnostics.Stopwatch.StartNew();
			try
			{
				var searcher = CreateSearch();
				var results = (await _heartbeatLogic.GetMetaData<List<UserResponse<string>>>
					(MetaDataType.FetchUsersViaPostLiked, _assignment.CustomerTopic.Category._id)).ToList();

				await _heartbeatLogic.RefreshMetaData(MetaDataType.FetchMediaByLikers, _assignment.CustomerTopic.Category._id);
				foreach (var _ in results.TakeAny(takeMediaAmount))
				{
					if (_ == null) continue;
					foreach (var d in _.ObjectItem)
					{
						var suggested = await searcher.SearchUsersMediaDetailInstagram(d.Username, limit);
						if (suggested != null)
						{
							var sugVCut = suggested.CutObjects(cutBy).ToList();
							foreach (var z in sugVCut?.TakeAny(takeUserMediaAmount))
							{
								z.Medias = z.Medias.Where(t =>
									!t.HasLikedBefore
									&& !t.HasSeen
									&& t.User.Username != _assignment.Customer.InstagramAccount.Username).ToList();
								await _heartbeatLogic.AddMetaData(MetaDataType.FetchMediaByLikers, 
									_assignment.CustomerTopic.Category._id, new __Meta__<Media>(z));
							}
						}
						await Task.Delay(TimeSpan.FromSeconds(sleepTime));
					}
				}
			}
			catch (Exception ee)
			{
				Console.WriteLine(ee.Message);
			}
			watch.Stop();
			Console.WriteLine($"Ended - BuildMediaFromUsersLikers : Took {TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds).TotalSeconds}s");
		}
		public async Task BuildUsersFromCommenters(int limit = 1, int cutBy = 1, double sleepTime = 0.05,
			int takeMediaAmount = 7, int takeUserMediaAmount = 100)
		{
			Console.WriteLine("Began - BuildUsersFromCommenters");
			var watch = System.Diagnostics.Stopwatch.StartNew();
			try
			{
				var searcher = CreateSearch();

				var results = (await _heartbeatLogic.GetMetaData<Media>(MetaDataType.FetchMediaByTopic, 
					_assignment.CustomerTopic.Category._id)).ToList();
				var recentResults = await _heartbeatLogic.GetMetaData<Media>(MetaDataType.FetchMediaByTopicRecent,
					_assignment.CustomerTopic.Category._id);

				var meta_S = recentResults as __Meta__<Media>[] ?? recentResults.ToArray();
				
				if (meta_S.Any())
					results.AddRange(meta_S);
				
				await _heartbeatLogic.RefreshMetaData(MetaDataType.FetchUsersViaPostCommented,
					_assignment.CustomerTopic.Category._id);

				foreach (var v in results.TakeAny(takeMediaAmount))
				{
					if (v == null) continue;
					foreach (var _ in v.ObjectItem.Medias)
					{
						if (!_.IsCommentsDisabled && int.TryParse(_.CommentCount, out var count) && count > 0 && _.MediaFrom == MediaFrom.Instagram && _.MediaId != null)
						{
							var suggested = await searcher.SearchInstagramMediaCommenters(_.MediaId, limit);
							if (suggested == null) return;
							if (suggested.Count < 1) return;

							var separatedCommenter = suggested.TakeAny(takeUserMediaAmount).ToList().CutObject(cutBy);
							foreach (var s in separatedCommenter)
							{
								var filtered = s.Where(x => x.Username != _assignment.Customer.InstagramAccount.Username).ToList();
								await _heartbeatLogic.AddMetaData(MetaDataType.FetchUsersViaPostCommented,
									_assignment.CustomerTopic.Category._id,
									new __Meta__<List<UserResponse<InstaComment>>>(filtered));
							}
						}
						await Task.Delay(TimeSpan.FromSeconds(sleepTime));
					}
				}
			}
			catch (Exception ee)
			{
				Console.WriteLine(ee.Message);
			}
			watch.Stop();
			Console.WriteLine($"Ended - BuildUsersFromCommenters : Took {TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds).TotalSeconds}s");
		}
		public async Task BuildCommentsFromSpecifiedSource(MetaDataType extractFrom, MetaDataType saveTo,
			bool includeUser = false, int limit = 1, int cutBy = 1, double secondSleep = 0.05,
			int takeMediaAmount = 10, int takeuserAmount = 100)
		{
			Console.WriteLine("Began - BuildCommentsFromSpecifiedSource " + saveTo.ToString());
			var watch = System.Diagnostics.Stopwatch.StartNew();
			try
			{
				var searcher = CreateSearch();
				var res = await _heartbeatLogic.GetMetaData<Media>(extractFrom, 
					_assignment.CustomerTopic.Category._id, 
					includeUser ? _assignment.Customer.InstagramAccount.Id : null);

				if (res != null)
				{
					await _heartbeatLogic.RefreshMetaData(saveTo, _assignment.CustomerTopic.Category._id);
					foreach (var _ in res.TakeAny(takeMediaAmount))
					{
						if (_ == null) continue;
						foreach (var __ in _.ObjectItem.Medias)
						{
							if (!__.IsCommentsDisabled && int.TryParse(__.CommentCount, out var count) && count > 0 && __.MediaFrom == MediaFrom.Instagram && __.MediaId != null && !__.HasSeen)
							{
								var suggested = await searcher.SearchInstagramMediaCommenters(__.MediaId, limit);

								if (suggested != null)
								{
									var calcutta = suggested.TakeAny(takeuserAmount).ToList().CutObject(cutBy);
									foreach (var filtered in calcutta.Select(s => 
										s.Where(x => x.Username != _assignment.Customer.InstagramAccount.Username).ToList()))
									{
										await _heartbeatLogic.AddMetaData(saveTo, _assignment.CustomerTopic.Category._id,
											new __Meta__<List<UserResponse<InstaComment>>>(filtered),
											includeUser ? _assignment.Customer.InstagramAccount.Id : null);
									}
								}
							}
							await Task.Delay(TimeSpan.FromSeconds(secondSleep));
						}
					}
				}
			}
			catch (Exception ee)
			{
				Console.WriteLine(ee.Message);
			}
			watch.Stop();
			Console.WriteLine($"Ended - BuildCommentsFromSpecifiedSource  {saveTo.ToString()} : Took {TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds).TotalSeconds}s");
		}
		public async Task BuildMediaFromUsersCommenters(int limit = 1, int cutBy = 1, double secondsSleep = 0.05,
			int takeMediaAmount = 10, int takeUserMediaAmount = 10)
		{
			Console.WriteLine("Began - BuildMediaFromUsersCommenters");
			var watch = System.Diagnostics.Stopwatch.StartNew();
			try
			{
				var searcher = CreateSearch();
				var results = (await _heartbeatLogic.GetMetaData<List<UserResponse<InstaComment>>>
					(MetaDataType.FetchUsersViaPostCommented, _assignment.CustomerTopic.Category._id)).ToList();

				await _heartbeatLogic.RefreshMetaData(MetaDataType.FetchMediaByCommenters, _assignment.CustomerTopic.Category._id);
				foreach (var _ in results.TakeAny(takeMediaAmount))
				{
					if (_ == null) continue;
					foreach (var commenter in _.ObjectItem)
					{
						var suggested = await searcher.SearchUsersMediaDetailInstagram(commenter.Username, limit);
						if (suggested != null)
						{
							var sugVCut = suggested.CutObjects(cutBy).ToList();
							foreach (var z in sugVCut.TakeAny(takeUserMediaAmount))
							{
								z.Medias = z.Medias.Where(t =>
									!t.HasLikedBefore &&
									!t.HasSeen).ToList();

								var comments = new __Meta__<Media>(z);
								await _heartbeatLogic.AddMetaData(MetaDataType.FetchMediaByCommenters,
									_assignment.CustomerTopic.Category._id, comments);
							}
						}
						await Task.Delay(TimeSpan.FromSeconds(secondsSleep));
					}
				}
			}
			catch (Exception ee)
			{
				Console.WriteLine(ee.Message);
			}
			watch.Stop();
			Console.WriteLine($"Ended - BuildMediaFromUsersCommenters : Took {TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds).TotalSeconds}s");

		}
		#endregion

		#region External Search Stuff

		private SearchImageModel GoogleQueryBuilder(string color, string topics, IEnumerable<string> sites, int limit = 10,
			string imageType = null, string exactSize = null, string similarImage = null)
		{
			var query = new SearchImageModel
			{
				color = color,
				prefix_keywords = string.Join(", ", sites),
				keywords = topics,
				limit = limit,
				no_download = true,
				print_urls = true,
				type = imageType == "any" ? null : imageType,
				exact_size = exactSize,
				proxy = null,
				similar_images = similarImage,
				size = string.IsNullOrEmpty(exactSize) ? "large" : null
			};
			return query;
		}

		public async Task BuildGoogleImages(int limit = 50, int topicAmount = 1, int cutBy = 1)
		{
			Console.WriteLine("Began - BuildGoogleImages");
			var watch = System.Diagnostics.Stopwatch.StartNew();
			try
			{
				var res = await _heartbeatLogic.GetMetaData<Media>(MetaDataType.FetchMediaForSpecificUserGoogle,
					_assignment.CustomerTopic.Category._id, _assignment.Customer.InstagramAccount.Id);

				var by = new By { ActionType = 0, User = _assignment.Customer.InstagramAccount.Id };

				var meta_S = res as __Meta__<Media>[] ?? res.ToArray();

				if (!meta_S.Any() || meta_S.Where(x => x != null)
					.All(s => !s.SeenBy.Any(sb => sb.User == by.User && sb.ActionType == by.ActionType)))
				{
					var searcher = CreateSearch();

					var selectSubTopic = _assignment.CustomerTopic.Topics.TakeAny(1).SingleOrDefault();
					if (selectSubTopic == null)
						return;

					var relatedTopic = await _topicLookup.GetTopicByParentId(selectSubTopic._id);
					relatedTopic.Add(selectSubTopic);

					var searchQueryTopics = relatedTopic.TakeAny(1).SingleOrDefault();

					var prf = _assignment.Customer.Profile;

					var colorSelect = prf.Theme.Colors.TakeAny(1).SingleOrDefault();
					var imageType = ((ImageType)prf.AdditionalConfigurations.ImageType).GetDescription();

					if (colorSelect != null)
					{
						var go = searcher.SearchViaGoogle(GoogleQueryBuilder(colorSelect.Name, searchQueryTopics.Name,
							prf.AdditionalConfigurations.Sites, limit, exactSize: prf.AdditionalConfigurations.PostSize, imageType: imageType));
						
						if (go != null)
						{
							if (go.StatusCode == ResponseCode.Success)
							{
								await _heartbeatLogic.RefreshMetaData(MetaDataType.FetchMediaForSpecificUserGoogle, 
									_assignment.CustomerTopic.Category._id, 
									_assignment.Customer.InstagramAccount.Id);

								var cut = go.Result.CutObjects(cutBy).ToList();
								cut.ForEach(async x =>
								{
									await _heartbeatLogic.AddMetaData(MetaDataType.FetchMediaForSpecificUserGoogle,
										_assignment.CustomerTopic.Category._id,
										new __Meta__<Media>(x), _assignment.Customer.InstagramAccount.Id);
								});
							}
						}
					}
				}
			}
			catch (Exception ee)
			{
				Console.WriteLine(ee.Message);
			}
			Console.WriteLine($"Ended - BuildGoogleImages : Took {TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds).TotalSeconds}s");
		}
		public async Task BuildYandexImagesQuery(int limit = 2, int topicAmount = 1, int cutBy = 1)
		{
			Console.WriteLine("Began - BuildYandexImagesQuery");
			var watch = System.Diagnostics.Stopwatch.StartNew();
			try
			{
				var searcher = CreateSearch();
				// should have proxy for scrape

				var res = await _heartbeatLogic.GetMetaData<Media>(MetaDataType.FetchMediaForSepcificUserYandexQuery,
					_assignment.CustomerTopic.Category._id, _assignment.Customer.InstagramAccount.Id);

				var by = new By { ActionType = 0, User = _assignment.Customer.InstagramAccount.Id };

				var meta_S = res as __Meta__<Media>[] ?? res.ToArray();
				if (!meta_S.Any() || meta_S.Where(x => x != null).All(s => !s.SeenBy.Any(sb => sb.User == by.User && sb.ActionType == by.ActionType)))
				{
					var selectSubTopic = _assignment.CustomerTopic.Topics.TakeAny(1).SingleOrDefault();
					if (selectSubTopic == null)
						return;

					var relatedTopic = await _topicLookup.GetTopicByParentId(selectSubTopic._id);
					relatedTopic.Add(selectSubTopic);

					var searchQuery = relatedTopic.TakeAny(1).SingleOrDefault();

					var prf = _assignment.Customer.Profile;
					var colorSelect = prf.Theme.Colors.TakeAny(1).SingleOrDefault();

					if (colorSelect != null)
					{
						var yandexSearchQuery = new YandexSearchQuery
						{
							OriginalTopic = _assignment.CustomerTopic.Category,
							SearchQuery = searchQuery.Name,
							Color = colorSelect.Name.GetValueFromDescription<ColorType>(),
							Format = FormatType.Any,
							Orientation = Orientation.Any,
							Size = SizeType.Large,
							Type = (ImageType)prf.AdditionalConfigurations.ImageType == ImageType.Any ? Enum.GetValues(typeof(ImageType))
								.Cast<ImageType>().Where(x => x != ImageType.Any).TakeAny(1).SingleOrDefault() : (ImageType)prf.AdditionalConfigurations.ImageType
						};

						var yan = searcher.SearchViaYandex(yandexSearchQuery, limit);
						if (yan != null)
						{
							switch (yan.StatusCode)
							{
								case ResponseCode.Success:
									{
										await _heartbeatLogic.RefreshMetaData(MetaDataType.FetchMediaForSepcificUserYandexQuery,
											_assignment.CustomerTopic.Category._id, _assignment.Customer.InstagramAccount.Id);
										var cut = yan.Result.CutObjects(cutBy).ToList();
										cut.ForEach(async x =>
										{
											await _heartbeatLogic.AddMetaData(MetaDataType.FetchMediaForSepcificUserYandexQuery,
												_assignment.CustomerTopic.Category._id,
												new __Meta__<Media>(x), _assignment.Customer.InstagramAccount.Id);
										});
										break;
									}
								case ResponseCode.Timeout:
									break;
								case ResponseCode.CaptchaRequired:
									break;
							}
						}
					}
				}
			}
			catch (Exception ee)
			{
				Console.WriteLine(ee.Message);
			}
			Console.WriteLine($"Ended - BuildYandexImagesQuery : Took {TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds).TotalSeconds}s");
		}
		public async Task BuildYandexImages(int limit = 3, int takeTopicAmount = 1, int cutBy = 1)
		{
			Console.WriteLine("Began - BuildYandexImages");
			var watch = System.Diagnostics.Stopwatch.StartNew();
			try
			{
				var searcher = CreateSearch();

				var imagesLike = _assignment.Customer.Profile.Theme.ImagesLike;
				if (imagesLike == null) return;
				var res = await _heartbeatLogic.GetMetaData<Media>(MetaDataType.FetchMediaForSpecificUserYandex, 
					_assignment.CustomerTopic.Category._id, _assignment.Customer.InstagramAccount.Id);

				var by = new By { ActionType = 0, User = _assignment.Customer.InstagramAccount.Id };

				var meta_S = res as __Meta__<Media>[] ?? res.ToArray();
				if (!meta_S.Any() || meta_S.Where(x => x != null).All(s => !s.SeenBy.Any(sb => sb.User == by.User && sb.ActionType == by.ActionType)))
				{
					var filter = imagesLike.Where(s => s.TopicGroup.Equals(_assignment.CustomerTopic.Category));
					
					var groupSimilarImages = filter as GroupImagesAlike[] ?? filter.ToArray();
					var yan = searcher.SearchViaYandexBySimilarImages(groupSimilarImages.TakeAny(takeTopicAmount).ToList(), limit);

					if (yan == null
					|| yan.StatusCode == ResponseCode.CaptchaRequired
					|| yan.StatusCode == ResponseCode.InternalServerError
					|| yan.StatusCode == ResponseCode.ReachedEndAndNull)
					{
						yan = searcher.SearchYandexSimilarSafeMode(groupSimilarImages.TakeAny(takeTopicAmount).ToList(), limit * 25);
						if (yan == null || yan.StatusCode == ResponseCode.CaptchaRequired)
							yan = searcher.SearchSimilarImagesViaGoogle(groupSimilarImages.TakeAny(takeTopicAmount).ToList(), limit * 25);
					}
					if (yan != null)
					{
						switch (yan.StatusCode)
						{
							case ResponseCode.Success:
								{
									await _heartbeatLogic.RefreshMetaData(MetaDataType.FetchMediaForSpecificUserYandex, 
										_assignment.CustomerTopic.Category._id,
										_assignment.Customer.InstagramAccount.Id);

									var cut = yan.Result.CutObjects(cutBy).ToList();
									cut.ForEach(async x =>
									{
										await _heartbeatLogic.AddMetaData(MetaDataType.FetchMediaForSpecificUserYandex, _assignment.CustomerTopic.Category._id,
											new __Meta__<Media>(x), _assignment.Customer.InstagramAccount.Id);
									});
									break;
								}
							case ResponseCode.Timeout:
								//could try with a different proxy
								//await BuildYandexImages(limit,takeTopicAmount,cutBy);
								break;
							case ResponseCode.CaptchaRequired:
								{
									if (yan.Result?.Medias?.Count > 0)
									{
										await _heartbeatLogic.RefreshMetaData(MetaDataType.FetchMediaForSpecificUserYandex,
											_assignment.CustomerTopic.Category._id, _assignment.Customer.InstagramAccount.Id);
										var cut = yan.Result.CutObjects(cutBy).ToList();
										cut.ForEach(async x =>
										{
											await _heartbeatLogic.AddMetaData(MetaDataType.FetchMediaForSpecificUserYandex,
												_assignment.CustomerTopic.Category._id,
												new __Meta__<Media>(x), _assignment.Customer.InstagramAccount.Id);
										});
									}

									break;
								}
						}
					}
				}
			}
			catch (Exception ee)
			{
				Console.WriteLine(ee.Message);
			}
			Console.WriteLine($"Ended - BuildYandexImages : Took {TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds).TotalSeconds}s");
		}
		#endregion
	}
}
