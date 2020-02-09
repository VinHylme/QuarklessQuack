using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InstagramApiSharp;
using InstagramApiSharp.Classes.Models;
using Quarkless.Models.Common.Extensions;
using Quarkless.Models.InstagramClient.Interfaces;
using Quarkless.Models.InstagramSearch;
using Quarkless.Models.InstagramSearch.Interfaces;
using Quarkless.Models.Proxy;
using Quarkless.Models.ResponseResolver.Interfaces;
using Quarkless.Models.SearchResponse;
using Quarkless.Models.SearchResponse.Enums;
using Quarkless.Models.Topic;
using Location = Quarkless.Models.Common.Models.Location;

namespace Quarkless.Logic.InstagramSearch
{
	public class InstagramContentSearch : IInstagramContentSearch
	{
		private readonly IApiClientContainer _container;
		private readonly IResponseResolver _responseResolver;
		public InstagramContentSearch(IApiClientContainer clientContainer,
			IResponseResolver responseResolver, ProxyModel proxy = null)
		{
			_container = clientContainer;
			_responseResolver = responseResolver;
		}

		public async Task<IEnumerable<UserResponse<InstaReelFeed>>> GetUserFeedStories(int limit = 1)
		{
			var feedResponse = await _responseResolver
				.WithClient(_container)
				.WithAttempts(1)
				.WithResolverAsync(() => _container.Story.GetStoryFeedWithPostMethodAsync());

			if (!feedResponse.Response.Succeeded && feedResponse.Response.Value.Items.Count<=0) return null;

			return feedResponse.Response.Value.Items.Select(_ => new UserResponse<InstaReelFeed>
			{
				FullName = _.User.FullName,
				IsVerified = _.User.IsVerified,
				IsPrivate = _.User.IsPrivate,
				ProfilePicture = _.User.ProfilePicture,
				Username = _.User.UserName,
				UserId = _.User.Pk,
				Object = _
			});
		}

		public async Task<IEnumerable<UserResponse<InstaStory>>> GetUserStoriesByTopic(CTopic topic, int limit = 1)
		{
			var topicSearchResponse = await _responseResolver
				.WithClient(_container)
				.WithAttempts(1)
				.WithResolverAsync(() => _container.Feeds.GetTagFeedAsync(topic.Name,
					PaginationParameters.MaxPagesToLoad(limit)));

			if (!topicSearchResponse.Response.Succeeded &&
				topicSearchResponse.Response.Value.StoriesItemsCount <= 0) return null;

			return topicSearchResponse.Response.Value.Stories.Select(_ => new UserResponse<InstaStory>
			{
				FullName = _.User.FullName,
				IsVerified = _.User.IsVerified,
				IsPrivate = _.User.IsPrivate,
				UserId = _.User.Pk,
				Username = _.User.UserName,
				ProfilePicture = _.User.ProfilePicture,
				Topic = topic,
				Object = _
			});
		}

		public async Task<IEnumerable<UserResponse<string>>> GetUsersFollowersList(string username, int limit,
			string query = null, bool mutualFirst = true)
		{
			var userResponse = await _responseResolver
				.WithClient(_container)
				.WithAttempts(1)
				.WithResolverAsync(()=> _container.User.GetUserFollowersAsync(username,
					PaginationParameters.MaxPagesToLoad(limit), query, mutualFirst));

			if (!userResponse.Response.Succeeded) return null;
			var users = userResponse.Response.Value;
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
		public async Task<IEnumerable<UserResponse<string>>> GetUserFollowingList(string username, int limit, string query = null)
		{
			var userResponse = await _responseResolver
				.WithClient(_container)
				.WithAttempts(1)
				.WithResolverAsync(()=> _container.User.GetUserFollowingAsync(username,
				PaginationParameters.MaxPagesToLoad(limit), query,
				InstagramApiSharp.Enums.InstaFollowingOrderType.DateFollowedEarliest));
			if (!userResponse.Response.Succeeded) return null;
			var users = userResponse.Response.Value;
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
			var res = await _responseResolver
				.WithClient(_container)
				.WithAttempts(1)
				.WithResolverAsync(()=> _container.User.GetSuggestionUsersAsync(PaginationParameters.MaxPagesToLoad(limit)));
			if (!res.Response.Succeeded) return null;
			var usersTotals = new List<UserResponse<UserSuggestionDetails>>();
			var users = res.Response.Value;
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
		/// <summary>
		/// INCOMPLETE
		/// </summary>
		/// <param name="userId"></param>
		/// <param name="chainingIds"></param>
		/// <returns></returns>
		public async Task<List<UserResponse<UserSuggestionDetails>>> GetSuggestedDetails(long userId, long[] chainingIds)
		{
			var res = await _responseResolver
				.WithClient(_container)
				.WithAttempts(1)
				.WithResolverAsync(()=> _container.User.GetSuggestionDetailsAsync(userId, chainingIds));
			if (!res.Response.Succeeded) return null;
			var usersTotals = new List<UserResponse<UserSuggestionDetails>>();
			var users = res.Response.Value;
			usersTotals.AddRange(users.Select(_ => new UserResponse<UserSuggestionDetails>
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
		public async Task<List<UserResponse<string>>> SearchInstagramMediaLikers(CTopic mediaTopic, string mediaId)
		{
			var userLikerResult = await _responseResolver
				.WithClient(_container)
				.WithAttempts(1)
				.WithResolverAsync(()=> _container.Media.GetMediaLikersAsync(mediaId));
			if (userLikerResult.Response.Succeeded)
			{
				return userLikerResult.Response.Value.Select(s => new UserResponse<string>
				{
					Object = mediaId,
					UserId = s.Pk,
					Username = s.UserName,
					FullName = s.FullName,
					IsPrivate = s.IsPrivate,
					IsVerified = s.IsVerified,
					ProfilePicture = s.ProfilePicture + "||" + s.ProfilePictureId,
					Topic = mediaTopic
				}).ToList();
			}

			return null;
		}
		public async Task<List<UserResponse<InstaComment>>> SearchInstagramMediaCommenters(CTopic mediaTopic, string mediaId, int limit)
		{
			try
			{
				var userCommentResult = await _responseResolver
					.WithClient(_container)
					.WithAttempts(1)
					.WithResolverAsync(()=> _container.Comment.GetMediaCommentsAsync(mediaId, PaginationParameters.MaxPagesToLoad(limit)));
				if (!userCommentResult.Response.Succeeded) return null;
				var results = userCommentResult.Response.Value.Comments;
				return results.Select(res => new UserResponse<InstaComment>
					{
						Object = res,
						MediaId = mediaId,
						FullName = res.User.FullName,
						IsPrivate = res.User.IsPrivate,
						IsVerified = res.User.IsVerified,
						ProfilePicture = res?.User?.ProfilePicture,
						UserId = res.User.Pk,
						Username = res.User.UserName,
						Topic = mediaTopic
					})
					.ToList();
			}
			catch (Exception ee)
			{
				Console.WriteLine(ee.Message);
				return null;
			}
		}
		public async Task<InstaFullUserInfo> SearchInstagramFullUserDetail(long userId)
		{
			var userDetailsResp = await _responseResolver
				.WithClient(_container)
				.WithAttempts(1)
				.WithResolverAsync(()=> _container.User.GetFullUserInfoAsync(userId));
			return userDetailsResp.Response.Succeeded ? userDetailsResp.Response.Value : null;
		}
		public async Task<Media> SearchMediaDetailInstagram(IEnumerable<CTopic> topics, int limit, bool isRecent = false)
		{
			var medias = new Media();
			foreach (var topic in topics)
			{
				var mediasResults = !isRecent ? await _container.Hashtag.GetTopHashtagMediaListAsync(topic.Name.OnlyWords(), PaginationParameters.MaxPagesToLoad(limit))
					: await _container.Hashtag.GetRecentHashtagMediaListAsync(topic.Name.OnlyWords(), PaginationParameters.MaxPagesToLoad(limit));
				if (mediasResults.Succeeded)
				{
					medias.Medias.AddRange(mediasResults.Value.Medias.Select(s =>
					{
						var mediaDetail = new MediaResponse
						{
							LikesCount = s.LikesCount,
							MediaId = s.Pk,
							HasLikedBefore = s.HasLiked,
							HasAudio = s.HasAudio,
							IsCommentsDisabled = s.IsCommentsDisabled,
							Location = s?.Location,
							ViewCount = s.ViewCount,
							Caption = s?.Caption?.Text,
							Explore = s?.Explore,
							FilterType = s.FilterType,
							HasSeen = s.IsSeen,
							NumberOfQualities = s.NumberOfQualities,
							PhotosOfI = s.PhotoOfYou,
							PreviewComments = s?.PreviewComments,
							ProductTags = s?.ProductTags,
							ProductType = s.ProductType,
							TopLikers = s?.TopLikers,
							TakenAt = s.TakenAt,
							UserTags = s?.UserTags,
							IsFollowing = s?.User?.FriendshipStatus?.Following,
							CommentCount = s.CommentsCount,
							Topic = topic,
							MediaFrom = MediaFrom.Instagram,
							MediaType = s.MediaType,
							User = new UserResponse
							{
								FullName = s.User.FullName,
								IsPrivate = s.User.IsPrivate,
								IsVerified = s.User.IsVerified,
								ProfilePicture = s.User.ProfilePicture,
								Topic = topic,
								UserId = s.User.Pk,
								Username = s.User.UserName
							}
						};
						var mediaDetailMediaUrl = new List<string>();
						switch (s.MediaType)
						{
							case InstaMediaType.Image:
							{
								var im = s.Images.FirstOrDefault()?.Uri;
								if (im != null)
									mediaDetailMediaUrl.Add(im);
								break;
							}
							case InstaMediaType.Video:
							{
								var iv = s.Videos.FirstOrDefault()?.Uri;
								if (iv != null)
									mediaDetailMediaUrl.Add(iv);
								break;
							}
							case InstaMediaType.Carousel:
								s.Carousel.ForEach(x =>
								{
									var videos = x.Videos.FirstOrDefault()?.Uri;
									if (videos != null)
										mediaDetailMediaUrl.Add(videos);
									var images = x.Images.FirstOrDefault()?.Uri;
									if (images != null)
										mediaDetailMediaUrl.Add(images);
								});
								break;
						}
						mediaDetail.MediaUrl = mediaDetailMediaUrl;
						return mediaDetail;
					}));
				}
			}
			return medias;
		}
		public async Task<Media> SearchMediaDetailInstagram(IEnumerable<string> topics, int limit, bool isRecent = false)
		{
			var medias = new Media();
			foreach (var topic in topics)
			{
				var mediasResults = !isRecent ? await _container.Hashtag.GetTopHashtagMediaListAsync(topic.OnlyWords(), PaginationParameters.MaxPagesToLoad(limit))
					: await _container.Hashtag.GetRecentHashtagMediaListAsync(topic.OnlyWords(), PaginationParameters.MaxPagesToLoad(limit));
				if (mediasResults.Succeeded)
				{
					medias.Medias.AddRange(mediasResults.Value.Medias.Select(s =>
					{
						var mediaDetail = new MediaResponse
						{
							LikesCount = s.LikesCount,
							MediaId = s.Pk,
							HasLikedBefore = s.HasLiked,
							HasAudio = s.HasAudio,
							IsCommentsDisabled = s.IsCommentsDisabled,
							Location = s?.Location,
							ViewCount = s.ViewCount,
							Caption = s?.Caption?.Text,
							Explore = s?.Explore,
							FilterType = s.FilterType,
							HasSeen = s.IsSeen,
							NumberOfQualities = s.NumberOfQualities,
							PhotosOfI = s.PhotoOfYou,
							PreviewComments = s?.PreviewComments,
							ProductTags = s?.ProductTags,
							ProductType = s.ProductType,
							TopLikers = s?.TopLikers,
							TakenAt = s.TakenAt,
							UserTags = s?.UserTags,
							IsFollowing = s?.User?.FriendshipStatus?.Following,
							CommentCount = s.CommentsCount,
							MediaFrom = MediaFrom.Instagram,
							MediaType = s.MediaType,
							User = new UserResponse
							{
								FullName = s.User.FullName,
								IsPrivate = s.User.IsPrivate,
								IsVerified = s.User.IsVerified,
								ProfilePicture = s.User.ProfilePicture,
								UserId = s.User.Pk,
								Username = s.User.UserName
							}
						};
						var mediaDetailMediaUrl = new List<string>();
						switch (s.MediaType)
						{
							case InstaMediaType.Image:
							{
								var im = s.Images.FirstOrDefault()?.Uri;
								if (im != null)
									mediaDetailMediaUrl.Add(im);
								break;
							}
							case InstaMediaType.Video:
							{
								var iv = s.Videos.FirstOrDefault()?.Uri;
								if (iv != null)
									mediaDetailMediaUrl.Add(iv);
								break;
							}
							case InstaMediaType.Carousel:
								s.Carousel.ForEach(x =>
								{
									var videos = x.Videos.FirstOrDefault()?.Uri;
									if (videos != null)
										mediaDetailMediaUrl.Add(videos);
									var images = x.Images.FirstOrDefault()?.Uri;
									if (images != null)
										mediaDetailMediaUrl.Add(images);
								});
								break;
						}
						mediaDetail.MediaUrl = mediaDetailMediaUrl;
						return mediaDetail;
					}));
				}
			}
			return medias;
		}
		public async Task<Media> SearchUsersMediaDetailInstagram(CTopic topic, string userName, int limit)
		{
			var medias = await _responseResolver
				.WithClient(_container)
				.WithAttempts(1)
				.WithResolverAsync(()=> _container.User
					.GetUserMediaAsync(userName, PaginationParameters.MaxPagesToLoad(limit)));
			if (medias.Response.Succeeded)
			{
				return new Media
				{
					Medias = medias.Response.Value.Select(s =>
					{
						var mediaDetail = new MediaResponse
						{
							LikesCount = s.LikesCount,
							MediaId = s.Pk,
							HasLikedBefore = s.HasLiked,
							HasAudio = s.HasAudio,
							IsCommentsDisabled = s.IsCommentsDisabled,
							Location = s?.Location,
							ViewCount = s.ViewCount,
							Caption = s?.Caption?.Text,
							Explore = s?.Explore,
							FilterType = s.FilterType,
							HasSeen = s.IsSeen,
							NumberOfQualities = s.NumberOfQualities,
							PhotosOfI = s.PhotoOfYou,
							PreviewComments = s?.PreviewComments,
							ProductTags = s?.ProductTags,
							ProductType = s.ProductType,
							TopLikers = s?.TopLikers,
							TakenAt = s.TakenAt,
							UserTags = s?.UserTags,
							IsFollowing = s?.User?.FriendshipStatus?.Following,
							CommentCount = s.CommentsCount,
							MediaFrom = MediaFrom.Instagram,
							MediaType = s.MediaType,
							Topic = topic,
							User = new UserResponse
							{
								FullName = s.User.FullName,
								IsPrivate = s.User.IsPrivate,
								IsVerified = s.User.IsVerified,
								ProfilePicture = s.User.ProfilePicture,
								UserId = s.User.Pk,
								Username = s.User.UserName
							}
						};
						var totals = new List<string>();
						switch (s.MediaType)
						{
							case InstaMediaType.Image:
							{
								var im = s.Images.FirstOrDefault()?.Uri;
								if (im != null)
									totals.Add(im);
								break;
							}
							case InstaMediaType.Video:
							{
								var iv = s.Videos.FirstOrDefault()?.Uri;
								if (iv != null)
									totals.Add(iv);
								break;
							}
							case InstaMediaType.Carousel:
								s.Carousel.ForEach(x =>
								{
									var videos = x.Videos.FirstOrDefault()?.Uri;
									if (videos != null)
										totals.Add(videos);
									var images = x.Images.FirstOrDefault()?.Uri;
									if (images != null)
										totals.Add(images);
								});
								break;
						}
						mediaDetail.MediaUrl = totals;
						return mediaDetail;
					}).ToList()
				};
			}
			return null;
		}
		public async Task<Media> SearchTopLocationMediaDetailInstagram(Location location, int limit)
		{
			var locationResult = await _responseResolver
				.WithClient(_container)
				.WithAttempts(1)
				.WithResolverAsync(()=> _container.Location
					.SearchPlacesAsync(location.City.OnlyWords(), PaginationParameters.MaxPagesToLoad(limit)));
			if (!locationResult.Response.Succeeded && !locationResult.Response.Value.Items.Any()) return null;
			var id = locationResult.Response.Value.Items.First().Location.Pk;
			var mediasRes = await _container.Location.GetTopLocationFeedsAsync(id, PaginationParameters.MaxPagesToLoad(limit));
			if (mediasRes.Succeeded)
			{
				return new Media
				{
					Medias = mediasRes.Value.Medias.Select(s =>
					{
						var mediaDetail = new MediaResponse
						{
							LikesCount = s.LikesCount,
							MediaId = s.Pk,
							HasLikedBefore = s.HasLiked,
							HasAudio = s.HasAudio,
							IsCommentsDisabled = s.IsCommentsDisabled,
							Location = s?.Location,
							ViewCount = s.ViewCount,
							Caption = s?.Caption?.Text,
							Explore = s?.Explore,
							FilterType = s.FilterType,
							HasSeen = s.IsSeen,
							NumberOfQualities = s.NumberOfQualities,
							PhotosOfI = s.PhotoOfYou,
							PreviewComments = s?.PreviewComments,
							ProductTags = s?.ProductTags,
							ProductType = s.ProductType,
							TopLikers = s?.TopLikers,
							TakenAt = s.TakenAt,
							UserTags = s?.UserTags,
							IsFollowing = s?.User?.FriendshipStatus?.Following,
							CommentCount = s.CommentsCount,
							MediaFrom = MediaFrom.Instagram,
							MediaType = s.MediaType,
							User = new UserResponse
							{
								FullName = s.User.FullName,
								IsPrivate = s.User.IsPrivate,
								IsVerified = s.User.IsVerified,
								ProfilePicture = s.User.ProfilePicture,
								UserId = s.User.Pk,
								Username = s.User.UserName
							}
						};
						var mediaDetailMediaUrl = new List<string>();
						switch (s.MediaType)
						{
							case InstaMediaType.Image:
							{
								var im = s.Images.FirstOrDefault()?.Uri;
								if (im != null)
									mediaDetailMediaUrl.Add(im);
								break;
							}
							case InstaMediaType.Video:
							{
								var iv = s.Videos.FirstOrDefault()?.Uri;
								if (iv != null)
									mediaDetailMediaUrl.Add(iv);
								break;
							}
							case InstaMediaType.Carousel:
								s.Carousel.ForEach(x =>
								{
									var videos = x.Videos.FirstOrDefault()?.Uri;
									if (videos != null)
										mediaDetailMediaUrl.Add(videos);
									var images = x.Images.FirstOrDefault()?.Uri;
									if (images != null)
										mediaDetailMediaUrl.Add(images);
								});
								break;
						}
						mediaDetail.MediaUrl = mediaDetailMediaUrl;
						return mediaDetail;
					}).ToList()
				};
			}
			return null;
		}
		public async Task<Media> SearchRecentLocationMediaDetailInstagram(Location location, int limit)
		{
			var locationResult = await _responseResolver
				.WithClient(_container)
				.WithAttempts(1)
				.WithResolverAsync(()=> _container.Location
					.SearchPlacesAsync(location.City, PaginationParameters.MaxPagesToLoad(limit)));
			if (!locationResult.Response.Succeeded && !locationResult.Response.Value.Items.Any()) return null;
			var id = locationResult.Response.Value.Items.First().Location.Pk;
			var mediasRes = await _container.Location.GetRecentLocationFeedsAsync(id, PaginationParameters.MaxPagesToLoad(limit));
			if (mediasRes.Succeeded)
			{
				return new Media
				{
					Medias = mediasRes.Value.Medias.Select(s =>
					{
						var mediaDetail = new MediaResponse
						{
							LikesCount = s.LikesCount,
							MediaId = s.Pk,
							HasLikedBefore = s.HasLiked,
							HasAudio = s.HasAudio,
							IsCommentsDisabled = s.IsCommentsDisabled,
							Location = s.Location,
							ViewCount = s.ViewCount,
							Caption = s.Caption?.Text,
							Explore = s.Explore,
							FilterType = s.FilterType,
							HasSeen = s.IsSeen,
							NumberOfQualities = s.NumberOfQualities,
							PhotosOfI = s.PhotoOfYou,
							PreviewComments = s.PreviewComments,
							ProductTags = s.ProductTags,
							ProductType = s.ProductType,
							TopLikers = s.TopLikers,
							TakenAt = s.TakenAt,
							UserTags = s.UserTags,
							IsFollowing = s.User?.FriendshipStatus?.Following,
							CommentCount = s.CommentsCount,
							MediaFrom = MediaFrom.Instagram,
							MediaType = s.MediaType,
							User = new UserResponse
							{
								FullName = s.User?.FullName,
								IsPrivate = s.User?.IsPrivate ?? false,
								IsVerified = s.User?.IsVerified ?? false,
								ProfilePicture = s.User?.ProfilePicture,
								UserId = s.User?.Pk ?? 0,
								Username = s.User?.UserName
							}
						};
						var mediaDetailMediaUrl = new List<string>();
						switch (s.MediaType)
						{
							case InstaMediaType.Image:
							{
								var im = s.Images.FirstOrDefault()?.Uri;
								if (im != null)
									mediaDetailMediaUrl.Add(im);
								break;
							}
							case InstaMediaType.Video:
							{
								var iv = s.Videos.FirstOrDefault()?.Uri;
								if (iv != null)
									mediaDetailMediaUrl.Add(iv);
								break;
							}
							case InstaMediaType.Carousel:
								s.Carousel.ForEach(x =>
								{
									var videos = x.Videos.FirstOrDefault()?.Uri;
									if (videos != null)
										mediaDetailMediaUrl.Add(videos);
									var images = x.Images.FirstOrDefault()?.Uri;
									if (images != null)
										mediaDetailMediaUrl.Add(images);
								});
								break;
						}
						mediaDetail.MediaUrl = mediaDetailMediaUrl;
						return mediaDetail;
					}).ToList()
				};
			}
			return null;
		}
		public async Task<Media> SearchUserFeedMediaDetailInstagram(string[] seenMedias = null, bool requestRefresh = false, int limit = 1)
		{
			var medias = new Media();
			var mediasResults = await _responseResolver
				.WithClient(_container)
				.WithAttempts(1)
				.WithResolverAsync(()=> _container.Feeds.GetUserTimelineFeedAsync(PaginationParameters.MaxPagesToLoad(limit), seenMedias, requestRefresh));
			if (mediasResults.Response.Succeeded)
			{
				medias.Medias.AddRange(mediasResults.Response.Value.Medias.Select(s =>
				{
					var mediaDetail = new MediaResponse
					{
						LikesCount = s.LikesCount,
						MediaId = s.Pk,
						HasLikedBefore = s.HasLiked,
						HasAudio = s.HasAudio,
						IsCommentsDisabled = s.IsCommentsDisabled,
						Location = s?.Location,
						ViewCount = s.ViewCount,
						Caption = s?.Caption?.Text,
						Explore = s?.Explore,
						FilterType = s.FilterType,
						HasSeen = s.IsSeen,
						NumberOfQualities = s.NumberOfQualities,
						PhotosOfI = s.PhotoOfYou,
						PreviewComments = s?.PreviewComments,
						ProductTags = s?.ProductTags,
						ProductType = s.ProductType,
						TopLikers = s?.TopLikers,
						TakenAt = s.TakenAt,
						UserTags = s?.UserTags,
						IsFollowing = s?.User?.FriendshipStatus?.Following,
						CommentCount = s.CommentsCount,
						MediaFrom = MediaFrom.Instagram,
						MediaType = s.MediaType,
						User = new UserResponse
						{
							FullName = s.User?.FullName,
							IsPrivate = s.User?.IsPrivate ?? false,
							IsVerified = s.User?.IsVerified ?? false,
							ProfilePicture = s.User?.ProfilePicture,
							UserId = s.User?.Pk ?? 0,
							Username = s.User?.UserName
						}
					};
					var mediaDetailMediaUrl = new List<string>();
					switch (s.MediaType)
					{
						case InstaMediaType.Image:
						{
							var im = s.Images.FirstOrDefault()?.Uri;
							if (im != null)
								mediaDetailMediaUrl.Add(im);
							break;
						}
						case InstaMediaType.Video:
						{
							var iv = s.Videos.FirstOrDefault()?.Uri;
							if (iv != null)
								mediaDetailMediaUrl.Add(iv);
							break;
						}
						case InstaMediaType.Carousel:
							s.Carousel.ForEach(x =>
							{
								var videos = x.Videos.FirstOrDefault()?.Uri;
								if (videos != null)
									mediaDetailMediaUrl.Add(videos);
								var images = x.Images.FirstOrDefault()?.Uri;
								if (images != null)
									mediaDetailMediaUrl.Add(images);
							});
							break;
					}
					mediaDetail.MediaUrl = mediaDetailMediaUrl;
					return mediaDetail;
				}));
			}
			return medias;
		}
		public async Task<InstaDirectInboxContainer> SearchUserInbox(int limit = 1)
		{
			var results = await _responseResolver
				.WithClient(_container)
				.WithAttempts(1)
				.WithResolverAsync(()=> _container.Messaging.GetDirectInboxAsync(PaginationParameters.MaxPagesToLoad(limit)));

			return results.Response.Succeeded ? results.Response.Value : null;
		}
	}
}