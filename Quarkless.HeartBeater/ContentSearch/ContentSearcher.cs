using ContentSearcher;
using InstagramApiSharp;
using InstagramApiSharp.Classes.Models;
using Newtonsoft.Json;
using QuarklessContexts.Models.Profiles;
using QuarklessContexts.Models.ServicesModels.SearchModels;
using QuarklessLogic.Handlers.ClientProvider;
using QuarklessLogic.Handlers.RestSharpClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Quarkless.HeartBeater.ContentSearch
{
	public class ContentSearcher : IContentSearcher
	{
		private IAPIClientContainer _container;
		private readonly IRestSharpClientManager _restSharpClient;
		private readonly YandexImageSearch _yandexImageSearch;
		public ContentSearcher(IRestSharpClientManager restSharpClient, IAPIClientContainer worker)
		{
			_restSharpClient = restSharpClient;
			_yandexImageSearch = new YandexImageSearch();
			_container = worker;
		}
		public void ChangeUser(IAPIClientContainer newUser)
		{
			_container = newUser;
		}
		public async Task<IEnumerable<UserResponse<string>>> GetUserFollowingList(string username, int limit, string query = null)
		{
			var usersresp = await _container.User.GetUserFollowingAsync(username, PaginationParameters.MaxPagesToLoad(limit), query);
			if (usersresp.Succeeded)
			{
				var users = usersresp.Value;
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
			return null;
		}
		public async Task<List<UserResponse<UserSuggestionDetails>>> GetSuggestedPeopleToFollow(int limit)
		{
			var res = await _container.User.GetSuggestionUsersAsync(PaginationParameters.MaxPagesToLoad(limit));
			if (res.Succeeded)
			{
				List<UserResponse<UserSuggestionDetails>> usersTotals = new List<UserResponse<UserSuggestionDetails>>();
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
			return null;
		}
		public async Task<List<UserResponse<string>>> SearchInstagramMediaLikers(string mediaId)
		{
			var userLikersRes = await _container.Media.GetMediaLikersAsync(mediaId);
			if (userLikersRes.Succeeded)
			{
				return userLikersRes.Value.Select(s => new UserResponse<string>
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
				var userCommentersRes = await _container.Comment.GetMediaCommentsAsync(mediaId, PaginationParameters.MaxPagesToLoad(limit));
				if (userCommentersRes.Succeeded)
				{
					var results = userCommentersRes.Value.Comments;
					List<UserResponse<InstaComment>> commentResp = new List<UserResponse<InstaComment>>();
					foreach(var res in results)
					{
						commentResp.Add(new UserResponse<InstaComment>
						{
							Object = res,
							MediaId = mediaId,
							FullName = res.User.FullName,
							IsPrivate = res.User.IsPrivate,
							IsVerified = res.User.IsVerified,
							ProfilePicture = res?.User?.ProfilePicture,
							UserId = res.User.Pk,
							Username = res.User.UserName
						});
					}
					return commentResp;
				}
				return null;
			}
			catch(Exception ee)
			{
				Console.WriteLine(ee.Message);
				return null;
			}
		}	
		public async Task<InstaFullUserInfo> SearchInstagramFullUserDetail(long userId)
		{
			var userDetailsResp = await _container.User.GetFullUserInfoAsync(userId);
			if (userDetailsResp.Succeeded)
			{
				return userDetailsResp.Value;
			}
			return null;
		}
		public async Task<Media> SearchMediaDetailInstagram(List<string> topics, int limit)
		{
			Media medias = new Media();
			foreach (var topic in topics)
			{
				var mediasResults = await _container.Hashtag.GetTopHashtagMediaListAsync(topic, PaginationParameters.MaxPagesToLoad(limit));
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
							totalurls.Add(s.Images.FirstOrDefault().Uri);
						}
						else if (s.MediaType == InstaMediaType.Video)
						{
							totalurls.Add(s.Videos.FirstOrDefault().Uri);
						}
						else if (s.MediaType == InstaMediaType.Carousel)
						{
							s.Carousel.Select(x =>
							{
								var videos = x.Videos.FirstOrDefault().Uri;
								if (videos != null)
									totalurls.Add(videos);
								var images = x.Images.FirstOrDefault().Uri;
								if (images != null)
									totalurls.Add(images);
								return s;
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
			var medias = await _container.User.GetUserMediaAsync(userName, PaginationParameters.MaxPagesToLoad(limit));
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
							totalurls.Add(s.Images.FirstOrDefault().Uri);
						}
						else if (s.MediaType == InstaMediaType.Video)
						{
							totalurls.Add(s.Videos.FirstOrDefault().Uri);
						}
						else if (s.MediaType == InstaMediaType.Carousel)
						{
							s.Carousel.Select(x =>
							{
								var videos = x.Videos.FirstOrDefault().Uri;
								if (videos != null)
									totalurls.Add(videos);
								var images = x.Images.FirstOrDefault().Uri;
								if (images != null)
									totalurls.Add(images);
								return s;
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
			var mediasResults = await _container.Feeds.GetUserTimelineFeedAsync(PaginationParameters.MaxPagesToLoad(limit), seenMedias, requestRefresh);
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
						totalurls.Add(s.Images.FirstOrDefault().Uri);
					}
					else if (s.MediaType == InstaMediaType.Video)
					{
						totalurls.Add(s.Videos.FirstOrDefault().Uri);
					}
					else if (s.MediaType == InstaMediaType.Carousel)
					{
						s.Carousel.Select(x =>
						{
							var videos = x.Videos.FirstOrDefault().Uri;
							if (videos != null)
								totalurls.Add(videos);
							var images = x.Images.FirstOrDefault().Uri;
							if (images != null)
								totalurls.Add(images);
							return s;
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
		public Media SearchViaGoogle(SearchImageModel searchImageQuery)
		{
			var results = _restSharpClient.PostRequest("http://127.0.0.1:5000/", "searchImages", JsonConvert.SerializeObject(searchImageQuery), null);
			if (results.IsSuccessful)
			{
				TempMedia responseValues = JsonConvert.DeserializeObject<TempMedia>(results.Content);
				var casted = new Media{ Medias = responseValues.MediasObject.Select(s=>new MediaResponse
					{
						Topic = s.Topic,
						MediaFrom = MediaFrom.Google,
						MediaType = InstaMediaType.Image,
						MediaUrl = new List<string> {  s.MediaUrl }
					}).ToList()};
				return casted;
			}
			return null;
		}
		public Media SearchSimilarImagesViaGoogle(List<GroupImagesAlike> imagesAlikes, int limit)
		{
			Media medias = new Media();
			foreach(var images in imagesAlikes) { 
				SearchImageModel searchImage = new SearchImageModel
				{
					no_download = true,
					similar_images = images.Url,
					limit = limit,
				};
				var res = _restSharpClient.PostRequest("http://127.0.0.1:5000","searchImages",JsonConvert.SerializeObject(searchImage));
				TempMedia responseValues = JsonConvert.DeserializeObject<TempMedia>(res.Content);
				medias.Medias.AddRange(responseValues.MediasObject.Select(s => new MediaResponse
				{
					Topic = images.TopicGroup,
					MediaFrom = MediaFrom.Google,
					MediaType = InstaMediaType.Image,
					MediaUrl = new List<string> { s.MediaUrl }
				}).ToList());
			}
			return medias;
		}
		public Media SearchViaYandexBySimilarImages(List<GroupImagesAlike> imagesSimilarUrls, int limit)
		{
			var images = _yandexImageSearch.Search(imagesSimilarUrls, limit);
			return images;
		}
		public Media SearchViaYandex(YandexSearchQuery yandexSearchQuery, int limit)
		{
			return _yandexImageSearch.SearchQueryREST(yandexSearchQuery,limit);
		}
	}
}
