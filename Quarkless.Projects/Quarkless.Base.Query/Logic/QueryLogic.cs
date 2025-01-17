﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using InstagramApiSharp.Classes.Models;
using MoreLinq.Extensions;
using Quarkless.Base.ContentSearch.Models.Enums;
using Quarkless.Base.ContentSearch.Models.Interfaces;
using Quarkless.Base.ContentSearch.Models.Models;
using Quarkless.Base.Hashtag.Models.Interfaces;
using Quarkless.Base.HashtagGenerator.Models;
using Quarkless.Base.HashtagGenerator.Models.Interfaces;
using Quarkless.Base.Heartbeat.Models;
using Quarkless.Base.Heartbeat.Models.Interfaces;
using Quarkless.Base.InstagramClient.Logic;
using Quarkless.Base.InstagramClient.Models.Interfaces;
using Quarkless.Base.Lookup.Models.Interfaces;
using Quarkless.Base.Profile.Models.Enums;
using Quarkless.Base.Query.Models;
using Quarkless.Base.Query.Models.Interfaces;
using Quarkless.Base.ReportHandler.Models.Interfaces;
using Quarkless.Base.RestSharpClientManager.Models.Interfaces;
using Quarkless.Base.Topic.Models.Interfaces;
using Quarkless.Logic.Common;
using Quarkless.Models.Common.Enums;
using Quarkless.Models.Common.Extensions;
using Quarkless.Models.Common.Models;
using Quarkless.Models.Common.Models.Topic;
using Quarkless.Models.SearchResponse;

namespace Quarkless.Base.Query.Logic
{
	public class QueryLogic : CommonInterface, IQueryLogic
	{
		private readonly IRestSharpClientManager _restSharpClientManager;
		private readonly ISearchProvider _searchProvider;
		private readonly ITopicLookupLogic _topicLookupLogic;
		private readonly ISearchingCache _searchingCache;
		private readonly IHashtagLogic _hashtagLogic;
		private readonly IHeartbeatLogic _heartbeatLogic;
		private readonly ILookupLogic _lookupLogic;
		private readonly IApiClientContext _context;
		private readonly IHashtagGenerator _hashtagGenerator;

		public QueryLogic(IReportHandler reportHandler, IRestSharpClientManager restSharpClientManager,
			ISearchProvider searchProvider, ISearchingCache searchingCache, ITopicLookupLogic topicLookupLogic,
			IHashtagGenerator hashtagGenerator, IHashtagLogic hashtagLogic, IHeartbeatLogic heartbeatLogic,
			ILookupLogic lookupLogic, IApiClientContext context)
		:base(reportHandler, nameof(QueryLogic))
		{
			_lookupLogic = lookupLogic;
			_heartbeatLogic = heartbeatLogic;
			_hashtagLogic = hashtagLogic;
			_searchingCache = searchingCache;
			_hashtagGenerator = hashtagGenerator;
			_restSharpClientManager = restSharpClientManager;
			_topicLookupLogic = topicLookupLogic;
			_searchProvider = searchProvider;
			_context = context;
		}
		
		public object SearchPlaces(string query)
		{

			try
			{
				var results = _restSharpClientManager.GetRequest("https://maps.googleapis.com/maps/api/place/textsearch/json?query=" + query + "&key=AIzaSyD9hK0Uc_QZ-ejA6cXKrEdCJAOmerEsp0s",null);

				return results.Content;
			}
			catch(Exception err)
			{
				Console.WriteLine(err);
				return null;
			}
		}
		public object AutoCompleteSearchPlaces(string query, int radius = 500)
		{
			try
			{
				return _restSharpClientManager.GetRequest($"https://maps.googleapis.com/maps/api/place/autocomplete/json?input={query}&radius={radius}&key=AIzaSyD9hK0Uc_QZ-ejA6cXKrEdCJAOmerEsp0s",null).Content;
			}
			catch(Exception err)
			{
				Console.WriteLine(err);
				return null;
			}
		}	

		public async Task<Quarkless.Models.SearchResponse.Media> SimilarImagesSearch(string userId, int limit = 1, 
			int offset = 0, IEnumerable<string> urls = null, bool moreAccurate = false)
		{
			if(urls == null || (limit == 0)) return null;
			var requestData = urls as string[] ?? urls.ToArray();
			var strurl = requestData.Select(_ => new GroupImagesAlike { TopicGroup = new CTopic(), Url = _ }).ToList();
			var newLimit = limit;
			Quarkless.Models.SearchResponse.Media response;
			if (newLimit == -1)
			{
				var searchRequest = new SearchRequest
				{
					RequestData = requestData,
					Limit = newLimit,
					Offset = offset > 0 ? Math.Abs(newLimit - limit) : offset,
				};
				var cacheRes = await _searchingCache.GetSearchData(userId, searchRequest);
				if (cacheRes != null)
				{
					response = cacheRes.ResponseData;
					return response;
				}

				if (moreAccurate)
				{
					response = _searchProvider.YandexSearch.QuerySimilarImages(strurl, 6).Result.Result;
					response.Medias = response.Medias?.Distinct().Reverse().ToList();
					searchRequest.ResponseData = response;
					await _searchingCache.StoreSearchData(userId, searchRequest);
				}
				else
				{
					var res = _searchProvider.GoogleSearch.SearchSimilarImagesViaGoogle(strurl, 99);
					if (res.StatusCode == ResponseCode.Success)
					{
						response = res.Result;
						searchRequest.ResponseData = response;
						await _searchingCache.StoreSearchData(userId, searchRequest);
					}
					else
					{
						response = _searchProvider.YandexSearch.QuerySimilarImages(strurl, 6).Result.Result;
						response.Medias = response.Medias?.Distinct().Reverse().ToList();
						searchRequest.ResponseData = response;
						await _searchingCache.StoreSearchData(userId, searchRequest);
					}
				}
			}
			else
			{
				if (offset == 1) offset = 0;
				if (offset > 0)
				{
					newLimit *= (offset);
				}

				var searchRequest = new SearchRequest
				{
					RequestData = requestData,
					Limit = newLimit,
					Offset = offset > 0 ? Math.Abs(newLimit - limit) : offset,
				};
				var cacheRes = await _searchingCache.GetSearchData(userId, searchRequest);
				if (cacheRes != null)
				{
					response = cacheRes.ResponseData;
					return response;
				}

				if (moreAccurate)
				{
					response = _searchProvider.YandexSearch.QuerySimilarImages(strurl, 1 + offset).Result.Result;
					response.Medias = response.Medias?.Distinct().Reverse().ToList();
					searchRequest.ResponseData = response;
					await _searchingCache.StoreSearchData(userId, searchRequest);
				}
				else
				{
					var res = _searchProvider.GoogleSearch.SearchSimilarImagesViaGoogle(strurl, newLimit,
						offset > 0 ? Math.Abs(newLimit - limit) : offset);
					if (res.StatusCode == ResponseCode.Success)
					{
						response = res.Result;
						searchRequest.ResponseData = response;
						await _searchingCache.StoreSearchData(userId, searchRequest);
					}
					else
					{
						response = _searchProvider.YandexSearch.QuerySimilarImages(strurl, 1 + offset).Result.Result;
						response.Medias = response.Medias?.Distinct().Reverse().ToList();
						searchRequest.ResponseData = response;
						await _searchingCache.StoreSearchData(userId, searchRequest);
					}
				}
			}

			return response;
		}

		public async Task<IEnumerable<CTopic>> GetRelatedTopics(string topicParentId)
			=> await _topicLookupLogic.GetTopicsByParentId(topicParentId);
		public async Task<ProfileConfiguration> GetProfileConfig()
		{
			var topics = await _topicLookupLogic.GetCategories();
			return new ProfileConfiguration
			{
				Categories = topics.Where(_ => !string.IsNullOrEmpty(_.Name)),
				ColorsAllowed = Enum.GetValues(typeof(ColorType)).Cast<ColorType>().Select(v=>v.GetDescription()).ToList(),
				ImageTypes = Enum.GetValues(typeof(ImageType)).Cast<ImageType>().Select(v=>v.GetDescription()).ToList(),
				Orientations = Enum.GetValues(typeof(Orientation)).Cast<Orientation>().Select(v=>v.GetDescription()).ToList(),
				SizeTypes = Enum.GetValues(typeof(SizeType)).Cast<SizeType>().Select(v=>v.GetDescription()).ToList(),
				SearchTypes = Enum.GetValues(typeof(SearchType)).Cast<SearchType>().Select(v=>v.GetDescription()).ToList(),
				Languages = CultureInfo.GetCultures(CultureTypes.AllCultures).Where(_=>!_.Name.Contains("-")).Distinct().ToDictionary(_ => _.Name, _ => _.EnglishName),
				CanUserEditProfile = true
			};
		}

		public async Task<IEnumerable<string>> BuildHashtags(SuggestHashtagRequest suggestHashtagRequest)
		{
			var results = await _hashtagGenerator.SuggestHashtags(new Source
			{
				ProfileTopic = suggestHashtagRequest.ProfileTopic,
				MediaTopic = suggestHashtagRequest.MediaTopic,
				ImageUrls = suggestHashtagRequest.MediaUrls.ToList()
			},false,true);

			return results.Results.Select(_ => _.Name).Take(26);
		}
		public async Task<SubTopics> GetRelatedKeywords(string topicName)
		{
			var res = await _searchingCache.GetRelatedTopic(topicName);
			if (res != null && res.RelatedTopics.Count>0) return res;

			if (topicName.Contains('('))
			{
				var iterative = topicName.Split('(');
				var totalRes = new SubTopics {TopicName = topicName, RelatedTopics = new List<string>()};
				foreach (var topic in iterative)
				{
					var filtered = Regex.Replace(topic.ToLower(), "[^0-9a-zA-Z]+", "");
					var search = await _hashtagLogic.SearchHashtagAsync(filtered);
					if (!search.Succeeded)
					{
						var sRel = await _hashtagLogic.SearchRelatedHashtagAsync(topic, 1);
						if (!sRel.Succeeded) return null;
						totalRes.RelatedTopics.AddRange(sRel.Value.RelatedHashtags.Select(x=>x.Name));
					}
					else
					{
						totalRes.RelatedTopics.AddRange(search.Value.Select(x=>x.Name));
					}
				}
				if(totalRes.RelatedTopics.Count > 0)
					await _searchingCache.StoreRelatedTopics(totalRes);
				return totalRes;
			}
			var cleanedTopic = Regex.Replace(topicName.ToLower(), "[^0-9a-zA-Z]+", "");
			var hashtagsRes = await _hashtagLogic.SearchHashtagAsync(cleanedTopic);
			if (!hashtagsRes.Succeeded)
			{
				var related = await _hashtagLogic.SearchRelatedHashtagAsync(cleanedTopic, 1);
				if (!related.Succeeded) return null;
				var subTopics = new SubTopics
				{
					TopicName = topicName,
					RelatedTopics = related.Value.RelatedHashtags.Select(s => s.Name).ToList()
				};
				if(subTopics.RelatedTopics.Count > 0)
					await _searchingCache.StoreRelatedTopics(subTopics);
				return subTopics;

			}
			else
			{
				var subTopics = new SubTopics
				{
					TopicName = topicName,
					RelatedTopics = hashtagsRes.Value.Select(s=>s.Name).ToList()
				};
				if(subTopics.RelatedTopics.Count > 0)
					await _searchingCache.StoreRelatedTopics(subTopics);
				return subTopics;
			}
		}

		#region Search Instagram Stuff
		public async Task<Quarkless.Models.SearchResponse.Media> SearchMediasByTopic(IEnumerable<string> topics, string username, string instagramAccountId, int limit = 1)
		{
			return await RunCodeWithLoggerExceptionAsync(async () =>
			{
				var search = _searchProvider.InstagramSearch(
					new ApiClientContainer(_context, username, instagramAccountId), null);
				
				var results = await search.SearchMediaDetailInstagram(topics, limit);
				
				if (results != null) return results;
				await Warn($"Nothing was found for search query {string.Join(',', topics)}", 
					nameof(SearchMediasByTopic), username, instagramAccountId);  
				return new Quarkless.Models.SearchResponse.Media();
			}, nameof(SearchMediasByTopic), username, instagramAccountId);
		}

		public async Task<IEnumerable<LookupContainer<UserResponse>>> SearchUsersByTopic(IEnumerable<string> topics,
			string username, string instagramAccountId, int limit = 1)
		{
			return await RunCodeWithLoggerExceptionAsync(async () =>
			{
				var medias = await SearchMediasByTopic(topics, username, instagramAccountId, limit);
				var lookups = await _lookupLogic.Get(username, instagramAccountId);
				if (medias != null)
				{
					var users = medias.Medias.Select(x => new LookupContainer<UserResponse>
					{
						Object = x.User,
						Lookup = lookups.Where(s=>s.ObjectId == x.User.UserId.ToString())
							.OrderByDescending(d => d.LastModified)
							.DistinctBy(y => y.ActionType)
					}).DistinctBy(x => x.Object.Username);
					return users;
				}
				await Warn("Empty list returned", nameof(GetUsersTargetList), username, instagramAccountId);  
				return EmptyList<LookupContainer<UserResponse>>();

			}, nameof(SearchUsersByTopic), username, instagramAccountId);
		}

		public async Task<IEnumerable<LookupContainer<UserResponse>>> SearchUsersByLocation(Location location,
			string username, string instagramAccountId, int limit = 1)
		{
			return await RunCodeWithLoggerExceptionAsync(async () =>
			{
				var medias = await SearchMediasByLocation(location, username, instagramAccountId, limit);
				var lookups = await _lookupLogic.Get(username, instagramAccountId);
				if (medias != null)
				{
					var users = medias.Medias.Select(x => new LookupContainer<UserResponse>
					{
						Object = x.User,
						Lookup = lookups.Where(s => s.ObjectId == x.User.UserId.ToString())
							.OrderByDescending(d => d.LastModified)
							.DistinctBy(y => y.ActionType)
					}).DistinctBy(x => x.Object.Username);
					return users;
				}
				await Warn("Empty list returned", nameof(GetUsersTargetList), username, instagramAccountId);  
				return EmptyList<LookupContainer<UserResponse>>();

			}, nameof(SearchUsersByLocation), username, instagramAccountId);
		}
		public async Task<Quarkless.Models.SearchResponse.Media> SearchMediasByLocation(Location location, string username, string instagramAccountId, int limit = 1)
		{
			return await RunCodeWithLoggerExceptionAsync(async () =>
			{
				
				var search = _searchProvider.InstagramSearch(
					new ApiClientContainer(_context, username, instagramAccountId), null);
				
				var results = await search.SearchTopLocationMediaDetailInstagram(location, limit);

				if (results != null) return results;
				await Warn($"Nothing was found for search query {location.City}", 
					nameof(SearchMediasByLocation), username, instagramAccountId);  
				return new Quarkless.Models.SearchResponse.Media();
			}, nameof(SearchMediasByLocation), username, instagramAccountId);
		}
		#endregion
		#region Heartbeat Logic Stuff

		public async Task<IEnumerable<CommentMedia>> GetRecentComments(ProfileRequest profile)
		{
			return await RunCodeWithLoggerExceptionAsync(async () =>
			{
				var metaDataComments =
					await _heartbeatLogic.GetMetaData<List<UserResponse<InstaComment>>>(new MetaDataFetchRequest
					{
						MetaDataType = MetaDataType.UsersRecentComments,
						ProfileCategoryTopicId = profile.Topic.Category._id,
						InstagramId = profile.InstagramAccountId
					});
				var metaS = metaDataComments as Meta<List<UserResponse<InstaComment>>>[] ?? metaDataComments.ToArray();

				if (metaS.Any())
				{
					var recentComments = metaS.Select(x=>x.ObjectItem).SquashMe().ToList();
					var medias = await GetUsersMedia(profile);

					return (from mediaResponseSingle in medias 
						where recentComments.Exists(x => x.MediaId == mediaResponseSingle.MediaId) 
						select new CommentMedia {Media = mediaResponseSingle, Comments = recentComments.Where(x => x.MediaId == mediaResponseSingle.MediaId).ToList()}).ToList();
				}
				await Warn("Empty Comments", nameof(GetRecentComments), profile.AccountId, profile.InstagramAccountId);
				return EmptyList<CommentMedia>();
			}, nameof(GetRecentComments), profile.AccountId, profile.InstagramAccountId);
		}
		public async Task<InstaDirectInbox> GetUserInbox(ProfileRequest profile)
		{
			return await RunCodeWithLoggerExceptionAsync(async () =>
			{
				var inbox = await _heartbeatLogic.GetMetaData<InstaDirectInboxContainer>(
					new MetaDataFetchRequest
					{
						MetaDataType = MetaDataType.FetchUserDirectInbox,
						ProfileCategoryTopicId = profile.Topic.Category._id,
						InstagramId = profile.InstagramAccountId
					});
				if (inbox != null)
				{
					var items = inbox.FirstOrDefault()?.ObjectItem;
					items?.Inbox.Threads.ForEach(_ => { _.Items.Reverse(); });
					return items?.Inbox;
				}
				await Warn("Inbox Empty", nameof(GetUserInbox), profile.AccountId, profile.InstagramAccountId);
				return null;
			},nameof(GetUserInbox), profile.AccountId, profile.InstagramAccountId);
		}
		public async Task<IEnumerable<Quarkless.Models.SearchResponse.Media>> GetUsersFeed(ProfileRequest profile)
		{
			return await RunCodeWithLoggerExceptionAsync(async () =>
			{
				var medias = await _heartbeatLogic.GetMetaData<Quarkless.Models.SearchResponse.Media>(new MetaDataFetchRequest
				{
					MetaDataType = MetaDataType.FetchUsersFeed,
					ProfileCategoryTopicId = profile.Topic.Category._id,
					InstagramId = profile.InstagramAccountId
				});

				if (medias != null) return medias.Select(x => x.ObjectItem);
				await Warn("Empty list returned", nameof(GetUsersFeed), profile.AccountId, profile.InstagramAccountId);  
				return EmptyList<Quarkless.Models.SearchResponse.Media>();
			}, nameof(GetUsersFeed), profile.AccountId, profile.InstagramAccountId);
		}
		public async Task<IEnumerable<MediaResponseSingle>> GetUsersMedia(ProfileRequest profile)
		{
			return (await RunCodeWithLoggerExceptionAsync(async () =>
			{
				var medias = await _heartbeatLogic.GetMetaData<Quarkless.Models.SearchResponse.Media>(new MetaDataFetchRequest
				{
					MetaDataType = MetaDataType.FetchUserOwnProfile,
					ProfileCategoryTopicId = profile.Topic.Category._id,
					InstagramId = profile.InstagramAccountId
				});
				if (medias == null)
				{
					await Warn("Empty list returned", nameof(GetUsersMedia), profile.AccountId, profile.InstagramAccountId);
					return EmptyList<MediaResponseSingle>();
				}
				var newResults = medias.Select(x => x.ObjectItem.Medias).SquashMe().Select(x =>
				{
					var media = new List<MediaResponseSingle>();
					foreach (var mediaUrl in x.MediaUrl)
					{
						media.Add(new MediaResponseSingle
						{
							Caption = x.Caption,
							CommentCount = x.CommentCount,
							Domain = x.Domain,
							Explore = x.Explore,
							FilterType = x.FilterType,
							HasSeen = x.HasSeen,
							HasAudio = x.HasAudio,
							HasLikedBefore = x.HasLikedBefore,
							IsCommentsDisabled = x.IsCommentsDisabled,
							IsFollowing = x.IsFollowing,
							MediaId = x.MediaId,
							PhotosOfI = x.PhotosOfI,
							PreviewComments = x.PreviewComments,
							ProductTags = x.ProductTags,
							ProductType = x.ProductType,
							NumberOfQualities = x.NumberOfQualities,
							TakenAt = x.TakenAt,
							Title = x.Title,
							TopLikers = x.TopLikers,
							Topic = x.Topic,
							MediaType = x.MediaType,
							UserTags = x.UserTags,
							User = x.User,
							MediaUrl = mediaUrl,
							MediaFrom = x.MediaFrom,
							LikesCount = x.LikesCount,
							Location = x.Location,
							ViewCount = x.ViewCount
						});
					}

					return media;
				}).SquashMe();
				return newResults;

			}, nameof(GetUsersMedia), profile.AccountId, profile.InstagramAccountId)).OrderByDescending(x=>x.TakenAt);
		}
		public async Task<IEnumerable<Quarkless.Models.SearchResponse.Media>> GetMediasTargetList(ProfileRequest profile)
		{
			return await RunCodeWithLoggerExceptionAsync(async () =>
			{
				var medias = await _heartbeatLogic.GetMetaData<Quarkless.Models.SearchResponse.Media>(new MetaDataFetchRequest
				{
					MetaDataType = MetaDataType.FetchMediaByUserTargetList,
					ProfileCategoryTopicId = profile.Topic.Category._id,
					InstagramId = profile.InstagramAccountId
				});
				if (medias != null) return medias.Select(x => x.ObjectItem);
				await Warn("Empty list returned", nameof(GetMediasTargetList), profile.AccountId, profile.InstagramAccountId);  
				return EmptyList<Quarkless.Models.SearchResponse.Media>();
			}, nameof(GetUsersTargetList), profile.AccountId, profile.InstagramAccountId);
		}
		public async Task<IEnumerable<Quarkless.Models.SearchResponse.Media>> GetMediasByLocation(ProfileRequest profile)
		{
			return await RunCodeWithLoggerExceptionAsync(async () =>
			{
				var medias = await _heartbeatLogic.GetMetaData<Quarkless.Models.SearchResponse.Media>(new MetaDataFetchRequest
				{
					MetaDataType = MetaDataType.FetchMediaByUserLocationTargetList,
					ProfileCategoryTopicId = profile.Topic.Category._id,
					InstagramId = profile.InstagramAccountId
				});

				if (medias != null) return medias.Select(x => x.ObjectItem);
				await Warn("Empty list returned", nameof(GetMediasByLocation), profile.AccountId, profile.InstagramAccountId);  
				return EmptyList<Quarkless.Models.SearchResponse.Media>();
			}, nameof(GetMediasByLocation), profile.AccountId, profile.InstagramAccountId);
		}
		public async Task<IEnumerable<LookupContainer<UserResponse>>> GetUsersTargetList(ProfileRequest profile)
		{
			return await RunCodeWithLoggerExceptionAsync(async () =>
			{
				var medias = await _heartbeatLogic.GetMetaData<Quarkless.Models.SearchResponse.Media>(new MetaDataFetchRequest
				{
					MetaDataType = MetaDataType.FetchMediaByUserTargetList,
					ProfileCategoryTopicId = profile.Topic.Category._id,
					InstagramId = profile.InstagramAccountId
				});

				if(medias == null)
				{ 
					await Warn("Empty list returned", nameof(GetUsersTargetList), profile.AccountId, profile.InstagramAccountId);  
					return EmptyList<LookupContainer<UserResponse>>();
				}
				var commenters = await _heartbeatLogic.GetMetaData<List<UserResponse<InstaComment>>>
					(new MetaDataFetchRequest
				{
					MetaDataType = MetaDataType.FetchCommentsViaUserTargetList,
					ProfileCategoryTopicId = profile.Topic.Category._id,
					InstagramId = profile.InstagramAccountId
				});
				var lookups = await _lookupLogic.Get(profile.AccountId, profile.InstagramAccountId);
				var users = medias.Select(a=>a.ObjectItem.Medias)
					.SquashMe().Select(x=>new LookupContainer<UserResponse>
					{
						Object = x.User, 
						Lookup = lookups.Where(s=>s.ObjectId == x.User.UserId.ToString())
							.OrderByDescending(d=>d.LastModified)
							.DistinctBy(y=>y.ActionType)
					}).ToList();

				if (commenters == null) return users.DistinctBy(x => x.Object.Username);
				{
					var comments = commenters.Select(x=>x.ObjectItem).SquashMe();
					users.AddRange(comments.Select(u=> new LookupContainer<UserResponse>
					{
						Object = new UserResponse {
							FullName = u.FullName,
							IsPrivate = u.IsPrivate,
							ProfilePicture = u.ProfilePicture,
							IsVerified = u.IsVerified,
							Topic = u.Topic,
							UserId = u.UserId,
							Username = u.Username,
						},
						Lookup = lookups.Where(s=>s.ObjectId == u.UserId.ToString()).DistinctBy(x=>x.ActionType)
					}));
				}
				return users.DistinctBy(x=>x.Object.Username);
			}, nameof(GetUserByLocation), profile.AccountId, profile.InstagramAccountId);
		}
		public async Task<IEnumerable<LookupContainer<UserResponse>>> GetUserByLocation(ProfileRequest profile)
		{
			return await RunCodeWithLoggerExceptionAsync(async () =>
			{
				var medias = await _heartbeatLogic.GetMetaData<Quarkless.Models.SearchResponse.Media>(
					new MetaDataFetchRequest
					{
						MetaDataType = MetaDataType.FetchMediaByUserLocationTargetList,
						ProfileCategoryTopicId = profile.Topic.Category._id,
						InstagramId = profile.InstagramAccountId
					});
				
				if(medias == null)
				{ 
					await Warn("Empty list returned", nameof(GetUserByLocation), profile.AccountId, profile.InstagramAccountId);  
					return EmptyList<LookupContainer<UserResponse>>();
				}
				
				var commenters = await _heartbeatLogic.GetMetaData<List<UserResponse<InstaComment>>>(new MetaDataFetchRequest
				{
					MetaDataType = MetaDataType.FetchCommentsViaLocationTargetList,
					ProfileCategoryTopicId = profile.Topic.Category._id,
					InstagramId = profile.InstagramAccountId
				});
				var lookups = await _lookupLogic.Get(profile.AccountId, profile.InstagramAccountId);
				var users = medias.Select(a=>a.ObjectItem.Medias)
					.SquashMe().Select(x=>new LookupContainer<UserResponse>
				{
					Object = x.User, 
					Lookup = lookups.Where(s=>s.ObjectId == x.User.UserId.ToString())
						.OrderByDescending(d=>d.LastModified)
						.DistinctBy(y=>y.ActionType)
				}).ToList();

				if (commenters == null)
					return users.DistinctBy(x=>x.Object.Username);

				var comments = commenters.Select(x=>x.ObjectItem).SquashMe();
				users.AddRange(comments.Select(u=> new LookupContainer<UserResponse>
				{
					Object = new UserResponse {
						FullName = u.FullName,
						IsPrivate = u.IsPrivate,
						ProfilePicture = u.ProfilePicture,
						IsVerified = u.IsVerified,
						Topic = u.Topic,
						UserId = u.UserId,
						Username = u.Username,
					},
					Lookup = lookups.Where(s=>s.ObjectId == u.UserId.ToString())
						.OrderByDescending(d=>d.LastModified)
						.DistinctBy(y=>y.ActionType)
				}));

				return users.DistinctBy(x=>x.Object.Username);
			}, nameof(GetUserByLocation), profile.AccountId, profile.InstagramAccountId);
		}
		public async Task<IEnumerable<LookupContainer<UserResponse<string>>>> GetUsersFollowingList(ProfileRequest profile)
		{
			return await RunCodeWithLoggerExceptionAsync(async () =>
			{
				var users = await _heartbeatLogic.GetMetaData<List<UserResponse<string>>>(new MetaDataFetchRequest
					{
						MetaDataType = MetaDataType.FetchUsersFollowingList,
						ProfileCategoryTopicId = profile.Topic.Category._id,
						InstagramId = profile.InstagramAccountId
					});
				var lookups = await _lookupLogic.Get(profile.AccountId, profile.InstagramAccountId);
				if (users != null) return users.Select(x => x.ObjectItem).SquashMe()
					.Select(x=>new LookupContainer<UserResponse<string>>
					{
						Object = x, 
						Lookup = lookups.Where(s=>s.ObjectId == x.UserId.ToString())
							.OrderByDescending(d=>d.LastModified)
							.DistinctBy(y=>y.ActionType)
					}).ToList();
				await Warn("Empty list returned", nameof(GetUsersFollowingList), profile.AccountId, profile.InstagramAccountId); 
				return EmptyList<LookupContainer<UserResponse<string>>>();
			}, nameof(GetUsersFollowingList), profile.AccountId, profile.InstagramAccountId);
		}
		public async Task<IEnumerable<LookupContainer<UserResponse<string>>>> GetUsersFollowerList(ProfileRequest profile)
		{
			return await RunCodeWithLoggerExceptionAsync(async () =>
			{
				var users = await _heartbeatLogic.GetMetaData<List<UserResponse<string>>>(new MetaDataFetchRequest
				{
					MetaDataType = MetaDataType.FetchUsersFollowerList,
					ProfileCategoryTopicId = profile.Topic.Category._id, 
					InstagramId = profile.InstagramAccountId
				});
				var lookups = await _lookupLogic.Get(profile.AccountId, profile.InstagramAccountId);
				if (users != null) return users.Select(x => x.ObjectItem).SquashMe()
					.Select(x=>new LookupContainer<UserResponse<string>>
					{
						Object = x, 
						Lookup = lookups.Where(s=>s.ObjectId == x.UserId.ToString())
							.OrderByDescending(d=>d.LastModified)
							.DistinctBy(y=>y.ActionType)
					}).ToList();
				await Warn("Returned empty list", nameof(GetUsersFollowerList), profile.AccountId, profile.InstagramAccountId); 
				return EmptyList<LookupContainer<UserResponse<string>>>();
			}, nameof(GetUsersFollowerList), profile.AccountId, profile.InstagramAccountId);
		}
		public async Task<IEnumerable<LookupContainer<UserResponse<UserSuggestionDetails>>>> GetUsersSuggestedFollowingList(ProfileRequest profile)
		{
			return await RunCodeWithLoggerExceptionAsync(async () =>
			{
				var users = await _heartbeatLogic.GetMetaData<List<UserResponse<UserSuggestionDetails>>>(new MetaDataFetchRequest
				{
					MetaDataType = MetaDataType.FetchUsersFollowSuggestions,
					ProfileCategoryTopicId = profile.Topic.Category._id, 
					InstagramId = profile.InstagramAccountId
				});
				var lookups = await _lookupLogic.Get(profile.AccountId, profile.InstagramAccountId);
				if (users != null) return users.Select(x => x.ObjectItem).SquashMe()
					.Select(x=>new LookupContainer<UserResponse<UserSuggestionDetails>>
					{
						Object = x, 
						Lookup = lookups.Where(s=>s.ObjectId == x.UserId.ToString())
							.OrderByDescending(d=>d.LastModified)
							.DistinctBy(y=>y.ActionType)
					}).ToList();
				await Warn("Returned empty list", nameof(GetUsersSuggestedFollowingList), profile.AccountId, profile.InstagramAccountId); 
				return EmptyList<LookupContainer<UserResponse<UserSuggestionDetails>>>();
			}, nameof(GetUsersSuggestedFollowingList), profile.AccountId, profile.InstagramAccountId);
		}
		#endregion
	}
}
