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
			PostActionOptions imageActionOptions = actionOptions as PostActionOptions;
			ResultCarrier<IEnumerable<TimelineEventModel>> Results = new ResultCarrier<IEnumerable<TimelineEventModel>>();
			if (user == null)
			{
				Results.IsSuccesful = false;
				Results.Info = new ErrorResponse
				{
					Message = $"user is null, user: {user.OAccountId}, instaId: {user.OInstagramAccountUsername}",
					StatusCode = System.Net.HttpStatusCode.NotFound
				};
				return Results;
			}
			try
			{
				var location = user.Profile.LocationTargetList?.ElementAtOrDefault(SecureRandom.Next(user.Profile.LocationTargetList.Count-1));
				//var profileColor = user.Profile.Theme.Colors[(SecureRandom.Next(0, user.Profile.Theme.Colors.Count-1))];
				Topics topic_;
				if(user.Profile.Topics.SubTopics==null || user.Profile.Topics.SubTopics.Count <= 0) { 
					topic_ = _builder.GetTopic(user, user.Profile, 20).GetAwaiter().GetResult();
				}
				else
				{
					topic_ = user.Profile.Topics;
				}

				List<__Meta__<Media>> TotalResults = new List<__Meta__<Media>>();
				MetaDataType selectedAction = MetaDataType.None;
				int searchTypeSelected = user.Profile.AdditionalConfigurations.SearchTypes[SecureRandom.Next(user.Profile.AdditionalConfigurations.SearchTypes.Count)];

				switch (searchTypeSelected)
				{
					case (int)SearchType.Google:
						int[] ran = new int[] { 0, 1};
						int num = ran.ElementAt(SecureRandom.Next(ran.Length-1));
						MetaDataType selectedQuery = num == 0 ? MetaDataType.FetchMediaForSpecificUserGoogle : MetaDataType.FetchMediaForSepcificUserYandexQuery;
						var gores = _heartbeatLogic.GetMetaData<Media>(selectedQuery,user.Profile.Topics.TopicFriendlyName, user.Profile.InstagramAccountId).GetAwaiter().GetResult();
						selectedAction = selectedQuery;
						if (gores != null)
							TotalResults = gores.ToList();
						break;
					case (int)SearchType.Instagram:
						if (user.Profile.UserTargetList != null && user.Profile.UserTargetList.Count() > 0)
						{
							int[] action = new int[] { 0, 1 };
							int numpicked = action.ElementAt(SecureRandom.Next(action.Length-1));
							string userId = user.OInstagramAccountUser;
							MetaDataType selected = MetaDataType.FetchMediaByUserTargetList;
							if(numpicked == 1)
							{
								userId = null;
								selected = MetaDataType.FetchMediaByTopic;
							}
							TotalResults = _heartbeatLogic.GetMetaData<Media>(selected, user.Profile.Topics.TopicFriendlyName, userId).GetAwaiter().GetResult().ToList();
							selectedAction = selected;
						}
						else { 
							TotalResults = _heartbeatLogic.GetMetaData<Media>(MetaDataType.FetchMediaByTopic,user.Profile.Topics.TopicFriendlyName).GetAwaiter().GetResult().ToList();
							selectedAction = MetaDataType.FetchMediaByTopic;
						}
						break;
					case (int)SearchType.Yandex:
						if (user.Profile.Theme.ImagesLike != null && user.Profile.Theme.ImagesLike.Count > 0)
						{
							var yanres = _heartbeatLogic.GetMetaData<Media>(MetaDataType.FetchMediaForSpecificUserYandex,user.Profile.Topics.TopicFriendlyName, user.Profile.InstagramAccountId).GetAwaiter().GetResult();
							selectedAction = MetaDataType.FetchMediaForSpecificUserYandex;
							if (yanres != null)
								TotalResults = yanres.ToList();
						}
						break;
				}
				if(selectedAction==MetaDataType.None || TotalResults.Count <=0)
				{
					Results.IsSuccesful = false;
					Results.Info = new ErrorResponse
					{
						Message = $"no action selected, user: {user.OAccountId}, instaId: {user.OInstagramAccountUsername}",
						StatusCode = System.Net.HttpStatusCode.NotFound
					};
					return Results;
				};

				By by = new By
				{
					ActionType = (int)ActionType.CreatePost,
					User = user.Profile.InstagramAccountId
				};
				var filteredResults = TotalResults.Where(exclude=>!exclude.SeenBy.Any(e=>e.User == by.User
				&& (e.ActionType == by.ActionType))).ToList();

				TempSelect _selectedMedia = new TempSelect();
				System.Drawing.Size size = new System.Drawing.Size(900,900);

				List<Chance<InstaMediaType>> typeOfPost = new List<Chance<InstaMediaType>>()
				{
					new Chance<InstaMediaType> {Object = InstaMediaType.Video, Probability = 0.25 },
					new Chance<InstaMediaType>{ Object = InstaMediaType.Image, Probability = 0.30 },
					new Chance<InstaMediaType>{ Object = InstaMediaType.Carousel, Probability = 0.45 }
				};

				var typeSelected = SecureRandom.ProbabilityRoll(typeOfPost);
				int carouselAmount = SecureRandom.Next(2,4);
				int currAmount = 0;
				InstaMediaType _enteredType = InstaMediaType.All;
				lock (filteredResults) { 
					foreach (var result in filteredResults.Shuffle())
					{
						var media = result.ObjectItem.Medias.FirstOrDefault();
						if(_enteredType != InstaMediaType.All)
						{
							if(media.MediaType != _enteredType)
							{
								continue;
							}
						}

						result.SeenBy.Add(by);
						if(selectedAction == MetaDataType.FetchMediaByTopic)
						{
							_heartbeatLogic.UpdateMetaData(selectedAction, user.Profile.Topics.TopicFriendlyName, result).GetAwaiter().GetResult();
						}
						else { 
							_heartbeatLogic.UpdateMetaData(selectedAction, user.Profile.Topics.TopicFriendlyName, result, user.Profile.InstagramAccountId).GetAwaiter().GetResult();
						}

						if (media != null) {
							if(media.MediaType == InstaMediaType.Carousel)
							{
								_enteredType = InstaMediaType.Carousel;
								foreach(var url in media.MediaUrl)
								{
									var imBytes = url.DownloadMedia();
									var colorfreq = imBytes.ByteToBitmap().GetColorPercentage().OrderBy(_ => _.Value);
									var profileColors = user.Profile.Theme.Colors.Select(s => System.Drawing.Color.FromArgb(s.Red, s.Green, s.Blue));
									if (colorfreq.Take(5).Select(x => x.Key).SimilarColors(profileColors, user.Profile.Theme.Percentage / 100))
									{ 
										if (_selectedMedia.MediaData.Count > 0)
										{
											var oas = _selectedMedia.MediaData[0].MediaBytes.GetClosestAspectRatio();
											var cas = imBytes.GetClosestAspectRatio();
											if (cas == oas)
											{
												_selectedMedia.MediaType = InstaMediaType.Carousel;
												_selectedMedia.MediaData.Add(new MediaData
												{
													MediaBytes = imBytes,
													SelectedMedia = result
												});
												currAmount++;
											}
										}
										else
										{
											_selectedMedia.MediaType = InstaMediaType.Carousel;
											_selectedMedia.MediaData.Add(new MediaData
											{
												MediaBytes = imBytes,
												SelectedMedia = result
											});
											currAmount++;
										}
									}
								}
							}
							else if(media.MediaType == InstaMediaType.Video)
							{
								_enteredType = InstaMediaType.Video;
								var url = media.MediaUrl.FirstOrDefault();
								if (url != null)
								{
									var bytes_ = url.DownloadMedia();
									if (bytes_ != null)
									{
										var profileColor = user.Profile.Theme.Colors.Select(s => System.Drawing.Color.FromArgb(s.Red, s.Green, s.Blue)).ElementAt(user.Profile.Theme.Colors.Count-1);
										var simPath = profileColor.IsVideoSimilar(bytes_,user.Profile.Theme.Percentage/100,10);
										if (!string.IsNullOrEmpty(simPath))
										{
											_selectedMedia.MediaData.Add(new MediaData
											{
												Url = simPath,
												MediaBytes = bytes_,
												SelectedMedia = result
											});
											_selectedMedia.MediaType = InstaMediaType.Video;
											break;
										}
									}
								}
							}
							else if(media.MediaType == InstaMediaType.Image)
							{
								_enteredType = InstaMediaType.Image;
								var url = media.MediaUrl.FirstOrDefault();
								if (url != null)
								{
									var imBytes = url.DownloadMedia();
									if (imBytes != null) { 
										if (imBytes.ImageSizeCheckFromByte(size))
										{
											var colorfreq = imBytes.ByteToBitmap().GetColorPercentage().OrderBy(_ => _.Value);
											var profileColors = user.Profile.Theme.Colors.Select(s => System.Drawing.Color.FromArgb(s.Red, s.Green, s.Blue));
											if (colorfreq.Take(5).Select(x => x.Key).SimilarColors(profileColors, user.Profile.Theme.Percentage / 100))
											{
												if (typeSelected == InstaMediaType.Image)
												{
													if (imBytes.GetAspectRatio() < 1.7)
													{
														_selectedMedia.MediaData.Add(new MediaData 
														{ MediaBytes = imBytes, SelectedMedia = result});										
														_selectedMedia.MediaType = InstaMediaType.Image;
														break;
													}
												}
												else if (typeSelected == InstaMediaType.Carousel)
												{
													if (imBytes.GetAspectRatio() > 1.6)
													{
														var toCarousel = imBytes.CreateCarousel();
														_selectedMedia.MediaType = InstaMediaType.Carousel;
														_selectedMedia.MediaData = toCarousel.Select(x => new MediaData { MediaBytes = x, SelectedMedia = result }).ToList();
														break;
													}
													else if(imBytes.GetAspectRatio() < 1.6 && currAmount < carouselAmount )
													{
														if (_selectedMedia.MediaData.Count > 0)
														{
															var oas = _selectedMedia.MediaData[0].MediaBytes.GetClosestAspectRatio();
															var cas = imBytes.GetClosestAspectRatio();
															if(cas == oas)
															{
																_selectedMedia.MediaType = InstaMediaType.Carousel;
																_selectedMedia.MediaData.Add(new MediaData
																{
																	MediaBytes = imBytes,
																	SelectedMedia = result
																});
																currAmount++;
															}
														}
														else
														{
															_selectedMedia.MediaType = InstaMediaType.Carousel;
															_selectedMedia.MediaData.Add(new MediaData
															{
																MediaBytes = imBytes,
																SelectedMedia = result
															});
															currAmount++;
														}
													}
													else { break; }
													}
											}
										}
									}
								}
							}
						}
					}

					if(_selectedMedia.MediaData==null || _selectedMedia.MediaData.Count <=0)
					{
						Results.IsSuccesful = false;
						Results.Info = new ErrorResponse
						{
							Message = $"could not find any good image to post, user: {user.OAccountId}, instaId: {user.OInstagramAccountUsername}",
							StatusCode = System.Net.HttpStatusCode.NotFound
						};
						return Results;
					}
				}

				RestModel restModel = new RestModel
				{
					RequestType = RequestType.POST,
					User = user
				};
				if(_selectedMedia.MediaType == InstaMediaType.Image) { 
					var imageBytes = _selectedMedia.MediaData.SingleOrDefault();
					var instaimage = new InstaImageUpload()
					{
						ImageBytes = imageBytes.MediaBytes.ResizeToClosestAspectRatio(),
					};
					var selectedImageMedia = imageBytes.SelectedMedia.ObjectItem.Medias.FirstOrDefault();
					var credit = selectedImageMedia.User?.Username;
					UploadPhotoModel uploadPhoto = new UploadPhotoModel
					{
						Caption = _builder.GenerateMediaInfo(topic_, selectedImageMedia.Topic, user.Profile.Language, credit),
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
				}
				else if(_selectedMedia.MediaType == InstaMediaType.Carousel)
				{
					var selectedCarouselMedia = _selectedMedia.MediaData.FirstOrDefault().SelectedMedia.ObjectItem.Medias.FirstOrDefault();
					var credit = selectedCarouselMedia.User?.Username;
					UploadAlbumModel uploadAlbum = new UploadAlbumModel
					{
						Caption = _builder.GenerateMediaInfo(topic_, selectedCarouselMedia.Topic, user.Profile.Language, credit),
						Location = user.shortInstagram.Location != null ? new InstaLocationShort
						{
							Address = user.shortInstagram.Location.Address,
							Lat = user.shortInstagram.Location.Coordinates.Latitude,
							Lng = user.shortInstagram.Location.Coordinates.Longitude,
							Name = user.shortInstagram.Location.City
						} : null,
						Album = _selectedMedia.MediaData.Select(f=> new InstaAlbumUpload 
							{ 
								ImageToUpload = new InstaImageUpload { ImageBytes = f.MediaBytes.ResizeToClosestAspectRatio()
							},
						}).ToArray(),	
					};
					restModel.BaseUrl = UrlConstants.UploadCarousel;
					restModel.JsonBody = JsonConvert.SerializeObject(uploadAlbum);
				}
				else if(_selectedMedia.MediaType == InstaMediaType.Video)
				{
					var selectedVideoMedia = _selectedMedia.MediaData.FirstOrDefault().SelectedMedia.ObjectItem.Medias.FirstOrDefault();
					var credit = selectedVideoMedia.User?.Username;
					UploadVideoModel uploadVideo = new UploadVideoModel
					{
						Caption = _builder.GenerateMediaInfo(topic_,selectedVideoMedia.Topic,user.Profile.Language,credit),
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
								Uri = _selectedMedia.MediaData.FirstOrDefault().Url,
								VideoBytes = _selectedMedia.MediaData.FirstOrDefault().MediaBytes
							},
							VideoThumbnail = new InstaImage {  ImageBytes = _selectedMedia.MediaData.FirstOrDefault().MediaBytes.GenerateVideoThumbnail() }
						}
					};
					restModel.BaseUrl = UrlConstants.UploadVideo;
					restModel.JsonBody = JsonConvert.SerializeObject(uploadVideo);
				}

				Results.IsSuccesful = true;
				Results.Results = new List<TimelineEventModel>
				{ 
					new TimelineEventModel
					{ 
						ActionName = $"CreatePost_{_selectedMedia.MediaType.ToString()}_{imageStrategySettings.ImageStrategyType.ToString()}", 
						Data = restModel,
						ExecutionTime = imageActionOptions.ExecutionTime
					} 
				};
				return Results;
			}
			catch (Exception ee)
			{
				Results.IsSuccesful = false;
				Results.Info = new ErrorResponse
				{
					Message = $"{ee.Message}, user: {user.OAccountId}, instaId: {user.OInstagramAccountUsername}",
					StatusCode = System.Net.HttpStatusCode.InternalServerError,
					Exception = ee
				};
				return Results;
			}
		}

		public IActionCommit IncludeUser(UserStoreDetails userStoreDetails)
		{
			user = userStoreDetails;
			return this;
		}
	}

}
