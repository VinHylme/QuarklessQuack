using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InstagramApiSharp.Classes.Models;
using MoreLinq;
using Quarkless.Base.ContentSearch;
using Quarkless.Models.Common.Enums;
using Quarkless.Models.Common.Extensions;
using Quarkless.Models.Common.Models;
using Quarkless.Models.ContentSearch.Enums;
using Quarkless.Models.ContentSearch.Models;
using Quarkless.Models.ContentSearch.Models.Yandex;
using Quarkless.Models.Heartbeat;
using Quarkless.Models.Heartbeat.Interfaces;
using Quarkless.Models.InstagramAccounts;
using Quarkless.Models.InstagramAccounts.Interfaces;
using Quarkless.Models.InstagramSearch;
using Quarkless.Models.Lookup.Interfaces;
using Quarkless.Models.SearchResponse;
using Quarkless.Models.SearchResponse.Enums;
using Quarkless.Models.Services.Heartbeat;
using Quarkless.Models.Services.Heartbeat.Extensions;
using Quarkless.Models.Topic;
using Quarkless.Models.Topic.Interfaces;
using Quarkless.Models.WorkerManager.Interfaces;

namespace Quarkless.Logic.Services.Heartbeat
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
			ITopicLookupLogic topicLookup, IWorkerManager workerManager,
			ISearchProvider searchProvider, FullUserDetail customer)
		{
			_customer = customer;
			_heartbeatLogic = heartbeatLogic;
			_instagramAccountLogic = accountLogic;
			_topicLookup = topicLookup;
			_searchProvider = searchProvider;
			_workerManager = workerManager;
		}

		#region Filters
		private bool CanInteract(MediaResponse media, string instagramUsername = null)
		{
			if (string.IsNullOrEmpty(instagramUsername))
				return !media.HasLikedBefore || !media.HasSeen;

			return !media.HasLikedBefore || !media.HasSeen || media.User.Username != instagramUsername;
		}
		#endregion

		#region Instagram Stuff
		public async Task BuildBase(int limit = 2, int cutBy = 1, int takeTopicAmount = 1)
		{
			Console.WriteLine("Began - BuildBase");
			var watch = System.Diagnostics.Stopwatch.StartNew();
			try
			{
				if (!_customer.Profile.ProfileTopic.Topics.Any())
					return;

				var topicTotal = new List<CTopic>();

				var profileSubTopic = _customer.Profile.ProfileTopic.Topics.Shuffle().First();

				topicTotal.Add(profileSubTopic);
				topicTotal.AddRange(_topicLookup.GetTopicsByParentId(profileSubTopic._id, true).Result);

				if (!topicTotal.Any())
					return;

				var topicSelect = topicTotal.DistinctBy(x => x.Name).TakeAny(takeTopicAmount).ToList();

				await _workerManager.PerformTaskOnWorkers(async workers =>
				{
					var top = workers.PerformAction(async worker =>
					{
						var instagramSearch = 
							_searchProvider.InstagramSearch(worker.Client, worker.Client.GetContext.Container.Proxy);

						var topMedias = await instagramSearch.SearchMediaDetailInstagram(topicSelect, limit);

						if (topMedias != null)
						{
							var uniqueMedias = topMedias.Medias.Where(_ => CanInteract(_)).ToList();

							await _heartbeatLogic.RefreshMetaData(new MetaDataFetchRequest
							{
								MetaDataType = MetaDataType.FetchMediaByTopic,
								ProfileCategoryTopicId = _customer.Profile.ProfileTopic.Category._id,
								InstagramId = _customer.InstagramAccount.Id,
								AccountId = _customer.InstagramAccount.AccountId
							});
							
							var meta = uniqueMedias.CutObject(cutBy);
							meta.ToList().ForEach(async z =>
							{
								await _heartbeatLogic.AddMetaData(new MetaDataCommitRequest<Media>
								{
									MetaDataType = MetaDataType.FetchMediaByTopic,
									ProfileCategoryTopicId = _customer.Profile.ProfileTopic.Category._id,
									InstagramId = _customer.InstagramAccount.Id,
									AccountId = _customer.InstagramAccount.AccountId,
									Data = new Meta<Media>(new Media{ Medias = z })
								});
							});
						}
					});

					var recent = workers.PerformAction(async worker =>
					{
						var instagramSearch = _searchProvider.InstagramSearch(worker.Client,
							worker.Client.GetContext.Container.Proxy);
						var recentMedias = await instagramSearch.SearchMediaDetailInstagram(topicSelect, limit, true);

						if (recentMedias != null)
						{
							var uniqueMedias = recentMedias.Medias.Where(_ => CanInteract(_)).ToList();

							await _heartbeatLogic.RefreshMetaData(new MetaDataFetchRequest
							{
								MetaDataType = MetaDataType.FetchMediaByTopicRecent,
								ProfileCategoryTopicId = _customer.Profile.ProfileTopic.Category._id,
								InstagramId = _customer.InstagramAccount.Id,
								AccountId = _customer.InstagramAccount.AccountId
							});

							var meta = uniqueMedias.CutObject(cutBy);

							meta.ToList().ForEach(async z =>
							{
								await _heartbeatLogic.AddMetaData(new MetaDataCommitRequest<Media>
								{
									MetaDataType = MetaDataType.FetchMediaByTopicRecent,
									ProfileCategoryTopicId = _customer.Profile.ProfileTopic.Category._id,
									InstagramId = _customer.InstagramAccount.Id,
									AccountId = _customer.InstagramAccount.AccountId,
									Data = new Meta<Media>(new Media{Medias = z})
								});
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
			finally
			{
				watch.Stop();
				Console.WriteLine(
					$"Ended - BuildBase : Took {TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds).TotalSeconds}s");
			}
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
					var profileSubTopic = _customer.Profile.ProfileTopic.Topics.Shuffle().First();

					var randomTopic = (await _topicLookup.GetTopicsByParentId(profileSubTopic._id,true)).Shuffle()
						.FirstOrDefault();

					if (randomTopic == null) return;

					await _workerManager.PerformTaskOnWorkers(async workers =>
					{
						foreach (var userLoc in userTargetList)
						{
							var results = await workers.PerformQueryTask<Media>(async (worker, query, l) =>
							{
								var instagramSearch = _searchProvider.InstagramSearch(worker.Client,
									worker.Client.GetContext.Container.Proxy);

								return await instagramSearch.SearchUsersMediaDetailInstagram(randomTopic, query, l);
							}, userLoc, limit);

							if (results == null) continue;

							var uniqueUsers =
								results.Medias.Where(_ => CanInteract(_)).ToList();

							await _heartbeatLogic.RefreshMetaData(new MetaDataFetchRequest
							{
								MetaDataType = MetaDataType.FetchMediaByUserTargetList,
								ProfileCategoryTopicId = _customer.Profile.ProfileTopic.Category._id,
								InstagramId = _customer.InstagramAccount.Id,
								AccountId = _customer.InstagramAccount.AccountId
							});

							foreach (var media in uniqueUsers.CutObject(cutBy))
							{
								await _heartbeatLogic.AddMetaData(new MetaDataCommitRequest<Media>
								{
									MetaDataType = MetaDataType.FetchMediaByUserTargetList,
									ProfileCategoryTopicId = _customer.Profile.ProfileTopic.Category._id,
									InstagramId = user.Id,
									AccountId = user.AccountId,
									Data = new Meta<Media>(new Media{Medias = media})
								});
							}
						}
					});
				}
			}
			catch (Exception ee)
			{
				Console.WriteLine(ee.Message);
			}
			finally
			{
				watch.Stop();
				Console.WriteLine(
					$"Ended - BuildUsersTargetListMedia : Took {TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds).TotalSeconds}s");
			}
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
							var results = await workers.PerformQueryTask(async (worker, location, lim) =>
							{
								var instagramSearch = _searchProvider.InstagramSearch(worker.Client,
									worker.Client.GetContext.Container.Proxy);
								var topMediasByLocation =
									await instagramSearch.SearchTopLocationMediaDetailInstagram((Location) location,
										lim);

								var recentMediasByLocation =
									await instagramSearch.SearchRecentLocationMediaDetailInstagram((Location) location,
										lim);

								await Task.Delay(TimeSpan.FromSeconds(1));

								if (recentMediasByLocation != null && recentMediasByLocation.Medias.Count > 0)
									topMediasByLocation?.Medias.AddRange(recentMediasByLocation.Medias);

								return topMediasByLocation;
							}, targetLocation, limit);

							if (results == null) continue;

							var uniqueLocationResult = results
								.Medias.Where(_ => CanInteract(_, user.Username)).ToList();

							await _heartbeatLogic.RefreshMetaData(new MetaDataFetchRequest
							{
								MetaDataType = MetaDataType.FetchMediaByUserLocationTargetList,
								ProfileCategoryTopicId = _customer.Profile.ProfileTopic.Category._id,
								InstagramId = _customer.InstagramAccount.Id,
								AccountId = _customer.InstagramAccount.AccountId
							});

							foreach (var media in uniqueLocationResult.CutObject(cutBy))
							{
								await _heartbeatLogic.AddMetaData(new MetaDataCommitRequest<Media>
								{
									MetaDataType = MetaDataType.FetchMediaByUserLocationTargetList,
									ProfileCategoryTopicId = _customer.Profile.ProfileTopic.Category._id,
									InstagramId = user.Id,
									AccountId = user.AccountId,
									Data = new Meta<Media>(new Media{Medias = media })
								});
							}
						}
					});
				}
			}
			catch (Exception ee)
			{
				Console.WriteLine(ee.Message);
			}
			finally
			{
				watch.Stop();
				Console.WriteLine(
					$"Ended - BuildLocationTargetListMedia : Took {TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds).TotalSeconds}s");
			}
		}

		public async Task BuildUsersOwnMedias(int limit = 1, int cutBy = 1)
		{
			Console.WriteLine("Began - BuildUsersOwnMedias");
			var watch = System.Diagnostics.Stopwatch.StartNew();
			try
			{
				var user = _customer.InstagramAccount;

				var profileSubTopic = _customer.Profile.ProfileTopic.Topics.Shuffle().First();
				var randomTopic = (await _topicLookup.GetTopicsByParentId(profileSubTopic._id, true)).Shuffle()
					.FirstOrDefault();

				if (randomTopic == null) return;

				await _workerManager.PerformTaskOnWorkers(async workers =>
				{
					var results = await workers.PerformQueryTask(async (worker, username, lim) =>
					{
						var instagramSearch = _searchProvider.InstagramSearch(worker.Client,
							worker.Client.GetContext.Container.Proxy);
						return await instagramSearch.SearchUsersMediaDetailInstagram(randomTopic, username, lim);
					}, user.Username, limit);

					if (results != null)
					{
						await _heartbeatLogic.RefreshMetaData(new MetaDataFetchRequest
						{
							MetaDataType = MetaDataType.FetchUserOwnProfile,
							ProfileCategoryTopicId = _customer.Profile.ProfileTopic.Category._id,
							InstagramId = _customer.InstagramAccount.Id,
							AccountId = _customer.InstagramAccount.AccountId
						});

						foreach (var media in results.CutObjects(cutBy))
						{
							await _heartbeatLogic.AddMetaData(new MetaDataCommitRequest<Media>
							{
								MetaDataType = MetaDataType.FetchUserOwnProfile,
								ProfileCategoryTopicId = _customer.Profile.ProfileTopic.Category._id,
								InstagramId = user.Id,
								AccountId = user.AccountId,
								Data = new Meta<Media>(media)
							});
						}

						await workers.PerformAction(async worker =>
						{
							var instagramSearch = _searchProvider.InstagramSearch(worker.Client,
								worker.Client.GetContext.Container.Proxy);
							UserResponse userDetail;
							if (!results.Medias.Any())
							{
								var userResult = await worker.Client.User.GetUserAsync(user.Username);
								if (userResult.Succeeded)
								{
									userDetail = new UserResponse
									{
										FullName = userResult.Value.FullName,
										IsPrivate = userResult.Value.IsPrivate,
										IsVerified = userResult.Value.IsVerified,
										ProfilePicture = userResult.Value.ProfilePicture,
										Username = userResult.Value.UserName,
										UserId = userResult.Value.Pk
									};
								}
								else
								{
									return;
								}
							}
							else
							{
								userDetail = results.Medias.First().User;
							}

							var details = await instagramSearch.SearchInstagramFullUserDetail(userDetail.UserId);
							if (details.UserDetail.Pk != 0)
							{
								await _instagramAccountLogic.PartialUpdateInstagramAccount(user.AccountId, user.Id,
									new InstagramAccountModel
									{
										UserId = details.UserDetail.Pk,
										FollowersCount = details.UserDetail.FollowerCount,
										FollowingCount = details.UserDetail.FollowingCount,
										TotalPostsCount = details.UserDetail.MediaCount,
										Email = details.UserDetail.PublicEmail,
										ProfilePicture =
											details.UserDetail.ProfilePicture ?? details.UserDetail.ProfilePicUrl,
										FullName = details.UserDetail.FullName,
										UserBiography = new Biography
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
			finally
			{
				watch.Stop();
				Console.WriteLine(
					$"Ended - BuildUsersOwnMedias : Took {TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds).TotalSeconds}s");
			}
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
							tempWorker.Client.GetContext.Container.Proxy);

						var results = await instagramSearch.SearchUserFeedMediaDetailInstagram(limit: limit);

						if (results != null)
						{
							var uniqueMedias = results.Medias
								.Where(_ => CanInteract(_, user.Username)).ToList();

							await _heartbeatLogic.RefreshMetaData(new MetaDataFetchRequest
							{
								MetaDataType = MetaDataType.FetchUsersFeed,
								ProfileCategoryTopicId = _customer.Profile.ProfileTopic.Category._id,
								InstagramId = user.Id,
								AccountId = user.AccountId
							});

							foreach (var media in uniqueMedias.CutObject(cutBy))
							{
								await _heartbeatLogic.AddMetaData(new MetaDataCommitRequest<Media>
								{
									MetaDataType = MetaDataType.FetchUsersFeed,
									ProfileCategoryTopicId = _customer.Profile.ProfileTopic.Category._id,
									InstagramId = user.Id,
									AccountId = user.AccountId,
									Data = new Meta<Media>(new Media{Medias = media})
								});
							}
						}
					});
				});
			}
			catch (Exception err)
			{
				Console.WriteLine(err);
			}
			finally
			{
				watch.Stop();
				Console.WriteLine(
					$"Ended - BuildUsersFeed : Took {TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds).TotalSeconds}s");
			}
		}

		public async Task BuildUsersStoryFromTopics(int limit = 1, int cutBy = 1)
		{
			Console.WriteLine("Began - BuildUsersStoryFromTopics");
			var watch = System.Diagnostics.Stopwatch.StartNew();
			try
			{
				var user = _customer.InstagramAccount;
				var topicTotal = new List<CTopic>();
				if (!_customer.Profile.ProfileTopic.Topics.Any())
					return;

				var profileSubTopic = _customer.Profile.ProfileTopic.Topics.Shuffle().First();
				topicTotal.AddRange(_topicLookup.GetTopicsByParentId(profileSubTopic._id, true).Result);

				if (!topicTotal.Any())
					return;

				var topicSelect = topicTotal.DistinctBy(x => x.Name).Shuffle().First();

				await _workerManager.PerformTaskOnWorkers(async workers =>
				{
					await workers.PerformAction(async worker =>
					{
						var instagramSearch = _searchProvider.InstagramSearch(worker.Client,
							worker.Client.GetContext.Container.Proxy);

						var results = await instagramSearch.GetUserStoriesByTopic(topicSelect , limit);
						if (results == null)
							return;

						var uniqueResults = results
							.Where(_ => !_.IsPrivate).ToList();

						await _heartbeatLogic.RefreshMetaData(new MetaDataFetchRequest
						{
							MetaDataType = MetaDataType.FetchUsersStoryViaTopics,
							ProfileCategoryTopicId = _customer.Profile.ProfileTopic.Category._id,
							InstagramId = user.Id,
							AccountId = user.AccountId
						});

						foreach (var userStoryObject in uniqueResults.CutObject(cutBy))
						{
							await _heartbeatLogic.AddMetaData(new MetaDataCommitRequest<List<UserResponse<InstaStory>>>
							{
								MetaDataType = MetaDataType.FetchUsersStoryViaTopics,
								ProfileCategoryTopicId = _customer.Profile.ProfileTopic.Category._id,
								InstagramId = user.Id,
								AccountId = user.AccountId,
								Data = new Meta<List<UserResponse<InstaStory>>>(userStoryObject)
							});
						}
					});
				});
			}
			catch (Exception err)
			{
				Console.WriteLine(err);
			}
			finally
			{
				watch.Stop();
				Console.WriteLine(
					$"Ended - BuildUsersStoryFromTopics : Took {TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds).TotalSeconds}s");
			}
		}

		public async Task BuildUsersStoryFeed(int limit = 1, int cutBy = 1)
		{
			Console.WriteLine("Began - BuildUsersStoryFeed");
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
							tempWorker.Client.GetContext.Container.Proxy);

						var results = await instagramSearch.GetUserFeedStories(limit);
						if (results == null)
							return;

						var uniqueResults = results
							.Where(_ => !_.IsPrivate && _.Object.Seen <= 0).ToList();

						await _heartbeatLogic.RefreshMetaData(new MetaDataFetchRequest
						{
							MetaDataType = MetaDataType.FetchUsersStoryFeed,
							ProfileCategoryTopicId = _customer.Profile.ProfileTopic.Category._id,
							InstagramId = user.Id,
							AccountId = user.AccountId
						});

						foreach (var userStoryObject in uniqueResults.ToList().CutObject(cutBy))
						{
							await _heartbeatLogic.AddMetaData(new MetaDataCommitRequest<List<UserResponse<InstaReelFeed>>>
							{
								MetaDataType = MetaDataType.FetchUsersStoryFeed,
								ProfileCategoryTopicId = _customer.Profile.ProfileTopic.Category._id,
								InstagramId = user.Id,
								AccountId = user.AccountId,
								Data = new Meta<List<UserResponse<InstaReelFeed>>>(userStoryObject)
							});
						}
					});
				});
			}
			catch (Exception err)
			{
				Console.WriteLine(err);
			}
			finally
			{
				watch.Stop();
				Console.WriteLine(
					$"Ended - BuildUsersStoryFeed : Took {TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds).TotalSeconds}s");
			}
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
							worker.Client.GetContext.Container.Proxy);

						var fetchUsersMedia = await instagramSearch.GetUserFollowingList(user.Username, limit);

						if (fetchUsersMedia != null)
						{
							var uniqueUsers = fetchUsersMedia.ToList();

							await _heartbeatLogic.RefreshMetaData(new MetaDataFetchRequest
							{
								MetaDataType = MetaDataType.FetchUsersFollowingList,
								ProfileCategoryTopicId = _customer.Profile.ProfileTopic.Category._id,
								InstagramId = _customer.InstagramAccount.Id,
								AccountId = _customer.InstagramAccount.AccountId
							});

							foreach (var fetchUsers in uniqueUsers.CutObject(cutBy))
							{
								await _heartbeatLogic.AddMetaData(new MetaDataCommitRequest<List<UserResponse<string>>>
								{
									MetaDataType = MetaDataType.FetchUsersFollowingList,
									ProfileCategoryTopicId = _customer.Profile.ProfileTopic.Category._id,
									InstagramId = user.Id,
									AccountId = user.AccountId,
									Data = new Meta<List<UserResponse<string>>>(fetchUsers)
								});
							}
						}
					});
				});

			}
			catch (Exception ee)
			{
				Console.WriteLine(ee.Message);
			}
			finally
			{
				watch.Stop();
				Console.WriteLine(
					$"Ended - BuildUserUnfollowList : Took {TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds).TotalSeconds}s");
			}
		}

		public async Task BuildUserFollowerList(int limit = 3, int cutBy = 1)
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
							worker.Client.GetContext.Container.Proxy);

						var fetchUsersMedia = await instagramSearch.GetUsersFollowersList(user.Username, limit);

						if (fetchUsersMedia != null)
						{
							var uniqueUsers = fetchUsersMedia.ToList();

							await _heartbeatLogic.RefreshMetaData(new MetaDataFetchRequest
							{
								MetaDataType = MetaDataType.FetchUsersFollowerList,
								ProfileCategoryTopicId = _customer.Profile.ProfileTopic.Category._id,
								InstagramId = _customer.InstagramAccount.Id,
								AccountId = _customer.InstagramAccount.AccountId
							});

							foreach (var userObject in uniqueUsers.CutObject(cutBy))
							{
								await _heartbeatLogic.AddMetaData(new MetaDataCommitRequest<List<UserResponse<string>>>
								{
									MetaDataType = MetaDataType.FetchUsersFollowerList,
									ProfileCategoryTopicId = _customer.Profile.ProfileTopic.Category._id,
									InstagramId = user.Id,
									AccountId = user.AccountId,
									Data = new Meta<List<UserResponse<string>>>(userObject)
								});
							}
						}
					});
				});
			}
			catch (Exception ee)
			{
				Console.WriteLine(ee.Message);
			}
			finally
			{
				watch.Stop();
				Console.WriteLine(
					$"Ended - BuildUserFollowerList : Took {TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds).TotalSeconds}s");
			}
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
							tempWorker.Client.GetContext.Container.Proxy);

						var fetchUsersMedia = await instagramSearch.GetSuggestedPeopleToFollow(limit);

						if (fetchUsersMedia != null)
						{
							var uniqueUsers = fetchUsersMedia.ToList();

							await _heartbeatLogic.RefreshMetaData(new MetaDataFetchRequest
							{
								MetaDataType = MetaDataType.FetchUsersFollowSuggestions,
								ProfileCategoryTopicId = _customer.Profile.ProfileTopic.Category._id,
								InstagramId = _customer.InstagramAccount.Id,
								AccountId = _customer.InstagramAccount.AccountId
							});

							foreach (var fetchedUsers in uniqueUsers.CutObject(cutObjectBy))
							{
								await _heartbeatLogic.AddMetaData(
									new MetaDataCommitRequest<List<UserResponse<UserSuggestionDetails>>>
									{
										MetaDataType = MetaDataType.FetchUsersFollowSuggestions,
										ProfileCategoryTopicId = _customer.Profile.ProfileTopic.Category._id,
										InstagramId = user.Id,
										AccountId = user.AccountId,
										Data = new Meta<List<UserResponse<UserSuggestionDetails>>>(fetchedUsers)
									});
							}
						}
					});
				});
			}
			catch (Exception ee)
			{
				Console.WriteLine(ee.Message);
			}
			finally
			{
				watch.Stop();
				Console.WriteLine(
					$"Ended - BuildUsersFollowSuggestions : Took {TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds).TotalSeconds}s");
			}
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
							tempWorker.Client.GetContext.Container.Proxy);

						var fetchUserInbox = await instagramSearch.SearchUserInbox(limit);
						if (fetchUserInbox != null)
						{
							await _heartbeatLogic.RefreshMetaData(new MetaDataFetchRequest
							{
								MetaDataType = MetaDataType.FetchUserDirectInbox,
								ProfileCategoryTopicId = _customer.Profile.ProfileTopic.Category._id,
								InstagramId = _customer.InstagramAccount.Id,
								AccountId = _customer.InstagramAccount.AccountId
							});

							await _heartbeatLogic.AddMetaData(new MetaDataCommitRequest<InstaDirectInboxContainer>
							{
								MetaDataType = MetaDataType.FetchUserDirectInbox,
								ProfileCategoryTopicId = _customer.Profile.ProfileTopic.Category._id,
								InstagramId = user.Id,
								AccountId = user.AccountId,
								Data = new Meta<InstaDirectInboxContainer>(fetchUserInbox)
							});
						}
					});
				});
			}
			catch (Exception ee)
			{
				Console.WriteLine(ee.Message);
			}
			finally
			{
				watch.Stop();
				Console.WriteLine(
					$"Ended - BuildUsersInbox : Took {TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds).TotalSeconds}s");
			}
		}

		public async Task BuildUsersRecentComments(int howManyMedias = 10, int limit = 1)
		{
			Console.WriteLine("Began - BuildUsersRecentComments");
			var watch = System.Diagnostics.Stopwatch.StartNew();

			try
			{
				var user = _customer.InstagramAccount;

				var request = new MetaDataFetchRequest
				{
					MetaDataType = MetaDataType.UsersRecentComments,
					ProfileCategoryTopicId = _customer.Profile.ProfileTopic.Category._id,
					InstagramId = _customer.InstagramAccount.Id, AccountId = _customer.InstagramAccount.AccountId
				};
				await _heartbeatLogic.RefreshMetaData(request);

				var mediasForUser = (await _heartbeatLogic.GetMetaData<Media>(new MetaDataFetchRequest
					{
						MetaDataType = MetaDataType.FetchUserOwnProfile,
						ProfileCategoryTopicId = _customer.Profile.ProfileTopic.Category._id,
						InstagramId = user.Id,
						AccountId = user.AccountId
					}))
					.Select(x => x.ObjectItem.Medias)
					.SquashMe()
					.OrderByDescending(x => x.TakenAt)
					.Take(howManyMedias);

				await _workerManager.PerformTaskOnWorkers(async workers =>
				{
					await workers.PerformAction(async worker =>
					{
						var instagramSearch = _searchProvider.InstagramSearch(worker.Client,
							worker.Client.GetContext.Container.Proxy);

						foreach (var media in mediasForUser)
						{
							var comments =
								await instagramSearch.SearchInstagramMediaCommenters(media.Topic, media.MediaId, limit);

							await _heartbeatLogic.AddMetaData(
								new MetaDataCommitRequest<List<UserResponse<InstaComment>>>
								{
									MetaDataType = MetaDataType.UsersRecentComments,
									ProfileCategoryTopicId = _customer.Profile.ProfileTopic.Category._id,
									InstagramId = user.Id,
									AccountId = user.AccountId,
									Data = new Meta<List<UserResponse<InstaComment>>>(comments)
								});
						}
					});
				});
			}
			catch (Exception ee)
			{
				Console.WriteLine(ee.Message);
			}
			finally
			{
				watch.Stop();
				Console.WriteLine(
					$"Ended - BuildUsersRecentComments : Took {TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds).TotalSeconds}s");
			}
		}

		public async Task BuildUserFromLikers(int cutBy = 1, double sleepTime = 0.05,
			int takeMediaAmount = 7, int takeUserMediaAmount = 100)
		{
			Console.WriteLine("Began - BuildUserFromLikers");
			var watch = System.Diagnostics.Stopwatch.StartNew();
			try
			{
				var results = (await _heartbeatLogic.GetMetaData<Media>(new MetaDataFetchRequest
				{
					MetaDataType = MetaDataType.FetchMediaByTopic,
					ProfileCategoryTopicId = _customer.Profile.ProfileTopic.Category._id,
					InstagramId = _customer.InstagramAccount.Id,
					AccountId = _customer.InstagramAccount.AccountId
				})).ToList();

				var recentResults = await _heartbeatLogic.GetMetaData<Media>(new MetaDataFetchRequest
				{
					MetaDataType = MetaDataType.FetchMediaByTopicRecent,
					ProfileCategoryTopicId = _customer.Profile.ProfileTopic.Category._id,
					InstagramId = _customer.InstagramAccount.Id,
					AccountId = _customer.InstagramAccount.AccountId
				});

				var metaS = recentResults as Meta<Media>[] ?? recentResults.ToArray();
				if (metaS.Any())
					results.AddRange(metaS);

				await _heartbeatLogic.RefreshMetaData(new MetaDataFetchRequest
				{
					MetaDataType = MetaDataType.FetchUsersViaPostLiked,
					ProfileCategoryTopicId = _customer.Profile.ProfileTopic.Category._id,
					InstagramId = _customer.InstagramAccount.Id,
					AccountId = _customer.InstagramAccount.AccountId
				});

				await _workerManager.PerformTaskOnWorkers(async workers =>
				{
					await workers.PerformAction(async worker =>
					{
						var instagramSearch = _searchProvider.InstagramSearch(worker.Client,
							worker.Client.GetContext.Container.Proxy);
						
						foreach (var medias in results.TakeAny(takeMediaAmount))
						{
							if (medias == null) continue;

							foreach (var media in medias.ObjectItem.Medias.Where(
								t => t.MediaFrom == MediaFrom.Instagram))
							{
								var suggested =
									await instagramSearch.SearchInstagramMediaLikers(media.Topic, media.MediaId);

								if (suggested == null) return;

								if (suggested.Count < 1) return;

								var uniqueUsers = suggested
									.Where(_ => _.Username != _customer.InstagramAccount.Username)
									.TakeAny(takeUserMediaAmount).ToList();

								var separateSuggested = uniqueUsers.CutObject(cutBy);

								separateSuggested.ForEach(async userObject =>
								{
									await _heartbeatLogic.AddMetaData(
										new MetaDataCommitRequest<List<UserResponse<string>>>
										{
											MetaDataType = MetaDataType.FetchUsersViaPostLiked,
											ProfileCategoryTopicId = _customer.Profile.ProfileTopic.Category._id,
											InstagramId = _customer.InstagramAccount.Id,
											AccountId = _customer.InstagramAccount.AccountId,
											Data = new Meta<List<UserResponse<string>>>(userObject)
										});
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
			finally
			{
				watch.Stop();
				Console.WriteLine(
					$"Ended - BuildUserFromLikers : Took {TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds).TotalSeconds}s");
			}
		}

		public async Task BuildMediaFromUsersLikers(int limit = 1, int cutBy = 1, double sleepTime = 0.05,
			int takeMediaAmount = 7, int takeUserMediaAmount = 10)
		{
			Console.WriteLine("Began - BuildMediaFromUsersLikers");
			var watch = System.Diagnostics.Stopwatch.StartNew();
			try
			{
				var results = (await _heartbeatLogic.GetMetaData<List<UserResponse<string>>>
				(new MetaDataFetchRequest
				{
					MetaDataType = MetaDataType.FetchUsersViaPostLiked,
					ProfileCategoryTopicId = _customer.Profile.ProfileTopic.Category._id,
					InstagramId = _customer.InstagramAccount.Id,
					AccountId = _customer.InstagramAccount.AccountId
				})).ToList();

				if (!results.Any())
					return;

				var profileSubTopic = _customer.Profile.ProfileTopic.Topics.Shuffle().First();
				var randomTopic = (await _topicLookup.GetTopicsByParentId(profileSubTopic._id, true)).Shuffle()
					.FirstOrDefault();

				if (randomTopic == null) return;

				await _heartbeatLogic.RefreshMetaData(new MetaDataFetchRequest
				{
					MetaDataType = MetaDataType.FetchMediaByLikers,
					ProfileCategoryTopicId = _customer.Profile.ProfileTopic.Category._id,
					InstagramId = _customer.InstagramAccount.Id,
					AccountId = _customer.InstagramAccount.AccountId
				});

				await _workerManager.PerformTaskOnWorkers(async workers =>
				{
					await workers.PerformAction(async worker =>
					{
						var instagramSearch = _searchProvider.InstagramSearch(worker.Client,
							worker.Client.GetContext.Container.Proxy);

						foreach (var media in results.TakeAny(takeMediaAmount))
						{
							if (media == null) continue;
							foreach (var user in media.ObjectItem)
							{
								if(user.IsPrivate)
									continue;

								var suggested = await instagramSearch.SearchUsersMediaDetailInstagram(
									randomTopic, user.Username, limit);

								if (suggested != null)
								{
									var uniqueUsers = suggested.Medias
										.Where(_ => CanInteract(_, _customer.InstagramAccount.Username))
										.TakeAny(takeUserMediaAmount).ToList();

									foreach (var mediaObject in uniqueUsers.CutObject(cutBy))
									{
										await _heartbeatLogic.AddMetaData(new MetaDataCommitRequest<Media>
										{
											MetaDataType = MetaDataType.FetchMediaByLikers,
											ProfileCategoryTopicId = _customer.Profile.ProfileTopic.Category._id,
											InstagramId = _customer.InstagramAccount.Id,
											AccountId = _customer.InstagramAccount.AccountId,
											Data = new Meta<Media>(new Media{Medias = mediaObject})
										});
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
			finally
			{
				watch.Stop();
				Console.WriteLine(
					$"Ended - BuildMediaFromUsersLikers : Took {TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds).TotalSeconds}s");
			}
		}

		public async Task BuildUsersFromCommenters(int limit = 1, int cutBy = 1, double sleepTime = 0.05,
			int takeMediaAmount = 7, int takeUserMediaAmount = 100)
		{
			Console.WriteLine("Began - BuildUsersFromCommenters");
			var watch = System.Diagnostics.Stopwatch.StartNew();
			try
			{
				var results = (await _heartbeatLogic.GetMetaData<Media>(new MetaDataFetchRequest
				{
					MetaDataType = MetaDataType.FetchMediaByTopic,
					ProfileCategoryTopicId = _customer.Profile.ProfileTopic.Category._id,
					InstagramId = _customer.InstagramAccount.Id,
					AccountId = _customer.InstagramAccount.AccountId
				})).ToList();

				var recentResults = await _heartbeatLogic.GetMetaData<Media>(new MetaDataFetchRequest
				{
					MetaDataType = MetaDataType.FetchMediaByTopicRecent,
					ProfileCategoryTopicId = _customer.Profile.ProfileTopic.Category._id,
					InstagramId = _customer.InstagramAccount.Id,
					AccountId = _customer.InstagramAccount.AccountId
				});

				var metaS = recentResults as Meta<Media>[] ?? recentResults.ToArray();

				if (metaS.Any())
					results.AddRange(metaS);

				await _heartbeatLogic.RefreshMetaData(new MetaDataFetchRequest
				{
					MetaDataType = MetaDataType.FetchUsersViaPostCommented,
					ProfileCategoryTopicId = _customer.Profile.ProfileTopic.Category._id,
					InstagramId = _customer.InstagramAccount.Id,
					AccountId = _customer.InstagramAccount.AccountId
				});

				await _workerManager.PerformTaskOnWorkers(async workers =>
				{
					await workers.PerformAction(async worker =>
					{
						var instagramSearch = _searchProvider.InstagramSearch(worker.Client,
							worker.Client.GetContext.Container.Proxy);

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
										.SearchInstagramMediaCommenters(media.Topic, media.MediaId, limit);

									if (suggested == null) return;
									if (suggested.Count < 1) return;

									var uniqueUsers = suggested
										.Where(_ => _.Username != _customer.InstagramAccount.Username)
										.TakeAny(takeUserMediaAmount)
										.ToList()
										.CutObject(cutBy);

									foreach (var filtered in uniqueUsers)
									{
										await _heartbeatLogic.AddMetaData(
											new MetaDataCommitRequest<List<UserResponse<InstaComment>>>
											{
												MetaDataType = MetaDataType.FetchUsersViaPostCommented,
												ProfileCategoryTopicId = _customer.Profile.ProfileTopic.Category._id,
												InstagramId = _customer.InstagramAccount.Id,
												AccountId = _customer.InstagramAccount.AccountId,
												Data = new Meta<List<UserResponse<InstaComment>>>(filtered)
											});
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
			finally
			{
				watch.Stop();
				Console.WriteLine(
					$"Ended - BuildUsersFromCommenters : Took {TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds).TotalSeconds}s");
			}
		}

		public async Task BuildCommentsFromSpecifiedSource(MetaDataType extractFrom, MetaDataType saveTo, int limit = 1,
			int cutBy = 1, double secondSleep = 0.05,
			int takeMediaAmount = 10, int takeUserAmount = 100)
		{
			Console.WriteLine("Began - BuildCommentsFromSpecifiedSource " + saveTo.ToString());
			var watch = System.Diagnostics.Stopwatch.StartNew();
			try
			{
				var res = await _heartbeatLogic.GetMetaData<Media>(new MetaDataFetchRequest
				{
					MetaDataType = extractFrom,
					ProfileCategoryTopicId = _customer.Profile.ProfileTopic.Category._id,
					InstagramId = _customer.InstagramAccount.Id,
					AccountId = _customer.InstagramAccount.AccountId
				});

				if (res != null && res.Any())
				{
					await _heartbeatLogic.RefreshMetaData(new MetaDataFetchRequest
					{
						MetaDataType = saveTo,
						ProfileCategoryTopicId = _customer.Profile.ProfileTopic.Category._id,
						InstagramId = _customer.InstagramAccount.Id,
						AccountId = _customer.InstagramAccount.AccountId
					});

					await _workerManager.PerformTaskOnWorkers(async workers =>
					{
						await workers.PerformAction(async worker =>
						{
							var instagramSearch = _searchProvider.InstagramSearch(worker.Client,
								worker.Client.GetContext.Container.Proxy);

							foreach (var medias in res.TakeAny(takeMediaAmount))
							{
								if (medias == null) continue;
								foreach (var media in medias.ObjectItem.Medias)
								{
									if (!media.IsCommentsDisabled 
										&& int.TryParse(media.CommentCount, out var count) 
										&& count > 0 && media.MediaFrom == MediaFrom.Instagram
										&& media.MediaId != null && !media.HasSeen)
									{
										var suggested = await instagramSearch
											.SearchInstagramMediaCommenters(media.Topic, media.MediaId, limit);

										if (suggested != null)
										{
											var uniqueUsers = suggested
												.Where(_ => _.Username != _customer.InstagramAccount.Username)
												.TakeAny(takeUserAmount)
												.ToList()
												.CutObject(cutBy);

											foreach (var filtered in uniqueUsers)
											{
												await _heartbeatLogic.AddMetaData(
													new MetaDataCommitRequest<List<UserResponse<InstaComment>>>
													{
														MetaDataType = saveTo,
														ProfileCategoryTopicId =
															_customer.Profile.ProfileTopic.Category._id,
														InstagramId = _customer.InstagramAccount.Id,
														AccountId = _customer.InstagramAccount.AccountId,
														Data = new Meta<List<UserResponse<InstaComment>>>(filtered),
													});
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
			finally
			{
				watch.Stop();
				Console.WriteLine(
					$"Ended - BuildCommentsFromSpecifiedSource  {saveTo.ToString()} : Took {TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds).TotalSeconds}s");
			}
		}

		public async Task BuildMediaFromUsersCommenters(int limit = 1, int cutBy = 1, double secondsSleep = 0.05,
			int takeMediaAmount = 10, int takeUserMediaAmount = 10)
		{
			Console.WriteLine("Began - BuildMediaFromUsersCommenters");
			var watch = System.Diagnostics.Stopwatch.StartNew();
			try
			{
				var results = (await _heartbeatLogic.GetMetaData<List<UserResponse<InstaComment>>>
				(new MetaDataFetchRequest
				{
					MetaDataType = MetaDataType.FetchUsersViaPostCommented,
					ProfileCategoryTopicId = _customer.Profile.ProfileTopic.Category._id,
					InstagramId = _customer.InstagramAccount.Id,
					AccountId = _customer.InstagramAccount.AccountId
				})).ToList();

				if (!results.Any())
					return;

				await _heartbeatLogic.RefreshMetaData(new MetaDataFetchRequest
				{
					MetaDataType = MetaDataType.FetchMediaByCommenters,
					ProfileCategoryTopicId = _customer.Profile.ProfileTopic.Category._id,
					InstagramId = _customer.InstagramAccount.Id,
					AccountId = _customer.InstagramAccount.AccountId
				});

				var profileSubTopic = _customer.Profile.ProfileTopic.Topics.Shuffle().First();
				var randomTopic = (await _topicLookup.GetTopicsByParentId(profileSubTopic._id, true)).Shuffle()
					.FirstOrDefault();

				if (randomTopic == null) return;

				await _workerManager.PerformTaskOnWorkers(async workers =>
				{
					await workers.PerformAction(async worker =>
					{
						var instagramSearch = _searchProvider.InstagramSearch(worker.Client,
							worker.Client.GetContext.Container.Proxy);
						
						foreach (var medias in results.TakeAny(takeMediaAmount))
						{
							if (medias == null) continue;
							foreach (var commenter in medias.ObjectItem)
							{
								if (commenter.IsPrivate)
									continue;

								var suggested =
									await instagramSearch.SearchUsersMediaDetailInstagram(randomTopic,
										commenter.Username, limit);

								if (suggested != null)
								{
									var uniqueUsers = suggested.Medias
										.Where(_ => CanInteract(_))
										.TakeAny(takeUserMediaAmount)
										.ToList()
										.CutObject(cutBy);
									
									foreach (var commentMedias in uniqueUsers)
									{
										await _heartbeatLogic.AddMetaData(new MetaDataCommitRequest<Media>
										{
											MetaDataType = MetaDataType.FetchMediaByCommenters,
											ProfileCategoryTopicId = _customer.Profile.ProfileTopic.Category._id,
											InstagramId = _customer.InstagramAccount.Id,
											AccountId = _customer.InstagramAccount.AccountId,
											Data = new Meta<Media>(new Media{Medias = commentMedias})
										});
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
			finally
			{
				watch.Stop();
				Console.WriteLine(
					$"Ended - BuildMediaFromUsersCommenters : Took {TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds).TotalSeconds}s");
			}
		}
		#endregion

		#region External Search Stuff
		private SearchImageModel GoogleQueryBuilder(string color, string topics, IEnumerable<string> sites, int limit = 10,
			string imageType = null, string exactSize = null, string similarImage = null)
		{
			var query = new SearchImageModel
			{
				Color = color,
				PrefixKeywords = string.Join(", ", sites),
				Keywords = topics,
				Limit = limit,
				NoDownload = true,
				PrintUrls = true,
				Type = imageType == "any" ? null : imageType,
				ExactSize = exactSize,
				Proxy = null,
				SimilarImages = similarImage,
				Size = string.IsNullOrEmpty(exactSize) ? "large" : null
			};
			return query;
		}

		public async Task BuildGoogleImages(int limit = 50, int topicAmount = 1, int cutBy = 1)
		{
			Console.WriteLine("Began - BuildGoogleImages");
			var watch = System.Diagnostics.Stopwatch.StartNew();
			try
			{
				if (!_customer.Profile.ProfileTopic.Topics.Any())
					return;

				var selectProfileSubTopic = _customer.Profile.ProfileTopic.Topics.Shuffle().First();
				var relatedTopic = await _topicLookup.GetTopicsByParentId(selectProfileSubTopic._id, true);
				var pickedTopic = relatedTopic.Shuffle().FirstOrDefault();
				var searchQueryTopics = pickedTopic;

				var colorSelect = _customer.Profile.Theme.Colors.TakeAny(1).FirstOrDefault();
				var imageType = ((ImageType)_customer.Profile.AdditionalConfigurations.ImageType).GetDescription();

				if (colorSelect != null)
				{
					var go = _searchProvider.GoogleSearch.WithProxy()
						.SearchViaGoogle(pickedTopic, GoogleQueryBuilder(colorSelect.Name,
						searchQueryTopics?.Name, _customer.Profile.AdditionalConfigurations.Sites,
						limit, exactSize: _customer.Profile.AdditionalConfigurations.PostSize, imageType: imageType));

					if (go != null)
					{
						if (go.StatusCode == ResponseCode.Success)
						{
							await _heartbeatLogic.RefreshMetaData(new MetaDataFetchRequest
							{
								MetaDataType = MetaDataType.FetchMediaForSpecificUserGoogle,
								ProfileCategoryTopicId = _customer.Profile.ProfileTopic.Category._id,
								InstagramId = _customer.InstagramAccount.Id,
								AccountId = _customer.InstagramAccount.AccountId
							});

							var cut = go.Result.Medias.ToList().CutObject(cutBy);

							cut.ForEach(async mediaItems =>
							{
								await _heartbeatLogic.AddMetaData(new MetaDataCommitRequest<Media>
								{
									MetaDataType = MetaDataType.FetchMediaForSpecificUserGoogle,
									ProfileCategoryTopicId = _customer.Profile.ProfileTopic.Category._id,
									InstagramId = _customer.InstagramAccount.Id,
									AccountId = _customer.InstagramAccount.AccountId,
									Data = new Meta<Media>(new Media{Medias = mediaItems})
								});
							});
						}
					}
				}
			}
			catch (Exception ee)
			{
				Console.WriteLine(ee.Message);
			}
			finally
			{
				watch.Stop();
				Console.WriteLine(
					$"Ended - BuildGoogleImages : Took {TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds).TotalSeconds}s");
			}
		}

		public async Task BuildYandexImagesQuery(int limit = 2, int topicAmount = 1, int cutBy = 1)
		{
			Console.WriteLine("Began - BuildYandexImagesQuery");
			var watch = System.Diagnostics.Stopwatch.StartNew();
			try
			{
				// should have proxy for scrape
				if (!_customer.Profile.ProfileTopic.Topics.Any())
						return;

				var selectProfileSubTopic = _customer.Profile.ProfileTopic.Topics.Shuffle().First();
				var relatedTopic = (await _topicLookup.GetTopicsByParentId(selectProfileSubTopic._id, true));
				var searchQuery = relatedTopic.Shuffle().FirstOrDefault();

				var prf = _customer.Profile;
				var colorSelect = prf.Theme.Colors.TakeAny(1).FirstOrDefault();

				if (searchQuery != null && colorSelect != null)
				{
					var yandexSearchQuery = new YandexSearchQuery
					{
						OriginalTopic = _customer.Profile.ProfileTopic.Category,
						SearchQuery = searchQuery.Name,
						Color = colorSelect.Name.GetValueFromDescription<ColorType>(),
						Format = FormatType.Any,
						Orientation = Orientation.Any,
						Size = SizeType.Large,
						Type = (ImageType) prf.AdditionalConfigurations.ImageType == ImageType.Any
							? Enum.GetValues(typeof(ImageType))
								.Cast<ImageType>().Where(x => x != ImageType.Any).TakeAny(1).FirstOrDefault()
							: (ImageType) prf.AdditionalConfigurations.ImageType
					};

					var yan = _searchProvider.YandexSearch.WithProxy().SearchQueryRest(yandexSearchQuery, limit);
					if (yan != null)
					{
						switch (yan.StatusCode)
						{
							case ResponseCode.Success:
							{
								await _heartbeatLogic.RefreshMetaData(new MetaDataFetchRequest
								{
									MetaDataType = MetaDataType.FetchMediaForSpecificUserYandexQuery,
									ProfileCategoryTopicId = _customer.Profile.ProfileTopic.Category._id,
									InstagramId = _customer.InstagramAccount.Id,
									AccountId = _customer.InstagramAccount.AccountId
								});

								var cut = yan.Result.Medias.ToList().CutObject(cutBy);

								cut.ForEach(async mediaItems =>
								{
									await _heartbeatLogic.AddMetaData(new MetaDataCommitRequest<Media>
									{
										MetaDataType = MetaDataType.FetchMediaForSpecificUserYandexQuery,
										ProfileCategoryTopicId = _customer.Profile.ProfileTopic.Category._id,
										InstagramId = _customer.InstagramAccount.Id,
										AccountId = _customer.InstagramAccount.AccountId,
										Data = new Meta<Media>(new Media{Medias = mediaItems})
									});
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
			catch (Exception ee)
			{
				Console.WriteLine(ee.Message);
			}
			finally
			{
				watch.Stop();
				Console.WriteLine(
					$"Ended - BuildYandexImagesQuery : Took {TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds).TotalSeconds}s");
			}
		}
		public async Task BuildYandexImages(int limit = 3, int takeTopicAmount = 1, int cutBy = 1)
		{
			Console.WriteLine("Began - BuildYandexImages");
			var watch = System.Diagnostics.Stopwatch.StartNew();
			try
			{
				var imagesLike = _customer.Profile.Theme.ImagesLike;
				if (imagesLike == null) return;

				var filter = imagesLike.Where(s =>
					s.TopicGroup.Name.Equals(_customer.Profile.ProfileTopic.Category.Name));

				var groupSimilarImages = filter as GroupImagesAlike[] ?? filter.ToArray();
				var yan = _searchProvider.YandexSearch.WithProxy()
					.SearchRelatedImagesRest(groupSimilarImages.TakeAny(takeTopicAmount).ToList(), limit);

				if (yan == null || yan.StatusCode == ResponseCode.CaptchaRequired
								|| yan.StatusCode == ResponseCode.InternalServerError
								|| yan.StatusCode == ResponseCode.ReachedEndAndNull)
				{
					yan = _searchProvider.YandexSearch.WithProxy()
						.SearchSafeButSlow(groupSimilarImages.TakeAny(takeTopicAmount).ToList(), limit * 25);

					if (yan == null || yan.StatusCode == ResponseCode.CaptchaRequired)
						yan = _searchProvider.GoogleSearch.WithProxy().SearchSimilarImagesViaGoogle
							(groupSimilarImages.TakeAny(takeTopicAmount).ToList(), limit * 25);
				}

				if (yan?.Result != null)
				{
					switch (yan.StatusCode)
					{
						case ResponseCode.Success:
						{
							await _heartbeatLogic.RefreshMetaData(new MetaDataFetchRequest
							{
								MetaDataType = MetaDataType.FetchMediaForSpecificUserYandex,
								ProfileCategoryTopicId = _customer.Profile.ProfileTopic.Category._id,
								InstagramId = _customer.InstagramAccount.Id,
								AccountId = _customer.InstagramAccount.AccountId
							});

							var cut = yan.Result.Medias
								.DistinctBy(_=>_.MediaUrl)
								.ToList()
								.CutObject(cutBy);

								cut.ForEach(async mediaItems =>
							{
								await _heartbeatLogic.AddMetaData(new MetaDataCommitRequest<Media>
								{
									MetaDataType = MetaDataType.FetchMediaForSpecificUserYandex,
									ProfileCategoryTopicId = _customer.Profile.ProfileTopic.Category._id,
									InstagramId = _customer.InstagramAccount.Id,
									AccountId = _customer.InstagramAccount.AccountId,
									Data = new Meta<Media>(new Media{Medias = mediaItems})
								});
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
								await _heartbeatLogic.RefreshMetaData(new MetaDataFetchRequest
								{
									MetaDataType = MetaDataType.FetchMediaForSpecificUserYandex,
									ProfileCategoryTopicId = _customer.Profile.ProfileTopic.Category._id,
									InstagramId = _customer.InstagramAccount.Id,
									AccountId = _customer.InstagramAccount.AccountId
								});

								var cut = yan.Result.Medias
									.DistinctBy(_ => _.MediaUrl)
									.ToList()
									.CutObject(cutBy);

									cut.ForEach(async mediaItems =>
								{
									await _heartbeatLogic.AddMetaData(new MetaDataCommitRequest<Media>
									{
										MetaDataType = MetaDataType.FetchMediaForSpecificUserYandex,
										ProfileCategoryTopicId = _customer.Profile.ProfileTopic.Category._id,
										InstagramId = _customer.InstagramAccount.Id,
										AccountId = _customer.InstagramAccount.AccountId,
										Data = new Meta<Media>(new Media{Medias = mediaItems})
									});
								});
							}

							break;
						}
					}
				}
			}
			catch (Exception ee)
			{
				Console.WriteLine(ee.Message);
			}
			finally
			{
				watch.Stop();
				Console.WriteLine(
					$"Ended - BuildYandexImages : Took {TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds).TotalSeconds}s");
			}
		}
		#endregion
	}
}
