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
using Quarkless.Models.Actions.Enums.ActionTypes;
using Quarkless.Models.Actions.Factory.Action_Options;
using Quarkless.Models.Actions.Interfaces;
using Quarkless.Models.Actions.Models;
using Quarkless.Models.Common.Enums;
using Quarkless.Models.Common.Extensions;
using Quarkless.Models.Common.Models;
using Quarkless.Models.Common.Models.Carriers;
using Quarkless.Models.ContentInfo.Interfaces;
using Quarkless.Models.HashtagGenerator;
using Quarkless.Models.Heartbeat;
using Quarkless.Models.Heartbeat.Interfaces;
using Quarkless.Models.Lookup.Interfaces;
using Quarkless.Models.Media;
using Quarkless.Models.Profile.Enums;
using Quarkless.Models.SearchResponse;
using Quarkless.Models.SearchResponse.Enums;

namespace Quarkless.Logic.Actions.Action_Instances
{
	internal class CreatePostAction : BaseAction, IActionCommit
	{
		private readonly IContentInfoBuilder _contentInfoBuilder;
		private readonly IHeartbeatLogic _heartbeatLogic;
		private readonly UserStoreDetails _user;
		private PostActionOptions _actionOptions;

		internal CreatePostAction(UserStoreDetails userStoreDetails, IContentInfoBuilder contentInfoBuilder,
			IHeartbeatLogic heartbeatLogic, ILookupLogic lookupLogic)
			: base(lookupLogic, ActionType.CreatePost, userStoreDetails)
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
		private async Task<MediaData> ProcessVideo(string mediaUrl)
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

			var s3UrlLink = await UploadToS3(videoBytes, $"Video_{videoBytes.GetHashCode()}_{Guid.NewGuid()}");
			
			return new MediaData
			{
				MediaType = InstaMediaType.Video,
				MediaBytes = videoBytes,
				Url = s3UrlLink
			};
		}
		private async Task<(List<Meta<Media>>, MetaDataType)> GetMediaFromSearchType(SearchType searchTypeSelected)
		{
			switch (searchTypeSelected)
			{
				case SearchType.Google:
					{
						var googleResults = await _heartbeatLogic.GetMetaData<Media>(new MetaDataFetchRequest
						{
							MetaDataType = MetaDataType.FetchMediaForSpecificUserGoogle,
							ProfileCategoryTopicId = _user.Profile.ProfileTopic.Category._id,
							InstagramId = _user.Profile.InstagramAccountId,
							AccountId = _user.AccountId
						});

						return (googleResults != null ? googleResults.ToList() : new List<Meta<Media>>(),
							MetaDataType.FetchMediaForSpecificUserGoogle);
					}
				case SearchType.Instagram:
					{
						var optionPick = new List<Chance<MetaDataType>>();

						if (_user.Profile.AdditionalConfigurations.EnableOnlyAutoRepostFromUserTargetList)
						{
							optionPick.Add(new Chance<MetaDataType>
							{
								Object = MetaDataType.FetchMediaByUserTargetList,
								Probability = 0.50
							});
						}
						else
						{
							optionPick.Add(new Chance<MetaDataType>
							{
								Object = MetaDataType.FetchMediaByTopic,
								Probability = 0.50
							});
							if (_user.Profile.UserTargetList != null && _user.Profile.UserTargetList.Any())
							{
								optionPick.Add(new Chance<MetaDataType>
								{
									Object = MetaDataType.FetchMediaByUserTargetList,
									Probability = 0.50
								});
							}
							if (_user.Profile.LocationTargetList != null && _user.Profile.LocationTargetList.Any())
							{
								optionPick.Add(new Chance<MetaDataType>
								{
									Object = MetaDataType.FetchMediaByUserLocationTargetList,
									Probability = 0.50
								});
							}
						}

						var selectedMetaDataType = SecureRandom.ProbabilityRoll(optionPick);

						var resultsFromMetaData = (await _heartbeatLogic.GetMetaData<Media>(new MetaDataFetchRequest
						{
							MetaDataType = selectedMetaDataType,
							ProfileCategoryTopicId = _user.Profile.ProfileTopic.Category._id,
							InstagramId = _user.ShortInstagram.Id,
							AccountId = _user.AccountId
						}))?.ToList();

						if (resultsFromMetaData == null || !resultsFromMetaData.Any() 
							&& selectedMetaDataType != MetaDataType.FetchMediaByTopic
							&& !_user.Profile.AdditionalConfigurations.EnableOnlyAutoRepostFromUserTargetList)
						{
							resultsFromMetaData = (await _heartbeatLogic.GetMetaData<Media>(new MetaDataFetchRequest
							{
								MetaDataType = MetaDataType.FetchMediaByTopic,
								ProfileCategoryTopicId = _user.Profile.ProfileTopic.Category._id,
								InstagramId = _user.ShortInstagram.Id
							}))?.ToList();
						}

						return (resultsFromMetaData ?? new List<Meta<Media>>(), selectedMetaDataType);
					}
				case SearchType.Yandex:
					{
						var yandexResults = await _heartbeatLogic.GetMetaData<Media>(new MetaDataFetchRequest
						{
							MetaDataType = MetaDataType.FetchMediaForSpecificUserYandex,
							ProfileCategoryTopicId = _user.Profile.ProfileTopic.Category._id,
							InstagramId = _user.ShortInstagram.Id,
							AccountId = _user.AccountId
						});
						return (yandexResults != null ? yandexResults.ToList() : new List<Meta<Media>>(),
							MetaDataType.FetchMediaForSpecificUserYandex);
					}
				default: throw new Exception("Search type was invalid");
			}
		}

		private TempSelect FindImageFromList(in List<Meta<Media>> mediaItems, MetaDataType selectedAction)
		{
			var selectedMedia = new TempSelect();
			var filterMediaSize = new Size(850, 850);

			foreach (var mediaItem in mediaItems)
			{
				var media = mediaItem.ObjectItem?.Medias.FirstOrDefault();
				if (media == null) continue;

				AddObjectToLookup(media.MediaId).GetAwaiter().GetResult();

				var imageUrl = media.MediaUrl.FirstOrDefault();

				var resultImage = ProcessImage(imageUrl, filterMediaSize).Result;
				if (resultImage == null)
					continue;

				selectedMedia.MediaType = InstaMediaType.Image;
				resultImage.SelectedMedia = mediaItem;
				selectedMedia.MediaData.Add(resultImage);
				break;
			}

			return selectedMedia;
		}
		private TempSelect FindVideoFromList(in List<Meta<Media>> mediaItems, MetaDataType selectedAction)
		{
			var selectedMedia = new TempSelect();

			foreach (var mediaItem in mediaItems)
			{
				var media = mediaItem.ObjectItem?.Medias.FirstOrDefault();
				if (media == null) continue;

				AddObjectToLookup(media.MediaId).GetAwaiter().GetResult();

				var videoUrl = media.MediaUrl.FirstOrDefault();
				var resultVideo = ProcessVideo(videoUrl).Result;
				if (resultVideo == null)
					continue;

				selectedMedia.MediaType = InstaMediaType.Video;
				resultVideo.SelectedMedia = mediaItem;
				selectedMedia.MediaData.Add(resultVideo);
				break;
			}

			return selectedMedia;
		}
		private TempSelect FindCarouselFromList(in List<Meta<Media>> mediaItems, MetaDataType selectedAction)
		{
			var selectedMedia = new TempSelect();
			var filterMediaSize = new Size(850, 850);
			var carouselAmount = SecureRandom.Next(2, 4);
			var currentAmount = 0;
			const int howManyMediasToTakeFromInstagramCarousel = 2;

			foreach (var mediaItem in mediaItems)
			{
				if (currentAmount > carouselAmount)
					break;

				foreach (var objectItemMedia in mediaItem.ObjectItem.Medias)
				{
					if(objectItemMedia == null)
						continue;

					AddObjectToLookup(objectItemMedia.MediaId).GetAwaiter().GetResult();

					var fromInsta = objectItemMedia.MediaFrom == MediaFrom.Instagram;
					
					switch (objectItemMedia.MediaType)
					{
						case InstaMediaType.Image:
						{
							var imageResponse = ProcessImage(objectItemMedia.MediaUrl.First(), filterMediaSize).Result;
							if (imageResponse == null)
								continue;

							//if there are already images in the list (make sure they are the same ratio)
							if (selectedMedia.MediaData.Count > 0) 
							{
								var oas = _actionOptions.PostAnalyser.Manipulation.ImageEditor
									.GetClosestAspectRatio(selectedMedia.MediaData[0].MediaBytes);

								var cas = _actionOptions.PostAnalyser.Manipulation.ImageEditor
									.GetClosestAspectRatio(imageResponse.MediaBytes);

								if (Math.Abs(oas - cas) > 0.06) continue;

								selectedMedia.MediaType = InstaMediaType.Carousel;
								imageResponse.SelectedMedia = mediaItem;
								selectedMedia.MediaData.Add(imageResponse);
							}
							else
							{
								selectedMedia.MediaType = InstaMediaType.Carousel;
								imageResponse.SelectedMedia = mediaItem;
								selectedMedia.MediaData.Add(imageResponse);
							}

							currentAmount++;
							continue;
						}
						case InstaMediaType.Video when fromInsta:
						{
							var resultCarouselVideo = ProcessVideo(objectItemMedia.MediaUrl.First()).Result;
							if (resultCarouselVideo == null)
								continue;

							selectedMedia.MediaType = InstaMediaType.Carousel;
							resultCarouselVideo.SelectedMedia = mediaItem;
							selectedMedia.MediaData.Add(resultCarouselVideo);

							carouselAmount++;
							break;
						}
						case InstaMediaType.Video:
							continue;
						case InstaMediaType.Carousel:
						{
							if(!objectItemMedia.MediaUrl.Any()) continue;

							foreach (var mediaUrl in objectItemMedia.MediaUrl.Take(howManyMediasToTakeFromInstagramCarousel))
							{
								var imageResponse = ProcessImage(mediaUrl, filterMediaSize).Result;
								if (imageResponse == null)
									continue;

								if (imageResponse.IncorrectFormat) //is video
								{
									var resultCarouselVideo = ProcessVideo(mediaUrl).Result;
									if (resultCarouselVideo == null)
										continue;

									selectedMedia.MediaType = InstaMediaType.Carousel;
									resultCarouselVideo.SelectedMedia = mediaItem;
									selectedMedia.MediaData.Add(resultCarouselVideo);

									carouselAmount++;
								}
								else
								{
									if (selectedMedia.MediaData.Count > 0)
									{
										var oas = _actionOptions.PostAnalyser.Manipulation.ImageEditor
											.GetClosestAspectRatio(selectedMedia.MediaData[0].MediaBytes);

										var cas = _actionOptions.PostAnalyser.Manipulation.ImageEditor
											.GetClosestAspectRatio(imageResponse.MediaBytes);

										if (Math.Abs(oas - cas) > 0.1) continue;

										selectedMedia.MediaType = InstaMediaType.Carousel;
										imageResponse.SelectedMedia = mediaItem;
										selectedMedia.MediaData.Add(imageResponse);
									}
									else
									{
										selectedMedia.MediaType = InstaMediaType.Carousel;
										imageResponse.SelectedMedia = mediaItem;
										selectedMedia.MediaData.Add(imageResponse);
									}

									currentAmount++;
								}
							}

							break;
						}
					}
				}
			}

			return selectedMedia;
		}

		public async Task<ResultCarrier<EventActionModel>> PushAsync(DateTimeOffset executionTime)
		{
			if(_actionOptions == null)
				throw new Exception("Action Option cannot be null");
			Console.WriteLine($"Create Media Action Started: {_user.AccountId}, {_user.InstagramAccountUsername}, {_user.InstagramAccountUser}");
			
			//todo: first select the action type
			//if image then select from all search types
			//if video then select from instagram only
			//if carousel then either instagram carousel types (or all search types and create carousel)

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
				PostMediaActionType actionType;
				if (_actionOptions.PostMediaActionType == PostMediaActionType.Any)
				{
					var likeActionsChances = new List<Chance<PostMediaActionType>>
					{
						new Chance<PostMediaActionType> {Object = PostMediaActionType.Image, Probability = 0.55},
						new Chance<PostMediaActionType> {Object = PostMediaActionType.Video, Probability = 0.20},
						new Chance<PostMediaActionType> {Object = PostMediaActionType.Carousel, Probability = 0.25},
					};
					actionType = SecureRandom.ProbabilityRoll(likeActionsChances);
				}
				else
				{
					actionType = _actionOptions.PostMediaActionType;
				}

				var allowedSearchTypes = GetValidSearchTypes(); // to not include based on user

				switch (actionType)	//to not include based on post type
				{
					case PostMediaActionType.Image: //all/any search types allowed
					case PostMediaActionType.Carousel: //all/any search types (but instagram only if video included too)
						break;
					case PostMediaActionType.Video: //only instagram
						allowedSearchTypes.RemoveAll(_ => _ == SearchType.Google || _ == SearchType.Yandex);
						break;
				}

				var searchTypeSelected = allowedSearchTypes[SecureRandom.Next(allowedSearchTypes.Count - 1)];
				var totalResults = await GetMediaFromSearchType(searchTypeSelected);

				if(!totalResults.Item1.Any())
				{
					results.IsSuccessful = false;
					results.Info = new ErrorResponse
					{
						Message = $"no action selected or results were empty, user: {_user.AccountId}, instaId: {_user.InstagramAccountUsername}",
						StatusCode = System.Net.HttpStatusCode.NotFound
					};
					return results;
				}

				var lookups = await GetLookupItems();

				// remove any duplicates (not reliable will need to make sure they are not refreshed)
				var filteredResults = totalResults.Item1;

				//get meta medias by the action type selected (e.g. video, image or carousel)
				var filterByMediaType = filteredResults.Select(meta =>
					new Meta<Media>(new Media
						{
							Errors = meta.ObjectItem?.Errors ?? 0,
							Medias = meta.ObjectItem?.Medias
								?.Where(a=>a.MediaType == (InstaMediaType)((int)actionType)
										&& !lookups.Exists(_=>_.ObjectId == a.MediaId)).ToList() 
								?? new List<MediaResponse>()
						})).Shuffle().ToList();

				
				if ((!filterByMediaType.Any() || !filterByMediaType.All(s=>s.ObjectItem.Medias.Any())))
				{
					//if no carousel post was found
					if (actionType == PostMediaActionType.Carousel)
					{
						// create carousel from images
						filterByMediaType = filteredResults.Select(meta =>
							new Meta<Media>(new
								Media
								{
									Errors = meta.ObjectItem.Errors,
									Medias = meta.ObjectItem?.Medias
									?.Where(a => a.MediaType == InstaMediaType.Image)?.ToList()
									?? new List<MediaResponse>()})).Shuffle().ToList();
					}
					else
					{
						results.IsSuccessful = false;
						results.Info = new ErrorResponse
						{
							Message = $"could not find any good {actionType.GetDescription()} to post, user: {_user.AccountId}, instaId: {_user.InstagramAccountUsername}",
							StatusCode = System.Net.HttpStatusCode.NotFound
						};
						return results;
					}
				}

				var selectedMedia = new TempSelect();

				switch (actionType)
				{
					case PostMediaActionType.Image:
						selectedMedia = FindImageFromList(filterByMediaType, totalResults.Item2);
						break;
					case PostMediaActionType.Video:
						selectedMedia = FindVideoFromList(filterByMediaType, totalResults.Item2);
						break;
					case PostMediaActionType.Carousel:
						selectedMedia = FindCarouselFromList(filterByMediaType, totalResults.Item2);
						break;
				}

				if (selectedMedia.MediaData == null || selectedMedia.MediaData.Count <= 0)
				{
					results.IsSuccessful = false;
					results.Info = new ErrorResponse
					{
						Message = $"could not find any good {actionType.GetDescription()} to post, user: {_user.AccountId}, instaId: {_user.InstagramAccountUsername}",
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
				
				//if carousel was picked and we only have one media (change their type)
				if (selectedMedia.MediaType == InstaMediaType.Carousel && selectedMedia.MediaData.Count <= 1)
				{
					var selectedMediaData = selectedMedia.MediaData.First();
					var downloaded = _actionOptions.PostAnalyser.Manager.DownloadMedia(selectedMediaData.Url);
					selectedMedia.MediaType = downloaded.IsValidImage() ? InstaMediaType.Image : InstaMediaType.Video;
				}

				switch (selectedMedia.MediaType)
				{
					case InstaMediaType.Image:
					{
						var imageData = selectedMedia.MediaData.First();
						
						var imageUpload = new InstaImageUpload
						{
							Uri = imageData.Url
							#region Commented out because slows down when storing
							/* ImageBytes = _actionOptions.PostAnalyser.Manipulation.ImageEditor
								.ResizeToClosestAspectRatio(_actionOptions.PostAnalyser.Manager
									.DownloadMedia(imageData.Url))
							*/
							#endregion
						};

						var selectedImageMedia = imageData.SelectedMedia.ObjectItem.Medias.First();

						var credit = selectedImageMedia.User?.Username;
						var mediaInfo = await _contentInfoBuilder
							.GenerateMediaInfo(new Source
							{
								ProfileTopic = _user.Profile.ProfileTopic,
								MediaTopic = selectedImageMedia?.Topic,
								ImageBytes = new []
								{
									_actionOptions.PostAnalyser.Manager
										.DownloadMedia(imageData.Url)
								}
							},
							credit,
							hashtagPickAmount: SecureRandom.Next(20,25),
							defaultCaption: _user.Profile.AdditionalConfigurations.DefaultCaption,
							generateCaption: _user.Profile.AdditionalConfigurations.AutoGenerateCaption);

						mediaInfo.MediaType = InstaMediaType.Image;
						
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
						var selectedMediaData = selectedMedia.MediaData.First();
						var selectedVideoMedia = selectedMediaData.SelectedMedia.ObjectItem.Medias.First();
						var credit = selectedVideoMedia.User?.Username;

						var mediaInfo = await _contentInfoBuilder
							.GenerateMediaInfo(new Source
								{
									ProfileTopic = _user.Profile.ProfileTopic,
									MediaTopic = selectedVideoMedia.Topic
							},
								credit,
								hashtagPickAmount: SecureRandom.Next(20, 25),
								defaultCaption: _user.Profile.AdditionalConfigurations.DefaultCaption,
								generateCaption: _user.Profile.AdditionalConfigurations.AutoGenerateCaption);

						var video = selectedMedia.MediaData.First();

						var videoThumb =
							_actionOptions.PostAnalyser.Manipulation.VideoEditor.GenerateVideoThumbnail(video.MediaBytes);

						var videoUri = await UploadToS3(videoThumb, $"VideoThumb_{Guid.NewGuid()}");

						mediaInfo.MediaType = InstaMediaType.Video;
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
									//VideoBytes = _actionOptions.PostAnalyser.Manager.DownloadMedia(video.Url)
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
							var selectedCarouselMedia = selectedMediaData.SelectedMedia.ObjectItem.Medias.First();

							var credit = selectedCarouselMedia.User?.Username;
							MediaInfo mediaInfo;

							var downloaded = _actionOptions.PostAnalyser.Manager.DownloadMedia(selectedMediaData.Url);

							if (downloaded.IsValidImage())
								mediaInfo = await _contentInfoBuilder.GenerateMediaInfo(
									new Source
									{
										ProfileTopic = _user.Profile.ProfileTopic,
										MediaTopic = selectedCarouselMedia?.Topic,
										ImageBytes = new [] {downloaded}
									},
									credit,
									hashtagPickAmount: SecureRandom.Next(20, 25),
									defaultCaption: _user.Profile.AdditionalConfigurations.DefaultCaption,
									generateCaption: _user.Profile.AdditionalConfigurations.AutoGenerateCaption);
							else
								mediaInfo = await _contentInfoBuilder.GenerateMediaInfo(
									new Source
									{
										ProfileTopic = _user.Profile.ProfileTopic,
										MediaTopic = selectedCarouselMedia?.Topic,
									},
									credit,
									hashtagPickAmount: SecureRandom.Next(20, 25),
									defaultCaption: _user.Profile.AdditionalConfigurations.DefaultCaption,
									generateCaption: _user.Profile.AdditionalConfigurations.AutoGenerateCaption);

							mediaInfo.MediaType = InstaMediaType.Carousel;
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
										//ImageBytes = _actionOptions.PostAnalyser.Manager.DownloadMedia(f.Url)
									} : null,
									VideoToUpload = f.MediaType == InstaMediaType.Video ? new InstaVideoUpload
									{
										Video = new InstaVideo
										{
											Uri = f.Url,
											//VideoBytes = _actionOptions.PostAnalyser.Manager.DownloadMedia(f.Url)
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
