using InstagramApiSharp.Classes.Models;
using Newtonsoft.Json;
using Quarkless.Services.Interfaces;
using QuarklessContexts.Extensions;
using QuarklessContexts.Models.ContentBuilderModels;
using QuarklessContexts.Models.MediaModels;
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

namespace Quarkless.Services.ActionBuilders.EngageActions
{
	class CreateImagePost : IActionCommit
	{
		private readonly ProfileModel _profile;
		private readonly UserStore _user;
		private readonly IContentManager _builder;
		private ImageStrategySettings imageStrategySettings;
		public CreateImagePost(IContentManager builder, ProfileModel profile, UserStore user)
		{
			_builder = builder;
			_profile = profile;
			_user = user;
		}

		public IActionCommit IncludeStrategy(IStrategySettings strategy)
		{
			imageStrategySettings = strategy as ImageStrategySettings;
			return this;
		}

		public IEnumerable<TimelineEventModel> Push(IActionOptions actionOptions)
		{
			ImageActionOptions imageActionOptions = actionOptions as ImageActionOptions;
			Console.WriteLine("Create Photo Action Started");
			try
			{
				string exactSize = _profile.AdditionalConfigurations.PostSize;
				var location = _profile.LocationTargetList?.ElementAtOrDefault(SecureRandom.Next(_profile.LocationTargetList.Count));
				var profileColor = _profile.Theme.Colors.ElementAt(SecureRandom.Next(0, _profile.Theme.Colors.Count));
				var topics = _builder.GetTopics(_profile.TopicList, 15).GetAwaiter().GetResult();
				var topicSelect = topics.ElementAt(SecureRandom.Next(0, topics.Count));

				List<string> pickedSubsTopics = topicSelect.SubTopics.TakeAny(2).ToList();
				pickedSubsTopics.Add(topicSelect.TopicName);
				List<PostsModel> TotalResults = new List<PostsModel>();

				switch (_profile.AdditionalConfigurations.SearchTypes.ElementAtOrDefault(SecureRandom.Next(_profile.AdditionalConfigurations.SearchTypes.Count)))
				{
					case (int)SearchType.Google:
						var gres = _builder.GetGoogleImages(profileColor.Name, pickedSubsTopics, _profile.AdditionalConfigurations.Sites, imageActionOptions.ImageFetchLimit,
							exactSize: exactSize);
						if (gres != null)
							TotalResults.AddRange(gres);
						break;
					case (int)SearchType.Instagram:
						TotalResults.AddRange(_builder.GetMediaInstagram(InstaMediaType.Image, pickedSubsTopics.ToList()));
						break;
					case (int)SearchType.Yandex:
						if (_profile.Theme.ImagesLike != null && _profile.Theme.ImagesLike.Count > 0)
						{
							List<GroupImagesAlike> groupImagesAlikes = new List<GroupImagesAlike>
							{
								_profile.Theme.ImagesLike.
								Where(s=>s.TopicGroup.ToLower() == topicSelect.TopicName.ToLower()).
								ElementAtOrDefault(SecureRandom.Next(_profile.Theme.ImagesLike.Count))
							};
							var yanres = _builder.GetYandexSimilarImages(groupImagesAlikes, imageActionOptions.ImageFetchLimit * 8);
							if (yanres != null)
								TotalResults.AddRange(yanres);
						}
						break;
				}

				if (TotalResults.Count <= 0) return null;
				List<PostsModel> currentUsersMedia = _builder.GetUserMedia(limit: 1).ToList();

				List<byte[]> userMediaBytes = new List<byte[]>();
				if (currentUsersMedia.Count <= 0) return null;
				Parallel.ForEach(currentUsersMedia.First().MediaData, act =>
				{
					userMediaBytes.Add(act.DownloadMedia());
				});

				List<byte[]> imagesBytes = new List<byte[]>();
				var resultSelect = TotalResults.ElementAtOrDefault(SecureRandom.Next(TotalResults.Count));
				Parallel.ForEach(resultSelect.MediaData.TakeAny(SecureRandom.Next(resultSelect.MediaData.Count / 2)), s => imagesBytes.Add(s?.DownloadMedia()));

				imagesBytes = userMediaBytes.Where(u => u != null)
					.RemoveDuplicateImages(imagesBytes, 0.69)
					.ResizeManyToClosestAspectRatio()
					.Where(s => s != null).ToList();


				System.Drawing.Color profileColorRGB = System.Drawing.Color.FromArgb(profileColor.Alpha, profileColor.Red, profileColor.Green, profileColor.Blue);

				var selectedImage = profileColorRGB.MostSimilarImage(imagesBytes.Where(_ => _ != null).ToList()).SharpenImage(4);

				if (selectedImage == null) return null;
				var instaimage = new InstaImageUpload()
				{
					ImageBytes = selectedImage,
				};
				UploadPhotoModel uploadPhoto = new UploadPhotoModel
				{
					Caption = _builder.GenerateMediaInfo(topicSelect, _profile.Language),
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
					User = _user
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
	}

}
