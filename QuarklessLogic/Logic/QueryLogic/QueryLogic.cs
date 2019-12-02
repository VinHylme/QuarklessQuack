using QuarklessContexts.Extensions;
using QuarklessContexts.Models.Profiles;
using QuarklessContexts.Models.QueryModels;
using QuarklessContexts.Models.QueryModels.Settings;
using QuarklessContexts.Models.ServicesModels.SearchModels;
using QuarklessLogic.Handlers.RestSharpClient;
using QuarklessLogic.Logic.HashtagLogic;
using QuarklessLogic.ServicesLogic;
using QuarklessLogic.ServicesLogic.ContentSearch;
using QuarklessRepositories.RedisRepository.SearchCache;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MoreLinq.Extensions;
using Quarkless.Interfacing;
using QuarklessLogic.ServicesLogic.HeartbeatLogic;
using QuarklessContexts.Models.ServicesModels.HeartbeatModels;
using Quarkless.Interfacing.Objects;
using QuarklessContexts.Models.Sections;
using QuarklessRepositories.RedisRepository.LoggerStoring;
using InstagramApiSharp.Classes.Models;
using QuarklessContexts.Models.LookupModels;
using QuarklessLogic.Handlers.ClientProvider;
using QuarklessLogic.Logic.LookupLogic;

namespace QuarklessLogic.Logic.QueryLogic
{
	public class QueryLogic : CommonInterface, IQueryLogic
	{
		private readonly IRestSharpClientManager _restSharpClientManager;
		private readonly IContentSearcherHandler _contentSearcherHandler;
		private readonly ITopicServicesLogic _topicServicesLogic;
		private readonly ISearchingCache _searchingCache;
		private readonly IHashtagLogic _hashtagLogic;
		private readonly IHeartbeatLogic _heartbeatLogic;
		private readonly ILookupLogic _lookupLogic;
		private readonly IAPIClientContext _context;
		public QueryLogic(IRestSharpClientManager restSharpClientManager, IContentSearcherHandler contentSearcherHandler,
			ISearchingCache searchingCache, ITopicServicesLogic topicServicesLogic, 
			IHashtagLogic hashtagLogic, IHeartbeatLogic heartbeatLogic, ILoggerStore loggerStore, ILookupLogic lookupLogic, IAPIClientContext context)
			: base(loggerStore, Sections.QueryLogic.GetDescription())
		{
			_lookupLogic = lookupLogic;
			_heartbeatLogic = heartbeatLogic;
			_hashtagLogic = hashtagLogic;
			_searchingCache = searchingCache;
			_restSharpClientManager = restSharpClientManager;
			_contentSearcherHandler = contentSearcherHandler;
			_topicServicesLogic = topicServicesLogic;
			_context = context;
		}
		public object SearchPlaces(string query)
		{

			try
			{
				var results = _restSharpClientManager.GetRequest("https://maps.googleapis.com/maps/api/place/textsearch/json?query=" + query + "&key=AIzaSyD9hK0Uc_QZ-ejA6cXKrEdCJAOmerEsp0s",null);

				return results.Content;
			}
			catch(Exception ee)
			{
				return null;
			}
		}
		public object AutoCompleteSearchPlaces(string query, int radius = 500)
		{
			try
			{
				return _restSharpClientManager.GetRequest($"https://maps.googleapis.com/maps/api/place/autocomplete/json?input={query}&radius={radius}&key=AIzaSyD9hK0Uc_QZ-ejA6cXKrEdCJAOmerEsp0s",null).Content;
			}
			catch(Exception e)
			{
				return null;
			}
		}	
		public async Task<Media> SimilarImagesSearch(string userId, int limit = 1, int offset = 0, IEnumerable<string> urls = null, bool moreAccurate = false)
		{
			if(urls == null || (limit == 0)) return null;
			var requestData = urls as string[] ?? urls.ToArray();
			var strurl = requestData.Select(_ => new GroupImagesAlike { TopicGroup = "any", Url = _ }).ToList();
			var newLimit = limit;
			Media response;
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
					response = _contentSearcherHandler.SearchViaYandexBySimilarImages(strurl, 6, 0).Result;
					response.Medias.Reverse();
					searchRequest.ResponseData = response;
					await _searchingCache.StoreSearchData(userId, searchRequest);
				}
				else
				{
					var res = _contentSearcherHandler.SearchSimilarImagesViaGoogle(strurl, 99);
					if (res.StatusCode == QuarklessContexts.Models.ResponseModels.ResponseCode.Success)
					{
						response = res.Result;
						searchRequest.ResponseData = response;
						await _searchingCache.StoreSearchData(userId, searchRequest);
					}
					else
					{
						response = _contentSearcherHandler.SearchViaYandexBySimilarImages(strurl, 6, 0).Result;
						response.Medias.Reverse();
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
					response = _contentSearcherHandler.SearchViaYandexBySimilarImages(strurl, 1 + offset, offset).Result;
					response.Medias.Reverse();
					searchRequest.ResponseData = response;
					await _searchingCache.StoreSearchData(userId, searchRequest);
				}
				else
				{
					var res = _contentSearcherHandler.SearchSimilarImagesViaGoogle(strurl, newLimit,
						offset > 0 ? Math.Abs(newLimit - limit) : offset);
					if (res.StatusCode == QuarklessContexts.Models.ResponseModels.ResponseCode.Success)
					{
						response = res.Result;
						searchRequest.ResponseData = response;
						await _searchingCache.StoreSearchData(userId, searchRequest);
					}
					else
					{
						response = _contentSearcherHandler.SearchViaYandexBySimilarImages(strurl, 1 + offset, offset).Result;
						response.Medias.Reverse();
						searchRequest.ResponseData = response;
						await _searchingCache.StoreSearchData(userId, searchRequest);
					}
				}
			}

			return response;
		}
		public async Task<ProfileConfiguration> GetProfileConfig()
		{
			return new ProfileConfiguration
			{
				Topics = (await _topicServicesLogic.GetAllTopicCategories()).OrderBy(item=>item.CategoryName).ToList(),
				ColorsAllowed = Enum.GetValues(typeof(ColorType)).Cast<ColorType>().Select(v=>v.GetDescription()).ToList(),
				ImageTypes = Enum.GetValues(typeof(ImageType)).Cast<ImageType>().Select(v=>v.GetDescription()).ToList(),
				Orientations = Enum.GetValues(typeof(Orientation)).Cast<Orientation>().Select(v=>v.GetDescription()).ToList(),
				SizeTypes = Enum.GetValues(typeof(SizeType)).Cast<SizeType>().Select(v=>v.GetDescription()).ToList(),
				SearchTypes = Enum.GetValues(typeof(SearchType)).Cast<SearchType>().Select(v=>v.GetDescription()).ToList(),
				Languages = CultureInfo.GetCultures(CultureTypes.AllCultures).Where(_=>!_.Name.Contains("-")).Distinct().ToDictionary(_ => _.Name, _ => _.EnglishName),
				CanUserEditProfile = true
			};
		}
		public async Task<IEnumerable<string>> BuildHashtags(string topic, string subcategory, string language,
			int limit = 1, int pickRate = 20)
		{
			var res = (await _hashtagLogic.GetHashtagsByTopicAndLanguage(topic.OnlyWords(), language.ToUpper().OnlyWords(), language.MapLanguages().OnlyWords(),limit)).Shuffle().ToList();
			var clean = new Regex(@"[^\w\d]");
			if (res.Count <= 0) return null;
			var hashtags = new List<string>();
			while (hashtags.Count < pickRate) { 
				var chosenHashtags = new List<string>();
				foreach(var hashtagres in res)
				{
					if (string.IsNullOrEmpty(hashtagres.Language)) continue;
					var hlang = clean.Replace(hashtagres.Language.ToLower(),"");
					var langpicked = clean.Replace(language.MapLanguages().ToLower(),"");

					if(hlang == langpicked)
						chosenHashtags.AddRange(hashtagres.Hashtags);
				}
				if (chosenHashtags.Count <= 0) continue;
				var chosenHashtagsFiltered = chosenHashtags.Where(space => space.Count(oc => oc == ' ') <= 1);
				var hashtagsFiltered = chosenHashtagsFiltered as string[] ?? chosenHashtagsFiltered.ToArray();
				if (!hashtagsFiltered.Any()) return null;
				hashtags.AddRange(hashtagsFiltered.Where(s => s.Length >= 3 && s.Length <= 30));
			}
			return hashtags.Take(pickRate);
		}
		public async Task<SubTopics> GetRelatedKeywords(string topicName)
		{
			var res = await _searchingCache.GetReleatedTopic(topicName);
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
						var sRel = await _hashtagLogic.SearchReleatedHashtagAsync(topic, 1);
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
				var related = await _hashtagLogic.SearchReleatedHashtagAsync(cleanedTopic, 1);
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
		public async Task<Media> SearchMediasByTopic(IEnumerable<string> topics, string username, string instagramAccountId, int limit = 1)
		{
			return await RunCodeWithLoggerExceptionAsync(async () =>
			{
				_contentSearcherHandler.ChangeUser(new APIClientContainer(_context,username, instagramAccountId));
				var results = await _contentSearcherHandler.SearchMediaDetailInstagram(topics.ToList(), limit);
				if (results != null) return results;
				await Warn($"Nothing was found for search query {string.Join(',', topics)}", 
					nameof(SearchMediasByTopic), username, instagramAccountId);  
				return new Media();
			}, nameof(SearchMediasByTopic), username, instagramAccountId);
		}

		public async Task<IEnumerable<LookupContainer<UserResponse>>> SearchUsersByTopic(IEnumerable<string> topics,
			string username, string instagramAccountId, int limit = 1)
		{
			return await RunCodeWithLoggerExceptionAsync(async () =>
			{
				var medias = await SearchMediasByTopic(topics, username, instagramAccountId, limit);
				if (medias != null)
				{
					var users = medias.Medias.Select(x => new LookupContainer<UserResponse>
					{
						Object = x.User,
						Lookup = _lookupLogic.Get(username, instagramAccountId,
								x.User.UserId.ToString()).GetAwaiter().GetResult()
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
				if (medias != null)
				{
					var users = medias.Medias.Select(x => new LookupContainer<UserResponse>
					{
						Object = x.User,
						Lookup = _lookupLogic.Get(username, instagramAccountId,
								x.User.UserId.ToString()).GetAwaiter().GetResult()
							.OrderByDescending(d => d.LastModified)
							.DistinctBy(y => y.ActionType)
					}).DistinctBy(x => x.Object.Username);
					return users;
				}
				await Warn("Empty list returned", nameof(GetUsersTargetList), username, instagramAccountId);  
				return EmptyList<LookupContainer<UserResponse>>();

			}, nameof(SearchUsersByLocation), username, instagramAccountId);
		}
		public async Task<Media> SearchMediasByLocation(Location location, string username, string instagramAccountId, int limit = 1)
		{
			return await RunCodeWithLoggerExceptionAsync(async () =>
			{
				_contentSearcherHandler.ChangeUser(new APIClientContainer(_context,username, instagramAccountId));
				var results = await _contentSearcherHandler.SearchTopLocationMediaDetailInstagram(location, limit);
				if (results != null) return results;
				await Warn($"Nothing was found for search query {location.City}", 
					nameof(SearchMediasByLocation), username, instagramAccountId);  
				return new Media();
			}, nameof(SearchMediasByLocation), username, instagramAccountId);
		}
		#endregion
		#region Heartbeat Logic Stuff

		public async Task<IEnumerable<CommentMedia>> GetRecentComments(SString accountId,
			SString instagramAccountId, SString topic)
		{
			return await RunCodeWithLoggerExceptionAsync(async () =>
			{
				var metaDataComments =
					await _heartbeatLogic.GetMetaData<List<UserResponse<InstaComment>>>(
						MetaDataType.UsersRecentComments, topic, instagramAccountId);
				var metaS = metaDataComments as __Meta__<List<UserResponse<InstaComment>>>[] ?? metaDataComments.ToArray();

				if (metaS.Any())
				{
					var recentComments = metaS.Select(x=>x.ObjectItem).SquashMe().ToList();
					var medias = await GetUsersMedia(accountId, instagramAccountId, topic);

					return (from mediaResponseSingle in medias 
						where recentComments.Exists(x => x.MediaId == mediaResponseSingle.MediaId) select new CommentMedia {Media = mediaResponseSingle, Comments = recentComments.Where(x => x.MediaId == mediaResponseSingle.MediaId).ToList()}).ToList();
				}
				await Warn("Empty Comments", nameof(GetRecentComments), accountId, instagramAccountId);
				return EmptyList<CommentMedia>();
			}, nameof(GetRecentComments), accountId, instagramAccountId);
		}

		public async Task<InstaDirectInbox> GetUserInbox(SString accountId, SString instagramAccountId, SString topic)
		{
			return await RunCodeWithLoggerExceptionAsync(async () =>
			{
				var inbox = await _heartbeatLogic.GetMetaData<InstaDirectInboxContainer>(
					MetaDataType.FetchUserDirectInbox, topic, instagramAccountId);
				if (inbox != null)
				{
					var items = inbox.FirstOrDefault()?.ObjectItem;
					items?.Inbox.Threads.ForEach(_ => { _.Items.Reverse(); });
					return items?.Inbox;
				}
				await Warn("Inbox Empty", nameof(GetUserInbox), accountId, instagramAccountId);
				return null;
			},nameof(GetUserInbox), accountId, instagramAccountId);
		}
		public async Task<IEnumerable<Media>> GetUsersFeed(SString accountId, SString instagramAccountId, SString topic)
		{
			return await RunCodeWithLoggerExceptionAsync(async () =>
			{
				var medias = await _heartbeatLogic.GetMetaData<Media>(MetaDataType.FetchUsersFeed, topic, instagramAccountId);
				if (medias != null) return medias.Select(x => x.ObjectItem);
				await Warn("Empty list returned", nameof(GetUsersFeed), accountId, instagramAccountId);  
				return EmptyList<Media>();
			}, nameof(GetUsersFeed), accountId, instagramAccountId);
		}
		public async Task<IEnumerable<MediaResponseSingle>> GetUsersMedia(SString accountId, SString instagramAccountId, SString topic)
		{
			return (await RunCodeWithLoggerExceptionAsync(async () =>
			{
				var medias = await _heartbeatLogic.GetMetaData<Media>(MetaDataType.FetchUserOwnProfile, topic, instagramAccountId);
				if (medias == null)
				{
					await Warn("Empty list returned", nameof(GetUsersMedia), accountId, instagramAccountId);
					return EmptyList<MediaResponseSingle>();
				}
				var newResults = medias.Select(x => x.ObjectItem.Medias).SquashMe().Select(x =>
				{
					var medi = new List<MediaResponseSingle>();
					foreach (var mediaUrl in x.MediaUrl)
					{
						medi.Add(new MediaResponseSingle
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

					return medi;
				}).SquashMe();
				return newResults;

			}, nameof(GetUsersMedia), accountId, instagramAccountId)).OrderByDescending(x=>x.TakenAt);
		}
		public async Task<IEnumerable<Media>> GetMediasTargetList(SString accountId, SString instagramAccountId, SString topic)
		{
			return await RunCodeWithLoggerExceptionAsync(async () =>
			{
				var medias = await _heartbeatLogic.GetMetaData<Media>(MetaDataType.FetchMediaByUserTargetList, topic, instagramAccountId);
				if (medias != null) return medias.Select(x => x.ObjectItem);
				await Warn("Empty list returned", nameof(GetMediasTargetList), accountId, instagramAccountId);  
				return EmptyList<Media>();
			}, nameof(GetUsersTargetList), accountId, instagramAccountId);
		}
		public async Task<IEnumerable<Media>> GetMediasByLocation(SString accountId, SString instagramAccountId, SString topic)
		{
			return await RunCodeWithLoggerExceptionAsync(async () =>
			{
				var medias = await _heartbeatLogic.GetMetaData<Media>(MetaDataType.FetchMediaByUserLocationTargetList, topic, instagramAccountId);
				if (medias != null) return medias.Select(x => x.ObjectItem);
				await Warn("Empty list returned", nameof(GetMediasByLocation), accountId, instagramAccountId);  
				return EmptyList<Media>();
			}, nameof(GetMediasByLocation), accountId, instagramAccountId);
		}
		public async Task<IEnumerable<LookupContainer<UserResponse>>> GetUsersTargetList(SString accountId, SString instagramAccountId, SString topic)
		{
			return await RunCodeWithLoggerExceptionAsync(async () =>
			{
				var medias = await _heartbeatLogic.GetMetaData<Media>(MetaDataType.FetchMediaByUserTargetList, topic, instagramAccountId);
				if(medias == null)
				{ 
					await Warn("Empty list returned", nameof(GetUsersTargetList), accountId, instagramAccountId);  
					return EmptyList<LookupContainer<UserResponse>>();
				}
				var commenters = await _heartbeatLogic.GetMetaData<List<UserResponse<InstaComment>>>(MetaDataType.FetchCommentsViaUserTargetList, topic, instagramAccountId);
				var users = medias.Select(a=>a.ObjectItem.Medias)
					.SquashMe().Select(x=>new LookupContainer<UserResponse>
					{
						Object = x.User, 
						Lookup = _lookupLogic.Get(accountId, instagramAccountId, 
							x.User.UserId.ToString()).GetAwaiter().GetResult()
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
						Lookup = _lookupLogic.Get(accountId, instagramAccountId, 
							u.UserId.ToString()).GetAwaiter().GetResult().DistinctBy(x=>x.ActionType)
					}));
				}
				return users.DistinctBy(x=>x.Object.Username);
			}, nameof(GetUserByLocation), accountId, instagramAccountId);
		}
		public async Task<IEnumerable<LookupContainer<UserResponse>>> GetUserByLocation(SString accountId, SString instagramAccountId, SString topic)
		{
			return await RunCodeWithLoggerExceptionAsync(async () =>
			{
				var medias = await _heartbeatLogic.GetMetaData<Media>(MetaDataType.FetchMediaByUserLocationTargetList, topic, instagramAccountId);
				
				if(medias == null)
				{ 
					await Warn("Empty list returned", nameof(GetUserByLocation), accountId, instagramAccountId);  
					return EmptyList<LookupContainer<UserResponse>>();
				}
				
				var commenters = await _heartbeatLogic.GetMetaData<List<UserResponse<InstaComment>>>(MetaDataType.FetchCommentsViaLocationTargetList, topic, instagramAccountId);
				var users = medias.Select(a=>a.ObjectItem.Medias)
				.SquashMe().Select(x=>new LookupContainer<UserResponse>
				{
					Object = x.User, 
					Lookup = _lookupLogic.Get(accountId, instagramAccountId, 
						x.User.UserId.ToString()).GetAwaiter().GetResult()
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
					Lookup = _lookupLogic.Get(accountId, instagramAccountId,
						u.UserId.ToString()).GetAwaiter().GetResult()
						.OrderByDescending(d=>d.LastModified)
						.DistinctBy(y=>y.ActionType)
				}));

				return users.DistinctBy(x=>x.Object.Username);
			}, nameof(GetUserByLocation), accountId, instagramAccountId);
		}
		public async Task<IEnumerable<LookupContainer<UserResponse<string>>>> GetUsersFollowingList(SString accountId, SString instagramAccountId, SString topic)
		{
			return await RunCodeWithLoggerExceptionAsync(async () =>
			{
				var users = await _heartbeatLogic.GetMetaData<List<UserResponse<string>>>(MetaDataType.FetchUsersFollowingList, topic, instagramAccountId);
				if (users != null) return users.Select(x => x.ObjectItem).SquashMe()
					.Select(x=>new LookupContainer<UserResponse<string>>
					{
						Object = x, 
						Lookup = _lookupLogic.Get(accountId, instagramAccountId, 
							x.UserId.ToString()).GetAwaiter().GetResult()
							.OrderByDescending(d=>d.LastModified)
							.DistinctBy(y=>y.ActionType)
					}).ToList();
				await Warn("Empty list returned", nameof(GetUsersFollowingList), accountId, instagramAccountId); 
				return EmptyList<LookupContainer<UserResponse<string>>>();
			}, nameof(GetUsersFollowingList), accountId, instagramAccountId);
		}
		public async Task<IEnumerable<LookupContainer<UserResponse<string>>>> GetUsersFollowerList(SString accountId, SString instagramAccountId, SString topic)
		{
			return await RunCodeWithLoggerExceptionAsync(async () =>
			{
				var users = await _heartbeatLogic.GetMetaData<List<UserResponse<string>>>(MetaDataType.FetchUsersFollowerList, topic, instagramAccountId);
				if (users != null) return users.Select(x => x.ObjectItem).SquashMe()
					.Select(x=>new LookupContainer<UserResponse<string>>
					{
						Object = x, 
						Lookup = _lookupLogic.Get(accountId, instagramAccountId, 
							x.UserId.ToString()).GetAwaiter().GetResult()
							.OrderByDescending(d=>d.LastModified)
							.DistinctBy(y=>y.ActionType)
					}).ToList();
				await Warn("Returned empty list", nameof(GetUsersFollowerList), accountId, instagramAccountId); 
				return EmptyList<LookupContainer<UserResponse<string>>>();
			}, nameof(GetUsersFollowerList), accountId, instagramAccountId);
		}
		public async Task<IEnumerable<LookupContainer<UserResponse<UserSuggestionDetails>>>> GetUsersSuggestedFollowingList(SString accountId, SString instagramAccountId, SString topic)
		{
			return await RunCodeWithLoggerExceptionAsync(async () =>
			{
				var users = await _heartbeatLogic.GetMetaData<List<UserResponse<UserSuggestionDetails>>>(MetaDataType.FetchUsersFollowSuggestions, topic, instagramAccountId);
				if (users != null) return users.Select(x => x.ObjectItem).SquashMe()
					.Select(x=>new LookupContainer<UserResponse<UserSuggestionDetails>>
					{
						Object = x, 
						Lookup = _lookupLogic.Get(accountId, instagramAccountId, 
							x.UserId.ToString()).GetAwaiter().GetResult()
							.OrderByDescending(d=>d.LastModified)
							.DistinctBy(y=>y.ActionType)
					}).ToList();
				await Warn("Returned empty list", nameof(GetUsersSuggestedFollowingList), accountId, instagramAccountId); 
				return EmptyList<LookupContainer<UserResponse<UserSuggestionDetails>>>();
			}, nameof(GetUsersSuggestedFollowingList), accountId, instagramAccountId);
		}
		#endregion
	}
}
