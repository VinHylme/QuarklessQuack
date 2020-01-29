using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using InstagramApiSharp.Classes.Models;
using MoreLinq.Extensions;
using Quarkless.Analyser.Extensions;
using Quarkless.Models.Actions;
using Quarkless.Models.Actions.Factory.Action_Options;
using Quarkless.Models.Actions.Interfaces;
using Quarkless.Models.Actions.Models;
using Quarkless.Models.Common.Enums;
using Quarkless.Models.Common.Extensions;
using Quarkless.Models.Common.Models;
using Quarkless.Models.Common.Models.Carriers;
using Quarkless.Models.ContentInfo.Interfaces;
using Quarkless.Models.Heartbeat;
using Quarkless.Models.Heartbeat.Interfaces;
using Quarkless.Models.Media;
using Quarkless.Models.Profile.Enums;
using Quarkless.Models.SearchResponse;

namespace Quarkless.Logic.Actions.Action_Instances
{
	internal class CreatePostAction : IActionCommit
	{
		private readonly IContentInfoBuilder _contentInfoBuilder;
		private readonly IHeartbeatLogic _heartbeatLogic;
		private readonly UserStoreDetails _user;
		private PostActionOptions _actionOptions;

		internal CreatePostAction(UserStoreDetails userStoreDetails, IContentInfoBuilder contentInfoBuilder, IHeartbeatLogic heartbeatLogic)
		{
			_contentInfoBuilder = contentInfoBuilder;
			_heartbeatLogic = heartbeatLogic;
			_user = userStoreDetails ?? throw new Exception("Please specify a user");
		}

		private List<SearchType> GetValidSearchTypes()
		{
			var allowedSearchTypes = new List<SearchType>();
			if (_user.Profile.Theme.ImagesLike != null)
				if (_user.Profile.Theme.ImagesLike.Count > 0)
					allowedSearchTypes.Add(SearchType.Yandex);

			if (_user.Profile.AdditionalConfigurations.AllowRepost)
				allowedSearchTypes.Add(SearchType.Instagram);

			allowedSearchTypes.Add(SearchType.Google);
			return allowedSearchTypes;
		}
		private async Task<string> UploadToS3(byte[] media, string keyName)
		{
			await using var mediaStream = new MemoryStream(media);
			return await _actionOptions.S3BucketLogic.UploadStreamFile(mediaStream, keyName);
		}

		private async Task<MediaData> ProcessImage(string mediaUrl, Size size)
		{
			if (string.IsNullOrEmpty(mediaUrl))
				return null;

			var imageBytes = _actionOptions.PostAnalyser.Manager.DownloadMedia(mediaUrl);

			if (imageBytes == null)
				return null;

			if (!imageBytes.IsValidImage())
			{
				return new MediaData
				{
					IncorrectFormat = true
				};
			}
			if (!_actionOptions.PostAnalyser.Manipulation.ImageEditor.IsImageGood(imageBytes,
				_user.Profile.Theme.Colors.Select(s => Color.FromArgb(s.Red, s.Green, s.Blue)),
				_user.Profile.Theme.Percentage, size)) return null;

			var s3UrlLink = await UploadToS3(imageBytes, $"Image_{imageBytes.GetHashCode()}_{Guid.NewGuid()}");

			return new MediaData
			{
				MediaBytes = imageBytes,
				Url = s3UrlLink,
				MediaType = InstaMediaType.Image
			};
		}
		private MediaData ProcessVideo(string mediaUrl)
		{
			if (string.IsNullOrEmpty(mediaUrl))
				return null;

			var videoBytes = _actionOptions.PostAnalyser.Manager.DownloadMedia(mediaUrl);

			if (videoBytes == null)
				return null;

			if (!_actionOptions.PostAnalyser.Manipulation.VideoEditor.IsVideoGood(videoBytes,
				_user.Profile.Theme.Colors.Select(s => Color.FromArgb(s.Red, s.Green, s.Blue))
					.ElementAt(_user.Profile.Theme.Colors.Count - 1),
				_user.Profile.Theme.Percentage, 10)) return null;

			var s3UrlLink = UploadToS3(videoBytes, $"Video_{videoBytes.GetHashCode()}_{Guid.NewGuid()}").GetAwaiter().GetResult();
			return new MediaData
			{
				MediaType = InstaMediaType.Video,
				MediaBytes = videoBytes,
				Url = s3UrlLink
			};
		}

		public async Task<ResultCarrier<EventActionModel>> PushAsync(DateTimeOffset executionTime)
		{
			if(_actionOptions == null)
				throw new Exception("Action Option cannot be null");
			Console.WriteLine($"Create Media Action Started: {_user.AccountId}, {_user.InstagramAccountUsername}, {_user.InstagramAccountUser}");
			
			var results = new ResultCarrier<EventActionModel>();
			if (!_user.Profile.AdditionalConfigurations.EnableAutoPosting)
			{
				results.IsSuccessful = true;
				results.Info = new ErrorResponse
				{
					Message = $"User {_user.AccountId} of {_user.InstagramAccountUsername} has disabled auto posting"
				};
				return results;
			}
			if (!_user.Profile.ProfileTopic.Topics.Any())
			{
				results.IsSuccessful = false;
				results.Info = new ErrorResponse
				{
					Message =
						$"Profile Topics is empty, user: {_user.AccountId}, instaId: {_user.InstagramAccountUsername}",
					StatusCode = System.Net.HttpStatusCode.NotFound
				};
				return results;
			}

			try
			{
				var totalResults = new List<Meta<Media>>();
				var selectedAction = MetaDataType.None;
				var allowedSearchTypes = GetValidSearchTypes();

				var searchTypeSelected = allowedSearchTypes[SecureRandom.Next(allowedSearchTypes.Count)];

				switch (searchTypeSelected)
				{
					case SearchType.Google:
					{
						var googleResults = await _heartbeatLogic.GetMetaData<Media>(new MetaDataFetchRequest
						{
							MetaDataType = MetaDataType.FetchMediaForSpecificUserGoogle,
							ProfileCategoryTopicId = _user.Profile.ProfileTopic.Category._id,
							InstagramId = _user.Profile.InstagramAccountId
						});
						selectedAction = MetaDataType.FetchMediaForSpecificUserGoogle;
						if (googleResults != null)
							totalResults = googleResults.ToList();
						break;
					}
					case SearchType.Instagram:
					{
						if (_user.Profile.UserTargetList != null && _user.Profile.UserTargetList.Any())
						{
							var action = new[] {0, 1};
							var pickedRandom = action.ElementAt(SecureRandom.Next(action.Length - 1));
							var userId = _user.ShortInstagram.Id;
							var selected = MetaDataType.FetchMediaByUserTargetList;

							if (pickedRandom == 1)
							{
								userId = null;
								selected = MetaDataType.FetchMediaByTopic;
							}

							totalResults = (await _heartbeatLogic.GetMetaData<Media>(new MetaDataFetchRequest
							{
								MetaDataType = selected,
								ProfileCategoryTopicId = _user.Profile.ProfileTopic.Category._id,
								InstagramId = userId
							}))?.ToList();

							selectedAction = selected;
						}
						else
						{
							totalResults = (await _heartbeatLogic.GetMetaData<Media>(new MetaDataFetchRequest
							{
								MetaDataType = MetaDataType.FetchMediaByTopic,
								ProfileCategoryTopicId = _user.Profile.ProfileTopic.Category._id,
								InstagramId = _user.ShortInstagram.Id
							}))?.ToList();

							selectedAction = MetaDataType.FetchMediaByTopic;
						}

						break;
					}
					case SearchType.Yandex:
					{
						if (_user.Profile.Theme.ImagesLike != null && _user.Profile.Theme.ImagesLike.Count > 0)
						{
							var yandexResults = await _heartbeatLogic.GetMetaData<Media>(new MetaDataFetchRequest
							{
								MetaDataType = MetaDataType.FetchMediaForSpecificUserYandex,
								ProfileCategoryTopicId = _user.Profile.ProfileTopic.Category._id,
								InstagramId = _user.ShortInstagram.Id
							});

							selectedAction = MetaDataType.FetchMediaForSpecificUserYandex;
							if (yandexResults != null)
								totalResults = yandexResults.ToList();
						}

						break;
					}
					default: throw new Exception("Search type was invalid");
				}

				if (selectedAction == MetaDataType.None || totalResults.Count <= 0)
				{
					results.IsSuccessful = false;
					results.Info = new ErrorResponse
					{
						Message = $"no action selected or results were empty, user: {_user.AccountId}, instaId: {_user.InstagramAccountUsername}",
						StatusCode = System.Net.HttpStatusCode.NotFound
					};
					return results;
				};

				var by = new By
				{
					ActionType = (int)ActionType.CreatePost,
					User = _user.Profile.InstagramAccountId
				};

				var filteredResults = totalResults.Where(exclude => !exclude.SeenBy.Any(e => e.User == by.User
					&& (e.ActionType == by.ActionType))).ToList();

				var selectedMedia = new TempSelect();
				var filterMediaSize = new System.Drawing.Size(850, 850);
				var carouselAmount = SecureRandom.Next(2, 4);
				var currentAmount = 0;
				var enteredType = InstaMediaType.All;

				foreach (var mediaData in filteredResults.Shuffle())
				{
					var media = mediaData.ObjectItem?.Medias.FirstOrDefault();
					if (media == null) continue;

					if (enteredType != InstaMediaType.All)
					{
						if (media.MediaType != enteredType)
						{
							continue;
						}
					}

					mediaData.SeenBy.Add(by);
					await _heartbeatLogic.UpdateMetaData(new MetaDataCommitRequest<Media>
					{
						AccountId = _user.AccountId,
						InstagramId = _user.InstagramAccountUser,
						ProfileCategoryTopicId = _user.Profile.ProfileTopic.Category._id,
						MetaDataType = selectedAction,
						Data = mediaData
					});

					switch (media.MediaType)
					{
						case InstaMediaType.Image:
						{
							enteredType = InstaMediaType.Image;
							var imageUrl = media.MediaUrl.FirstOrDefault();
							var resultImage = await ProcessImage(imageUrl, filterMediaSize);
							if (resultImage == null)
								continue;

							selectedMedia.MediaType = InstaMediaType.Image;
							resultImage.SelectedMedia = mediaData;
							selectedMedia.MediaData.Add(resultImage);
							goto Finished;
						}
						case InstaMediaType.Video:
						{
							enteredType = InstaMediaType.Video;
							var videoUrl = media.MediaUrl.FirstOrDefault();
							var resultVideo = ProcessVideo(videoUrl);
							if (resultVideo == null)
								continue;

							selectedMedia.MediaType = InstaMediaType.Video;
							resultVideo.SelectedMedia = mediaData;
							selectedMedia.MediaData.Add(resultVideo);
							goto Finished;
						}
						case InstaMediaType.Carousel when currentAmount < carouselAmount:
						{
							foreach (var mediaUrl in media.MediaUrl)
							{
								var resultCarousel = await ProcessImage(mediaUrl, filterMediaSize);
								if (resultCarousel == null)
									continue;

								if (!resultCarousel.IncorrectFormat)
								{
									if (selectedMedia.MediaData.Count > 0)
									{
										var oas = _actionOptions.PostAnalyser.Manipulation.ImageEditor
											.GetClosestAspectRatio(selectedMedia.MediaData[0].MediaBytes);

										var cas = _actionOptions.PostAnalyser.Manipulation.ImageEditor
											.GetClosestAspectRatio(resultCarousel.MediaBytes);

										if (Math.Abs(oas - cas) > 0.5) continue;

										selectedMedia.MediaType = InstaMediaType.Carousel;
										resultCarousel.SelectedMedia = mediaData;
										selectedMedia.MediaData.Add(resultCarousel);
									}
									else
									{
										selectedMedia.MediaType = InstaMediaType.Carousel;
										resultCarousel.SelectedMedia = mediaData;
										selectedMedia.MediaData.Add(resultCarousel);
									}

									carouselAmount++;
								}
								else
								{
									var resultCarouselVideo = ProcessVideo(mediaUrl);
									if (resultCarouselVideo == null)
										continue;

									selectedMedia.MediaType = InstaMediaType.Carousel;
									resultCarouselVideo.SelectedMedia = mediaData;
									selectedMedia.MediaData.Add(resultCarouselVideo);
									carouselAmount++;
								}
							}

							break;
						}
						case InstaMediaType.Carousel when currentAmount > carouselAmount:
						{
							goto Finished;
						}
						default: continue;
					}
				}

				Finished:

				if (selectedMedia.MediaData == null || selectedMedia.MediaData.Count <= 0)
				{
					results.IsSuccessful = false;
					results.Info = new ErrorResponse
					{
						Message = $"could not find any good image to post, user: {_user.AccountId}, instaId: {_user.InstagramAccountUsername}",
						StatusCode = System.Net.HttpStatusCode.NotFound
					};
					return results;
				}

				var @event = new EventActionModel($"CreatePost_{selectedMedia.MediaType.ToString()}_{_actionOptions.StrategySettings.StrategyType.ToString()}")
				{
					ActionType = ActionType.CreatePost,
					User = new UserStore
					{
						InstagramAccountUser = _user.InstagramAccountUser,
						AccountId = _user.AccountId,
						InstagramAccountUsername = _user.InstagramAccountUsername
					}
				};

				switch (selectedMedia.MediaType)
				{
					case InstaMediaType.Image:
					{
						var imageData = selectedMedia.MediaData.FirstOrDefault();
						var imageUpload = new InstaImageUpload
						{
							Uri = imageData.Url,
							ImageBytes = _actionOptions.PostAnalyser.Manager.DownloadMedia(imageData.Url)
						};
						var selectedImageMedia = imageData.SelectedMedia.ObjectItem.Medias.FirstOrDefault();

						var credit = selectedImageMedia.User?.Username;
						MediaInfo mediaInfo;

						if (selectedImageMedia.Topic == null)
							mediaInfo = await _contentInfoBuilder.GenerateMediaInfo(_user.Profile.ProfileTopic, null,
								credit, SecureRandom.Next(20, 28), new[] {imageData.Url});
						else
							mediaInfo = await _contentInfoBuilder.GenerateMediaInfo(_user.Profile.ProfileTopic,
								selectedImageMedia.Topic, credit, SecureRandom.Next(20, 28));

						if (!_user.Profile.AdditionalConfigurations.AutoGenerateCaption)
							mediaInfo.Caption = string.Empty;

						var uploadPhoto = new UploadPhotoModel
						{
							MediaTopic = selectedImageMedia.Topic,
							MediaInfo = mediaInfo,
							Image = imageUpload,
							Location = _user.ShortInstagram.Location != null
								? new InstaLocationShort
								{
									Address = _user.ShortInstagram.Location.Address,
									Lat = _user.ShortInstagram.Location.Coordinates.Latitude,
									Lng = _user.ShortInstagram.Location.Coordinates.Longitude,
									Name = _user.ShortInstagram.Location.City
								}
								: null
						};

						@event.DataObjects.Add(new EventBody(uploadPhoto, uploadPhoto.GetType(), executionTime));
						break;
					}
					case InstaMediaType.Video:
					{
						var selectedMediaData = selectedMedia.MediaData.FirstOrDefault();
						var selectedVideoMedia = selectedMediaData.SelectedMedia.ObjectItem.Medias.FirstOrDefault();
						var credit = selectedVideoMedia.User?.Username;

						var mediaInfo = await _contentInfoBuilder.GenerateMediaInfo(_user.Profile.ProfileTopic,
							selectedVideoMedia.Topic, credit, SecureRandom.Next(20, 28));

						if (!_user.Profile.AdditionalConfigurations.AutoGenerateCaption)
							mediaInfo.Caption = string.Empty;

						var video = selectedMedia.MediaData.FirstOrDefault();

						var videoThumb =
							_actionOptions.PostAnalyser.Manipulation.VideoEditor.GenerateVideoThumbnail(video.MediaBytes);

						var videoUri = await UploadToS3(videoThumb, $"VideoThumb_{Guid.NewGuid()}");
						var uploadVideo = new UploadVideoModel
						{
							MediaTopic = selectedVideoMedia.Topic,
							MediaInfo = mediaInfo,
							Location = _user.ShortInstagram.Location != null
								? new InstaLocationShort
								{
									Address = _user.ShortInstagram.Location.Address,
									Lat = _user.ShortInstagram.Location.Coordinates.Latitude,
									Lng = _user.ShortInstagram.Location.Coordinates.Longitude,
									Name = _user.ShortInstagram.Location.City
								}
								: null,
							Video = new InstaVideoUpload
							{
								Video = new InstaVideo
								{
									Uri = video.Url,
									VideoBytes = _actionOptions.PostAnalyser.Manager.DownloadMedia(video.Url)
								},
								VideoThumbnail = new InstaImage
								{
									Uri = videoUri
								}
							}
						};
						@event.DataObjects.Add(new EventBody(uploadVideo, uploadVideo.GetType(), executionTime));
						break;
					}
					case InstaMediaType.Carousel:
					{
							var selectedMediaData = selectedMedia.MediaData.First();
							var selectedCarouselMedia = selectedMediaData.SelectedMedia.ObjectItem.Medias.FirstOrDefault();

							var credit = selectedCarouselMedia.User?.Username;
							MediaInfo mediaInfo;

							if (selectedCarouselMedia.Topic == null)
								mediaInfo = _contentInfoBuilder.GenerateMediaInfo(_user.Profile.ProfileTopic, null,
									credit, SecureRandom.Next(20, 28), new[] { selectedMediaData.Url }).Result;
							else
								mediaInfo = _contentInfoBuilder.GenerateMediaInfo(_user.Profile.ProfileTopic, 
									selectedCarouselMedia.Topic, credit, SecureRandom.Next(20, 28)).Result;
							

							if (!_user.Profile.AdditionalConfigurations.AutoGenerateCaption)
								mediaInfo.Caption = string.Empty;

							var uploadAlbum = new UploadAlbumModel
							{
								MediaTopic = selectedCarouselMedia.Topic,
								MediaInfo = mediaInfo,
								Location = _user.ShortInstagram.Location != null ? new InstaLocationShort
								{
									Address = _user.ShortInstagram.Location.Address,
									Lat = _user.ShortInstagram.Location.Coordinates.Latitude,
									Lng = _user.ShortInstagram.Location.Coordinates.Longitude,
									Name = _user.ShortInstagram.Location.City
								} : null,
								Album = selectedMedia.MediaData.Select(f => new InstaAlbumUpload
								{
									ImageToUpload = f.MediaType == InstaMediaType.Image ? new InstaImageUpload
									{
										Uri = f.Url,
										ImageBytes = _actionOptions.PostAnalyser.Manager.DownloadMedia(f.Url)
									} : null,
									VideoToUpload = f.MediaType == InstaMediaType.Video ? new InstaVideoUpload
									{
										Video = new InstaVideo
										{
											Uri = f.Url,
											VideoBytes = _actionOptions.PostAnalyser.Manager.DownloadMedia(f.Url)
										},
										VideoThumbnail = new InstaImage
										{
											Uri = UploadToS3(_actionOptions.PostAnalyser.Manipulation.VideoEditor.GenerateVideoThumbnail(f.MediaBytes), $"VideoThumb_{Guid.NewGuid()}").GetAwaiter().GetResult()
										}
									} : null
								}).ToArray(),
							};
							@event.DataObjects.Add(new EventBody(uploadAlbum, uploadAlbum.GetType(), executionTime));
							break;
					}
					default: throw new Exception("Media Type not selected");
				}
				
				if (!@event.DataObjects.Any())
				{
					results.IsSuccessful = false;
					results.Info = new ErrorResponse
					{
						Message = $"@CreatePostAction, could not add selected media"
					};
					return results;
				}

				results.IsSuccessful = true;
				results.Results = @event;
				return results;
			}
			catch (Exception err)
			{
				Console.WriteLine(err);
				results.IsSuccessful = false;
				results.Info = new ErrorResponse
				{
					Message = $"{err.Message}, user: {_user.AccountId}, instaId: {_user.InstagramAccountUsername}",
					StatusCode = System.Net.HttpStatusCode.InternalServerError,
					Exception = err
				};
				return results;
			}
			finally
			{
				Console.WriteLine($"Create Media Action Ended: {_user.AccountId}, {_user.InstagramAccountUsername}, {_user.InstagramAccountUser}");
			}
		}

		public IActionCommit ModifyOptions(IActionOptions newOptions)
		{
			try
			{
				_actionOptions = newOptions as PostActionOptions;
			}
			catch (Exception err)
			{
				Console.WriteLine(err);
			}
			return this;
		}
	}
}
