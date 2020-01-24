using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using InstagramApiSharp.Classes.Models;
using MoreLinq;
using Newtonsoft.Json;
using Quarkless.Analyser.Extensions;
using Quarkless.Models.Common.Enums;
using Quarkless.Models.Common.Extensions;
using Quarkless.Models.Common.Models;
using Quarkless.Models.Common.Models.Carriers;
using Quarkless.Models.ContentInfo.Interfaces;
using Quarkless.Models.Heartbeat;
using Quarkless.Models.Heartbeat.Interfaces;
using Quarkless.Models.Media;
using Quarkless.Models.Profile.Enums;
using Quarkless.Models.RequestBuilder.Interfaces;
using Quarkless.Models.SearchResponse;
using Quarkless.Models.Services.Automation.Interfaces;
using Quarkless.Models.Services.Automation.Models.ActionOptions;
using Quarkless.Models.Services.Automation.Models.PostAction;
using Quarkless.Models.Services.Automation.Models.StrategySettings;
using Quarkless.Models.Storage.Interfaces;
using Quarkless.Models.Timeline;

namespace Quarkless.Logic.Services.Automation.Actions.EngageActions
{
	class CreatePost : IActionCommit
	{
		private UserStoreDetails user;
		private readonly IContentInfoBuilder _builder;
		private readonly IHeartbeatLogic _heartbeatLogic;
		private readonly IUrlReader _urlReader;
		private IS3BucketLogic _s3BucketLogic;
		private ImageStrategySettings imageStrategySettings;

		public CreatePost(IContentInfoBuilder builder, IHeartbeatLogic heartbeatLogic, IUrlReader urlReader)
		{
			_heartbeatLogic = heartbeatLogic;
			_builder = builder;
			_urlReader = urlReader;
		}

		public IActionCommit IncludeStorage(IStorage storage)
		{
			_s3BucketLogic = storage as IS3BucketLogic;
			return this;
		}
		public IActionCommit IncludeStrategy(IStrategySettings strategy)
		{
			imageStrategySettings = strategy as ImageStrategySettings;
			return this;
		}
		public IActionCommit IncludeUser(UserStoreDetails userStoreDetails)
		{
			user = userStoreDetails;
			return this;
		}

		#region Private Methods
		private async Task<string> UploadToS3(byte[] media, string keyName)
		{
			await using var mediaStream = new MemoryStream(media);
			return await _s3BucketLogic.UploadStreamFile(mediaStream, keyName);
		}

		#endregion

		public ResultCarrier<IEnumerable<TimelineEventModel>> Push(IActionOptions actionOptions)
		{
			Console.WriteLine($"Create Media Action Started: {user.OAccountId}, {user.OInstagramAccountUsername}, {user.OInstagramAccountUser}");
			var imageActionOptions = actionOptions as PostActionOptions;

			if (imageActionOptions == null) throw new ArgumentNullException();
			var postAnalyser = imageActionOptions.PostAnalyser;
			var results = new ResultCarrier<IEnumerable<TimelineEventModel>>();
			if (user == null)
			{
				results.IsSuccessful = false;
				results.Info = new ErrorResponse
				{
					Message = $"user is null, user: {user.OAccountId}, instaId: {user.OInstagramAccountUsername}",
					StatusCode = System.Net.HttpStatusCode.NotFound
				};
				return results;
			}
			if (!user.Profile.AdditionalConfigurations.EnableAutoPosting)
			{
				results.IsSuccessful = true;
				results.Info = new ErrorResponse
				{
					Message = $"User {user.OAccountId} of {user.OInstagramAccountUsername} has disabled auto posting"
				};
				return results;
			}
			if (!user.Profile.ProfileTopic.Topics.Any())
			{
				results.IsSuccessful = false;
				results.Info = new ErrorResponse
				{
					Message =
						$"Profile Topics is empty, user: {user.OAccountId}, instaId: {user.OInstagramAccountUsername}",
					StatusCode = System.Net.HttpStatusCode.NotFound
				};
				return results;
			}
			try
			{
				var totalResults = new List<Meta<Media>>();
				var selectedAction = MetaDataType.None;
				var allowedSearchTypes = new List<SearchType>();

				if(user.Profile.Theme.ImagesLike!=null)
					if (user.Profile.Theme.ImagesLike.Count > 0)
						allowedSearchTypes.Add(SearchType.Yandex);
				
				if(user.Profile.AdditionalConfigurations.AllowRepost)
					allowedSearchTypes.Add(SearchType.Instagram);

				allowedSearchTypes.Add(SearchType.Google);

				var searchTypeSelected = allowedSearchTypes[SecureRandom.Next(allowedSearchTypes.Count)];

				switch (searchTypeSelected)
				{
					case SearchType.Google:
						var googleResults = _heartbeatLogic.GetMetaData<Media>(new MetaDataFetchRequest
						{
							MetaDataType = MetaDataType.FetchMediaForSpecificUserGoogle,
							ProfileCategoryTopicId = user.Profile.ProfileTopic.Category._id,
							InstagramId = user.Profile.InstagramAccountId
						}).Result;

						selectedAction = MetaDataType.FetchMediaForSpecificUserGoogle;
						if (googleResults != null)
							totalResults = googleResults.ToList();
						break;
					case SearchType.Instagram:
						if (user.Profile.UserTargetList != null && user.Profile.UserTargetList.Any())
						{
							var action = new[] { 0, 1 };
							var pickedRandom = action.ElementAt(SecureRandom.Next(action.Length-1));
							var userId = user.ShortInstagram.Id;
							var selected = MetaDataType.FetchMediaByUserTargetList;
							if(pickedRandom == 1)
							{
								userId = null;
								selected = MetaDataType.FetchMediaByTopic;
							}

							totalResults = _heartbeatLogic.GetMetaData<Media>(new MetaDataFetchRequest
							{
								MetaDataType = selected,
								ProfileCategoryTopicId = user.Profile.ProfileTopic.Category._id,
								InstagramId = userId
							}).Result.ToList();
							
							selectedAction = selected;
						}
						else { 
							totalResults = _heartbeatLogic.GetMetaData<Media>(new MetaDataFetchRequest
							{
								MetaDataType = MetaDataType.FetchMediaByTopic,
								ProfileCategoryTopicId = user.Profile.ProfileTopic.Category._id,
								InstagramId = user.ShortInstagram.Id
							}).Result.ToList();
							
							selectedAction = MetaDataType.FetchMediaByTopic;
						}
						break;
					case SearchType.Yandex:
						if (user.Profile.Theme.ImagesLike != null && user.Profile.Theme.ImagesLike.Count > 0)
						{
							var yandexResults = _heartbeatLogic.GetMetaData<Media>(new MetaDataFetchRequest
							{
								MetaDataType = MetaDataType.FetchMediaForSpecificUserYandex,
								ProfileCategoryTopicId = user.Profile.ProfileTopic.Category._id,
								InstagramId = user.ShortInstagram.Id
							}).Result;
							
							selectedAction = MetaDataType.FetchMediaForSpecificUserYandex;
							if (yandexResults != null)
								totalResults = yandexResults.ToList();
						}
						break;
				}
				if(selectedAction == MetaDataType.None || totalResults.Count <=0)
				{
					results.IsSuccessful = false;
					results.Info = new ErrorResponse
					{
						Message = $"no action selected, user: {user.OAccountId}, instaId: {user.OInstagramAccountUsername}",
						StatusCode = System.Net.HttpStatusCode.NotFound
					};
					return results;
				};

				var by = new By
				{
					ActionType = (int)ActionType.CreatePost,
					User = user.Profile.InstagramAccountId
				};

				var filteredResults = totalResults.Where(exclude=>!exclude.SeenBy.Any(e=>e.User == by.User
				&& (e.ActionType == by.ActionType))).ToList();

				var selectedMedia = new TempSelect();
				var size = new System.Drawing.Size(850,850);

				var typeOfPost = new List<Chance<InstaMediaType>>()
				{
					new Chance<InstaMediaType> {Object = InstaMediaType.Video, Probability = 0.25 },
					new Chance<InstaMediaType>{ Object = InstaMediaType.Image, Probability = 0.30 },
					new Chance<InstaMediaType>{ Object = InstaMediaType.Carousel, Probability = 0.45 }
				};
				var typeSelected = SecureRandom.ProbabilityRoll(typeOfPost);

				var carouselAmount = SecureRandom.Next(2,4);
				var currentAmount = 0;

				var enteredType = InstaMediaType.All;
				foreach (var result in filteredResults.Shuffle())
				{
					var media = result.ObjectItem.Medias.FirstOrDefault();
					if (media == null) continue;
					if (enteredType != InstaMediaType.All)
					{
						if (media.MediaType != enteredType)
						{
							continue;
						}
					}

					result.SeenBy.Add(by);

					if (selectedAction == MetaDataType.FetchMediaByTopic)
					{
						_heartbeatLogic.UpdateMetaData(new MetaDataCommitRequest<Media>
							{
								MetaDataType = selectedAction, 
								ProfileCategoryTopicId = user.Profile.ProfileTopic.Category._id,
								InstagramId = user.ShortInstagram.Id,
								Data = result
							}).GetAwaiter().GetResult();
					}
					else
					{
						_heartbeatLogic.UpdateMetaData(new MetaDataCommitRequest<Media>
						{
							MetaDataType = selectedAction, 
							ProfileCategoryTopicId = user.Profile.ProfileTopic.Category._id,
							InstagramId = user.Profile.InstagramAccountId,
							Data = result
						}).GetAwaiter().GetResult();
					}

					if (media.MediaType == InstaMediaType.Carousel && currentAmount < carouselAmount)
					{
						enteredType = InstaMediaType.Carousel;
						foreach(var url in media.MediaUrl)
						{
							var imBytes =  postAnalyser.Manager.DownloadMedia(url);
							if (imBytes == null) continue;

							if (imBytes.IsValidImage())
							{
								if(!postAnalyser.Manipulation.ImageEditor.IsImageGood(imBytes, 
									user.Profile.Theme.Colors.Select(s => System.Drawing.Color.FromArgb(s.Red, s.Green, s.Blue)), 
									user.Profile.Theme.Percentage, size)) continue;
								var s3UrlLink = UploadToS3(postAnalyser.Manipulation.ImageEditor.ResizeToClosestAspectRatio(imBytes), $"Image_{imBytes.GetHashCode()}_{Guid.NewGuid()}").GetAwaiter().GetResult();
								if (selectedMedia.MediaData.Count > 0)
								{
									var oas = postAnalyser.Manipulation.ImageEditor.GetClosestAspectRatio(selectedMedia
										.MediaData[0].MediaBytes);
									var cas = postAnalyser.Manipulation.ImageEditor.GetClosestAspectRatio(imBytes);
									if (Math.Abs(oas - cas) > 0.5) continue;

									//add to the list of images to send
									selectedMedia.MediaType = InstaMediaType.Carousel;
									selectedMedia.MediaData.Add(new MediaData
									{
										Url = s3UrlLink,
										MediaBytes = imBytes,
										SelectedMedia = result,
										MediaType = InstaMediaType.Image
									});

									currentAmount++;
								}
								else
								{	
									//add to the list of images to send
									selectedMedia.MediaType = InstaMediaType.Carousel;
									selectedMedia.MediaData.Add(new MediaData
									{
										Url = s3UrlLink,
										MediaBytes = imBytes,
										SelectedMedia = result,
										MediaType = InstaMediaType.Image
									});

									currentAmount++;
								}
							}
							else
							{
								if(!postAnalyser.Manipulation.VideoEditor.IsVideoGood(imBytes, 
									user.Profile.Theme.Colors
										.Select(s => System.Drawing.Color.FromArgb(s.Red, s.Green, s.Blue))
										.ElementAt(user.Profile.Theme.Colors.Count - 1), 
									user.Profile.Theme.Percentage, 10)) continue;

								selectedMedia.MediaType = InstaMediaType.Carousel;
								var s3UrlLink = UploadToS3(imBytes, $"Video_{imBytes.GetHashCode()}_{Guid.NewGuid()}").GetAwaiter().GetResult();
								selectedMedia.MediaData.Add(new MediaData
								{
									Url = s3UrlLink,
									MediaBytes = imBytes,
									SelectedMedia = result,
									MediaType = InstaMediaType.Video
								});

								currentAmount++;
							}
						}
					}
					else if(media.MediaType == InstaMediaType.Video)
					{
						enteredType = InstaMediaType.Video;
						var url = media.MediaUrl.FirstOrDefault();
						var bytes = postAnalyser.Manager.DownloadMedia(url);
						if(!postAnalyser.Manipulation.VideoEditor.IsVideoGood(bytes,
							user.Profile.Theme.Colors
								.Select(s => System.Drawing.Color.FromArgb(s.Red, s.Green, s.Blue))
								.ElementAt(user.Profile.Theme.Colors.Count - 1),
							user.Profile.Theme.Percentage, 10)) continue;
						
						//add to the list of images to send
						var s3UrlLink = UploadToS3(bytes, $"Video_{bytes.GetHashCode()}_{Guid.NewGuid()}").GetAwaiter().GetResult();
						selectedMedia.MediaType = InstaMediaType.Video;
						selectedMedia.MediaData.Add(new MediaData
						{
							Url = s3UrlLink,
							MediaBytes = bytes,
							SelectedMedia = result,
							MediaType = InstaMediaType.Video
						});
						
						break;
					}
					else if(media.MediaType == InstaMediaType.Image)
					{
						enteredType = InstaMediaType.Image;
						var url = media.MediaUrl.FirstOrDefault();
						var imBytes = postAnalyser.Manager.DownloadMedia(url);
						if (!postAnalyser.Manipulation.ImageEditor.IsImageGood(imBytes,
							user.Profile.Theme.Colors.Select(s => System.Drawing.Color.FromArgb(s.Red, s.Green, s.Blue)),
							user.Profile.Theme.Percentage, size)) continue;

						//upload to s3 bucket

						var s3UrlLink = UploadToS3(imBytes, $"Image_{imBytes.GetHashCode()}_{Guid.NewGuid()}").GetAwaiter().GetResult();

						selectedMedia.MediaType = InstaMediaType.Image;
						selectedMedia.MediaData.Add(new MediaData
						{
							MediaBytes = imBytes,
							SelectedMedia = result,
							Url = s3UrlLink,
							MediaType = InstaMediaType.Image
						});
						break;

						#region split long image to carousel

//						if (typeSelected == InstaMediaType.Image)
//						{
//							if (!(imBytes.GetAspectRatio() < 1.7)) continue;
//
//							//add to the list of images to send
//							selectedMedia.MediaData.Add(
//							new MediaData
//							{
//								MediaBytes = imBytes, 
//								SelectedMedia = result
//							});
//
//							selectedMedia.Type = InstaMediaType.Image;
//							break;
//						}
//
//						if (typeSelected != InstaMediaType.Carousel) continue;
//						if (imBytes.GetAspectRatio() > 1.6)
//						{
//							var toCarousel = imBytes.CreateCarousel();
//							selectedMedia.Type = InstaMediaType.Carousel;
//
//							//for each image converted to add to the list of images to send
//							selectedMedia.MediaData = toCarousel.Select(x => 
//								new MediaData
//								{
//									MediaBytes = x, 
//									SelectedMedia = result
//								}).ToList();
//							break;
//						}
//
//						if(imBytes.GetAspectRatio() < 1.6 && currentAmount < carouselAmount )
//						{
//							if (selectedMedia.MediaData.Count > 0)
//							{
//								var oas = selectedMedia.MediaData[0].MediaBytes.GetClosestAspectRatio();
//								var cas = imBytes.GetClosestAspectRatio();
//								if (Math.Abs(oas - cas) > 0.5) continue;
//								selectedMedia.Type = InstaMediaType.Carousel;
//								selectedMedia.MediaData.Add(new MediaData
//								{
//									MediaBytes = imBytes,
//									SelectedMedia = result
//								});
//								currentAmount++;
//							}
//							else
//							{
//								selectedMedia.Type = InstaMediaType.Carousel;
//								selectedMedia.MediaData.Add(new MediaData
//								{
//									MediaBytes = imBytes,
//									SelectedMedia = result
//								});
//								currentAmount++;
//							}
//						}
//						else { break; }

						#endregion

					}
				}

				if (selectedMedia.MediaData == null || selectedMedia.MediaData.Count <= 0)
				{
					results.IsSuccessful = false;
					results.Info = new ErrorResponse
					{
						Message = $"could not find any good image to post, user: {user.OAccountId}, instaId: {user.OInstagramAccountUsername}",
						StatusCode = System.Net.HttpStatusCode.NotFound
					};
					return results;
				}

				var restModel = new RestModel
				{
					RequestType = RequestType.Post,
					User = user
				};

				switch (selectedMedia.MediaType)
				{
					case InstaMediaType.Image:
					{
						var imageData = selectedMedia.MediaData.FirstOrDefault();

						var imageUpload = new InstaImageUpload()
						{
							Uri = imageData.Url,
							//ImageBytes = imageBytes.MediaBytes.ResizeToClosestAspectRatio(),
						};

						var selectedImageMedia = imageData.SelectedMedia.ObjectItem.Medias.FirstOrDefault();
						var credit = selectedImageMedia.User?.Username;

						var mediaInfo = _builder.GenerateMediaInfo(user.Profile.ProfileTopic, selectedImageMedia.Topic,
							credit, SecureRandom.Next(20,28)).Result;
						
						if (!user.Profile.AdditionalConfigurations.AutoGenerateCaption)
							mediaInfo.Caption = string.Empty;
						
						var uploadPhoto = new UploadPhotoModel
						{
							MediaTopic = selectedImageMedia.Topic,
							MediaInfo = mediaInfo,
							Image = imageUpload,
							Location = user.ShortInstagram.Location !=null ? new InstaLocationShort
							{
								Address = user.ShortInstagram.Location.Address,
								Lat = user.ShortInstagram.Location.Coordinates.Latitude,
								Lng = user.ShortInstagram.Location.Coordinates.Longitude,
								Name = user.ShortInstagram.Location.City
							} : null
						};
						
						restModel.BaseUrl = _urlReader.UploadPhoto;
						restModel.JsonBody = JsonConvert.SerializeObject(uploadPhoto);
						break;
					}
					case InstaMediaType.Carousel:
					{
						var selectedCarouselMedia = selectedMedia.MediaData.First()
							.SelectedMedia.ObjectItem.Medias.FirstOrDefault();
						
						var credit = selectedCarouselMedia.User?.Username;
						var mediaInfo = _builder.GenerateMediaInfo(user.Profile.ProfileTopic, selectedCarouselMedia.Topic,
							credit, SecureRandom.Next(20, 28)).Result;
						
						if (!user.Profile.AdditionalConfigurations.AutoGenerateCaption)
							mediaInfo.Caption = string.Empty;
						
						var uploadAlbum = new UploadAlbumModel
						{
							MediaTopic = selectedCarouselMedia.Topic,
							MediaInfo = mediaInfo,
							Location = user.ShortInstagram.Location != null ? new InstaLocationShort
							{
								Address = user.ShortInstagram.Location.Address,
								Lat = user.ShortInstagram.Location.Coordinates.Latitude,
								Lng = user.ShortInstagram.Location.Coordinates.Longitude,
								Name = user.ShortInstagram.Location.City
							} : null,
							Album = selectedMedia.MediaData.Select(f => new InstaAlbumUpload
							{
								ImageToUpload = f.MediaType == InstaMediaType.Image ? new InstaImageUpload
								{
									Uri = f.Url
									//ImageBytes = f.MediaBytes.ResizeToClosestAspectRatio()
								} : null,
								VideoToUpload = f.MediaType == InstaMediaType.Video ? new InstaVideoUpload
								{
									Video = new InstaVideo
									{
										Uri = f.Url
									},
									VideoThumbnail = new InstaImage
									{
										Uri = UploadToS3(postAnalyser.Manipulation.VideoEditor.GenerateVideoThumbnail(f.MediaBytes),$"VideoThumb_{Guid.NewGuid()}").GetAwaiter().GetResult()
									}
								} : null
							}).ToArray(),	
						};
						
						restModel.BaseUrl = _urlReader.UploadCarousel;
						restModel.JsonBody = JsonConvert.SerializeObject(uploadAlbum);
						break;
					}
					case InstaMediaType.Video:
					{
						var selectedVideoMedia = selectedMedia.MediaData.FirstOrDefault().SelectedMedia.ObjectItem.Medias.FirstOrDefault();
						var credit = selectedVideoMedia.User?.Username;
						var mediaInfo = _builder.GenerateMediaInfo(user.Profile.ProfileTopic, selectedVideoMedia.Topic,
							credit, SecureRandom.Next(20, 28)).Result;

						if (!user.Profile.AdditionalConfigurations.AutoGenerateCaption)
							mediaInfo.Caption = string.Empty;
						var video = selectedMedia.MediaData.FirstOrDefault();

						var videoThumb = postAnalyser.Manipulation.VideoEditor.GenerateVideoThumbnail(video.MediaBytes);

						var videoUri = UploadToS3(videoThumb, $"VideoThumb_{Guid.NewGuid()}").GetAwaiter().GetResult();
						var uploadVideo = new UploadVideoModel
						{
							MediaTopic = selectedVideoMedia.Topic,
							MediaInfo = mediaInfo,
							Location = user.ShortInstagram.Location != null ? new InstaLocationShort
							{
								Address = user.ShortInstagram.Location.Address,
								Lat = user.ShortInstagram.Location.Coordinates.Latitude,
								Lng = user.ShortInstagram.Location.Coordinates.Longitude,
								Name = user.ShortInstagram.Location.City
							} : null,
							Video = new InstaVideoUpload
							{
								Video = new InstaVideo
								{
									Uri = video.Url,
									//VideoBytes = video.MediaBytes
								},
								VideoThumbnail = new InstaImage
								{
									Uri = videoUri
								}
							}
						};
						
						restModel.BaseUrl = _urlReader.UploadVideo;
						restModel.JsonBody = JsonConvert.SerializeObject(uploadVideo);
						postAnalyser.Manipulation.VideoEditor.DisposeVideos();
						break;
					}
				}

				results.IsSuccessful = true;
				results.Results = new List<TimelineEventModel>
				{ 
					new TimelineEventModel
					{ 
						ActionName = $"CreatePost_{selectedMedia.MediaType.ToString()}_{imageStrategySettings.ImageStrategyType.ToString()}", 
						Data = restModel,
						ExecutionTime = imageActionOptions.ExecutionTime
					} 
				};
				return results;
			}
			catch (Exception ee)
			{
				results.IsSuccessful = false;
				results.Info = new ErrorResponse
				{
					Message = $"{ee.Message}, user: {user.OAccountId}, instaId: {user.OInstagramAccountUsername}",
					StatusCode = System.Net.HttpStatusCode.InternalServerError,
					Exception = ee
				};
				return results;
			}
		}
	}
}
