using InstagramApiSharp;
using InstagramApiSharp.Classes.Models;
using Newtonsoft.Json;
using QuarklessContexts.Extensions;
using QuarklessContexts.Models.Profiles;
using QuarklessContexts.Models.Proxies;
using QuarklessContexts.Models.ResponseModels;
using QuarklessContexts.Models.ServicesModels.SearchModels;
using QuarklessContexts.Models.Topics;
using QuarklessLogic.ContentSearch;
using QuarklessLogic.Handlers.ClientProvider;
using QuarklessLogic.Handlers.RestSharpClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using QuarklessLogic.Logic.ResponseLogic;

namespace QuarklessLogic.ServicesLogic.ContentSearch
{
	public class ContentSearcherHandler : IContentSearcherHandler
	{
		private IAPIClientContainer _container;
		private readonly IResponseResolver _responseResolver;
		private readonly IRestSharpClientManager _restSharpClient;
		private readonly YandexImageSearch _yandexImageSearch;
		public ContentSearcherHandler(IAPIClientContainer worker, IResponseResolver responseResolver, ProxyModel proxy = null)
		{
			_container = worker;
			_responseResolver = responseResolver;
			_restSharpClient = new RestSharpClientManager();
			_restSharpClient.AddProxy(proxy);
			_yandexImageSearch = new YandexImageSearch(_restSharpClient);
		}
		public void ChangeUser(IAPIClientContainer newUser)
		{
			_container = newUser;
		}
		public async Task<IEnumerable<TopicCategories>> GetBusinessCategories()
		{
			var cat = await _responseResolver.WithClient(_container).WithResolverAsync(await _container.Business.GetCategoriesAsync());
			var categories = new List<TopicCategories>();
			if (!cat.Succeeded) return categories;
			foreach(var ca in cat.Value)
			{
				var subCategories = await _container.Business.GetSubCategoriesAsync(ca.CategoryId);
				if (subCategories.Succeeded)
				{
					categories.Add(new TopicCategories
					{
						CategoryName = ca.CategoryName,
						SubCategories = subCategories.Value.Select(s=>s.CategoryName).ToList()
					});
				}
				await Task.Delay(1000);
			}
			return categories;
		}
		public async Task<IEnumerable<UserResponse<string>>> GetUserFollowingList(string username, int limit, string query = null)
		{
			var userResponse = await _responseResolver.WithClient(_container).WithResolverAsync
				(await _container.User.GetUserFollowingAsync(username, 
				PaginationParameters.MaxPagesToLoad(limit), query, 
				InstagramApiSharp.Enums.InstaFollowingOrderType.DateFollowedEarliest));
			if (!userResponse.Succeeded) return null;
			var users = userResponse.Value;
			return users.Select(_ => new UserResponse<string>
			{
				Username = _.UserName,
				FullName = _.FullName,
				ProfilePicture = _.ProfilePicture,
				UserId = _.Pk,
				IsPrivate = _.IsPrivate,
				IsVerified = _.IsVerified,
			});
		}
		public async Task<List<UserResponse<UserSuggestionDetails>>> GetSuggestedPeopleToFollow(int limit)
		{
			var res = await _responseResolver.WithClient(_container).WithResolverAsync
				(await _container.User.GetSuggestionUsersAsync(PaginationParameters.MaxPagesToLoad(limit)));
			if (!res.Succeeded) return null;
			var usersTotals = new List<UserResponse<UserSuggestionDetails>>();
			var users = res.Value;
			usersTotals.AddRange(users.NewSuggestedUsers.Select(_ => new UserResponse<UserSuggestionDetails>
			{
				Object = new UserSuggestionDetails
				{
					IsNewSuggestions = _.IsNewSuggestion,
					Caption = _.Caption,
					FollowText = _.FollowText,
					Algorithm = _.Algorithm,
					Value = _.Value
				},
				UserId = _.User.Pk,
				Username = _.User.UserName,
				FullName = _.User.FullName,
				ProfilePicture = _.User.ProfilePicture,
				IsPrivate = _.User.IsPrivate,
				IsVerified = _.User.IsVerified
			}));
			usersTotals.AddRange(users.SuggestedUsers.Select(_ => new UserResponse<UserSuggestionDetails>
			{
				Object = new UserSuggestionDetails
				{
					IsNewSuggestions = _.IsNewSuggestion,
					Caption = _.Caption,
					FollowText = _.FollowText,
					Algorithm = _.Algorithm,
					Value = _.Value
				},
				UserId = _.User.Pk,
				Username = _.User.UserName,
				FullName = _.User.FullName,
				ProfilePicture = _.User.ProfilePicture,
				IsPrivate = _.User.IsPrivate,
				IsVerified = _.User.IsVerified
			}));
			return usersTotals;
		}
		public async Task<List<UserResponse<string>>> SearchInstagramMediaLikers(string mediaId)
		{
			var userLikerResult = await _responseResolver.WithClient(_container).WithResolverAsync
				(await _container.Media.GetMediaLikersAsync(mediaId));
			if (userLikerResult.Succeeded)
			{
				return userLikerResult.Value.Select(s => new UserResponse<string>
				{
					Object = mediaId,
					UserId = s.Pk,
					Username = s.UserName,
					FullName = s.FullName,
					IsPrivate = s.IsPrivate,
					IsVerified = s.IsVerified,
					ProfilePicture = s.ProfilePicture + "||" + s.ProfilePictureId
				}).ToList();
			}

			return null;
		}
		public async Task<List<UserResponse<InstaComment>>> SearchInstagramMediaCommenters(string mediaId, int limit)
		{
			try { 
				var userCommentResult = await _responseResolver.WithClient(_container).WithResolverAsync
					(await _container.Comment.GetMediaCommentsAsync(mediaId, PaginationParameters.MaxPagesToLoad(limit)));
				if (!userCommentResult.Succeeded) return null;
				var results = userCommentResult.Value.Comments;
				return results.Select(res => new UserResponse<InstaComment>
				{
					Object = res,
					MediaId = mediaId,
					FullName = res.User.FullName,
					IsPrivate = res.User.IsPrivate,
					IsVerified = res.User.IsVerified,
					ProfilePicture = res?.User?.ProfilePicture,
					UserId = res.User.Pk,
					Username = res.User.UserName
				})
				.ToList();
			}
			catch(Exception ee)
			{
				Console.WriteLine(ee.Message);
				return null;
			}
		}	
		public async Task<InstaFullUserInfo> SearchInstagramFullUserDetail(long userId)
		{
			var userDetailsResp = await _responseResolver.WithClient(_container).WithResolverAsync
				(await _container.User.GetFullUserInfoAsync(userId));
			return userDetailsResp.Succeeded ? userDetailsResp.Value : null;
		}

		public async Task<Media> SearchMediaDetailInstagram(List<string> topics, int limit, bool isRecent = false)
		{
			var medias = new Media();
			foreach (var topic in topics)
			{
				var mediasResults = !isRecent ? await _container.Hashtag.GetTopHashtagMediaListAsync(topic, PaginationParameters.MaxPagesToLoad(limit))
											: await _container.Hashtag.GetRecentHashtagMediaListAsync(topic,PaginationParameters.MaxPagesToLoad(limit));
				if (mediasResults.Succeeded)
				{
					medias.Medias.AddRange(mediasResults.Value.Medias.Select(s =>
					{
						MediaResponse mediaDetail = new MediaResponse();
						mediaDetail.LikesCount = s.LikesCount;
						mediaDetail.MediaId = s.Pk;
						mediaDetail.HasLikedBefore = s.HasLiked;
						mediaDetail.HasAudio = s.HasAudio;
						mediaDetail.IsCommentsDisabled = s.IsCommentsDisabled;
						mediaDetail.Location = s?.Location;
						mediaDetail.ViewCount = s.ViewCount;
						mediaDetail.Caption = s?.Caption?.Text;
						mediaDetail.Explore = s?.Explore;
						mediaDetail.FilterType = s.FilterType;
						mediaDetail.HasSeen = s.IsSeen;
						mediaDetail.NumberOfQualities = s.NumberOfQualities;
						mediaDetail.PhotosOfI = s.PhotoOfYou;
						mediaDetail.PreviewComments = s?.PreviewComments;
						mediaDetail.ProductTags = s?.ProductTags;
						mediaDetail.ProductType = s.ProductType;
						mediaDetail.TopLikers = s?.TopLikers;
						mediaDetail.TakenAt = s.TakenAt;
						mediaDetail.UserTags = s?.UserTags;
						mediaDetail.IsFollowing = s?.User?.FriendshipStatus?.Following;
						mediaDetail.CommentCount = s.CommentsCount;
						mediaDetail.Topic = topic;
						mediaDetail.MediaFrom = MediaFrom.Instagram;
						mediaDetail.MediaType = s.MediaType;
						mediaDetail.User = new UserResponse
						{
							FullName = s.User.FullName,
							IsPrivate = s.User.IsPrivate,
							IsVerified = s.User.IsVerified,
							ProfilePicture = s.User.ProfilePicture,
							Topic = topic,
							UserId = s.User.Pk,
							Username = s.User.UserName
						};
						var totalurls = new List<string>();
						if (s.MediaType == InstaMediaType.Image)
						{
							var im = s.Images.FirstOrDefault()?.Uri;
							if (im != null)
								totalurls.Add(im);
						}
						else if (s.MediaType == InstaMediaType.Video)
						{
							var iv = s.Videos.FirstOrDefault()?.Uri;
							if (iv != null)
								totalurls.Add(iv);
						}
						else if (s.MediaType == InstaMediaType.Carousel)
						{
							s.Carousel.ForEach(x =>
							{
								var videos = x.Videos.FirstOrDefault()?.Uri;
								if (videos != null)
									totalurls.Add(videos);
								var images = x.Images.FirstOrDefault()?.Uri;
								if (images != null)
									totalurls.Add(images);
							});
						}
						mediaDetail.MediaUrl = totalurls;
						return mediaDetail;
					}));
				}
			}
			return medias;
		}
		public async Task<Media> SearchUsersMediaDetailInstagram(string userName, int limit)
		{
			var medias = await _responseResolver.WithClient(_container).WithResolverAsync
				(await _container.User.GetUserMediaAsync(userName, PaginationParameters.MaxPagesToLoad(limit)));
			if (medias.Succeeded)
			{
				return new Media
				{
					Medias = medias.Value.Select(s =>
					{
						MediaResponse mediaDetail = new MediaResponse();
						mediaDetail.LikesCount = s.LikesCount;
						mediaDetail.MediaId = s.Pk;
						mediaDetail.HasLikedBefore = s.HasLiked;
						mediaDetail.HasAudio = s.HasAudio;
						mediaDetail.IsCommentsDisabled = s.IsCommentsDisabled;
						mediaDetail.Location = s?.Location;
						mediaDetail.ViewCount = s.ViewCount;
						mediaDetail.Caption = s?.Caption?.Text;
						mediaDetail.Explore = s?.Explore;
						mediaDetail.FilterType = s.FilterType;
						mediaDetail.HasSeen = s.IsSeen;
						mediaDetail.NumberOfQualities = s.NumberOfQualities;
						mediaDetail.PhotosOfI = s.PhotoOfYou;
						mediaDetail.PreviewComments = s?.PreviewComments;
						mediaDetail.ProductTags = s?.ProductTags;
						mediaDetail.ProductType = s.ProductType;
						mediaDetail.TopLikers = s?.TopLikers;
						mediaDetail.TakenAt = s.TakenAt;
						mediaDetail.UserTags = s?.UserTags;
						mediaDetail.IsFollowing = s?.User?.FriendshipStatus?.Following;
						mediaDetail.CommentCount = s.CommentsCount;
						mediaDetail.MediaFrom = MediaFrom.Instagram;
						mediaDetail.MediaType = s.MediaType;
						mediaDetail.User = new UserResponse
						{
							FullName = s.User.FullName,
							IsPrivate = s.User.IsPrivate,
							IsVerified = s.User.IsVerified,
							ProfilePicture = s.User.ProfilePicture,
							UserId = s.User.Pk,
							Username = s.User.UserName
						};
						var totalurls = new List<string>();
						if (s.MediaType == InstaMediaType.Image)
						{
							var im = s.Images.FirstOrDefault()?.Uri;
							if(im!=null)
								totalurls.Add(im);
						}
						else if (s.MediaType == InstaMediaType.Video)
						{
							var iv = s.Videos.FirstOrDefault()?.Uri;
							if(iv!=null)
								totalurls.Add(iv);
						}
						else if (s.MediaType == InstaMediaType.Carousel)
						{
							s.Carousel.ForEach(x =>
							{
								var videos = x.Videos.FirstOrDefault()?.Uri;
								if (videos != null)
									totalurls.Add(videos);
								var images = x.Images.FirstOrDefault()?.Uri;
								if (images != null)
									totalurls.Add(images);
							});
						}
						mediaDetail.MediaUrl = totalurls;
						return mediaDetail;
					}).ToList()
				};
			}
			return null;
		}
		public async Task<Media> SearchTopLocationMediaDetailInstagram(Location location, int limit)
		{
			var locationResult = await _responseResolver.WithClient(_container).WithResolverAsync
				(await _container.Location.SearchPlacesAsync(location.City, PaginationParameters.MaxPagesToLoad(limit)));
			if (!locationResult.Succeeded) return null;
			var id = locationResult.Value.Items.ElementAt(SecureRandom.Next(locationResult.Value.Items.Count - 1)).Location.Pk;
			var mediasRes = await _container.Location.GetTopLocationFeedsAsync(id,PaginationParameters.MaxPagesToLoad(limit));
			if (mediasRes.Succeeded)
			{
				return new Media
				{
					Medias = mediasRes.Value.Medias.Select(s =>
					{
						MediaResponse mediaDetail = new MediaResponse();
						mediaDetail.LikesCount = s.LikesCount;
						mediaDetail.MediaId = s.Pk;
						mediaDetail.HasLikedBefore = s.HasLiked;
						mediaDetail.HasAudio = s.HasAudio;
						mediaDetail.IsCommentsDisabled = s.IsCommentsDisabled;
						mediaDetail.Location = s?.Location;
						mediaDetail.ViewCount = s.ViewCount;
						mediaDetail.Caption = s?.Caption?.Text;
						mediaDetail.Explore = s?.Explore;
						mediaDetail.FilterType = s.FilterType;
						mediaDetail.HasSeen = s.IsSeen;
						mediaDetail.NumberOfQualities = s.NumberOfQualities;
						mediaDetail.PhotosOfI = s.PhotoOfYou;
						mediaDetail.PreviewComments = s?.PreviewComments;
						mediaDetail.ProductTags = s?.ProductTags;
						mediaDetail.ProductType = s.ProductType;
						mediaDetail.TopLikers = s?.TopLikers;
						mediaDetail.TakenAt = s.TakenAt;
						mediaDetail.UserTags = s?.UserTags;
						mediaDetail.IsFollowing = s?.User?.FriendshipStatus?.Following;
						mediaDetail.CommentCount = s.CommentsCount;
						mediaDetail.MediaFrom = MediaFrom.Instagram;
						mediaDetail.MediaType = s.MediaType;
						mediaDetail.User = new UserResponse
						{
							FullName = s.User.FullName,
							IsPrivate = s.User.IsPrivate,
							IsVerified = s.User.IsVerified,
							ProfilePicture = s.User.ProfilePicture,
							UserId = s.User.Pk,
							Username = s.User.UserName
						};
						var totalurls = new List<string>();
						if (s.MediaType == InstaMediaType.Image)
						{
							var im = s.Images.FirstOrDefault()?.Uri;
							if (im != null)
								totalurls.Add(im);
						}
						else if (s.MediaType == InstaMediaType.Video)
						{
							var iv = s.Videos.FirstOrDefault()?.Uri;
							if (iv != null)
								totalurls.Add(iv);
						}
						else if (s.MediaType == InstaMediaType.Carousel)
						{
							s.Carousel.ForEach(x =>
							{
								var videos = x.Videos.FirstOrDefault()?.Uri;
								if (videos != null)
									totalurls.Add(videos);
								var images = x.Images.FirstOrDefault()?.Uri;
								if (images != null)
									totalurls.Add(images);
							});
						}
						mediaDetail.MediaUrl = totalurls;
						return mediaDetail;
					}).ToList()
				};
			}
			return null;
		}
		public async Task<Media> SearchRecentLocationMediaDetailInstagram(Location location, int limit)
		{
			var locationResult = await _responseResolver.WithClient(_container).WithResolverAsync
				(await _container.Location.SearchPlacesAsync(location.Coordinates.Latitude, 
				location.Coordinates.Longitude, PaginationParameters.MaxPagesToLoad(limit)));
			if (!locationResult.Succeeded) return null;
			var id = locationResult.Value.Items.ElementAt(SecureRandom.Next(locationResult.Value.Items.Count - 1)).Location.Pk;
			var mediasRes = await _container.Location.GetRecentLocationFeedsAsync(id, PaginationParameters.MaxPagesToLoad(limit));
			if (mediasRes.Succeeded)
			{
				return new Media
				{
					Medias = mediasRes.Value.Medias.Select(s =>
					{
						MediaResponse mediaDetail = new MediaResponse();
						mediaDetail.LikesCount = s.LikesCount;
						mediaDetail.MediaId = s.Pk;
						mediaDetail.HasLikedBefore = s.HasLiked;
						mediaDetail.HasAudio = s.HasAudio;
						mediaDetail.IsCommentsDisabled = s.IsCommentsDisabled;
						mediaDetail.Location = s?.Location;
						mediaDetail.ViewCount = s.ViewCount;
						mediaDetail.Caption = s?.Caption?.Text;
						mediaDetail.Explore = s?.Explore;
						mediaDetail.FilterType = s.FilterType;
						mediaDetail.HasSeen = s.IsSeen;
						mediaDetail.NumberOfQualities = s.NumberOfQualities;
						mediaDetail.PhotosOfI = s.PhotoOfYou;
						mediaDetail.PreviewComments = s?.PreviewComments;
						mediaDetail.ProductTags = s?.ProductTags;
						mediaDetail.ProductType = s.ProductType;
						mediaDetail.TopLikers = s?.TopLikers;
						mediaDetail.TakenAt = s.TakenAt;
						mediaDetail.UserTags = s?.UserTags;
						mediaDetail.IsFollowing = s?.User?.FriendshipStatus?.Following;
						mediaDetail.CommentCount = s.CommentsCount;
						mediaDetail.MediaFrom = MediaFrom.Instagram;
						mediaDetail.MediaType = s.MediaType;
						mediaDetail.User = new UserResponse
						{
							FullName = s.User.FullName,
							IsPrivate = s.User.IsPrivate,
							IsVerified = s.User.IsVerified,
							ProfilePicture = s.User.ProfilePicture,
							UserId = s.User.Pk,
							Username = s.User.UserName
						};
						var totalurls = new List<string>();
						if (s.MediaType == InstaMediaType.Image)
						{
							var im = s.Images.FirstOrDefault()?.Uri;
							if (im != null)
								totalurls.Add(im);
						}
						else if (s.MediaType == InstaMediaType.Video)
						{
							var iv = s.Videos.FirstOrDefault()?.Uri;
							if (iv != null)
								totalurls.Add(iv);
						}
						else if (s.MediaType == InstaMediaType.Carousel)
						{
							s.Carousel.ForEach(x =>
							{
								var videos = x.Videos.FirstOrDefault()?.Uri;
								if (videos != null)
									totalurls.Add(videos);
								var images = x.Images.FirstOrDefault()?.Uri;
								if (images != null)
									totalurls.Add(images);
							});
						}
						mediaDetail.MediaUrl = totalurls;
						return mediaDetail;
					}).ToList()
				};
			}
			return null;
		}
		public async Task<Media> SearchUserFeedMediaDetailInstagram(string[] seenMedias = null, bool requestRefresh = false, int limit = 1)
		{
			Media medias = new Media();
			var mediasResults = await _responseResolver.WithClient(_container).WithResolverAsync
				(await _container.Feeds.GetUserTimelineFeedAsync(PaginationParameters.MaxPagesToLoad(limit), seenMedias, requestRefresh));
			if (mediasResults.Succeeded)
			{
				medias.Medias.AddRange(mediasResults.Value.Medias.Select(s =>
				{
					MediaResponse mediaDetail = new MediaResponse();
					mediaDetail.LikesCount = s.LikesCount;
					mediaDetail.MediaId = s.Pk;
					mediaDetail.HasLikedBefore = s.HasLiked;
					mediaDetail.HasAudio = s.HasAudio;
					mediaDetail.IsCommentsDisabled = s.IsCommentsDisabled;
					mediaDetail.Location = s?.Location;
					mediaDetail.ViewCount = s.ViewCount;
					mediaDetail.Caption = s?.Caption?.Text;
					mediaDetail.Explore = s?.Explore;
					mediaDetail.FilterType = s.FilterType;
					mediaDetail.HasSeen = s.IsSeen;
					mediaDetail.NumberOfQualities = s.NumberOfQualities;
					mediaDetail.PhotosOfI = s.PhotoOfYou;
					mediaDetail.PreviewComments = s?.PreviewComments;
					mediaDetail.ProductTags = s?.ProductTags;
					mediaDetail.ProductType = s.ProductType;
					mediaDetail.TopLikers = s?.TopLikers;
					mediaDetail.TakenAt = s.TakenAt;
					mediaDetail.UserTags = s?.UserTags;
					mediaDetail.IsFollowing = s?.User?.FriendshipStatus?.Following;
					mediaDetail.CommentCount = s.CommentsCount;
					mediaDetail.MediaFrom = MediaFrom.Instagram;
					mediaDetail.MediaType = s.MediaType;
					mediaDetail.User = new UserResponse
					{
						FullName = s.User.FullName,
						IsPrivate = s.User.IsPrivate,
						IsVerified = s.User.IsVerified,
						ProfilePicture = s.User.ProfilePicture,
						UserId = s.User.Pk,
						Username = s.User.UserName
					};
					var totalurls = new List<string>();
					if (s.MediaType == InstaMediaType.Image)
					{
						var im = s.Images.FirstOrDefault()?.Uri;
						if(im!=null)
							totalurls.Add(im);
					}
					else if (s.MediaType == InstaMediaType.Video)
					{
						var iv = s.Videos.FirstOrDefault()?.Uri;
						if(iv!=null)
							totalurls.Add(iv);
					}
					else if (s.MediaType == InstaMediaType.Carousel)
					{
						s.Carousel.ForEach(x =>
						{
							var videos = x.Videos.FirstOrDefault()?.Uri;
							if (videos != null)
								totalurls.Add(videos);
							var images = x.Images.FirstOrDefault()?.Uri;
							if (images != null)
								totalurls.Add(images);
						});
					}
					mediaDetail.MediaUrl = totalurls;
					return mediaDetail;
				}));
			}
			return medias;
		}

		#region Probably Old 
		public async Task<Media> SearchMediaInstagram(List<string> topics, InstaMediaType mediaType, int limit)
		{
			Media mediaresp = new Media();
			foreach (var topic in topics)
			{
				MediaResponse media_ = new MediaResponse();
				var results = await _container.Hashtag.GetTopHashtagMediaListAsync(topic, PaginationParameters.MaxPagesToLoad(limit));
				if (results.Succeeded)
				{
					media_.Topic = topic;
					foreach (var results_media in results.Value.Medias)
					{
						switch (mediaType)
						{
							case InstaMediaType.All:
								var image = results_media.Images.FirstOrDefault()?.Uri;
								if (!string.IsNullOrEmpty(image))
									media_.MediaUrl.Add(image);
								var video = results_media.Videos.FirstOrDefault()?.Uri;
								if (!string.IsNullOrEmpty(video))
									media_.MediaUrl.Add(video);
								results_media.Carousel.Select(s =>
								{
									var videos = s.Videos.FirstOrDefault()?.Uri;
									if (!string.IsNullOrEmpty(videos))
										media_.MediaUrl.Add(videos);
									var images = s.Images.FirstOrDefault()?.Uri;
									if (!string.IsNullOrEmpty(images))
										media_.MediaUrl.Add(images);
									return s;
								});
								break;
							case InstaMediaType.Image:
								var image_ = results_media.Images.FirstOrDefault()?.Uri;
								if (!string.IsNullOrEmpty(image_))
									media_.MediaUrl.Add(image_);
								break;
							case InstaMediaType.Video:
								var video_ = results_media.Videos.FirstOrDefault()?.Uri;
								if (!string.IsNullOrEmpty(video_))
									media_.MediaUrl.Add(video_);
								break;
							case InstaMediaType.Carousel:
								results_media.Carousel.Select(s =>
								{
									var videos = s.Videos.FirstOrDefault()?.Uri;
									if (videos != null)
										media_.MediaUrl.Add(videos);
									var images = s.Images.FirstOrDefault()?.Uri;
									if (images != null)
										media_.MediaUrl.Add(images);
									return s;
								});
								break;
						}
					}
					mediaresp.Medias.Add(media_);
				}
			}
			return mediaresp;
		}
		public async Task<Media> SearchMediaUser(string username = null, int limit = 1)
		{
			Media mediaresp = new Media();
			username = username ?? _container.GetContext.InstagramAccount.Username;
			var results = await _container.User.GetUserMediaAsync(username, PaginationParameters.MaxPagesToLoad(limit));
			if (results.Succeeded)
			{
				MediaResponse media = new MediaResponse();
				foreach (var lema in results.Value)
				{
					switch (lema.MediaType)
					{
						case InstaMediaType.Image:
							media.MediaUrl.Add(lema.Images.FirstOrDefault().Uri);
							break;
						case InstaMediaType.Video:
							media.MediaUrl.Add(lema.Videos.FirstOrDefault().Uri);
							break;
						case InstaMediaType.Carousel:
							lema.Carousel.Select(s =>
							{
								var videos = s.Videos.FirstOrDefault().Uri;
								if (videos != null)
									media.MediaUrl.Add(videos);
								var images = s.Images.FirstOrDefault().Uri;
								if (images != null)
									media.MediaUrl.Add(images);
								return s;
							});
							break;
					}
				}
				mediaresp.Medias.Add(media);
			}
			return mediaresp;
		}
		#endregion
		public struct TempMedia
		{
			public struct Medias
			{
				public string Topic { get; set; }
				public string MediaUrl { get; set; }
			}
			public List<Medias> MediasObject;
			public int errors { get; set; }
		}
		public SearchResponse<Media> SearchViaGoogle(SearchImageModel searchImageQuery)
		{
			var response = new SearchResponse<Media>();
			try
			{ 
				var results = _restSharpClient.PostRequest("http://127.0.0.1:5000/", "searchImages", JsonConvert.SerializeObject(searchImageQuery), null);
				if (results == null)
				{
					response.StatusCode = ResponseCode.InternalServerError;
					return response;
				}
				if (results.IsSuccessful)
				{
					var responseValues = JsonConvert.DeserializeObject<TempMedia>(results.Content);
					if(responseValues.MediasObject.Count <= 0)
					{
						response.StatusCode = ResponseCode.InternalServerError;
						response.Message = $"Google search returned no results for object: {JsonConvert.SerializeObject(searchImageQuery)}";
						return response;
					}

					var casted = new Media{ Medias = responseValues.MediasObject.Select(s=>new MediaResponse
					{
						Topic = s.Topic,
						MediaFrom = MediaFrom.Google,
						MediaType = InstaMediaType.Image,
						MediaUrl = new List<string> {  s.MediaUrl }
					}).ToList()};
					response.StatusCode = ResponseCode.Success;
					response.Result = casted;
					return response;
				}
			}
			catch(Exception ee)
			{
				response.Message = ee.Message;
				response.StatusCode = ResponseCode.InternalServerError;
				return response;
			}
			response.StatusCode = ResponseCode.ReachedEndAndNull;
			response.Message = $"SearchViaGoogle failed for  object{JsonConvert.SerializeObject(searchImageQuery)}";
			return response;
		}
		public SearchResponse<Media> SearchSimilarImagesViaGoogle(List<GroupImagesAlike> imagesAlikes, int limit, int offset = 0)
		{
			SearchResponse<Media> response = new SearchResponse<Media>();
			try { 
				foreach(var images in imagesAlikes) { 
					SearchImageModel searchImage = new SearchImageModel
					{
						no_download = true,
						similar_images = images.Url,
						limit = limit,
						offset = offset < limit ? offset : 0
					};
					var res = _restSharpClient.PostRequest("http://127.0.0.1:5000","searchImages",JsonConvert.SerializeObject(searchImage));
					TempMedia responseValues = JsonConvert.DeserializeObject<TempMedia>(res.Content);
					if (responseValues.MediasObject.Count <= 0)
					{
						response.StatusCode = ResponseCode.InternalServerError;
						response.Message = $"Google search returned no results for object: {JsonConvert.SerializeObject(searchImage)}";
						return response;
					}

					response.Result.Medias.AddRange(responseValues.MediasObject.Select(s => new MediaResponse
					{
						Topic = images.TopicGroup,
						MediaFrom = MediaFrom.Google,
						MediaType = InstaMediaType.Image,
						MediaUrl = new List<string> { s.MediaUrl }
					}).ToList());
				}
				response.StatusCode = ResponseCode.Success;
				return response;
			}
			catch(Exception ee)
			{
				response.Message = ee.Message;
				response.StatusCode = ResponseCode.InternalServerError;
				return response;
			}
		}
		public SearchResponse<Media> SearchYandexSimilarSafeMode(List<GroupImagesAlike> imagesAlikes, int limit)
		{
			return _yandexImageSearch.SearchSafeButSlow(imagesAlikes,limit);
		}
		public SearchResponse<Media> SearchViaYandexBySimilarImages(List<GroupImagesAlike> imagesSimilarUrls, int limit, int offset = 0)
		{
			var images = _yandexImageSearch.SearchRelatedImagesREST(imagesSimilarUrls, limit, offset);
			return images;
		}
		public SearchResponse<Media> SearchViaYandex(YandexSearchQuery yandexSearchQuery, int limit)
		{
			return _yandexImageSearch.SearchQueryREST(yandexSearchQuery,limit);
		}
	}
}
