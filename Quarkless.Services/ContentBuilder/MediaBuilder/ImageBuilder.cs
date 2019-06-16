using InstagramApiSharp.Classes.Models;
using Newtonsoft.Json;
using Quarkless.Queue.Jobs.JobOptions;
using Quarkless.Services.Interfaces;
using Quarkless.Services.RequestBuilder.Consts;
using QuarklessContexts.Extensions;
using QuarklessContexts.Models.ContentBuilderModels;
using QuarklessContexts.Models.MediaModels;
using QuarklessContexts.Models.Profiles;
using System;
using System.Collections.Generic;
using System.Linq;
using Quarkless.MediaAnalyser;
namespace Quarkless.Services.ContentBuilder.MediaBuilder
{
	public class ImageBuilder : IContent
	{
		private readonly ProfileModel _profile;
		private readonly UserStore _userSession;
		private readonly DateTime _executeTime;
		private readonly IContentBuilderManager _builder;
		private const int IMAGE_FETCH_LIMIT = 15;
		public ImageBuilder(UserStore userSession, IContentBuilderManager builder, ProfileModel profile, DateTime executeTime)
		{
			_builder = builder;
			_profile = profile;
			_executeTime = executeTime;
			_userSession = userSession;
		}
		public void Operate()
		{
			string exactSize = _profile.AdditionalConfigurations.PostSize;

			var profileColor = _profile.Theme.Colors.ElementAt(SecureRandom.Next(0,_profile.Theme.Colors.Count));
			var topics = _builder.GetTopics(_userSession, _profile.TopicList,15).GetAwaiter().GetResult();	
			var topicSelect = topics.ElementAt(SecureRandom.Next(0,topics.Count));
			List<string> pickedSubsTopics = topicSelect.SubTopics.TakeAny(2).ToList();
			
			List<PostsModel> TotalResults = new List<PostsModel>();

			switch (_profile.AdditionalConfigurations.SearchTypes.ElementAtOrDefault(SecureRandom.Next(_profile.AdditionalConfigurations.SearchTypes.Count)))
			{
				case (int) SearchType.Google:
					var gres = _builder.GetGoogleImages(profileColor.Name, pickedSubsTopics, _profile.AdditionalConfigurations.Sites, IMAGE_FETCH_LIMIT,
						exactSize: exactSize);
					if(gres!=null)
						TotalResults.AddRange(gres);
					break;
				case (int) SearchType.Instagram:
					TotalResults.AddRange(_builder.GetMediaInstagram(_userSession, InstaMediaType.Image, pickedSubsTopics.ToList()));
					break;
				case (int) SearchType.Yandex:
					if(_profile.Theme.ImagesLike!=null && _profile.Theme.ImagesLike.Count > 0) { 
						var yanres = _builder.GetYandexSimilarImages(_profile.Theme.ImagesLike.
							Where(t => t.TopicGroup.ToLower() == topicSelect.TopicName.ToLower()).ToList(), IMAGE_FETCH_LIMIT*2);
						if(yanres!=null)
							TotalResults.AddRange(yanres);
					}
					break;
			}
			
			if(TotalResults.Count<=0) return;

			List<byte[]> imagesBytes = new List<byte[]>();
			var resultSelect = TotalResults.ElementAtOrDefault(SecureRandom.Next(TotalResults.Count));
			imagesBytes.AddRange(resultSelect.MediaData.TakeAny(resultSelect.MediaData.Count/2).Select(s=>s.DownloadMedia()).Where(a=>a!=null));

			System.Drawing.Color profileColorRGB = System.Drawing.Color.FromArgb(profileColor.Alpha, profileColor.Red, profileColor.Green, profileColor.Blue);
			var selectedImage = profileColorRGB.MostSimilarImage(imagesBytes.Where(_=>_!=null).ToList()).SharpenImage(4);

			if(selectedImage == null) return;
			var instaimage = new InstaImageUpload(){
				ImageBytes = selectedImage,
			};
			
			var hash = _builder.GetHashTags(topicSelect.TopicName, 100,10).GetAwaiter().GetResult().ToList();
			hash.AddRange(topicSelect.SubTopics.Select(s => $"#{s}"));
			var hashtags = string.Join(Environment.NewLine, hash.TakeAny(28));
			var caption_ = _builder.GenerateText(topicSelect.TopicName.ToLower(),_profile.Language.ToUpper(),1,SecureRandom.Next(4), SecureRandom.Next(6));

			UploadPhotoModel uploadPhoto = new UploadPhotoModel
			{
				Caption = caption_ + Environment.NewLine + hashtags,
				Image = instaimage,
				Location = null
			};
			RestModel restModel = new RestModel
			{
				User = _userSession,
				BaseUrl = UrlConstants.UploadPhoto,
				RequestType = RequestType.POST,
				JsonBody = JsonConvert.SerializeObject(uploadPhoto)
			};
			_builder.AddToTimeline(restModel, _executeTime);
		}
	}
}

#region action code

//Action action = async()=> await _context.Media.UploadPhotoAsync(new InstaImageUpload(),"caption",new InstaLocationShort());

/*_builder.AddToTimeline(restModel,_executeTime);

var actionFunc = new Func<Task<IResult<InstaMedia>>>(async()=>
{
	return await _context.Media.UploadPhotoAsync(instaimage,"chaos");
});

_builder.AddActionToTimeLine(actionFunc,_executeTime);
*/

#endregion