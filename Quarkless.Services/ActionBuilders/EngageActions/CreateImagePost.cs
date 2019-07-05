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

		public IEnumerable<TimelineEventModel> Push(IActionOptions actionOptions)
		{
			ImageActionOptions imageActionOptions = actionOptions as ImageActionOptions;
			if(user==null) return null;
			Console.WriteLine("Create Photo Action Started");
			try
			{
				var location = _profile.LocationTargetList?.ElementAtOrDefault(SecureRandom.Next(_profile.LocationTargetList.Count));
				var profileColor = _profile.Theme.Colors.ElementAt(SecureRandom.Next(0, _profile.Theme.Colors.Count));
				var topic_ = _builder.GetTopic(_profile.Topic,20).GetAwaiter().GetResult();

				List<__Meta__<Media>?> TotalResults = new List<__Meta__<Media>?>();

				switch (_profile.AdditionalConfigurations.SearchTypes.ElementAtOrDefault(SecureRandom.Next(_profile.AdditionalConfigurations.SearchTypes.Count)))
				{
					case (int)SearchType.Google:
						var gores = _heartbeatLogic.GetMetaData<Media>(MetaDataType.FetchMediaForSpecificUserGoogle,_profile.Topic,_profile.InstagramAccountId).GetAwaiter().GetResult();
						if (gores != null)
							TotalResults = gores.ToList();
						break;
					case (int)SearchType.Instagram:
						TotalResults = _heartbeatLogic.GetMetaData<Media>(MetaDataType.FetchMediaByTopic,_profile.Topic).GetAwaiter().GetResult().ToList();
						break;
					case (int)SearchType.Yandex:
						if (_profile.Theme.ImagesLike != null && _profile.Theme.ImagesLike.Count > 0)
						{
							var yanres = _heartbeatLogic.GetMetaData<Media>(MetaDataType.FetchMediaForSpecificUserYandex,_profile.Topic,_profile.InstagramAccountId).GetAwaiter().GetResult();
							if (yanres != null)
								TotalResults = yanres.ToList();
						}
						break;
				}

				if (TotalResults.Count <= 0) return null;
				var currentUsersMedia = _heartbeatLogic.GetMetaData<Media>(MetaDataType.FetchUserOwnProfile,_profile.Topic,_profile.InstagramAccountId)
					.GetAwaiter().GetResult().ToList();

				List<byte[]> userMediaBytes = new List<byte[]>();
				if (currentUsersMedia.Count > 0) { 
					var filtered = currentUsersMedia.Select(s=> 
					new __Meta__<Media>(new Media
					{ 
							Medias = s.Value.ObjectItem.Medias.Where(x=>x.MediaType == InstaMediaType.Image).ToList(),
							errors = s.Value.ObjectItem.errors})
					).ToList();

					Parallel.ForEach(filtered, act =>
					{			
						userMediaBytes.Add(act.ObjectItem.Medias.First().MediaUrl.First().DownloadMedia());
					});
				}
				List<byte[]> imagesBytes = new List<byte[]>();

				var filterResults = TotalResults.Select(s=>
				new __Meta__<Media>(new Media 
				{ 
					Medias = s.Value.ObjectItem.Medias.Where(x=>x.MediaType == InstaMediaType.Image).ToList(),
					errors = s.Value.ObjectItem.errors
				})).ToList();

				if(filterResults.Count <=0 ) return null;
				Parallel.ForEach(filterResults.TakeAny(SecureRandom.Next(filterResults.Count)), 
					s => imagesBytes.Add(s.ObjectItem?.Medias.FirstOrDefault().MediaUrl.FirstOrDefault().DownloadMedia()));

				if(imagesBytes.Count <= 0) return null;

				if (userMediaBytes.Count > 0) { 
					imagesBytes = userMediaBytes.Where(u => u != null)
						.RemoveDuplicateImages(imagesBytes, 0.69)
						.ResizeManyToClosestAspectRatio()
						.Where(s => s != null).ToList();
				}

				System.Drawing.Color profileColorRGB = System.Drawing.Color.FromArgb(profileColor.Alpha, profileColor.Red, profileColor.Green, profileColor.Blue);

				var selectedImage = profileColorRGB.MostSimilarImage(imagesBytes.Where(_ => _ != null).ToList()).SharpenImage(4);

				if (selectedImage == null) return null;
				var instaimage = new InstaImageUpload()
				{
					ImageBytes = selectedImage,
				};
				UploadPhotoModel uploadPhoto = new UploadPhotoModel
				{
					Caption = _builder.GenerateMediaInfo(topic_, _profile.Language),
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
				return new List<TimelineEventModel>
				{ 
					new TimelineEventModel
					{ 
						ActionName = $"CreatePhoto_{imageStrategySettings.ImageStrategyType.ToString()}", 
						Data = restModel,
						ExecutionTime = imageActionOptions.ExecutionTime
					} 
				};
			}
			catch (Exception ee)
			{
				Console.WriteLine(ee.Message);
				return null;
			}
		}

		public IActionCommit IncludeUser(UserStoreDetails userStoreDetails)
		{
			user = userStoreDetails;
			return this;
		}
	}

}
