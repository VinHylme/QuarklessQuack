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
using QuarklessLogic.Handlers.SearchProvider;
using QuarklessLogic.Handlers.WorkerManagerService;
using QuarklessLogic.Logic.InstagramAccountLogic;
using QuarklessLogic.Logic.TopicLookupLogic;
using QuarklessLogic.ServicesLogic.HeartbeatLogic;

namespace Quarkless.Services.Heartbeat
{
	public class MetadataExtract
	{	
		private readonly FullUserDetail _customer;
		private readonly IHeartbeatLogic _heartbeatLogic;
		private readonly IInstagramAccountLogic _instagramAccountLogic;
		private readonly ITopicLookupLogic _topicLookup;
		private readonly IWorkerManager _workerManager;
		private readonly ISearchProvider _searchProvider;
		public MetadataExtract(IHeartbeatLogic heartbeatLogic, IInstagramAccountLogic accountLogic, 
			ITopicLookupLogic topicLookup, IWorkerManager workerManager, ISearchProvider searchProvider, 
			FullUserDetail customer)
		{
			_customer = customer;
			_heartbeatLogic = heartbeatLogic;
			_instagramAccountLogic = accountLogic;
			_topicLookup = topicLookup;
			_searchProvider = searchProvider;
			_workerManager = workerManager;
		}

		#region Instagram Stuff
		public async Task BuildBase(int limit = 2, int cutBy = 1, int takeTopicAmount = 1)
		{
			Console.WriteLine("Began - BuildBase");
			var watch = System.Diagnostics.Stopwatch.StartNew();
			try
			{
				var topicTotal = new List<CTopic>();
				_customer.Profile.ProfileTopic.Topics.ForEach(topic =>
				{
					topicTotal.Add(topic);
					var relatedTopic = _topicLookup.GetTopicByParentId(topic._id).Result;
					if(relatedTopic.Any())
						topicTotal.AddRange(relatedTopic);
				});

				if (!topicTotal.Any()) 
					return;

				var topicSelect = topicTotal.Distinct().TakeAny(takeTopicAmount).ToList();

				await _workerManager.PerformTaskOnWorkers(async workers =>
				{
					var top = workers.PerformAction(async worker =>
					{
						var instagramSearch = _searchProvider.InstagramSearch(worker.Client, worker.Client.GetContext.Proxy);
						var topMedias = await instagramSearch.SearchMediaDetailInstagram(topicSelect, limit);

						if (topMedias != null)
						{
							await _heartbeatLogic.RefreshMetaData(MetaDataType.FetchMediaByTopic,
								_customer.Profile.ProfileTopic.Category._id);
							var meta = topMedias.CutObjects(cutBy);

							meta.ToList().ForEach(async z =>
							{
								z.Medias = z.Medias.Where(t => !t.HasLikedBefore && !t.HasSeen
								     &&
								     t.User.Username != _customer
								         .InstagramAccount.Username).ToList();

								await _heartbeatLogic.AddMetaData(MetaDataType.FetchMediaByTopic,
									_customer.Profile.ProfileTopic.Category._id,
									new __Meta__<Media>(z));
							});
						}
					});
					var recent = workers.PerformAction(async worker =>
					{
						var instagramSearch = _searchProvider.InstagramSearch(worker.Client,
							worker.Client.GetContext.Proxy);
						var recentMedias = await instagramSearch.SearchMediaDetailInstagram(topicSelect, limit, true);
						
						if (recentMedias != null)
						{
							await _heartbeatLogic.RefreshMetaData(MetaDataType.FetchMediaByTopicRecent,
								_customer.Profile.ProfileTopic.Category._id);

							var meta = recentMedias.CutObjects(cutBy);

							meta.ToList().ForEach(async z =>
							{
								z.Medias = z.Medias.Where(t => !t.HasLikedBefore && !t.HasSeen
									&&
									t.User.Username != _customer
									.InstagramAccount.Username).ToList();

								await _heartbeatLogic.AddMetaData(MetaDataType.FetchMediaByTopicRecent,
									_customer.Profile.ProfileTopic.Category._id,
									new __Meta__<Media>(z));
							});
						}
					});
					await Task.WhenAll(top, recent);
				});
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
				var user = _customer.InstagramAccount;
				var userTargetList = _customer.Profile.UserTargetList;
				if (userTargetList != null && userTargetList.Count > 0)
				{
					await _workerManager.PerformTaskOnWorkers(async workers =>
					{
						foreach (var userLoc in userTargetList)
						{
							var results = await workers.PerformQueryTask<Media>(async (worker,query,l) =>
							{
								var instagramSearch = _searchProvider.InstagramSearch(worker.Client,
									worker.Client.GetContext.Proxy);

								return await instagramSearch.SearchUsersMediaDetailInstagram(query, l);
							}, userLoc, limit);

							if(results == null) continue;
							await _heartbeatLogic.RefreshMetaData(MetaDataType.FetchMediaByUserTargetList, user.Id);
							foreach (var s in results.CutObjects(cutBy))
							{
								s.Medias = s.Medias.Where(t => !t.HasLikedBefore && !t.HasSeen).ToList();
								await _heartbeatLogic.AddMetaData(MetaDataType.FetchMediaByUserTargetList,
									_customer.Profile.ProfileTopic.Category._id,
									new __Meta__<Media>(s), user.Id);
							}
						}
					});
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
				var user = _customer.InstagramAccount;
				var profileLocationTargetList = _customer.Profile.LocationTargetList;
				if (profileLocationTargetList != null && profileLocationTargetList.Count > 0)
				{
					await _workerManager.PerformTaskOnWorkers(async workers =>
					{
						foreach (var targetLocation in profileLocationTargetList)
						{
							var results = await workers.PerformQueryTask(async (worker,location,lim) =>
							{
								var instagramSearch = _searchProvider.InstagramSearch(worker.Client,
									worker.Client.GetContext.Proxy);
								var topMediasByLocation = await instagramSearch.SearchTopLocationMediaDetailInstagram((Location)location, lim);
								
								var recentMediasByLocation =
									await instagramSearch.SearchRecentLocationMediaDetailInstagram((Location)location, lim);
								
								await Task.Delay(TimeSpan.FromSeconds(1));

								if(recentMediasByLocation!=null && recentMediasByLocation.Medias.Count > 0)
									topMediasByLocation?.Medias.AddRange(recentMediasByLocation.Medias);

								return topMediasByLocation;
							},targetLocation, limit);
							if(results == null) continue;

							await _heartbeatLogic.RefreshMetaData(MetaDataType.FetchMediaByUserLocationTargetList,
								_customer.Profile.ProfileTopic.Category._id, user.Id);

							foreach (var s in results.CutObjects(cutBy))
							{
								s.Medias = s.Medias.Where(t => !t.HasLikedBefore && !t.HasSeen &&
									t.User.Username != _customer.InstagramAccount.Username).ToList();

								await _heartbeatLogic.AddMetaData(MetaDataType.FetchMediaByUserLocationTargetList,
									_customer.Profile.ProfileTopic.Category._id,
									new __Meta__<Media>(s), user.Id);
							}
						}
					});
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
				var user = _customer.InstagramAccount;
				await _workerManager.PerformTaskOnWorkers(async workers =>
				{
					var results = await workers.PerformQueryTask(async (worker, username, lim) =>
					{
						var instagramSearch = _searchProvider.InstagramSearch(worker.Client,
							worker.Client.GetContext.Proxy);
						return await instagramSearch.SearchUsersMediaDetailInstagram(username, lim);
					},user.Username, limit);

					if (results != null)
					{
						await _heartbeatLogic.RefreshMetaData(MetaDataType.FetchUserOwnProfile,
							_customer.Profile.ProfileTopic.Category._id, user.Id);

						foreach (var media in results.CutObjects(cutBy))
						{
							await _heartbeatLogic.AddMetaData(MetaDataType.FetchUserOwnProfile,
								_customer.Profile.ProfileTopic.Category._id,
								new __Meta__<Media>(media), user.Id);
						}

						var userDetail = results.Medias.First();
						await workers.PerformAction(async worker =>
						{
							var instagramSearch = _searchProvider.InstagramSearch(worker.Client,
								worker.Client.GetContext.Proxy);
							var details = await instagramSearch.SearchInstagramFullUserDetail(userDetail.User.UserId);
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
						});
					}
				});
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
				var user = _customer.InstagramAccount;
				await _workerManager.PerformTaskOnWorkers(async workers =>
				{
					await workers.PerformAction(async worker =>
					{
						var tempWorker = worker;
						tempWorker.ChangeUser(user.AccountId, user.Id);

						var instagramSearch = _searchProvider.InstagramSearch(tempWorker.Client,
							tempWorker.Client.GetContext.Proxy);

						var results = await instagramSearch.SearchUserFeedMediaDetailInstagram(limit: limit);

						if (results != null)
						{
							await _heartbeatLogic.RefreshMetaData(MetaDataType.FetchUsersFeed,
								_customer.Profile.ProfileTopic.Category._id, user.Id);

							foreach (var s in results.CutObjects(cutBy))
							{
								s.Medias = s.Medias.Where(_ => !_.HasLikedBefore && !_.HasSeen
									&& _.User.Username != user.Username).ToList();

								await _heartbeatLogic.AddMetaData(MetaDataType.FetchUsersFeed,
									_customer.Profile.ProfileTopic.Category._id,
									new __Meta__<Media>(s), user.Id);
							}
						}
					});
				});
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
				var user = _customer.InstagramAccount;
				await _workerManager.PerformTaskOnWorkers(async workers =>
				{
					await workers.PerformAction(async worker =>
					{
						var instagramSearch = _searchProvider.InstagramSearch(worker.Client,
							worker.Client.GetContext.Proxy);

						var fetchUsersMedia = await instagramSearch.GetUserFollowingList(user.Username, limit);

						if (fetchUsersMedia != null)
						{
							await _heartbeatLogic.RefreshMetaData(MetaDataType.FetchUsersFollowingList,
								_customer.Profile.ProfileTopic.Category._id, user.Id);

							foreach (var s in fetchUsersMedia.ToList().CutObject(cutBy))
							{
								await _heartbeatLogic.AddMetaData(MetaDataType.FetchUsersFollowingList,
									_customer.Profile.ProfileTopic.Category._id,
									new __Meta__<List<UserResponse<string>>>(s), user.Id);
							}
						}
					});
				});

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
				var user = _customer.InstagramAccount;
				await _workerManager.PerformTaskOnWorkers(async workers =>
				{
					await workers.PerformAction(async worker =>
					{
						var instagramSearch = _searchProvider.InstagramSearch(worker.Client,
							worker.Client.GetContext.Proxy);

						var fetchUsersMedia = await instagramSearch.GetUsersFollowersList(user.Username, limit);

						if (fetchUsersMedia != null)
						{
							await _heartbeatLogic.RefreshMetaData(MetaDataType.FetchUsersFollowerList,
								_customer.Profile.ProfileTopic.Category._id, user.Id);
							foreach (var s in fetchUsersMedia.ToList().CutObject(cutBy))
							{
								await _heartbeatLogic.AddMetaData(MetaDataType.FetchUsersFollowerList,
									_customer.Profile.ProfileTopic.Category._id,
									new __Meta__<List<UserResponse<string>>>(s), user.Id);
							}
						}
					});
				});
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
				var user = _customer.InstagramAccount;
				await _workerManager.PerformTaskOnWorkers(async workers =>
				{
					await workers.PerformAction(async worker =>
					{
						var tempWorker = worker;
						tempWorker.ChangeUser(user.AccountId, user.Id);

						var instagramSearch = _searchProvider.InstagramSearch(tempWorker.Client,
							tempWorker.Client.GetContext.Proxy);

						var fetchUsersMedia = await instagramSearch.GetSuggestedPeopleToFollow(limit);

						if (fetchUsersMedia != null)
						{
							await _heartbeatLogic.RefreshMetaData(MetaDataType.FetchUsersFollowSuggestions,
								_customer.Profile.ProfileTopic.Category._id, user.Id);

							foreach (var s in fetchUsersMedia.ToList().CutObject(cutObjectBy))
							{
								await _heartbeatLogic.AddMetaData(MetaDataType.FetchUsersFollowSuggestions,
									_customer.Profile.ProfileTopic.Category._id,
									new __Meta__<List<UserResponse<UserSuggestionDetails>>>(s), user.Id);
							}
						}
					});
				});
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
				var user = _customer.InstagramAccount;
				await _workerManager.PerformTaskOnWorkers(async workers =>
				{
					await workers.PerformAction(async worker =>
					{
						var tempWorker = worker;
						tempWorker.ChangeUser(user.AccountId, user.Id);

						var instagramSearch = _searchProvider.InstagramSearch(tempWorker.Client,
							tempWorker.Client.GetContext.Proxy);

						var fetchUserInbox = await instagramSearch.SearchUserInbox(limit);
						if (fetchUserInbox != null)
						{
							await _heartbeatLogic.RefreshMetaData(MetaDataType.FetchUserDirectInbox,
								_customer.Profile.ProfileTopic.Category._id, user.Id);

							await _heartbeatLogic.AddMetaData(MetaDataType.FetchUserDirectInbox,
								_customer.Profile.ProfileTopic.Category._id,
								new __Meta__<InstaDirectInboxContainer>(fetchUserInbox),
								user.Id);
						}
					});
				});
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
				var user = _customer.InstagramAccount;

				await _heartbeatLogic.RefreshMetaData(MetaDataType.UsersRecentComments, _customer.Profile.ProfileTopic.Category._id, user.Id);

				var mediasForUser = (await _heartbeatLogic.GetMetaData<Media>
						(MetaDataType.FetchUserOwnProfile, _customer.Profile.ProfileTopic.Category._id, user.Id))
					.Select(x => x.ObjectItem.Medias)
					.SquashMe()
					.OrderByDescending(x => x.TakenAt)
					.Take(howManyMedias);

				await _workerManager.PerformTaskOnWorkers(async workers =>
				{
					await workers.PerformAction(async worker =>
					{
						var instagramSearch = _searchProvider.InstagramSearch(worker.Client,
							worker.Client.GetContext.Proxy);

						foreach (var media in mediasForUser)
						{
							var comments = await instagramSearch.SearchInstagramMediaCommenters(media.MediaId, limit);

							await _heartbeatLogic.AddMetaData(MetaDataType.UsersRecentComments,
								_customer.Profile.ProfileTopic.Category._id,
								new __Meta__<List<UserResponse<InstaComment>>>(comments), user.Id);
						}
					});
				});
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
				var results = (await _heartbeatLogic.GetMetaData<Media>(MetaDataType.FetchMediaByTopic, 
					_customer.Profile.ProfileTopic.Category._id)).ToList();

				var recentResults = await _heartbeatLogic.GetMetaData<Media>(MetaDataType.FetchMediaByTopicRecent, 
					_customer.Profile.ProfileTopic.Category._id);

				var metaS = recentResults as __Meta__<Media>[] ?? recentResults.ToArray();
				if (metaS.Any())
					results.AddRange(metaS);

				await _heartbeatLogic.RefreshMetaData(MetaDataType.FetchUsersViaPostLiked, _customer.Profile.ProfileTopic.Category._id);
				await _workerManager.PerformTaskOnWorkers(async workers =>
				{
					await workers.PerformAction(async worker =>
					{
						var instagramSearch = _searchProvider.InstagramSearch(worker.Client,
							worker.Client.GetContext.Proxy);

						foreach (var medias in results.TakeAny(takeMediaAmount))
						{
							if (medias == null) continue;

							foreach (var media in medias.ObjectItem.Medias.Where(t => t.MediaFrom == MediaFrom.Instagram))
							{
								var suggested = await instagramSearch.SearchInstagramMediaLikers(media.MediaId);
								if (suggested == null) return;
								if (suggested.Count < 1) return;
								var separateSuggested = suggested.TakeAny(takeUserMediaAmount).ToList().CutObject(cutBy);
								separateSuggested.ForEach(async s =>
								{
									var filtered = s.Where(x => x.Username != _customer.InstagramAccount.Username).ToList();
									await _heartbeatLogic.AddMetaData(MetaDataType.FetchUsersViaPostLiked,
										_customer.Profile.ProfileTopic.Category._id,
										new __Meta__<List<UserResponse<string>>>(filtered));
								});
								await Task.Delay(TimeSpan.FromSeconds(sleepTime));
							}
						}
					});
				});
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
				var results = (await _heartbeatLogic.GetMetaData<List<UserResponse<string>>>
					(MetaDataType.FetchUsersViaPostLiked, _customer.Profile.ProfileTopic.Category._id)).ToList();

				await _heartbeatLogic.RefreshMetaData(MetaDataType.FetchMediaByLikers, _customer.Profile.ProfileTopic.Category._id);
				await _workerManager.PerformTaskOnWorkers(async workers =>
				{
					await workers.PerformAction(async worker =>
					{
						var instagramSearch = _searchProvider.InstagramSearch(worker.Client,
							worker.Client.GetContext.Proxy);

						foreach (var _ in results.TakeAny(takeMediaAmount))
						{
							if (_ == null) continue;
							foreach (var d in _.ObjectItem)
							{
								var suggested = await instagramSearch.SearchUsersMediaDetailInstagram(d.Username, limit);
								if (suggested != null)
								{
									var sugVCut = suggested.CutObjects(cutBy).ToList();
									foreach (var z in sugVCut?.TakeAny(takeUserMediaAmount))
									{
										z.Medias = z.Medias.Where(t => !t.HasLikedBefore && !t.HasSeen
											&& t.User.Username != _customer.InstagramAccount.Username).ToList();

										await _heartbeatLogic.AddMetaData(MetaDataType.FetchMediaByLikers,
											_customer.Profile.ProfileTopic.Category._id, new __Meta__<Media>(z));
									}
								}
								await Task.Delay(TimeSpan.FromSeconds(sleepTime));
							}
						}
					});
				});
				
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
				var results = (await _heartbeatLogic.GetMetaData<Media>(MetaDataType.FetchMediaByTopic, 
					_customer.Profile.ProfileTopic.Category._id)).ToList();

				var recentResults = await _heartbeatLogic.GetMetaData<Media>(MetaDataType.FetchMediaByTopicRecent,
					_customer.Profile.ProfileTopic.Category._id);

				var metaS = recentResults as __Meta__<Media>[] ?? recentResults.ToArray();
				
				if (metaS.Any())
					results.AddRange(metaS);
				
				await _heartbeatLogic.RefreshMetaData(MetaDataType.FetchUsersViaPostCommented,
					_customer.Profile.ProfileTopic.Category._id);

				await _workerManager.PerformTaskOnWorkers(async workers =>
				{
					await workers.PerformAction(async worker =>
					{
						var instagramSearch = _searchProvider.InstagramSearch(worker.Client,
							worker.Client.GetContext.Proxy);

						foreach (var medias in results.TakeAny(takeMediaAmount))
						{
							if (medias == null) continue;
							foreach (var media in medias.ObjectItem.Medias)
							{
								if (!media.IsCommentsDisabled && int.TryParse(media.CommentCount, out var count) &&
								    count > 0 &&
								    media.MediaFrom == MediaFrom.Instagram && media.MediaId != null)
								{
									var suggested = await instagramSearch
										.SearchInstagramMediaCommenters(media.MediaId, limit);

									if (suggested == null) return;
									if (suggested.Count < 1) return;

									var separatedCommenter = suggested.TakeAny(takeUserMediaAmount)
										.ToList().CutObject(cutBy);

									foreach (var filtered in separatedCommenter.Select(s => s.Where(x =>
											x.Username != _customer.InstagramAccount.Username).ToList()))
									{
										await _heartbeatLogic.AddMetaData(MetaDataType.FetchUsersViaPostCommented,
											_customer.Profile.ProfileTopic.Category._id,
											new __Meta__<List<UserResponse<InstaComment>>>(filtered));
									}
								}

								await Task.Delay(TimeSpan.FromSeconds(sleepTime));
							}
						}
					});
				});
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
			int takeMediaAmount = 10, int takeUserAmount = 100)
		{
			Console.WriteLine("Began - BuildCommentsFromSpecifiedSource " + saveTo.ToString());
			var watch = System.Diagnostics.Stopwatch.StartNew();
			try
			{
				var res = await _heartbeatLogic.GetMetaData<Media>(extractFrom, 
					_customer.Profile.ProfileTopic.Category._id, 
					includeUser ? _customer.InstagramAccount.Id : null);

				if (res != null)
				{
					await _heartbeatLogic.RefreshMetaData(saveTo, _customer.Profile.ProfileTopic.Category._id);
					await _workerManager.PerformTaskOnWorkers(async workers =>
					{
						await workers.PerformAction(async worker =>
						{
							var instagramSearch = _searchProvider.InstagramSearch(worker.Client,
								worker.Client.GetContext.Proxy);

							foreach (var medias in res.TakeAny(takeMediaAmount))
							{
								if (medias == null) continue;
								foreach (var media in medias.ObjectItem.Medias)
								{
									if (!media.IsCommentsDisabled && int.TryParse(media.CommentCount, out var count) &&
									    count > 0 && media.MediaFrom == MediaFrom.Instagram 
									    && media.MediaId != null && !media.HasSeen)
									{

										var suggested = await instagramSearch
											.SearchInstagramMediaCommenters(media.MediaId, limit);

										if (suggested != null)
										{
											var calcutta = suggested.TakeAny(takeUserAmount).ToList().CutObject(cutBy);
											
											foreach (var filtered in calcutta.Select(s =>
												s.Where(x => x.Username != _customer.InstagramAccount.Username)
													.ToList()))
											{
												await _heartbeatLogic.AddMetaData(saveTo,
													_customer.Profile.ProfileTopic.Category._id,
													new __Meta__<List<UserResponse<InstaComment>>>(filtered),
													includeUser ? _customer.InstagramAccount.Id : null);
											}
										}
									}
									await Task.Delay(TimeSpan.FromSeconds(secondSleep));
								}
							}
						});
					});
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
				var results = (await _heartbeatLogic.GetMetaData<List<UserResponse<InstaComment>>>
					(MetaDataType.FetchUsersViaPostCommented, _customer.Profile.ProfileTopic.Category._id)).ToList();

				await _heartbeatLogic.RefreshMetaData(MetaDataType.FetchMediaByCommenters, _customer.Profile.ProfileTopic.Category._id);
				await _workerManager.PerformTaskOnWorkers(async workers =>
				{
					await workers.PerformAction(async worker =>
					{
						var instagramSearch = _searchProvider.InstagramSearch(worker.Client,
							worker.Client.GetContext.Proxy);

						foreach (var medias in results.TakeAny(takeMediaAmount))
						{
							if (medias == null) continue;
							foreach (var commenter in medias.ObjectItem)
							{
								var suggested = await instagramSearch.
									SearchUsersMediaDetailInstagram(commenter.Username, limit);

								if (suggested != null)
								{
									var sugVCut = suggested.CutObjects(cutBy).ToList();
									foreach (var z in sugVCut.TakeAny(takeUserMediaAmount))
									{
										z.Medias = z.Medias.Where(t => !t.HasLikedBefore && !t.HasSeen).ToList();

										var comments = new __Meta__<Media>(z);
										await _heartbeatLogic.AddMetaData(MetaDataType.FetchMediaByCommenters,
											_customer.Profile.ProfileTopic.Category._id, comments);
									}
								}

								await Task.Delay(TimeSpan.FromSeconds(secondsSleep));
							}
						}
					});
				});
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
					_customer.Profile.ProfileTopic.Category._id, _customer.InstagramAccount.Id);

				var by = new By { ActionType = 0, User = _customer.InstagramAccount.Id };

				var metaS = res as __Meta__<Media>[] ?? res.ToArray();

				if (!metaS.Any() || metaS.Where(x => x != null)
					.All(s => !s.SeenBy.Any(sb => sb.User == by.User && sb.ActionType == by.ActionType)))
				{
					var selectSubTopic = _customer.Profile.ProfileTopic.Topics.TakeAny(1).SingleOrDefault();
					if (selectSubTopic == null)
						return;

					var relatedTopic = await _topicLookup.GetTopicByParentId(selectSubTopic._id);
					relatedTopic.Add(selectSubTopic);

					var searchQueryTopics = relatedTopic.TakeAny(1).SingleOrDefault();

					var prf = _customer.Profile;

					var colorSelect = prf.Theme.Colors.TakeAny(1).SingleOrDefault();
					var imageType = ((ImageType)prf.AdditionalConfigurations.ImageType).GetDescription();

					if (colorSelect != null)
					{
						var go = _searchProvider.GoogleSearch.WithProxy().SearchViaGoogle(GoogleQueryBuilder(colorSelect.Name, 
							searchQueryTopics?.Name, prf.AdditionalConfigurations.Sites, 
							limit, exactSize: prf.AdditionalConfigurations.PostSize, imageType: imageType));
						
						if (go != null)
						{
							if (go.StatusCode == ResponseCode.Success)
							{
								await _heartbeatLogic.RefreshMetaData(MetaDataType.FetchMediaForSpecificUserGoogle, 
									_customer.Profile.ProfileTopic.Category._id, 
									_customer.InstagramAccount.Id);

								var cut = go.Result.CutObjects(cutBy).ToList();
								cut.ForEach(async x =>
								{
									await _heartbeatLogic.AddMetaData(MetaDataType.FetchMediaForSpecificUserGoogle,
										_customer.Profile.ProfileTopic.Category._id,
										new __Meta__<Media>(x), _customer.InstagramAccount.Id);
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
				// should have proxy for scrape

				var res = await _heartbeatLogic.GetMetaData<Media>(MetaDataType.FetchMediaForSepcificUserYandexQuery,
					_customer.Profile.ProfileTopic.Category._id, _customer.InstagramAccount.Id);

				var by = new By { ActionType = 0, User = _customer.InstagramAccount.Id };

				var metaS = res as __Meta__<Media>[] ?? res.ToArray();
				if (!metaS.Any() || metaS.Where(x => x != null).All(s => !s.SeenBy.Any(sb => sb.User == by.User && sb.ActionType == by.ActionType)))
				{
					var selectSubTopic = _customer.Profile.ProfileTopic.Topics.TakeAny(1).SingleOrDefault();
					if (selectSubTopic == null)
						return;

					var relatedTopic = await _topicLookup.GetTopicByParentId(selectSubTopic._id);
					relatedTopic.Add(selectSubTopic);

					var searchQuery = relatedTopic.TakeAny(1).SingleOrDefault();

					var prf = _customer.Profile;
					var colorSelect = prf.Theme.Colors.TakeAny(1).SingleOrDefault();

					if (colorSelect != null)
					{
						var yandexSearchQuery = new YandexSearchQuery
						{
							OriginalTopic = _customer.Profile.ProfileTopic.Category,
							SearchQuery = searchQuery.Name,
							Color = colorSelect.Name.GetValueFromDescription<ColorType>(),
							Format = FormatType.Any,
							Orientation = Orientation.Any,
							Size = SizeType.Large,
							Type = (ImageType)prf.AdditionalConfigurations.ImageType == ImageType.Any ? Enum.GetValues(typeof(ImageType))
								.Cast<ImageType>().Where(x => x != ImageType.Any).TakeAny(1).SingleOrDefault() : (ImageType)prf.AdditionalConfigurations.ImageType
						};

						var yan = _searchProvider.YandexSearch.WithProxy().SearchQueryRest(yandexSearchQuery, limit);
						if (yan != null)
						{
							switch (yan.StatusCode)
							{
								case ResponseCode.Success:
									{
										await _heartbeatLogic.RefreshMetaData(MetaDataType.FetchMediaForSepcificUserYandexQuery,
											_customer.Profile.ProfileTopic.Category._id, _customer.InstagramAccount.Id);
										var cut = yan.Result.CutObjects(cutBy).ToList();
										cut.ForEach(async x =>
										{
											await _heartbeatLogic.AddMetaData(MetaDataType.FetchMediaForSepcificUserYandexQuery,
												_customer.Profile.ProfileTopic.Category._id,
												new __Meta__<Media>(x), _customer.InstagramAccount.Id);
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
				var imagesLike = _customer.Profile.Theme.ImagesLike;
				if (imagesLike == null) return;
				var res = await _heartbeatLogic.GetMetaData<Media>(MetaDataType.FetchMediaForSpecificUserYandex, 
					_customer.Profile.ProfileTopic.Category._id, _customer.InstagramAccount.Id);

				var by = new By { ActionType = 0, User = _customer.InstagramAccount.Id };

				var metaS = res as __Meta__<Media>[] ?? res.ToArray();

				if (!metaS.Any() || metaS.Where(x => x != null).All(s => !s.SeenBy.Any(sb => sb.User == by.User && sb.ActionType == by.ActionType)))
				{
					var filter = imagesLike.Where(s => s.TopicGroup.Name.Equals(_customer.Profile.ProfileTopic.Category.Name));
					
					var groupSimilarImages = filter as GroupImagesAlike[] ?? filter.ToArray();
					var yan = _searchProvider.YandexSearch.WithProxy().SearchRelatedImagesRest(groupSimilarImages.TakeAny(takeTopicAmount).ToList(), limit);
					
					if (yan == null || yan.StatusCode == ResponseCode.CaptchaRequired
					|| yan.StatusCode == ResponseCode.InternalServerError
					|| yan.StatusCode == ResponseCode.ReachedEndAndNull)
					{
						yan = _searchProvider.YandexSearch.WithProxy().SearchSafeButSlow(groupSimilarImages.TakeAny(takeTopicAmount).ToList(), limit * 25);
						
						if (yan == null || yan.StatusCode == ResponseCode.CaptchaRequired)
							yan = _searchProvider.GoogleSearch.WithProxy().SearchSimilarImagesViaGoogle
								(groupSimilarImages.TakeAny(takeTopicAmount).ToList(), limit * 25);
					}
					if (yan?.Result != null)
					{
						yan.Result.Medias = yan.Result.Medias?.Distinct().ToList();
						switch (yan.StatusCode)
						{
							case ResponseCode.Success:
								{
									await _heartbeatLogic.RefreshMetaData(MetaDataType.FetchMediaForSpecificUserYandex, 
										_customer.Profile.ProfileTopic.Category._id,
										_customer.InstagramAccount.Id);

									var cut = yan.Result.CutObjects(cutBy).ToList();
									cut.ForEach(async x =>
									{
										await _heartbeatLogic.AddMetaData(MetaDataType.FetchMediaForSpecificUserYandex, 
											_customer.Profile.ProfileTopic.Category._id,
											new __Meta__<Media>(x), _customer.InstagramAccount.Id);
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
											_customer.Profile.ProfileTopic.Category._id, _customer.InstagramAccount.Id);
										
										var cut = yan.Result.CutObjects(cutBy).ToList();
										
										cut.ForEach(async x =>
										{
											await _heartbeatLogic.AddMetaData(MetaDataType.FetchMediaForSpecificUserYandex,
												_customer.Profile.ProfileTopic.Category._id,
												new __Meta__<Media>(x), _customer.InstagramAccount.Id);
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
