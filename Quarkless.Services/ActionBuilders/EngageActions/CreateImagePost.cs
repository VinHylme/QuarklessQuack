using InstagramApiSharp.Classes.Models;
using Newtonsoft.Json;
using Quarkless.Services.Interfaces;
using QuarklessContexts.Extensions;
using QuarklessContexts.Models.Profiles;
using System;
using System.Collections.Generic;
using System.Linq;
using Quarkless.MediaAnalyser;
using System.Threading.Tasks;
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
using Quarkless.Services.Extensions;
using QuarklessContexts.Classes.Carriers;

namespace Quarkless.Services.ActionBuilders.EngageActions
{
	class CreateImagePost : IActionCommit
	{
		private readonly ProfileModel _profile;
		private UserStoreDetails user;
		private readonly IContentManager _builder;
		private readonly IHeartbeatLogic _heartbeatLogic;
		private ImageStrategySettings imageStrategySettings;
		public CreateImagePost(IContentManager builder, IHeartbeatLogic heartbeatLogic, ProfileModel profile)
		{
			_heartbeatLogic = heartbeatLogic;
			_builder = builder;
			_profile = profile;
		}

		public IActionCommit IncludeStrategy(IStrategySettings strategy)
		{
			imageStrategySettings = strategy as ImageStrategySettings;
			return this;
		}
		private struct TempSelect
		{
			public __Meta__<Media> SelectedImage;
			public byte[] ImagesBytes;
		}
		public ResultCarrier<IEnumerable<TimelineEventModel>> Push(IActionOptions actionOptions)
		{
			ImageActionOptions imageActionOptions = actionOptions as ImageActionOptions;
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
			Console.WriteLine("Create Photo Action Started");
			try
			{
				var location = _profile.LocationTargetList?.ElementAtOrDefault(SecureRandom.Next(_profile.LocationTargetList.Count));
				var profileColor = _profile.Theme.Colors.ElementAt(SecureRandom.Next(0, _profile.Theme.Colors.Count));
				var topic_ = _builder.GetTopic(_profile,20).GetAwaiter().GetResult();

				List<__Meta__<Media>?> TotalResults = new List<__Meta__<Media>?>();
				MetaDataType selectedAction = MetaDataType.None;
				switch (_profile.AdditionalConfigurations.SearchTypes.ElementAtOrDefault(SecureRandom.Next(_profile.AdditionalConfigurations.SearchTypes.Count)))
				{
					case (int)SearchType.Google:
						int[] ran = new int[] { 0, 1};
						int num = ran.ElementAt(SecureRandom.Next(ran.Length));
						var gores = _heartbeatLogic.GetMetaData<Media>(num==0?MetaDataType.FetchMediaForSpecificUserGoogle:MetaDataType.FetchMediaForSepcificUserYandexQuery,_profile.Topics.TopicFriendlyName, _profile.InstagramAccountId).GetAwaiter().GetResult();
						selectedAction = MetaDataType.FetchMediaForSpecificUserGoogle;
						if (gores != null)
							TotalResults = gores.ToList();
						break;
					case (int)SearchType.Instagram:
						TotalResults = _heartbeatLogic.GetMetaData<Media>(MetaDataType.FetchMediaByTopic,_profile.Topics.TopicFriendlyName).GetAwaiter().GetResult().ToList();
						selectedAction = MetaDataType.FetchMediaByTopic;
						break;
					case (int)SearchType.Yandex:
						if (_profile.Theme.ImagesLike != null && _profile.Theme.ImagesLike.Count > 0)
						{
							var yanres = _heartbeatLogic.GetMetaData<Media>(MetaDataType.FetchMediaForSpecificUserYandex,_profile.Topics.TopicFriendlyName, _profile.InstagramAccountId).GetAwaiter().GetResult();
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

				var filterResults = TotalResults.Select(s =>
				new __Meta__<Media>(new Media
				{
					Medias = s.Value.ObjectItem.Medias.Where(x => x.MediaType == InstaMediaType.Image).ToList(),
					errors = s.Value.ObjectItem.errors
				})).ToList();

				TempSelect _selectedImage = new TempSelect();
				System.Drawing.Size size = new System.Drawing.Size(700,700);
				By by = new By
				{
					ActionType = (int)ActionType.CreatePostTypeImage,
					User = _profile.InstagramAccountId
				};

				foreach (var result in filterResults)
				{
					if (!result.SeenBy.Contains(by))
					{
						result.SeenBy.Add(by);
						if(selectedAction == MetaDataType.FetchMediaByTopic)
						{
							_heartbeatLogic.UpdateMetaData(selectedAction, _profile.Topics.TopicFriendlyName, result).GetAwaiter().GetResult();
						}
						else { 
							_heartbeatLogic.UpdateMetaData(selectedAction, _profile.Topics.TopicFriendlyName, result, _profile.InstagramAccountId).GetAwaiter().GetResult();
						}
						var imbytes = result.ObjectItem.Medias.FirstOrDefault().MediaUrl.FirstOrDefault().DownloadMedia();
						var colorfreq = imbytes.ByteToBitmap().GetColorPercentage().OrderByDescending(_=>_.Value);
						var profileColors = _profile.Theme.Colors.Select(s=>System.Drawing.Color.FromArgb(s.Red,s.Green,s.Blue));

						if (imbytes.ImageSizeCheckFromByte(size) && colorfreq.Take(5).Select(x => x.Key).SimilarColors(profileColors,150))
						{
							_selectedImage.ImagesBytes = imbytes;
							_selectedImage.SelectedImage = result;
							break;
						}
						else
						{
							continue;
						}
					}
					else
					{
						continue;
					}
				}
				if(_selectedImage.ImagesBytes==null)
				{
					Results.IsSuccesful = false;
					Results.Info = new ErrorResponse
					{
						Message = $"could not find any good image to post, user: {user.OAccountId}, instaId: {user.OInstagramAccountUsername}",
						StatusCode = System.Net.HttpStatusCode.NotFound
					};
					return Results;
				}
				#region old
				//List<byte[]> imagesBytes = new List<byte[]>();
				//if(filterResults.Count <= 0) return null;

				//Parallel.ForEach(filterResults,s=>
				//{
				//	foreach(var media in s.ObjectItem.Medias)
				//	{
				//		foreach(var url in media.MediaUrl)
				//		{
				//			imagesBytes.Add(url.DownloadMedia());
				//		}
				//	}
				//});
				//imagesBytes = imagesBytes.SelectImageOnSize(new System.Drawing.Size(800,800)).ToList();
				//if(imagesBytes.Count <= 0) return null;

				//System.Drawing.Color profileColorRGB = System.Drawing.Color.FromArgb(profileColor.Alpha, profileColor.Red, profileColor.Green, profileColor.Blue);

				//var selectedImage = profileColorRGB.MostSimilarImage(imagesBytes.Where(_ => _ != null).ToList()).SharpenImage(4);

				//if (selectedImage == null) return null;
				#endregion

				var instaimage = new InstaImageUpload()
				{
					ImageBytes = _selectedImage.ImagesBytes.ResizeToClosestAspectRatio(),
				};
				var credit = _selectedImage.SelectedImage.ObjectItem.Medias.FirstOrDefault().User?.Username;
				UploadPhotoModel uploadPhoto = new UploadPhotoModel
				{
					Caption = _builder.GenerateMediaInfo(topic_, _profile.Language, credit),
					Image = instaimage,
					Location = location != null ? new InstaLocationShort
					{
						Address = location.Address,
						Lat = location.Coordinates.Latitude,
						Lng = location.Coordinates.Longitude,
						Name = location.City
					} : null
				};
				RestModel restModel = new RestModel
				{
					BaseUrl = UrlConstants.UploadPhoto,
					RequestType = RequestType.POST,
					JsonBody = JsonConvert.SerializeObject(uploadPhoto),
					User =  user
				};
				Results.IsSuccesful = true;
				Results.Results = new List<TimelineEventModel>
				{ 
					new TimelineEventModel
					{ 
						ActionName = $"CreatePhoto_{imageStrategySettings.ImageStrategyType.ToString()}", 
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
