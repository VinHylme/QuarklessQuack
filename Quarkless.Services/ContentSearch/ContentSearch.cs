using InstagramApiSharp;
using InstagramApiSharp.Classes.Models;
using Newtonsoft.Json;
using QuarklessContexts.Models.ServicesModels.SearchModels;
using QuarklessLogic.Handlers.ClientProvider;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using QuarklessContexts.Models.Profiles;
using QuarklessLogic.RestSharpClient;
using QuarklessContexts.Models.Timeline;
using ContentSearcher;
using QuarklessContexts.Extensions;
using QuarklessContexts.Models;

namespace Quarkless.Services.ContentSearch
{
	public class ContentSearch : IContentSearch
	{
		private readonly IRestSharpClientManager _restSharpClient;
		private readonly IAPIClientContext _context;
		private IAPIClientContainer _container { get; set; }
		private readonly YandexImageSearch _yandexImageSearch;
		public ContextContainer SetUserClient(IUserStoreDetails _user)
		{
			_container = new APIClientContainer(_context, _user.OAccountId, _user.OInstagramAccountUser);
			return _container.GetContext;
		}
		public async Task<IEnumerable<UserResponse<string>>> GetUserFollowingList(string username, int limit, string query = null)
		{
			var usersresp = await _container.User.GetUserFollowingAsync(username,PaginationParameters.MaxPagesToLoad(limit),query);
			if (usersresp.Succeeded)
			{
				var users = usersresp.Value;
				return users.Select(_=>new UserResponse<string>
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
		public ContentSearch(IRestSharpClientManager restSharpClient, IAPIClientContext clientContext)
		{
			_restSharpClient = restSharpClient;
			_context = clientContext;
			_yandexImageSearch = new YandexImageSearch();
		}
		public async Task<List<UserResponse<UserSuggestionDetails>>> GetSuggestedPeopleToFollow(int limit)
		{
			var res = await _container.User.GetSuggestionUsersAsync(PaginationParameters.MaxPagesToLoad(limit));
			if (res.Succeeded)
			{
				List<UserResponse<UserSuggestionDetails>> usersTotals = new List<UserResponse<UserSuggestionDetails>>();
				var users = res.Value;
				usersTotals.AddRange(users.NewSuggestedUsers.Select(_=>new UserResponse<UserSuggestionDetails>
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
				return userLikersRes.Value.Select(s => new UserResponse<string>{ 
					Object = mediaId,
					UserId = s.Pk,
					Username = s.UserName,
					FullName = s.FullName,
					IsPrivate = s.IsPrivate,
					IsVerified = s.IsVerified,
					ProfilePicture = s.ProfilePicture +"||"+ s.ProfilePictureId
				}).ToList();
			}
			return null;
		}
		public async Task<List<UserResponse<CommentResponse>>> SearchInstagramMediaCommenters(string mediaId, int limit)
		{
			var userCommentersRes = await _container.Comment.GetMediaCommentsAsync(mediaId, PaginationParameters.MaxPagesToLoad(limit));
			if (userCommentersRes.Succeeded)
			{
				return userCommentersRes.Value.Comments.Select(s => new UserResponse<CommentResponse>
				{
					Object = new CommentResponse
					{
						CommentId = s.Pk,
						LikeCount = s.LikesCount,
						TotalChildComments = s.ChildCommentCount,
						DidReportForSpam = s.DidReportAsSpam,
						Text = s.Text
					},
					UserId = s.User.Pk,
					Username = s.User.UserName,
					FullName = s.User.FullName,
					IsPrivate = s.User.IsPrivate,
					IsVerified = s.User.IsVerified,
					ProfilePicture = s.User.ProfilePicture + "||" + s.User.ProfilePictureId
				}).ToList();
			}
			return null;
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
		public async Task<IEnumerable<UserResponse<MediaDetail>>> SearchMediaDetailInstagram(List<string> topics, int limit)
		{
			List<UserResponse<MediaDetail>> medias = new List<UserResponse<MediaDetail>>();
			foreach(var topic in topics) { 
				var mediasResults = await _container.Hashtag.GetTopHashtagMediaListAsync(topic,PaginationParameters.MaxPagesToLoad(limit));
				if (mediasResults.Succeeded)
				{
					medias.AddRange(mediasResults.Value.Medias.Select(s=> 
					{
						MediaDetail mediaDetail = new MediaDetail();
						mediaDetail.LikesCount = s.LikesCount;
						mediaDetail.MediaId = s.Pk;
						mediaDetail.HasLikedBefore = s.HasLiked;
						mediaDetail.HasAudio = s.HasAudio;
						mediaDetail.IsCommentsDisabled = s.IsCommentsDisabled;
						mediaDetail.Location = s.Location;
						mediaDetail.ViewCount = s.ViewCount;
						mediaDetail.IsFollowing = s?.User?.FriendshipStatus?.Following;
						mediaDetail.CommentCount = s.CommentsCount;
						mediaDetail.Topic = topic;
						var totalurls = new List<string>();
						if(s.MediaType == InstaMediaType.Image)
						{
							totalurls.Add(s.Images.FirstOrDefault().Uri);
						}
						else if(s.MediaType == InstaMediaType.Video)
						{
							totalurls.Add(s.Videos.FirstOrDefault().Uri);
						}
						else if(s.MediaType == InstaMediaType.Carousel)
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

						return new UserResponse<MediaDetail>
						{
							Object = mediaDetail,
							FollowerCount = s.User.FollowersCount,
							FullName = s.User.FullName,
							IsPrivate = s.User.IsPrivate,
							IsVerified = s.User.IsVerified,
							Topic = topic,
							UserId = s.User.Pk,
							Username = s.User.UserName
						}; ;
					}));
				}
			}
			return medias;
		}
		public async Task<IEnumerable<UserResponse<MediaDetail>>> SearchUsersMediaDetailInstagram(string userName, int limit)
		{
			var medias = await _container.User.GetUserMediaAsync(userName,PaginationParameters.MaxPagesToLoad(limit));
			if (medias.Succeeded)
			{
				return medias.Value.Select(s=> {
					MediaDetail mediaDetail = new MediaDetail();
					mediaDetail.LikesCount = s.LikesCount;
					mediaDetail.MediaId = s.Pk;
					mediaDetail.HasLikedBefore = s.HasLiked;
					mediaDetail.HasAudio = s.HasAudio;
					mediaDetail.IsCommentsDisabled = s.IsCommentsDisabled;
					mediaDetail.Location = s.Location;
					mediaDetail.ViewCount = s.ViewCount;
					mediaDetail.IsFollowing = s?.User?.FriendshipStatus?.Following;
					mediaDetail.CommentCount = s.CommentsCount;
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

					return new UserResponse<MediaDetail>
					{
						Object = mediaDetail,
						FollowerCount = s.User.FollowersCount,
						FullName = s.User.FullName,
						IsPrivate = s.User.IsPrivate,
						IsVerified = s.User.IsVerified,
						UserId = s.User.Pk,
						Username = s.User.UserName
					}; ;
				});
			}
			return null;
		}
		public async Task<IEnumerable<UserResponse<MediaDetail>>> SearchUserFeedMediaDetailInstagram(string[] seenMedias = null, bool requestRefresh = false, int limit = 1)
		{
			List<UserResponse<MediaDetail>> medias = new List<UserResponse<MediaDetail>>();
			var mediasResults = await _container.Feeds.GetUserTimelineFeedAsync(PaginationParameters.MaxPagesToLoad(limit),seenMedias,requestRefresh);
			if (mediasResults.Succeeded)
			{
				medias.AddRange(mediasResults.Value.Medias.Select(s =>
				{
					MediaDetail mediaDetail = new MediaDetail();
					mediaDetail.LikesCount = s.LikesCount;
					mediaDetail.MediaId = s.Pk;
					mediaDetail.HasLikedBefore = s.HasLiked;
					mediaDetail.HasAudio = s.HasAudio;
					mediaDetail.IsCommentsDisabled = s.IsCommentsDisabled;
					mediaDetail.Location = s.Location;
					mediaDetail.ViewCount = s.ViewCount;
					mediaDetail.IsFollowing = s?.User?.FriendshipStatus?.Following;
					mediaDetail.CommentCount = s.CommentsCount;
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

					return new UserResponse<MediaDetail>
					{
						Object = mediaDetail,
						FollowerCount = s.User.FollowersCount,
						FullName = s.User.FullName,
						IsPrivate = s.User.IsPrivate,
						IsVerified = s.User.IsVerified,
						UserId = s.User.Pk,
						Username = s.User.UserName
					}; ;
				}));
			}
		return medias;
		}
		public async Task<Media> SearchMediaInstagram(List<string> topics, InstaMediaType mediaType, int limit)
		{
			Media mediaresp = new Media();
			foreach(var topic in topics) { 
				MediaResponse media_ = new MediaResponse();
				var results = await _container.Hashtag.GetTopHashtagMediaListAsync(topic,PaginationParameters.MaxPagesToLoad(limit));
				if (results.Succeeded)
				{
					media_.Topic = topic;
					foreach(var results_media in results.Value.Medias)
					{
						switch (mediaType)
						{
							case InstaMediaType.All:
								var image = results_media.Images.FirstOrDefault().Uri;		
								if(image!=null)
									media_.MediaUrl.Add(image);
								var video = results_media.Videos.FirstOrDefault().Uri;
								if(video!=null)
									media_.MediaUrl.Add(video);
								results_media.Carousel.Select(s =>
								{
									var videos = s.Videos.FirstOrDefault().Uri;
									if (videos != null)
										media_.MediaUrl.Add(videos);
									var images = s.Images.FirstOrDefault().Uri;
									if (images != null)
										media_.MediaUrl.Add(images);
									return s;
								});
								break;
							case InstaMediaType.Image:
								media_.MediaUrl.Add(results_media.Images.FirstOrDefault().Uri);
								break;
							case InstaMediaType.Video:
								media_.MediaUrl.Add(results_media.Videos.FirstOrDefault().Uri);
								break;
							case InstaMediaType.Carousel:
								results_media.Carousel.Select(s =>
								{
									var videos = s.Videos.FirstOrDefault().Uri;
									if (videos != null)
										media_.MediaUrl.Add(videos);
									var images = s.Images.FirstOrDefault().Uri;
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
				foreach(var lema in results.Value)
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
								if(videos!=null)
									media.MediaUrl.Add(videos);
								var images = s.Images.FirstOrDefault().Uri;
								if (images!=null)
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
		public Media SearchViaGoogle(SearchImageModel searchImageQuery)
		{
			var results = _restSharpClient.PostRequest("http://127.0.0.1:5000/","searchImages",JsonConvert.SerializeObject(searchImageQuery),null);
			if (results.IsSuccessful)
			{
				Media responseValues = JsonConvert.DeserializeObject<Media>(results.Content);
				return responseValues;
			}
			return null;
		}
		public Media SearchViaYandexBySimilarImages(List<GroupImagesAlike> imagesSimilarUrls, int limit)
		{
			var images =  _yandexImageSearch.Search(imagesSimilarUrls, limit);
			return images;
		}
	}
}
