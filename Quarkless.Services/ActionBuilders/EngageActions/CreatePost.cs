using InstagramApiSharp.Classes.Models;
using Newtonsoft.Json;
using Quarkless.Services.Interfaces;
using QuarklessContexts.Extensions;
using QuarklessContexts.Models.Profiles;
using System;
using System.Collections.Generic;
using System.Linq;
using Quarkless.MediaAnalyser;
using QuarklessLogic.Handlers.RequestBuilder.Consts;
using QuarklessContexts.Models.Timeline;
using QuarklessContexts.Enums;
using Quarkless.Services.Interfaces.Actions;
using Quarkless.Services.StrategyBuilders;
using QuarklessContexts.Models;
using QuarklessLogic.ServicesLogic.HeartbeatLogic;
using QuarklessContexts.Models.ServicesModels.HeartbeatModels;
using QuarklessContexts.Models.ServicesModels.SearchModels;
using QuarklessContexts.Models.MediaModels;
using QuarklessContexts.Classes.Carriers;
using MoreLinq;

namespace Quarkless.Services.ActionBuilders.EngageActions
{
	class CreatePost : IActionCommit
	{
		private UserStoreDetails user;
		private readonly IContentManager _builder;
		private readonly IHeartbeatLogic _heartbeatLogic;
		private ImageStrategySettings imageStrategySettings;
		public CreatePost(IContentManager builder, IHeartbeatLogic heartbeatLogic)
		{
			_heartbeatLogic = heartbeatLogic;
			_builder = builder;
		}

		public IActionCommit IncludeStrategy(IStrategySettings strategy)
		{
			imageStrategySettings = strategy as ImageStrategySettings;
			return this;
		}
		public struct MediaData
		{
			public __Meta__<Media> SelectedMedia;
			public byte[] MediaBytes;
			public string Url { get; set; }
		}
		public class TempSelect
		{
			public InstaMediaType MediaType;
			public List<MediaData> MediaData = new List<MediaData>();
		}
		
		public ResultCarrier<IEnumerable<TimelineEventModel>> Push(IActionOptions actionOptions)
		{
			Console.WriteLine($"Create Media Action Started: {user.OAccountId}, {user.OInstagramAccountUsername}, {user.OInstagramAccountUser}");
			var imageActionOptions = actionOptions as PostActionOptions;
			var results = new ResultCarrier<IEnumerable<TimelineEventModel>>();
			if (user == null)
			{
				results.IsSuccesful = false;
				results.Info = new ErrorResponse
				{
					Message = $"user is null, user: {user.OAccountId}, instaId: {user.OInstagramAccountUsername}",
					StatusCode = System.Net.HttpStatusCode.NotFound
				};
				return results;
			}
			if (!user.Profile.AdditionalConfigurations.EnableAutoPosting)
			{
				results.IsSuccesful = true;
				results.Info = new ErrorResponse
				{
					Message = $"User {user.OAccountId} of {user.OInstagramAccountUsername} has disabled auto posting"
				};
				return results;
			}
			try
			{
				Topics topic;

				if(user.Profile.Topics.SubTopics==null || user.Profile.Topics.SubTopics.Count <= 0) 
					topic = _builder.GetTopic(user, user.Profile, 20).GetAwaiter().GetResult();
				else
					topic = user.Profile.Topics;

				var totalResults = new List<__Meta__<Media>>();
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
						//var ran = new int[] { 0, 1};
						//var num = ran.ElementAt(SecureRandom.Next(ran.Length-1));
						//var selectedQuery = num == 0 ? MetaDataType.FetchMediaForSpecificUserGoogle : MetaDataType.FetchMediaForSepcificUserYandexQuery;
						var gores = _heartbeatLogic.GetMetaData<Media>(MetaDataType.FetchMediaForSpecificUserGoogle,user.Profile.Topics.TopicFriendlyName, user.Profile.InstagramAccountId).GetAwaiter().GetResult();
						selectedAction = MetaDataType.FetchMediaForSpecificUserGoogle;
						if (gores != null)
							totalResults = gores.ToList();
						break;
					case SearchType.Instagram:
						if (user.Profile.UserTargetList != null && user.Profile.UserTargetList.Any())
						{
							var action = new int[] { 0, 1 };
							var pickedRandom = action.ElementAt(SecureRandom.Next(action.Length-1));
							var userId = user.OInstagramAccountUser;
							var selected = MetaDataType.FetchMediaByUserTargetList;
							if(pickedRandom == 1)
							{
								userId = null;
								selected = MetaDataType.FetchMediaByTopic;
							}
							totalResults = _heartbeatLogic.GetMetaData<Media>(selected, user.Profile.Topics.TopicFriendlyName, userId).GetAwaiter().GetResult().ToList();
							selectedAction = selected;
						}
						else { 
							totalResults = _heartbeatLogic.GetMetaData<Media>(MetaDataType.FetchMediaByTopic,user.Profile.Topics.TopicFriendlyName).GetAwaiter().GetResult().ToList();
							selectedAction = MetaDataType.FetchMediaByTopic;
						}
						break;
					case SearchType.Yandex:
						if (user.Profile.Theme.ImagesLike != null && user.Profile.Theme.ImagesLike.Count > 0)
						{
							var yanres = _heartbeatLogic.GetMetaData<Media>(MetaDataType.FetchMediaForSpecificUserYandex,user.Profile.Topics.TopicFriendlyName, user.Profile.InstagramAccountId).GetAwaiter().GetResult();
							selectedAction = MetaDataType.FetchMediaForSpecificUserYandex;
							if (yanres != null)
								totalResults = yanres.ToList();
						}
						break;
				}
				if(selectedAction == MetaDataType.None || totalResults.Count <=0)
				{
					results.IsSuccesful = false;
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

					result.SeenBy.Add(@by);
					if (selectedAction == MetaDataType.FetchMediaByTopic)
					{
						_heartbeatLogic.UpdateMetaData(selectedAction, user.Profile.Topics.TopicFriendlyName, result).GetAwaiter().GetResult();
					}
					else
					{
						_heartbeatLogic.UpdateMetaData(selectedAction, user.Profile.Topics.TopicFriendlyName, result, user.Profile.InstagramAccountId).GetAwaiter().GetResult();
					}

					if (media.MediaType == InstaMediaType.Carousel && currentAmount < carouselAmount)
					{
						enteredType = InstaMediaType.Carousel;
						foreach(var url in media.MediaUrl)
						{
							var imBytes = url.DownloadMedia();
							var colorfreq = imBytes.ByteToBitmap().GetColorPercentage().OrderBy(_ => _.Value);
							var profileColors = user.Profile.Theme.Colors.Select(s => System.Drawing.Color.FromArgb(s.Red, s.Green, s.Blue));
							if (!colorfreq.Take(5).Select(x => x.Key)
								.SimilarColors(profileColors, user.Profile.Theme.Percentage / 100)) continue;
							if (selectedMedia.MediaData.Count > 0)
							{
								var oas = selectedMedia.MediaData[0].MediaBytes.GetClosestAspectRatio();
								var cas = imBytes.GetClosestAspectRatio();
								if (Math.Abs(oas - cas) > 0.5) continue;
								selectedMedia.MediaType = InstaMediaType.Carousel;
								selectedMedia.MediaData.Add(new MediaData
								{
									MediaBytes = imBytes,
									SelectedMedia = result
								});
								currentAmount++;
							}
							else
							{
								selectedMedia.MediaType = InstaMediaType.Carousel;
								selectedMedia.MediaData.Add(new MediaData
								{
									MediaBytes = imBytes,
									SelectedMedia = result
								});
								currentAmount++;
							}
						}
					}
					else if(media.MediaType == InstaMediaType.Video)
					{
						enteredType = InstaMediaType.Video;
						var url = media.MediaUrl.FirstOrDefault();
						var bytes = url?.DownloadMedia();
						if (bytes == null) continue;
						var profileColor = user.Profile.Theme.Colors.Select(s => System.Drawing.Color.FromArgb(s.Red, s.Green, s.Blue)).ElementAt(user.Profile.Theme.Colors.Count-1);
						var simPath = profileColor.IsVideoSimilar(bytes,user.Profile.Theme.Percentage/100,10).GetAwaiter().GetResult();
						if (string.IsNullOrEmpty(simPath)) continue;
						selectedMedia.MediaData.Add(new MediaData
						{
							Url = simPath,
							MediaBytes = bytes,
							SelectedMedia = result
						});
						selectedMedia.MediaType = InstaMediaType.Video;
						break;
					}
					else if(media.MediaType == InstaMediaType.Image)
					{
						enteredType = InstaMediaType.Image;
						var url = media.MediaUrl.FirstOrDefault();
						var imBytes = url?.DownloadMedia();
						if (imBytes == null) continue;
						if (!imBytes.ImageSizeCheckFromByte(size)) continue;
						var colorFrequency = imBytes.ByteToBitmap().GetColorPercentage().OrderBy(_ => _.Value);
						var profileColors = user.Profile.Theme.Colors.Select(s => System.Drawing.Color.FromArgb(s.Red, s.Green, s.Blue));
						if (!colorFrequency.Take(5).Select(x => x.Key)
							.SimilarColors(profileColors, user.Profile.Theme.Percentage / 100)) continue;
						{
							if (typeSelected == InstaMediaType.Image)
							{
								if (!(imBytes.GetAspectRatio() < 1.7)) continue;
								selectedMedia.MediaData.Add(new MediaData 
									{ MediaBytes = imBytes, SelectedMedia = result});										
								selectedMedia.MediaType = InstaMediaType.Image;
								break;
							}

							if (typeSelected != InstaMediaType.Carousel) continue;
							if (imBytes.GetAspectRatio() > 1.6)
							{
								var toCarousel = imBytes.CreateCarousel();
								selectedMedia.MediaType = InstaMediaType.Carousel;
								selectedMedia.MediaData = toCarousel.Select(x => new MediaData { MediaBytes = x, SelectedMedia = result }).ToList();
								break;
							}

							if(imBytes.GetAspectRatio() < 1.6 && currentAmount < carouselAmount )
							{
								if (selectedMedia.MediaData.Count > 0)
								{
									var oas = selectedMedia.MediaData[0].MediaBytes.GetClosestAspectRatio();
									var cas = imBytes.GetClosestAspectRatio();
									if (Math.Abs(oas - cas) > 0.5) continue;
									selectedMedia.MediaType = InstaMediaType.Carousel;
									selectedMedia.MediaData.Add(new MediaData
									{
										MediaBytes = imBytes,
										SelectedMedia = result
									});
									currentAmount++;
								}
								else
								{
									selectedMedia.MediaType = InstaMediaType.Carousel;
									selectedMedia.MediaData.Add(new MediaData
									{
										MediaBytes = imBytes,
										SelectedMedia = result
									});
									currentAmount++;
								}
							}
							else { break; }
						}
					}
				}

				if (selectedMedia.MediaData == null || selectedMedia.MediaData.Count <= 0)
				{
					results.IsSuccesful = false;
					results.Info = new ErrorResponse
					{
						Message = $"could not find any good image to post, user: {user.OAccountId}, instaId: {user.OInstagramAccountUsername}",
						StatusCode = System.Net.HttpStatusCode.NotFound
					};
					return results;
				}

				var restModel = new RestModel
				{
					RequestType = RequestType.POST,
					User = user
				};
				switch (selectedMedia.MediaType)
				{
					case InstaMediaType.Image:
					{
						var imageBytes = selectedMedia.MediaData.SingleOrDefault();
						var instaimage = new InstaImageUpload()
						{
							ImageBytes = imageBytes.MediaBytes.ResizeToClosestAspectRatio(),
						};
						var selectedImageMedia = imageBytes.SelectedMedia.ObjectItem.Medias.FirstOrDefault();
						var credit = selectedImageMedia.User?.Username;
						var mediaInfo = _builder.GenerateMediaInfo(topic, selectedImageMedia.Topic,
							user.Profile.Language, credit);
						if (!user.Profile.AdditionalConfigurations.AutoGenerateCaption)
							mediaInfo.Caption = string.Empty;
						var uploadPhoto = new UploadPhotoModel
						{
							MediaInfo = mediaInfo,
							Image = instaimage,
							Location = user.shortInstagram.Location !=null ? new InstaLocationShort
							{
								Address = user.shortInstagram.Location.Address,
								Lat = user.shortInstagram.Location.Coordinates.Latitude,
								Lng = user.shortInstagram.Location.Coordinates.Longitude,
								Name = user.shortInstagram.Location.City
							} : null
						};
						restModel.BaseUrl = UrlConstants.UploadPhoto;
						restModel.JsonBody = JsonConvert.SerializeObject(uploadPhoto);
						break;
					}
					case InstaMediaType.Carousel:
					{
						var selectedCarouselMedia = selectedMedia.MediaData.FirstOrDefault().SelectedMedia.ObjectItem.Medias.FirstOrDefault();
						var credit = selectedCarouselMedia.User?.Username;
						var mediaInfo = _builder.GenerateMediaInfo(topic, selectedCarouselMedia.Topic,
							user.Profile.Language, credit);
						if (!user.Profile.AdditionalConfigurations.AutoGenerateCaption)
							mediaInfo.Caption = string.Empty;
						var uploadAlbum = new UploadAlbumModel
						{
							MediaInfo = mediaInfo,
							Location = user.shortInstagram.Location != null ? new InstaLocationShort
							{
								Address = user.shortInstagram.Location.Address,
								Lat = user.shortInstagram.Location.Coordinates.Latitude,
								Lng = user.shortInstagram.Location.Coordinates.Longitude,
								Name = user.shortInstagram.Location.City
							} : null,
							Album = selectedMedia.MediaData.Select(f=> new InstaAlbumUpload 
							{ 
								ImageToUpload = new InstaImageUpload { ImageBytes = f.MediaBytes.ResizeToClosestAspectRatio()
								},
							}).ToArray(),	
						};
						restModel.BaseUrl = UrlConstants.UploadCarousel;
						restModel.JsonBody = JsonConvert.SerializeObject(uploadAlbum);
						break;
					}
					case InstaMediaType.Video:
					{
						var selectedVideoMedia = selectedMedia.MediaData.FirstOrDefault().SelectedMedia.ObjectItem.Medias.FirstOrDefault();
						var credit = selectedVideoMedia.User?.Username;
						var mediaInfo = _builder.GenerateMediaInfo(topic, selectedVideoMedia.Topic,
							user.Profile.Language, credit);
						if (!user.Profile.AdditionalConfigurations.AutoGenerateCaption)
							mediaInfo.Caption = string.Empty;
						var video = selectedMedia.MediaData.FirstOrDefault();
						var videoThumb = video.MediaBytes.GenerateVideoThumbnail().GetAwaiter().GetResult();
						var uploadVideo = new UploadVideoModel
						{
							MediaInfo = mediaInfo,
							Location = user.shortInstagram.Location != null ? new InstaLocationShort
							{
								Address = user.shortInstagram.Location.Address,
								Lat = user.shortInstagram.Location.Coordinates.Latitude,
								Lng = user.shortInstagram.Location.Coordinates.Longitude,
								Name = user.shortInstagram.Location.City
							} : null,
							Video = new InstaVideoUpload
							{
								Video = new InstaVideo
								{
									Uri = video.Url,
									VideoBytes = video.MediaBytes
								},
								VideoThumbnail = new InstaImage {  ImageBytes = videoThumb }
							}
						};
						restModel.BaseUrl = UrlConstants.UploadVideo;
						restModel.JsonBody = JsonConvert.SerializeObject(uploadVideo);
						break;
					}
				}

				results.IsSuccesful = true;
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
				results.IsSuccesful = false;
				results.Info = new ErrorResponse
				{
					Message = $"{ee.Message}, user: {user.OAccountId}, instaId: {user.OInstagramAccountUsername}",
					StatusCode = System.Net.HttpStatusCode.InternalServerError,
					Exception = ee
				};
				return results;
			}
		}

		public IActionCommit IncludeUser(UserStoreDetails userStoreDetails)
		{
			user = userStoreDetails;
			return this;
		}
	}

}
